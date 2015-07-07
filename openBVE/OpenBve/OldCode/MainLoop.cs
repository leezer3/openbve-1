using System;
using System.Windows.Forms;
using OpenBveApi.Math;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using OpenTK;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve {
	internal static class MainLoop {

		// declarations
		internal static bool LimitFramerate = false;
		private static bool Quit = false;
		private static int TimeFactor = 1;
		private static ViewPortMode CurrentViewPortMode = ViewPortMode.Scenery;

		// --------------------------------

		internal static void StartLoopEx(formMain.MainDialogResult result) {
			events = new Queue<InputEvent>();
			Renderer.Initialize();
			Renderer.InitializeLighting();
			Program.UI.SwapBuffers();
			MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
			MainLoop.InitializeMotionBlur();
			ProcessEvents();
			GL.Disable(EnableCap.Fog);
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0.0, (double)Screen.Width, (double)Screen.Height, 0.0, -1.0, 1.0);
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			Renderer.DrawLoadingScreen();
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
			Program.UI.SwapBuffers();
			Loading.LoadSynchronously(result.RouteFile, result.RouteEncoding, result.TrainFolder, result.TrainEncoding);
			Timetable.CreateTimetable();
			for (int i = 0; i < Interface.MessageCount; i++) {
				if (Interface.Messages[i].Type == Interface.MessageType.Critical) {
					MessageBox.Show("A critical error has occured:\n\n" + Interface.Messages[i].Text + "\n\nPlease inspect the error log file for further information.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
			}
			Renderer.InitializeLighting();
			Game.LogRouteName = System.IO.Path.GetFileName(result.RouteFile);
			Game.LogTrainName = System.IO.Path.GetFileName(result.TrainFolder);
			Game.LogDateTime = DateTime.Now;
			StartLoop();
		}
		
		// start loop
		private static void StartLoop() {
			// load in advance
			Textures.UnloadAllTextures();
			if (Interface.CurrentOptions.LoadInAdvance) {
				Textures.LoadAllTextures();
			}
			// camera
			ObjectManager.InitializeVisibility();
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.0, true, false);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -0.1, true, false);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.1, true, false);
			World.CameraTrackFollower.TriggerType = TrackManager.EventTriggerType.Camera;
			// starting time and track position
			Game.SecondsSinceMidnight = 0.0;
			Game.StartupTime = 0.0;
			int PlayerFirstStationIndex = -1;
			double PlayerFirstStationPosition = 0.0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerStop & Game.Stations[i].Stops.Length != 0) {
					PlayerFirstStationIndex = i;
					int s = Game.GetStopIndex(i, TrainManager.PlayerTrain.Cars.Length);
					if (s >= 0) {
						PlayerFirstStationPosition = Game.Stations[i].Stops[s].TrackPosition;
					} else {
						PlayerFirstStationPosition = Game.Stations[i].DefaultTrackPosition;
					}
					if (Game.Stations[i].ArrivalTime < 0.0) {
						if (Game.Stations[i].DepartureTime < 0.0) {
							Game.SecondsSinceMidnight = 0.0;
							Game.StartupTime = 0.0;
						} else {
							Game.SecondsSinceMidnight = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
							Game.StartupTime = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
						}
					} else {
						Game.SecondsSinceMidnight = Game.Stations[i].ArrivalTime;
						Game.StartupTime = Game.Stations[i].ArrivalTime;
					}
					break;
				}
			}
			int OtherFirstStationIndex = -1;
			double OtherFirstStationPosition = 0.0;
			double OtherFirstStationTime = 0.0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerPass & Game.Stations[i].Stops.Length != 0) {
					OtherFirstStationIndex = i;
					int s = Game.GetStopIndex(i, TrainManager.PlayerTrain.Cars.Length);
					if (s >= 0) {
						OtherFirstStationPosition = Game.Stations[i].Stops[s].TrackPosition;
					} else {
						OtherFirstStationPosition = Game.Stations[i].DefaultTrackPosition;
					}
					if (Game.Stations[i].ArrivalTime < 0.0) {
						if (Game.Stations[i].DepartureTime < 0.0) {
							OtherFirstStationTime = 0.0;
						} else {
							OtherFirstStationTime = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
						}
					} else {
						OtherFirstStationTime = Game.Stations[i].ArrivalTime;
					}
					break;
				}
			}
			if (Game.PrecedingTrainTimeDeltas.Length != 0) {
				OtherFirstStationTime -= Game.PrecedingTrainTimeDeltas[Game.PrecedingTrainTimeDeltas.Length - 1];
				if (OtherFirstStationTime < Game.SecondsSinceMidnight) {
					Game.SecondsSinceMidnight = OtherFirstStationTime;
				}
			}
			// initialize trains
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				TrainManager.InitializeTrain(TrainManager.Trains[i]);
				int s = i == TrainManager.PlayerTrain.TrainIndex ? PlayerFirstStationIndex : OtherFirstStationIndex;
				if (s >= 0) {
					if (Game.Stations[s].OpenLeftDoors) {
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
							TrainManager.Trains[i].Cars[j].Specs.AnticipatedLeftDoorsOpened = true;
						}
					}
					if (Game.Stations[s].OpenRightDoors) {
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
							TrainManager.Trains[i].Cars[j].Specs.AnticipatedRightDoorsOpened = true;
						}
					}
				}
				if (Game.Sections.Length != 0) {
					Game.Sections[0].Enter(TrainManager.Trains[i]);
				}
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
					double length = TrainManager.Trains[i].Cars[0].Length;
					TrainManager.MoveCar(TrainManager.Trains[i], j, -length, 0.01);
					TrainManager.MoveCar(TrainManager.Trains[i], j, length, 0.01);
				}
			}
			// score
			Game.CurrentScore.ArrivalStation = PlayerFirstStationIndex + 1;
			Game.CurrentScore.DepartureStation = PlayerFirstStationIndex;
			Game.CurrentScore.Maximum = 0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (i != PlayerFirstStationIndex & Game.PlayerStopsAtStation(i)) {
					if (i == 0 || Game.Stations[i - 1].StationType != Game.StationType.ChangeEnds) {
						Game.CurrentScore.Maximum += Game.ScoreValueStationArrival;
					}
				}
			}
			if (Game.CurrentScore.Maximum <= 0) {
				Game.CurrentScore.Maximum = Game.ScoreValueStationArrival;
			}
			// signals
			if (Game.Sections.Length > 0) {
				Game.UpdateSection(Game.Sections.Length - 1);
			}
			// move train in position
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				double p;
				if (i == TrainManager.PlayerTrain.TrainIndex) {
					p = PlayerFirstStationPosition;
				} else if (TrainManager.Trains[i].State == TrainManager.TrainState.Bogus) {
					p = Game.BogusPretrainInstructions[0].TrackPosition;
					TrainManager.Trains[i].AI = new Game.BogusPretrainAI(TrainManager.Trains[i]);
				} else {
					p = OtherFirstStationPosition;
				}
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
					TrainManager.MoveCar(TrainManager.Trains[i], j, p, 0.01);
				}
			}
			// timetable
			if (Timetable.DefaultTimetableDescription.Length == 0) {
				Timetable.DefaultTimetableDescription = Game.LogTrainName;
			}
			// initialize camera
			if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable) {
				World.CameraMode = World.CameraViewMode.InteriorLookAhead;
			}
			TrainManager.UpdateCamera(TrainManager.PlayerTrain);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
			World.CameraSavedInterior = new World.CameraAlignment();
			World.CameraSavedExterior = new World.CameraAlignment(new Vector3(-2.5, 1.5, -15.0), 0.3, -0.2, 0.0, PlayerFirstStationPosition, 1.0);
			World.CameraSavedTrack = new World.CameraAlignment(new Vector3(-3.0, 2.5, 0.0), 0.3, 0.0, 0.0, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - 10.0, 1.0);
			// timer
			Timers.Initialize();
			// framerate display
			TotalTimeElapsedForInfo = 0.0;
			TotalTimeElapsedForSectionUpdate = 0.0;
			TotalFramesElapsed = 0;
			// signalling sections
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				int s = TrainManager.Trains[i].CurrentSectionIndex;
				Game.Sections[s].Enter(TrainManager.Trains[i]);
			}
			if (Game.Sections.Length > 0) {
				Game.UpdateSection(Game.Sections.Length - 1);
			}
			// fast-forward until start time
			{
				Game.MinimalisticSimulation = true;
				const double w = 0.25;
				double u = Game.StartupTime - Game.SecondsSinceMidnight;
				if (u > 0) {
					while (true) {
						double v = u < w ? u : w; u -= v;
						Game.SecondsSinceMidnight += v;
						TrainManager.UpdateTrains(v);
						if (u <= 0.0) break;
						TotalTimeElapsedForSectionUpdate += v;
						if (TotalTimeElapsedForSectionUpdate >= 1.0) {
							if (Game.Sections.Length > 0) {
								Game.UpdateSection(Game.Sections.Length - 1);
							}
							TotalTimeElapsedForSectionUpdate = 0.0;
						}
					}
				}
				Game.MinimalisticSimulation = false;
			}
			// animated objects
			ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
			TrainManager.UpdateTrainObjects(0.0, true);
			// timetable
			if (TrainManager.PlayerTrain.Station >= 0) {
				Timetable.UpdateCustomTimetable(Game.Stations[TrainManager.PlayerTrain.Station].TimetableDaytimeTexture, Game.Stations[TrainManager.PlayerTrain.Station].TimetableNighttimeTexture);
				if (Timetable.CustomObjectsUsed != 0 & Timetable.CustomTimetableAvailable) {
					Timetable.CurrentTimetable = Timetable.TimetableState.Custom;
				}
			}
			// warnings / errors
			if (Interface.MessageCount != 0) {
				int filesNotFound = 0;
				int errors = 0;
				int warnings = 0;
				for (int i = 0; i < Interface.MessageCount; i++) {
					if (Interface.Messages[i].FileNotFound) {
						filesNotFound++;
					} else if (Interface.Messages[i].Type == Interface.MessageType.Error) {
						errors++;
					} else if (Interface.Messages[i].Type == Interface.MessageType.Warning) {
						warnings++;
					}
				}
				if (filesNotFound != 0) {
					Game.AddDebugMessage(filesNotFound.ToString() + " file(s) not found", 10.0);
				}
				if (errors != 0 & warnings != 0) {
					Game.AddDebugMessage(errors.ToString() + " error(s), " + warnings.ToString() + " warning(s)", 10.0);
				} else if (errors != 0) {
					Game.AddDebugMessage(errors.ToString() + " error(s)", 10.0);
				} else {
					Game.AddDebugMessage(warnings.ToString() + " warning(s)", 10.0);
				}
			}
//			if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin is Win32Plugin) {
//				Game.AddDebugMessage("The train uses a Win32 plugin.", 10.0);
//			}
			// loop
			World.InitializeCameraRestriction();
			Program.UI.UpdateFrame += UpdateEvent;
			Program.UI.RenderFrame += RenderEvent;
			Program.UI.Run();
			// finish
			try {
				Interface.SaveLogs();
			} catch { }
			try {
				Interface.SaveOptions();
			} catch { }
		}

		static void RenderEvent (object sender, FrameEventArgs e)
		{
			Renderer.RenderScene(TimeElapsed);
			Program.UI.SwapBuffers();
		}
		private static double TotalTimeElapsedForInfo;
		private static double TotalTimeElapsedForSectionUpdate;
		private static int TotalFramesElapsed;
		private static double TimeElapsed = 0;
		private static void UpdateEvent(object sender, FrameEventArgs e){

			// timer
			double RealTimeElapsed;
			if (Game.SecondsSinceMidnight >= Game.StartupTime) {
				RealTimeElapsed = Timers.GetElapsedTime();
				TimeElapsed = RealTimeElapsed * (double)TimeFactor;
			} else {
				RealTimeElapsed = 0.0;
				TimeElapsed = Game.StartupTime - Game.SecondsSinceMidnight;
			}
			TotalTimeElapsedForInfo += TimeElapsed;
			TotalTimeElapsedForSectionUpdate += TimeElapsed;
			TotalFramesElapsed++;
			if (TotalTimeElapsedForSectionUpdate >= 1.0) {
				if (Game.Sections.Length != 0) {
					Game.UpdateSection(Game.Sections.Length - 1);
				}
				TotalTimeElapsedForSectionUpdate = 0.0;
			}
			if (TotalTimeElapsedForInfo >= 0.2) {
				Game.InfoFrameRate = (double)TimeFactor * (double)TotalFramesElapsed / TotalTimeElapsedForInfo;
				TotalTimeElapsedForInfo = 0.0;
				TotalFramesElapsed = 0;
			}
			// events
			UpdateControlRepeats(RealTimeElapsed);
			ProcessEvents();
			World.CameraAlignmentDirection = new World.CameraAlignment();
			World.UpdateMouseGrab(TimeElapsed);
			ProcessControls(TimeElapsed);
			if (Quit) Program.UI.Exit();
			// update simulation in chunks
			{
				const double chunkTime = 1.0 / 75.0;
				if (TimeElapsed <= chunkTime) {
					Game.SecondsSinceMidnight += TimeElapsed;
					TrainManager.UpdateTrains(TimeElapsed);
				} else {
					const int maxChunks = 75;
					int chunks = Math.Min((int)Math.Round(TimeElapsed / chunkTime), maxChunks);
					double time = TimeElapsed / (double)chunks;
					for (int i = 0; i < chunks; i++) {
						Game.SecondsSinceMidnight += time;
						TrainManager.UpdateTrains(time);
					}
				}
			}
			// update in one piece
			ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
			if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead | World.CameraMode == World.CameraViewMode.Exterior) {
				TrainManager.UpdateCamera(TrainManager.PlayerTrain);
			}
			if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable) {
				World.UpdateDriverBody(TimeElapsed);
			}
			World.UpdateAbsoluteCamera(TimeElapsed);
			TrainManager.UpdateTrainObjects(TimeElapsed, false);
			if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead | World.CameraMode == World.CameraViewMode.Exterior) {
				ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
				int d = TrainManager.PlayerTrain.DriverCar;
				World.CameraSpeed = TrainManager.PlayerTrain.Cars[d].Specs.CurrentSpeed;
			} else {
				World.CameraSpeed = 0.0;
			}
			Game.UpdateScore(TimeElapsed);
			Game.UpdateMessages();
			Game.UpdateScoreMessages(TimeElapsed);
			Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
			Game.UpdateBlackBox();
			// pause/menu
			while (Game.CurrentInterface != Game.InterfaceType.Normal) {
				UpdateControlRepeats(RealTimeElapsed);
				ProcessEvents();
				ProcessControls(0.0);
				if (Quit) break;
				if (Game.CurrentInterface == Game.InterfaceType.Pause) {
					System.Threading.Thread.Sleep(10);
				}
				TimeElapsed = Timers.GetElapsedTime();
			}
			#if DEBUG
			CheckForOpenGlError("MainLoop");
			#endif
		}
		// --------------------------------

		// repeats
		private struct ControlRepeat {
			internal int ControlIndex;
			internal double Countdown;
			internal ControlRepeat(int controlIndex, double countdown) {
				this.ControlIndex = controlIndex;
				this.Countdown = countdown;
			}
		}
		private static ControlRepeat[] RepeatControls = new ControlRepeat[16];
		private static int RepeatControlsUsed = 0;
		private static void AddControlRepeat(int controlIndex) {
			if (RepeatControls.Length == RepeatControlsUsed) {
				Array.Resize<ControlRepeat>(ref RepeatControls, RepeatControls.Length << 1);
			}
			RepeatControls[RepeatControlsUsed] = new ControlRepeat(controlIndex, Interface.CurrentOptions.KeyRepeatDelay);
			RepeatControlsUsed++;
		}
		private static void RemoveControlRepeat(int controlIndex) {
			for (int i = 0; i < RepeatControlsUsed; i++) {
				if (RepeatControls[i].ControlIndex == controlIndex) {
					RepeatControls[i] = RepeatControls[RepeatControlsUsed - 1];
					RepeatControlsUsed--;
					break;
				}
			}
		}
		private static void UpdateControlRepeats(double timeElapsed) {
			for (int i = 0; i < RepeatControlsUsed; i++) {
				RepeatControls[i].Countdown -= timeElapsed;
				if (RepeatControls[i].Countdown <= 0.0) {
					int j = RepeatControls[i].ControlIndex;
					Interface.CurrentControls[j].AnalogState = 1.0;
					Interface.CurrentControls[j].DigitalState = Interface.DigitalControlState.Pressed;
					RepeatControls[i].Countdown += Interface.CurrentOptions.KeyRepeatInterval;
				}
			}
		}
		
		// process events
		private static Interface.KeyboardModifier CurrentKeyboardModifier = Interface.KeyboardModifier.None;
		private static void ProcessEvents() {
			JoystickPoll();
			while (events.Count > 0) {
				InputEvent ev = events.Dequeue();
				switch (ev.Type) {
				// quit
					case InputType.Quit:
						Quit = true;
						return;
				// resize
					case InputType.Resize:
						UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
						InitializeMotionBlur();
						break;
				// key down
					case InputType.KeyDown:
						OpenTK.Input.KeyboardKeyEventArgs keyDownData = (OpenTK.Input.KeyboardKeyEventArgs)ev.Data;
						if ((keyDownData.Modifiers & KeyModifiers.Control) != 0)
							CurrentKeyboardModifier |= Interface.KeyboardModifier.Shift;
						if ((keyDownData.Modifiers & KeyModifiers.Shift) != 0)
							CurrentKeyboardModifier |= Interface.KeyboardModifier.Ctrl;
						if ((keyDownData.Modifiers & KeyModifiers.Alt) != 0)
							CurrentKeyboardModifier |= Interface.KeyboardModifier.Alt;
						for (int i = 0; i < Interface.CurrentControls.Length; i++) {
							if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Keyboard) {
								if (Interface.CurrentControls[i].Element == (int)keyDownData.Key && Interface.CurrentControls[i].Modifier == CurrentKeyboardModifier) {
									Interface.CurrentControls[i].AnalogState = 1.0;
									Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
									AddControlRepeat(i);
								}
							}
						}
						break;
				// key up
					case InputType.KeyUp:
						OpenTK.Input.KeyboardKeyEventArgs keyUpData = (OpenTK.Input.KeyboardKeyEventArgs)ev.Data;
						if ((keyUpData.Modifiers & KeyModifiers.Control) != 0)
							CurrentKeyboardModifier &= ~Interface.KeyboardModifier.Shift;
						if ((keyUpData.Modifiers & KeyModifiers.Shift) != 0)
							CurrentKeyboardModifier &= ~Interface.KeyboardModifier.Ctrl;
						if ((keyUpData.Modifiers & KeyModifiers.Alt) != 0)
							CurrentKeyboardModifier &= ~Interface.KeyboardModifier.Alt;
						for (int i = 0; i < Interface.CurrentControls.Length; i++) {
							if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Keyboard) {
								if (Interface.CurrentControls[i].Element == (int)keyUpData.Key) {
									Interface.CurrentControls[i].AnalogState = 0.0;
									Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
									RemoveControlRepeat(i);
								}
							}
						}
						break;
				// joystick button down
					case InputType.JoyButtonDown:
						ButtonEventArgs buttonDownData = (ButtonEventArgs)ev.Data;
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Button) {
										if (Interface.CurrentControls[i].Device == (int)buttonDownData.Index & Interface.CurrentControls[i].Element == (int)buttonDownData.Button) {
											Interface.CurrentControls[i].AnalogState = 1.0;
											Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
											AddControlRepeat(i);
										}
									}
								}
							}
						}
						break;
				// joystick button up
					case InputType.JoyButtonUp:
						ButtonEventArgs buttonUpData = (ButtonEventArgs)ev.Data;
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Button) {
										if (Interface.CurrentControls[i].Device == (int)buttonUpData.Index & Interface.CurrentControls[i].Element == (int)buttonUpData.Button) {
											Interface.CurrentControls[i].AnalogState = 0.0;
											Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
											RemoveControlRepeat(i);
										}
									}
								}
							}
						}
						break;
				// joystick hat
					case InputType.JoyHatMotion:
						HatEventArgs hatData = (HatEventArgs)ev.Data;
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Hat) {
										if (Interface.CurrentControls[i].Device == (int)hatData.Index) {
											if (Interface.CurrentControls[i].Element == (int)hatData.Hat) {
												if ((int)Interface.CurrentControls[i].Direction == (int)hatData.Position) {
													Interface.CurrentControls[i].AnalogState = 1.0;
													Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
												} else {
													Interface.CurrentControls[i].AnalogState = 0.0;
													Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
												}
											}
										}
									}
								}
							}
						}
						break;
				// joystick axis
					case InputType.JoyAxisMotion:
						AxisEventArgs axisData = (AxisEventArgs)ev.Data;
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Axis) {
										if (Interface.CurrentControls[i].Device == (int)axisData.Index & Interface.CurrentControls[i].Element == (int)axisData.Axis) {
											double a = axisData.Value;
											if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogHalf) {
												if (Math.Sign(a) == Math.Sign((float)Interface.CurrentControls[i].Direction)) {
													a = Math.Abs(a);
													if (a < Interface.CurrentOptions.JoystickAxisThreshold) {
														Interface.CurrentControls[i].AnalogState = 0.0;
													} else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0) {
														Interface.CurrentControls[i].AnalogState = (a - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else {
														Interface.CurrentControls[i].AnalogState = 1.0;
													}
												}
											} else if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogFull) {
												a *= (double)Interface.CurrentControls[i].Direction;
												if (a > -Interface.CurrentOptions.JoystickAxisThreshold & a < Interface.CurrentOptions.JoystickAxisThreshold) {
													Interface.CurrentControls[i].AnalogState = 0.0;
												} else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0) {
													if (a < 0.0) {
														Interface.CurrentControls[i].AnalogState = (a + Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else if (a > 0.0) {
														Interface.CurrentControls[i].AnalogState = (a - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else {
														Interface.CurrentControls[i].AnalogState = 0.0;
													}
												} else {
													Interface.CurrentControls[i].AnalogState = (double)Math.Sign(a);
												}
											} else {
												if (Math.Sign(a) == Math.Sign((float)Interface.CurrentControls[i].Direction)) {
													a = Math.Abs(a);
													if (a < Interface.CurrentOptions.JoystickAxisThreshold) {
														a = 0.0;
													} else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0) {
														a = (a - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else {
														a = 1.0;
													}
													if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released | Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.ReleasedAcknowledged) {
														if (a > 0.67)
															Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
													} else {
														if (a < 0.33)
															Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
													}
												}
											}
										}
									}
								}
							}
						}
						break;
						case InputType.MouseButtonDown:
							MouseButtonEventArgs mouseButtonData = (MouseButtonEventArgs)ev.Data;
						// mouse button down
						if (mouseButtonData.Button == MouseButton.Right) {
							// mouse grab
							World.MouseGrabEnabled = !World.MouseGrabEnabled;
							if (World.MouseGrabEnabled) {
								World.MouseGrabTarget = new World.Vector2D(0.0, 0.0);
								Game.AddMessage(Interface.GetInterfaceString("notification_mousegrab_on"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
							} else {
								Game.AddMessage(Interface.GetInterfaceString("notification_mousegrab_off"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
							}
						}
						break;
					case InputType.MouseMotion:
						MouseMoveEventArgs mouseMotionData = (MouseMoveEventArgs)ev.Data;
						// mouse motion
						if (World.MouseGrabIgnoreOnce) {
							World.MouseGrabIgnoreOnce = false;
						} else if (World.MouseGrabEnabled) {
							World.MouseGrabTarget = new World.Vector2D((double)mouseMotionData.XDelta, (double)mouseMotionData.YDelta);
						}
						break;
				}
			}
		}

		// process controls
		private static void ProcessControls(double TimeElapsed) {
			switch (Game.CurrentInterface) {
				case Game.InterfaceType.Pause:
					// pause
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital) {
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed) {
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.MiscPause:
										Game.CurrentInterface = Game.InterfaceType.Normal;
										break;
									case Interface.Command.MenuActivate:
										Game.CreateMenu(false);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.MiscQuit:
										Game.CreateMenu(true);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.MiscFullscreen:
										Screen.ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
										Sounds.GlobalMute = !Sounds.GlobalMute;
										Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
								}
							}
						}
					} break;
				case Game.InterfaceType.Menu:
					// menu
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital) {
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed) {
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.MenuUp:
										{
											// up
											Game.MenuEntry[] a = Game.CurrentMenu;
											int j = Game.CurrentMenuSelection.Length - 1;
											int k = 0; while (k < j) {
												Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
												if (b == null) break;
												a = b.Entries; k++;
											}
											if (Game.CurrentMenuSelection[j] > 0 && !(a[Game.CurrentMenuSelection[j] - 1] is Game.MenuCaption)) {
												Game.CurrentMenuSelection[j]--;
											}
										} break;
									case Interface.Command.MenuDown:
										{
											// down
											Game.MenuEntry[] a = Game.CurrentMenu;
											int j = Game.CurrentMenuSelection.Length - 1;
											int k = 0; while (k < j) {
												Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
												if (b == null) break;
												a = b.Entries; k++;
											}
											if (Game.CurrentMenuSelection[j] < a.Length - 1) {
												Game.CurrentMenuSelection[j]++;
											}
										} break;
									case Interface.Command.MenuEnter:
										{
											// enter
											Game.MenuEntry[] a = Game.CurrentMenu;
											int j = Game.CurrentMenuSelection.Length - 1;
											{
												int k = 0;
												while (k < j) {
													Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
													if (b == null) break;
													a = b.Entries; k++;
												}
											}
											if (a[Game.CurrentMenuSelection[j]] is Game.MenuCommand) {
												// command
												Game.MenuCommand b = (Game.MenuCommand)a[Game.CurrentMenuSelection[j]];
												switch (b.Tag) {
													case Game.MenuTag.Back:
														// back
														if (Game.CurrentMenuSelection.Length <= 1) {
															Game.CurrentInterface = Game.InterfaceType.Normal;
														} else {
															Array.Resize<int>(ref Game.CurrentMenuSelection, Game.CurrentMenuSelection.Length - 1);
															Array.Resize<double>(ref Game.CurrentMenuOffsets, Game.CurrentMenuOffsets.Length - 1);
														} break;
													case Game.MenuTag.JumpToStation:
														// jump to station
														TrainManager.JumpTrain(TrainManager.PlayerTrain, b.Data);
														break;
													case Game.MenuTag.ExitToMainMenu:
														Program.RestartArguments = Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade ? "/review" : "";
														Quit = true;
														break;
													case Game.MenuTag.Quit:
														// quit
														Quit = true;
														break;
												}
											} else if (a[Game.CurrentMenuSelection[j]] is Game.MenuSubmenu) {
												// menu
												Game.MenuSubmenu b = (Game.MenuSubmenu)a[Game.CurrentMenuSelection[j]];
												int n = Game.CurrentMenuSelection.Length;
												Array.Resize<int>(ref Game.CurrentMenuSelection, n + 1);
												Array.Resize<double>(ref Game.CurrentMenuOffsets, n + 1);
												/* Select the first non-caption entry. */
												int selection;
												for (selection = 0; selection < b.Entries.Length; selection++) {
													if (!(b.Entries[selection] is Game.MenuCaption)) break;
												}
												/* Select the next station if this menu has stations in it. */
												int station = TrainManager.PlayerTrain.LastStation;
												if (TrainManager.PlayerTrain.Station == -1 | TrainManager.PlayerTrain.StationState != TrainManager.TrainStopState.Pending) {
													for (int k = station + 1; k < Game.Stations.Length; k++) {
														if (Game.StopsAtStation(k, TrainManager.PlayerTrain)) {
															station = k;
															break;
														}
													}
												}
												for (int k = selection + 1; k < b.Entries.Length; k++) {
													Game.MenuCommand c = b.Entries[k] as Game.MenuCommand;
													if (c != null && c.Tag == Game.MenuTag.JumpToStation) {
														if (c.Data <= station) {
															selection = k;
														}
													}
												}
												Game.CurrentMenuSelection[n] = selection < b.Entries.Length ? selection : 0;
												Game.CurrentMenuOffsets[n] = double.NegativeInfinity;
												a = b.Entries;
												for (int h = 0; h < a.Length; h++) {
													a[h].Highlight = h == 0 ? 1.0 : 0.0;
													a[h].Alpha = 0.0;
												}
											}
										} break;
									case Interface.Command.MenuBack:
										// back
										if (Game.CurrentMenuSelection.Length <= 1) {
											Game.CurrentInterface = Game.InterfaceType.Normal;
										} else {
											Array.Resize<int>(ref Game.CurrentMenuSelection, Game.CurrentMenuSelection.Length - 1);
											Array.Resize<double>(ref Game.CurrentMenuOffsets, Game.CurrentMenuOffsets.Length - 1);
										} break;
									case Interface.Command.MiscFullscreen:
										// fullscreen
										Screen.ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
										// mute
										Sounds.GlobalMute = !Sounds.GlobalMute;
										Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;

								}
							}
						}
					} break;
				case Game.InterfaceType.Normal:
					// normal
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogHalf | Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogFull) {
							// analog control
							if (Interface.CurrentControls[i].AnalogState != 0.0) {
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.PowerHalfAxis:
									case Interface.Command.PowerFullAxis:
										// power half/full-axis
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											double a = Interface.CurrentControls[i].AnalogState;
											if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
												a = 0.5 * (a + 1.0);
											}
											a *= (double)TrainManager.PlayerTrain.Specs.MaximumPowerNotch;
											int p = (int)Math.Round(a);
											TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, 0, true);
										} break;
									case Interface.Command.BrakeHalfAxis:
									case Interface.Command.BrakeFullAxis:
										// brake half/full-axis
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
												double a = Interface.CurrentControls[i].AnalogState;
												if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
													a = 0.5 * (a + 1.0);
												}
												int b = (int)Math.Round(3.0 * a);
												switch (b) {
													case 0:
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Release);
														break;
													case 1:
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
														break;
													case 2:
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Service);
														break;
													case 3:
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
														break;
												}
											} else {
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake) {
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
														a = 0.5 * (a + 1.0);
													}
													a *= (double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 2;
													int b = (int)Math.Round(a);
													bool q = b == 1;
													if (b > 0) b--;
													if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
														TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
														TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b, false);
													} else {
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
													}
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
												} else {
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
														a = 0.5 * (a + 1.0);
													}
													a *= (double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 1;
													int b = (int)Math.Round(a);
													if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
														TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
														TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b, false);
													} else {
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
													}
												}
											}
										} break;
									case Interface.Command.SingleFullAxis:
										// single full axis
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											if (TrainManager.PlayerTrain.Specs.HasHoldBrake) {
												double a = Interface.CurrentControls[i].AnalogState;
												int p = (int)Math.Round(a * (double)TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
												int b = (int)Math.Round(-a * (double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 2);
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												bool q = b == 1;
												if (b > 0) b--;
												if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
												} else {
													TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
												}
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
											} else {
												double a = Interface.CurrentControls[i].AnalogState;
												int p = (int)Math.Round(a * (double)TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
												int b = (int)Math.Round(-a * ((double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 1));
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
												} else {
													TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
												}
											}
										} break;
									case Interface.Command.ReverserFullAxis:
										// reverser full axis
										{
											double a = Interface.CurrentControls[i].AnalogState;
											int r = (int)Math.Round(a);
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, r, false);
										} break;
									case Interface.Command.CameraMoveForward:
										// camera move forward
										if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead | World.CameraMode == World.CameraViewMode.Exterior) {
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Z = s * Interface.CurrentControls[i].AnalogState;
										} else {
											World.CameraAlignmentDirection.TrackPosition = World.CameraExteriorTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveBackward:
										// camera move backward
										if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead | World.CameraMode == World.CameraViewMode.Exterior) {
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Z = -s * Interface.CurrentControls[i].AnalogState;
										} else {
											World.CameraAlignmentDirection.TrackPosition = -World.CameraExteriorTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveLeft:
										// camera move left
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.X = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveRight:
										// camera move right
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.X = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveUp:
										// camera move up
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Y = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveDown:
										// camera move down
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Y = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateLeft:
										// camera rotate left
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Yaw = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateRight:
										// camera rotate right
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Yaw = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateUp:
										// camera rotate up
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Pitch = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateDown:
										// camera rotate down
										{
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Pitch = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateCCW:
										// camera rotate ccw
										if ((World.CameraMode != World.CameraViewMode.Interior & World.CameraMode != World.CameraViewMode.InteriorLookAhead) | World.CameraRestriction != World.CameraRestrictionMode.On) {
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Roll = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateCW:
										// camera rotate cw
										if ((World.CameraMode != World.CameraViewMode.Interior & World.CameraMode != World.CameraViewMode.InteriorLookAhead) | World.CameraRestriction != World.CameraRestrictionMode.On) {
											double s = World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Roll = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraZoomIn:
										// camera zoom in
										if (TimeElapsed > 0.0) {
											World.CameraAlignmentDirection.Zoom = -World.CameraZoomTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraZoomOut:
										// camera zoom out
										if (TimeElapsed > 0.0) {
											World.CameraAlignmentDirection.Zoom = World.CameraZoomTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.TimetableUp:
										// timetable up
										if (TimeElapsed > 0.0) {
											const double scrollSpeed = 250.0;
											if (Timetable.CurrentTimetable == Timetable.TimetableState.Default) {
												Timetable.DefaultTimetablePosition += scrollSpeed * Interface.CurrentControls[i].AnalogState * TimeElapsed;
												if (Timetable.DefaultTimetablePosition > 0.0) Timetable.DefaultTimetablePosition = 0.0;
											} else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom) {
												Timetable.CustomTimetablePosition += scrollSpeed * Interface.CurrentControls[i].AnalogState * TimeElapsed;
												if (Timetable.CustomTimetablePosition > 0.0) Timetable.CustomTimetablePosition = 0.0;
											}
										} break;
									case Interface.Command.TimetableDown:
										// timetable down
										if (TimeElapsed > 0.0) {
											const double scrollSpeed = 250.0;
											if (Timetable.CurrentTimetable == Timetable.TimetableState.Default) {
												Timetable.DefaultTimetablePosition -= scrollSpeed * Interface.CurrentControls[i].AnalogState * TimeElapsed;
												double max;
												if (Timetable.DefaultTimetableTexture != null) {
													Textures.LoadTexture(Timetable.DefaultTimetableTexture, Textures.OpenGlTextureWrapMode.ClampClamp);
													max = Math.Min(Screen.Height - Timetable.DefaultTimetableTexture.Height, 0.0);
												} else {
													max = 0.0;
												}
												if (Timetable.DefaultTimetablePosition < max) Timetable.DefaultTimetablePosition = max;
											} else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom) {
												Timetable.CustomTimetablePosition -= scrollSpeed * Interface.CurrentControls[i].AnalogState * TimeElapsed;
												Textures.Texture texture = Timetable.CurrentCustomTimetableDaytimeTexture;
												if (texture == null) {
													texture = Timetable.CurrentCustomTimetableNighttimeTexture;
												}
												double max;
												if (texture != null) {
													Textures.LoadTexture(texture, Textures.OpenGlTextureWrapMode.ClampClamp);
													max = Math.Min(Screen.Height - texture.Height, 0.0);
												} else {
													max = 0.0;
												}
												if (Timetable.CustomTimetablePosition < max) Timetable.CustomTimetablePosition = max;
											}
										} break;
								}
							}
						} else if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital) {
							// digital control
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed) {
								// pressed
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.MiscQuit:
										// quit
										Game.CreateMenu(true);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.CameraInterior:
										// camera: interior
										SaveCameraSettings();
										bool lookahead = false;
										if (World.CameraMode != World.CameraViewMode.InteriorLookAhead & World.CameraRestriction == World.CameraRestrictionMode.NotAvailable) {
											Game.AddMessage(Interface.GetInterfaceString("notification_interior_lookahead"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											lookahead = true;
										} else {
											Game.AddMessage(Interface.GetInterfaceString("notification_interior"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										}
										World.CameraMode = World.CameraViewMode.Interior;
										RestoreCameraSettings();
										for (int j = 0; j <= TrainManager.PlayerTrain.DriverCar; j++) {
											if (TrainManager.PlayerTrain.Cars[j].CarSections.Length != 0) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
											}
										}
										for (int j = TrainManager.PlayerTrain.DriverCar + 1; j < TrainManager.PlayerTrain.Cars.Length; j++) {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
										}
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if (World.CameraRestriction != World.CameraRestrictionMode.NotAvailable) {
											if (!World.PerformCameraRestrictionTest()) {
												World.InitializeCameraRestriction();
											}
										}
										if (lookahead) {
											World.CameraMode = World.CameraViewMode.InteriorLookAhead;
										}
										break;
									case Interface.Command.CameraExterior:
										// camera: exterior
										Game.AddMessage(Interface.GetInterfaceString("notification_exterior"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										SaveCameraSettings();
										World.CameraMode = World.CameraViewMode.Exterior;
										RestoreCameraSettings();
										if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CarSections.Length >= 2) {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, 1);
										} else {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, -1);
										}
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
											if (j != TrainManager.PlayerTrain.DriverCar) {
												if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1) {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
												} else {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
												}
											}
										}
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										break;
									case Interface.Command.CameraTrack:
									case Interface.Command.CameraFlyBy:
										// camera: track / fly-by
										{
											SaveCameraSettings();
											if (Interface.CurrentControls[i].Command == Interface.Command.CameraTrack) {
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											} else {
												if (World.CameraMode == World.CameraViewMode.FlyBy) {
													World.CameraMode = World.CameraViewMode.FlyByZooming;
													Game.AddMessage(Interface.GetInterfaceString("notification_flybyzooming"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
												} else {
													World.CameraMode = World.CameraViewMode.FlyBy;
													Game.AddMessage(Interface.GetInterfaceString("notification_flybynormal"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
												}
											}
											RestoreCameraSettings();
											if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CarSections.Length >= 2) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, 1);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, -1);
											}
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
												if (j != TrainManager.PlayerTrain.DriverCar) {
													if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1) {
														TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
													} else {
														TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
													}
												}
											}
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											UpdateViewport(ViewPortChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										} break;
									case Interface.Command.CameraPreviousPOI:
										// camera: previous poi
										if (Game.ApplyPointOfInterest(-1, true)) {
											if (World.CameraMode != World.CameraViewMode.Track & World.CameraMode != World.CameraViewMode.FlyBy & World.CameraMode != World.CameraViewMode.FlyByZooming) {
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											}
											double z = World.CameraCurrentAlignment.Position.Z;
											World.CameraCurrentAlignment.Position = new Vector3(World.CameraCurrentAlignment.Position.X, World.CameraCurrentAlignment.Position.Y, 0.0);
											World.CameraCurrentAlignment.Zoom = 0.0;
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CarSections.Length >= 2) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, 1);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, -1);
											}
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
												if (j != TrainManager.PlayerTrain.DriverCar) {
													if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1) {
														TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
													} else {
														TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
													}
												}
											}
											TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraTrackFollower.TrackPosition + z, true, false);
											World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
											World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
											UpdateViewport(ViewPortChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										} break;
									case Interface.Command.CameraNextPOI:
										// camera: next poi
										if (Game.ApplyPointOfInterest(1, true)) {
											if (World.CameraMode != World.CameraViewMode.Track & World.CameraMode != World.CameraViewMode.FlyBy & World.CameraMode != World.CameraViewMode.FlyByZooming) {
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											}
											double z = World.CameraCurrentAlignment.Position.Z;
											World.CameraCurrentAlignment.Position = new Vector3(World.CameraCurrentAlignment.Position.X, World.CameraCurrentAlignment.Position.Y, 0.0);
											World.CameraCurrentAlignment.Zoom = 0.0;
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CarSections.Length >= 2) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, 1);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, -1);
											}
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
												if (j != TrainManager.PlayerTrain.DriverCar) {
													if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1) {
														TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
													} else {
														TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
													}
												}
											}
											TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraTrackFollower.TrackPosition + z, true, false);
											World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
											World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
											UpdateViewport(ViewPortChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										} break;
									case Interface.Command.CameraReset:
										// camera: reset
										if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead) {
											World.CameraCurrentAlignment.Position = new Vector3(0.0, 0.0, 0.0);
										}
										World.CameraCurrentAlignment.Yaw = 0.0;
										World.CameraCurrentAlignment.Pitch = 0.0;
										World.CameraCurrentAlignment.Roll = 0.0;
										if (World.CameraMode == World.CameraViewMode.Track) {
											TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition, true, false);
										} else if (World.CameraMode == World.CameraViewMode.FlyBy | World.CameraMode == World.CameraViewMode.FlyByZooming) {
											if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed >= 0.0) {
												double d = 30.0 + 4.0 * TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition + d, true, false);
											} else {
												double d = 30.0 - 4.0 * TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.Cars.Length - 1].RearAxle.Follower.TrackPosition - d, true, false);
											}
										}
										World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
										World.CameraCurrentAlignment.Zoom = 0.0;
										World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport(ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if ((World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead) & World.CameraRestriction == World.CameraRestrictionMode.On) {
											if (!World.PerformCameraRestrictionTest()) {
												World.InitializeCameraRestriction();
											}
										}
										break;
									case Interface.Command.CameraRestriction:
										// camera: restriction
										if (World.CameraRestriction != World.CameraRestrictionMode.NotAvailable) {
											if (World.CameraRestriction == World.CameraRestrictionMode.Off) {
												World.CameraRestriction = World.CameraRestrictionMode.On;
											} else {
												World.CameraRestriction = World.CameraRestrictionMode.Off;
											}
											World.InitializeCameraRestriction();
											if (World.CameraRestriction == World.CameraRestrictionMode.Off) {
												Game.AddMessage(Interface.GetInterfaceString("notification_camerarestriction_off"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											} else {
												Game.AddMessage(Interface.GetInterfaceString("notification_camerarestriction_on"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											}
										}
										break;
									case Interface.Command.SinglePower:
										// single power
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
											if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
												TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
											} else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
											} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
											} else if (b > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
											} else {
												int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
												if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
												}
											}
										} break;
									case Interface.Command.SingleNeutral:
										// single neutral
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												} else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (b > 0) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
												}
											}
										} break;
									case Interface.Command.SingleBrake:
										// single brake
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
											}
										} break;
									case Interface.Command.SingleEmergency:
										// single emergency
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
										} break;
									case Interface.Command.PowerIncrease:
										// power increase
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
											}
										} break;
									case Interface.Command.PowerDecrease:
										// power decrease
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											}
										} break;
									case Interface.Command.BrakeIncrease:
										// brake increase
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Service);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
												}
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
											}
										} break;
									case Interface.Command.BrakeDecrease:
										// brake decrease
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
												if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												} else if (TrainManager.PlayerTrain.Specs.HasHoldBrake & TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Release);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Release);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
												}
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												} else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (b > 0) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
												}
											}
										} break;
									case Interface.Command.BrakeEmergency:
										// brake emergency
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
										} break;
									case Interface.Command.DeviceConstSpeed:
										// const speed
										if (TrainManager.PlayerTrain.Specs.HasConstSpeed) {
											TrainManager.PlayerTrain.Specs.CurrentConstSpeed = !TrainManager.PlayerTrain.Specs.CurrentConstSpeed;
										} break;
									case Interface.Command.ReverserForward:
										// reverser forward
										if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver < 1) {
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, 1, true);
										} break;
									case Interface.Command.ReverserBackward:
										// reverser backward
										if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver > -1) {
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, -1, true);
										} break;
									case Interface.Command.HornPrimary:
									case Interface.Command.HornSecondary:
									case Interface.Command.HornMusic:
										// horn
										{
											int j = Interface.CurrentControls[i].Command == Interface.Command.HornPrimary ? 0 : Interface.CurrentControls[i].Command == Interface.Command.HornSecondary ? 1 : 2;
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns.Length > j) {
												Sounds.SoundBuffer buffer = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Buffer;
												if (buffer != null) {
													Vector3 pos = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Position;
													if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Loop) {
														if (Sounds.IsPlaying(TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Source)) {
															Sounds.StopSound(TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Source);
														} else {
															TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, true);
														}
													} else {
														TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, false);
													}
													if (TrainManager.PlayerTrain.Plugin != null) {
														TrainManager.PlayerTrain.Plugin.HornBlow(j == 0 ? OpenBveApi.Runtime.HornTypes.Primary : j == 1 ? OpenBveApi.Runtime.HornTypes.Secondary : OpenBveApi.Runtime.HornTypes.Music);
													}
												}
											}
										} break;
									case Interface.Command.DoorsLeft:
										// doors: left
										if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false) & TrainManager.TrainDoorState.Opened) == 0) {
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
												TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, true, false);
											}
										} else {
											if (TrainManager.PlayerTrain.Specs.DoorCloseMode != TrainManager.DoorMode.Automatic) {
												TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, true, false);
											}
										} break;
									case Interface.Command.DoorsRight:
										// doors: right
										if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true) & TrainManager.TrainDoorState.Opened) == 0) {
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
												TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, false, true);
											}
										} else {
											if (TrainManager.PlayerTrain.Specs.DoorCloseMode != TrainManager.DoorMode.Automatic) {
												TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, false, true);
											}
										} break;
									case Interface.Command.SecurityS:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.S);
										}
										break;
									case Interface.Command.SecurityA1:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.A1);
										}
										break;
									case Interface.Command.SecurityA2:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.A2);
										}
										break;
									case Interface.Command.SecurityB1:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.B1);
										}
										break;
									case Interface.Command.SecurityB2:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.B2);
										}
										break;
									case Interface.Command.SecurityC1:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.C1);
										}
										break;
									case Interface.Command.SecurityC2:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.C2);
										}
										break;
									case Interface.Command.SecurityD:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.D);
										}
										break;
									case Interface.Command.SecurityE:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.E);
										}
										break;
									case Interface.Command.SecurityF:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.F);
										}
										break;
									case Interface.Command.SecurityG:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.G);
										}
										break;
									case Interface.Command.SecurityH:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.H);
										}
										break;
									case Interface.Command.SecurityI:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.I);
										}
										break;
									case Interface.Command.SecurityJ:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.J);
										}
										break;
									case Interface.Command.SecurityK:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.K);
										}
										break;
									case Interface.Command.SecurityL:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyDown(OpenBveApi.Runtime.VirtualKeys.L);
										}
										break;
									case Interface.Command.TimetableToggle:
										// option: timetable
										if (Timetable.CustomTimetableAvailable) {
											switch (Timetable.CurrentTimetable) {
												case Timetable.TimetableState.Custom:
													Timetable.CurrentTimetable = Timetable.TimetableState.Default;
													break;
												case Timetable.TimetableState.Default:
													Timetable.CurrentTimetable = Timetable.TimetableState.None;
													break;
												default:
													Timetable.CurrentTimetable = Timetable.TimetableState.Custom;
													break;
											}
										} else {
											switch (Timetable.CurrentTimetable) {
												case Timetable.TimetableState.Default:
													Timetable.CurrentTimetable = Timetable.TimetableState.None;
													break;
												default:
													Timetable.CurrentTimetable = Timetable.TimetableState.Default;
													break;
											}
										} break;
									case Interface.Command.DebugWireframe:
										// option: wireframe
										Renderer.OptionWireframe = !Renderer.OptionWireframe;
										if (Renderer.OptionWireframe) {
											GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Line);
										} else {
											GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Fill);
										}
										Renderer.StaticOpaqueForceUpdate = true;
										break;
									case Interface.Command.DebugNormals:
										// option: normals
										Renderer.OptionNormals = !Renderer.OptionNormals;
										Renderer.StaticOpaqueForceUpdate = true;
										break;
									case Interface.Command.MiscAI:
										// option: AI
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											if (TrainManager.PlayerTrain.AI == null) {
												TrainManager.PlayerTrain.AI = new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain);
												if (TrainManager.PlayerTrain.Plugin != null && !TrainManager.PlayerTrain.Plugin.SupportsAI) {
													Game.AddMessage(Interface.GetInterfaceString("notification_aiunable"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 10.0);
												}
											} else {
												TrainManager.PlayerTrain.AI = null;
											}
										} break;
									case Interface.Command.MiscInterfaceMode:
										// option: debug
										switch (Renderer.CurrentOutputMode) {
											case Renderer.OutputMode.Default:
												Renderer.CurrentOutputMode = Interface.CurrentOptions.GameMode == Interface.GameMode.Expert ? Renderer.OutputMode.None : Renderer.OutputMode.Debug;
												break;
											case Renderer.OutputMode.Debug:
												Renderer.CurrentOutputMode = Renderer.OutputMode.None;
												break;
											default:
												Renderer.CurrentOutputMode = Renderer.OutputMode.Default;
												break;
										} break;
									case Interface.Command.MiscBackfaceCulling:
										// option: backface culling
										Renderer.OptionBackfaceCulling = !Renderer.OptionBackfaceCulling;
										Renderer.StaticOpaqueForceUpdate = true;
										Game.AddMessage(Interface.GetInterfaceString(Renderer.OptionBackfaceCulling ? "notification_backfaceculling_on" : "notification_backfaceculling_off"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										break;
									case Interface.Command.MiscCPUMode:
										// option: limit frame rate
										LimitFramerate = !LimitFramerate;
										Game.AddMessage(Interface.GetInterfaceString(LimitFramerate ? "notification_cpu_low" : "notification_cpu_normal"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										break;
									case Interface.Command.DebugBrakeSystems:
										// option: brake systems
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											Renderer.OptionBrakeSystems = !Renderer.OptionBrakeSystems;
										} break;
									case Interface.Command.MenuActivate:
										// menu
										Game.CreateMenu(false);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.MiscPause:
										// pause
										Game.CurrentInterface = Game.InterfaceType.Pause;
										break;
									case Interface.Command.MiscClock:
										// clock
										Renderer.OptionClock = !Renderer.OptionClock;
										break;
									case Interface.Command.MiscTimeFactor:
										// time factor
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											TimeFactor = TimeFactor == 1 ? 5 : 1;
											Game.AddMessage(TimeFactor.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x", Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0 * (double)TimeFactor);
										}
										break;
									case Interface.Command.MiscSpeed:
										// speed
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											Renderer.OptionSpeed++;
											if ((int)Renderer.OptionSpeed >= 3) Renderer.OptionSpeed = 0;
										} break;
									case Interface.Command.MiscFps:
										// fps
										Renderer.OptionFrameRates = !Renderer.OptionFrameRates;
										break;
									case Interface.Command.MiscFullscreen:
										// toggle fullscreen
										Screen.ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
										// mute
										Sounds.GlobalMute = !Sounds.GlobalMute;
										Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
								}
							} else if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released) {
								// released
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.ReleasedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.SecurityS:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.S);
										}
										break;
									case Interface.Command.SecurityA1:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.A1);
										}
										break;
									case Interface.Command.SecurityA2:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.A2);
										}
										break;
									case Interface.Command.SecurityB1:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.B1);
										}
										break;
									case Interface.Command.SecurityB2:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.B2);
										}
										break;
									case Interface.Command.SecurityC1:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.C1);
										}
										break;
									case Interface.Command.SecurityC2:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.C2);
										}
										break;
									case Interface.Command.SecurityD:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.D);
										}
										break;
									case Interface.Command.SecurityE:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.E);
										}
										break;
									case Interface.Command.SecurityF:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.F);
										}
										break;
									case Interface.Command.SecurityG:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.G);
										}
										break;
									case Interface.Command.SecurityH:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.H);
										}
										break;
									case Interface.Command.SecurityI:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.I);
										}
										break;
									case Interface.Command.SecurityJ:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.J);
										}
										break;
									case Interface.Command.SecurityK:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.K);
										}
										break;
									case Interface.Command.SecurityL:
										if (TrainManager.PlayerTrain.Plugin != null) {
											TrainManager.PlayerTrain.Plugin.KeyUp(OpenBveApi.Runtime.VirtualKeys.L);
										}
										break;
								}
							}
						}
					} break;
			}
		}
		
		// --------------------------------

		// save camera setting
		private static void SaveCameraSettings() {
			switch (World.CameraMode) {
				case World.CameraViewMode.Interior:
				case World.CameraViewMode.InteriorLookAhead:
					World.CameraSavedInterior = World.CameraCurrentAlignment;
					break;
				case World.CameraViewMode.Exterior:
					World.CameraSavedExterior = World.CameraCurrentAlignment;
					break;
				case World.CameraViewMode.Track:
				case World.CameraViewMode.FlyBy:
				case World.CameraViewMode.FlyByZooming:
					World.CameraSavedTrack = World.CameraCurrentAlignment;
					break;
			}
		}
		
		// restore camera setting
		private static void RestoreCameraSettings() {
			switch (World.CameraMode) {
				case World.CameraViewMode.Interior:
				case World.CameraViewMode.InteriorLookAhead:
					World.CameraCurrentAlignment = World.CameraSavedInterior;
					break;
				case World.CameraViewMode.Exterior:
					World.CameraCurrentAlignment = World.CameraSavedExterior;
					break;
				case World.CameraViewMode.Track:
				case World.CameraViewMode.FlyBy:
				case World.CameraViewMode.FlyByZooming:
					World.CameraCurrentAlignment = World.CameraSavedTrack;
					TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraSavedTrack.TrackPosition, true, false);
					World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
					break;
			}
			World.CameraCurrentAlignment.Zoom = 0.0;
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
		}

		// --------------------------------

		// update viewport
		internal enum ViewPortMode {
			Scenery = 0,
			Cab = 1
		}
		internal enum ViewPortChangeMode {
			ChangeToScenery = 0,
			ChangeToCab = 1,
			NoChange = 2
		}
		internal static void UpdateViewport(ViewPortChangeMode Mode) {
			CurrentViewPortMode = Mode == ViewPortChangeMode.ChangeToCab ? ViewPortMode.Cab : ViewPortMode.Scenery;
			GL.Viewport(0, 0, Screen.Width, Screen.Height);
			World.AspectRatio = (double)Screen.Width / (double)Screen.Height;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			const double invdeg = 57.295779513082320877;
			if (CurrentViewPortMode == ViewPortMode.Cab) {
				OpenTK.Matrix4d persp = OpenTK.Matrix4d.CreatePerspectiveFieldOfView(World.VerticalViewingAngle * invdeg * Math.PI / 180, World.AspectRatio, 0.025, 50.0);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref persp);
			} else {
				OpenTK.Matrix4d persp = OpenTK.Matrix4d.CreatePerspectiveFieldOfView(World.VerticalViewingAngle * invdeg * Math.PI / 180, World.AspectRatio, 0.5, World.BackgroundImageDistance);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref persp);
			}
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}

		// initialize motion blur
		internal static void InitializeMotionBlur() {
			if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None) {
				if (Renderer.PixelBufferOpenGlTextureIndex != 0) {
					GL.DeleteTextures(1, new int[] { Renderer.PixelBufferOpenGlTextureIndex });
					Renderer.PixelBufferOpenGlTextureIndex = 0;
				}
				int w = Interface.CurrentOptions.NoTextureResize ? Screen.Width : Textures.RoundUpToPowerOfTwo(Screen.Width);
				int h = Interface.CurrentOptions.NoTextureResize ? Screen.Height : Textures.RoundUpToPowerOfTwo(Screen.Height);
				Renderer.PixelBuffer = new byte[4 * w * h];
				int[] a = new int[1];
				GL.GenTextures(1, a);
				GL.BindTexture(TextureTarget.Texture2D, a[0]);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
				GL.TexImage2D(TextureTarget.Texture2D, 0,PixelInternalFormat.Rgb, w, h, 0,PixelFormat.Rgb,PixelType.UnsignedByte, Renderer.PixelBuffer);
				Renderer.PixelBufferOpenGlTextureIndex = a[0];
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0,PixelInternalFormat.Rgb, 0, 0, w, h, 0);
			}
		}
		
		#if DEBUG
		// check error
		private static void CheckForOpenGlError(string Location) {
			ErrorCode error = GL.GetError();
			if (error != ErrorCode.NoError) {
				string message = Location + ": ";
				switch (error) {
					case ErrorCode.InvalidEnum:
						message += "GL_INVALID_ENUM";
						break;
					case ErrorCode.InvalidValue:
						message += "GL_INVALID_VALUE";
						break;
					case ErrorCode.InvalidOperation:
						message += "GL_INVALID_OPERATION";
						break;
					case ErrorCode.StackOverflow:
						message += "GL_STACK_OVERFLOW";
						break;
					case ErrorCode.StackUnderflow:
						message += "GL_STACK_UNDERFLOW";
						break;
					case ErrorCode.OutOfMemory:
						message += "GL_OUT_OF_MEMORY";
						break;
					case ErrorCode.TableTooLarge:
						message += "GL_TABLE_TOO_LARGE";
						break;
					default:
						message += error.ToString();
						break;
				}
				throw new InvalidOperationException(message);
			}
		}
		#endif

		public enum InputType {
			Quit,Resize,KeyDown,KeyUp,JoyButtonDown,JoyButtonUp,JoyHatMotion,JoyAxisMotion,MouseButtonDown,MouseMotion
		}
		internal struct InputEvent {
			internal InputType Type;
			internal EventArgs Data;
			internal InputEvent(InputType type, EventArgs data){
				this.Type = type;
				this.Data = data;
			}
		}
		internal static Queue<InputEvent> events;

		internal static void KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			events.Enqueue(new InputEvent(InputType.KeyDown,e));
		}

		internal static void KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			events.Enqueue(new InputEvent(InputType.KeyUp,e));
		}

		internal static void Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			events.Enqueue(new InputEvent(InputType.Quit,e));
		}

		internal static void Resize(object sender, EventArgs e)
		{
			events.Enqueue(new InputEvent(InputType.Resize,e));
		}

		internal static void MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
		{
			events.Enqueue(new InputEvent(InputType.MouseButtonDown,e));
		}

		internal static void MouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
		{
			events.Enqueue(new InputEvent(InputType.MouseMotion,e));
		}
		internal struct JoyState
		{
			internal bool[] buttonsPressed;
			internal float[] axisValues;
			internal HatPosition[] hatsPositions;
			internal JoyState(int buttons, int axes, int hats){
				buttonsPressed = new bool[buttons];
				axisValues = new float[axes];
				hatsPositions = new HatPosition[hats];
			}
		}
		internal static JoyState[] OldJoyStates = new JoyState[Joysticks.AttachedJoysticks.Length];
		internal static void JoystickPoll(){
			for(int i = 0; i < Joysticks.AttachedJoysticks.Length;i++){
				int index = Joysticks.AttachedJoysticks[i].Index;
				var state = Joystick.GetState(index);
				if (state.IsConnected) {
					int buttons = Joystick.GetCapabilities(index).ButtonCount;
					int axes = Joystick.GetCapabilities(index).AxisCount;
					int hats = Joystick.GetCapabilities(index).HatCount;
					// buttons
					for (int k = 0; k < buttons; k++) {
						JoystickButton but = (JoystickButton)Enum.Parse(typeof(JoystickButton), "Button" + k);
						bool pressed = state.GetButton(but) == OpenTK.Input.ButtonState.Pressed;
						if (OldJoyStates[i].buttonsPressed[k] != pressed) {
							OldJoyStates[i].buttonsPressed[k] = pressed;
							events.Enqueue(new InputEvent(
								pressed ? InputType.JoyButtonDown : InputType.JoyButtonUp,
								new ButtonEventArgs(k, but)));
						}
					}
					// axes
					for (int k = 0; k < axes; k++) {
						JoystickAxis axis = (JoystickAxis)Enum.Parse(typeof(JoystickAxis), "Axis" + k);
						float position = state.GetAxis(axis);
						if (Math.Abs(OldJoyStates[i].axisValues[k] - position) > 0.001f) {
							OldJoyStates[i].axisValues[k] = position;
							events.Enqueue(new InputEvent(InputType.JoyAxisMotion,new AxisEventArgs(k, axis,position)));
						}
					}
					// hats
					for (int k = 0; k < hats; k++) {
						JoystickHat hat = (JoystickHat)Enum.Parse(typeof(JoystickHat), "Hat" + k);
						JoystickHatState position = state.GetHat(hat);
						if (OldJoyStates[i].hatsPositions[k] != position.Position) {
							OldJoyStates[i].hatsPositions[k] = position.Position;
							events.Enqueue(new InputEvent(InputType.JoyAxisMotion,new HatEventArgs(k, hat,position.Position)));
						}
					}

				} else {
					// TODO handle joysticks relist
				}
			}
		}
		[Serializable]
		internal sealed class ButtonEventArgs : EventArgs
		{
			internal int Index;
			internal JoystickButton Button;
			public ButtonEventArgs (int index, JoystickButton b)
			{
				Index = index;
				Button = b;
			}
		}
		[Serializable]
		internal sealed class HatEventArgs : EventArgs
		{
			internal int Index;
			internal JoystickHat Hat;
			internal HatPosition Position;
			public HatEventArgs (int index, JoystickHat h, HatPosition pos)
			{
				Index = index;
				Hat = h;
				Position = pos;
			}
		}
		[Serializable]
		internal sealed class AxisEventArgs : EventArgs
		{
			internal int Index;
			internal JoystickAxis Axis;
			internal float Value;
			public AxisEventArgs (int index, JoystickAxis a, float v)
			{
				Index = index;
				Axis = a;
				Value = v;
			}
		}
	}
}