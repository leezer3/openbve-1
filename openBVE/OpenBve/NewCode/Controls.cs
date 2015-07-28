using System;
using System.Globalization;
using OpenTK.Input;

namespace OpenBve
{
	internal static class Controls
    {
		internal enum Command {
			None = 0,
			PowerIncrease, PowerDecrease, PowerHalfAxis, PowerFullAxis,
			BrakeIncrease, BrakeDecrease, BrakeEmergency, BrakeHalfAxis, BrakeFullAxis,
			SinglePower, SingleNeutral, SingleBrake, SingleEmergency, SingleFullAxis,
			ReverserForward, ReverserBackward, ReverserFullAxis,
			DoorsLeft, DoorsRight,
			HornPrimary, HornSecondary, HornMusic,
			DeviceConstSpeed,
			SecurityS, SecurityA1, SecurityA2, SecurityB1, SecurityB2, SecurityC1, SecurityC2,
			SecurityD, SecurityE, SecurityF, SecurityG, SecurityH, SecurityI, SecurityJ, SecurityK, SecurityL,
			CameraInterior, CameraExterior, CameraTrack, CameraFlyBy,
			CameraMoveForward, CameraMoveBackward, CameraMoveLeft, CameraMoveRight, CameraMoveUp, CameraMoveDown,
			CameraRotateLeft, CameraRotateRight, CameraRotateUp, CameraRotateDown, CameraRotateCCW, CameraRotateCW,
			CameraZoomIn, CameraZoomOut, CameraPreviousPOI, CameraNextPOI, CameraReset, CameraRestriction,
			TimetableToggle, TimetableUp, TimetableDown,
			MiscClock, MiscSpeed, MiscFps, MiscAI, MiscInterfaceMode, MiscBackfaceCulling, MiscCPUMode,
			MiscTimeFactor, MiscPause, MiscMute, MiscFullscreen, MiscQuit,
			MenuActivate, MenuUp, MenuDown, MenuEnter, MenuBack,
			DebugWireframe, DebugNormals, DebugBrakeSystems
		}
		internal enum CommandType { Digital, AnalogHalf, AnalogFull }
		internal struct CommandInfo {
			internal Command Command;
			internal CommandType Type;
			internal string Name;
			internal string Description;
			internal CommandInfo(Command Command, CommandType Type, string Name) {
				this.Command = Command;
				this.Type = Type;
				this.Name = Name;
				this.Description = "N/A";
			}
		}

		// key infos
		internal struct KeyInfo {
			internal OpenTK.Input.Key Value;
			internal string Name;
			internal string Description;
			internal KeyInfo(Key Value, string Name, string Description) {
				this.Value = Value;
				this.Name = Name;
				this.Description = Description;
			}
		}
		internal static KeyInfo[] Keys = new KeyInfo[] {
			new KeyInfo(Key.Number0, "0", "0"),
			new KeyInfo(Key.Number1, "1", "1"),
			new KeyInfo(Key.Number2, "2", "2"),
			new KeyInfo(Key.Number3, "3", "3"),
			new KeyInfo(Key.Number4, "4", "4"),
			new KeyInfo(Key.Number5, "5", "5"),
			new KeyInfo(Key.Number6, "6", "6"),
			new KeyInfo(Key.Number7, "7", "7"),
			new KeyInfo(Key.Number8, "8", "8"),
			new KeyInfo(Key.Number9, "9", "9"),
			//new KeyInfo(Key.Number7/* | KeyModifiers.Shift*/, "AMPERSAND", "Ampersand"),
			//new KeyInfo(Key.KeypadMultiply, "ASTERISK", "Asterisk"),
			//new KeyInfo(Key.Number2  | KeyModifiers.Shift, "AT", "At"),
			//new KeyInfo(Key.Tilde | KeyModifiers.Shift, "BACKQUOTE", "Backquote"),
			new KeyInfo(Key.BackSlash, "BACKSLASH", "Backslash"),
			new KeyInfo(Key.BackSpace, "BACKSPACE", "Backspace"),
			//new KeyInfo(Key.PauseBreak, "BREAK", "Break"),
			new KeyInfo(Key.CapsLock, "CAPSLOCK", "Capslock"),
			new KeyInfo(Key.Quote, "CARET", "Caret"),
			new KeyInfo(Key.Clear, "CLEAR", "Clear"),
			//new KeyInfo(Key.Colon, "COLON", "Colon"),
			new KeyInfo(Key.Comma, "COMMA", "Comma"),
			new KeyInfo(Key.Delete, "DELETE", "Delete"),
			//new KeyInfo(Key.Dollar, "DOLLAR", "Dollar"),
			new KeyInfo(Key.Down, "DOWN", "Down"),
			new KeyInfo(Key.End, "END", "End"),
			//new KeyInfo(Key.Equals, "EQUALS", "Equals"),
			new KeyInfo(Key.Escape, "ESCAPE", "Escape"),
			//new KeyInfo(Key.Euro, "EURO", "Euro"),
			//new KeyInfo(Key.Exclamation, "EXCLAIM", "Exclamation"),
			new KeyInfo(Key.F1, "F1", "F1"),
			new KeyInfo(Key.F2, "F2", "F2"),
			new KeyInfo(Key.F3, "F3", "F3"),
			new KeyInfo(Key.F4, "F4", "F4"),
			new KeyInfo(Key.F5, "F5", "F5"),
			new KeyInfo(Key.F6, "F6", "F6"),
			new KeyInfo(Key.F7, "F7", "F7"),
			new KeyInfo(Key.F8, "F8", "F8"),
			new KeyInfo(Key.F9, "F9", "F9"),
			new KeyInfo(Key.F10, "F10", "F10"),
			new KeyInfo(Key.F11, "F11", "F11"),
			new KeyInfo(Key.F12, "F12", "F12"),
			new KeyInfo(Key.F13, "F13", "F13"),
			new KeyInfo(Key.F14, "F14", "F14"),
			new KeyInfo(Key.F15, "F15", "F15"),
			//new KeyInfo(Key.Greater, "GREATER", "Greater"),
			//new KeyInfo(Key.Hash, "HASH", "Hash"),
			//new KeyInfo(Key.Help, "HELP", "Help"),
			new KeyInfo(Key.Home, "HOME", "Home"),
			new KeyInfo(Key.Insert, "INSERT", "Insert"),
			new KeyInfo(Key.Keypad0, "KP0", "Keypad 0"),
			new KeyInfo(Key.Keypad1, "KP1", "Keypad 1"),
			new KeyInfo(Key.Keypad2, "KP2", "Keypad 2"),
			new KeyInfo(Key.Keypad3, "KP3", "Keypad 3"),
			new KeyInfo(Key.Keypad4, "KP4", "Keypad 4"),
			new KeyInfo(Key.Keypad5, "KP5", "Keypad 5"),
			new KeyInfo(Key.Keypad6, "KP6", "Keypad 6"),
			new KeyInfo(Key.Keypad7, "KP7", "Keypad 7"),
			new KeyInfo(Key.Keypad8, "KP8", "Keypad 8"),
			new KeyInfo(Key.Keypad9, "KP9", "Keypad 9"),
			new KeyInfo(Key.KeypadDivide, "KP_DIVIDE", "Keypad Divide"),
			new KeyInfo(Key.KeypadEnter, "KP_ENTER", "Keypad Enter"),
			new KeyInfo(Key.KeypadMinus, "KP_MINUS", "Keypad Minus"),
			new KeyInfo(Key.KeypadMultiply, "KP_MULTIPLY", "Keypad Multiply"),
			new KeyInfo(Key.KeypadPeriod, "KP_PERIOD", "Keypad Period"),
			new KeyInfo(Key.KeypadPlus, "KP_PLUS", "Keypad Plus"),
			new KeyInfo(Key.LAlt, "LALT", "Left Alt"),
			new KeyInfo(Key.LControl, "LCTRL", "Left Ctrl"),
			new KeyInfo(Key.Left, "LEFT", "Left"),
			//new KeyInfo(Key.LeftBracket, "LEFTBRACKET", "Left bracket"),
			//new KeyInfo(Key.LeftParenthesis, "LEFTPAREN", "Left parenthesis"),
			//new KeyInfo(Key.Less, "LESS", "Less"),
			//new KeyInfo(Key.LMeta, "LMETA", "Left Meta"),
			new KeyInfo(Key.LShift, "LSHIFT", "Left Shift"),
			new KeyInfo(Key.LWin, "LSUPER", "Left Application"),
			new KeyInfo(Key.Menu, "MENU", "Menu"),
			new KeyInfo(Key.Minus, "MINUS", "Minus"),
			new KeyInfo(Key.RAlt, "MODE", "Alt Gr"),
			new KeyInfo(Key.NumLock, "NUMLOCK", "Numlock"),
			new KeyInfo(Key.PageDown, "PAGEDOWN", "Page down"),
			new KeyInfo(Key.PageUp, "PAGEUP", "Page up"),
			new KeyInfo(Key.Pause, "PAUSE", "Pause"),
			new KeyInfo(Key.Minus, "PERIOD", "Period"),
			new KeyInfo(Key.Plus, "PLUS", "Plus"),
			//new KeyInfo(Key.Power, "POWER", "Power"),
			//new KeyInfo(Key.Print, "PRINT", "Print"),
			//new KeyInfo(Key.Question, "QUESTION", "Question"),
			new KeyInfo(Key.Quote, "QUOTE", "Quote"),
			//new KeyInfo(Key.DoubleQuote, "QUOTEDBL", "Quote double"),
			new KeyInfo(Key.RAlt, "RALT", "Right Alt"),
			new KeyInfo(Key.RControl, "RCTRL", "Right Ctrl"),
			//new KeyInfo(Key.Return, "RETURN", "Return"),
			new KeyInfo(Key.Right, "RIGHT", "Right"),
			//new KeyInfo(Key.RightBracket, "RIGHTBRACKET", "Right bracket"),
			//new KeyInfo(Key.RightParen, "RIGHTPAREN", "Right parenthesis"),
			//new KeyInfo(Key.RMeta, "RMETA", "Right Meta"),
			new KeyInfo(Key.RShift, "RSHIFT", "Right Shift"),
			new KeyInfo(Key.RWin, "RSUPER", "Right Application"),
			new KeyInfo(Key.ScrollLock, "SCROLLLOCK", "Scrolllock"),
			new KeyInfo(Key.Semicolon, "SEMICOLON", "Semicolon"),
			new KeyInfo(Key.Slash, "SLASH", "Slash"),
			new KeyInfo(Key.Space, "SPACE", "Space"),
			//new KeyInfo(Key.SysRq, "SYSREQ", "SysRq"),
			new KeyInfo(Key.Tab, "TAB", "Tab"),
			//new KeyInfo(Key.Underscore, "UNDERSCORE", "Underscore"),
			new KeyInfo(Key.Up, "UP", "Up"),
			new KeyInfo(Key.A, "a", "A"),
			new KeyInfo(Key.B, "b", "B"),
			new KeyInfo(Key.C, "c", "C"),
			new KeyInfo(Key.D, "d", "D"),
			new KeyInfo(Key.E, "e", "E"),
			new KeyInfo(Key.F, "f", "F"),
			new KeyInfo(Key.G, "g", "G"),
			new KeyInfo(Key.H, "h", "H"),
			new KeyInfo(Key.I, "i", "I"),
			new KeyInfo(Key.J, "j", "J"),
			new KeyInfo(Key.K, "k", "K"),
			new KeyInfo(Key.L, "l", "L"),
			new KeyInfo(Key.M, "m", "M"),
			new KeyInfo(Key.N, "n", "N"),
			new KeyInfo(Key.O, "o", "O"),
			new KeyInfo(Key.P, "p", "P"),
			new KeyInfo(Key.Q, "q", "Q"),
			new KeyInfo(Key.R, "r", "R"),
			new KeyInfo(Key.S, "s", "S"),
			new KeyInfo(Key.T, "t", "T"),
			new KeyInfo(Key.U, "u", "U"),
			new KeyInfo(Key.V, "v", "V"),
			new KeyInfo(Key.W, "w", "W"),
			new KeyInfo(Key.X, "x", "X"),
			new KeyInfo(Key.Y, "y", "Y"),
			new KeyInfo(Key.Z, "z", "Z")
		};

		// controls
		internal enum ControlMethod {
			Invalid = 0,
			Keyboard = 1,
			Joystick = 2
		}
		[Flags]
		internal enum KeyboardModifier {
			None = 0,
			Shift = 1,
			Ctrl = 2,
			Alt = 4
		}
		internal enum JoystickComponent { Invalid, Axis, Hat, Button }
		internal enum DigitalControlState {
			ReleasedAcknowledged = 0,
			Released = 1,
			Pressed = 2,
			PressedAcknowledged = 3
		}
		internal struct Control {
			internal Command Command;
			internal CommandType InheritedType;
			internal ControlMethod Method;
			internal KeyboardModifier Modifier;
			internal int Device;
			internal JoystickComponent Component;
			internal int Element;
			internal object Direction;
			internal DigitalControlState DigitalState;
			internal double AnalogState;
		}

		// control descriptions
		internal static string[] ControlDescriptions = new string[] { };
		internal static CommandInfo[] CommandInfos = new CommandInfo[] {
			new CommandInfo(Command.PowerIncrease, CommandType.Digital, "POWER_INCREASE"),
			new CommandInfo(Command.PowerDecrease, CommandType.Digital, "POWER_DECREASE"),
			new CommandInfo(Command.PowerHalfAxis, CommandType.AnalogHalf, "POWER_HALFAXIS"),
			new CommandInfo(Command.PowerFullAxis, CommandType.AnalogFull, "POWER_FULLAXIS"),
			new CommandInfo(Command.BrakeDecrease, CommandType.Digital, "BRAKE_DECREASE"),
			new CommandInfo(Command.BrakeIncrease, CommandType.Digital, "BRAKE_INCREASE"),
			new CommandInfo(Command.BrakeHalfAxis, CommandType.AnalogHalf, "BRAKE_HALFAXIS"),
			new CommandInfo(Command.BrakeFullAxis, CommandType.AnalogFull, "BRAKE_FULLAXIS"),
			new CommandInfo(Command.BrakeEmergency, CommandType.Digital, "BRAKE_EMERGENCY"),
			new CommandInfo(Command.SinglePower, CommandType.Digital, "SINGLE_POWER"),
			new CommandInfo(Command.SingleNeutral, CommandType.Digital, "SINGLE_NEUTRAL"),
			new CommandInfo(Command.SingleBrake, CommandType.Digital, "SINGLE_BRAKE"),
			new CommandInfo(Command.SingleEmergency, CommandType.Digital, "SINGLE_EMERGENCY"),
			new CommandInfo(Command.SingleFullAxis, CommandType.AnalogFull, "SINGLE_FULLAXIS"),
			new CommandInfo(Command.ReverserForward, CommandType.Digital, "REVERSER_FORWARD"),
			new CommandInfo(Command.ReverserBackward, CommandType.Digital, "REVERSER_BACKWARD"),
			new CommandInfo(Command.ReverserFullAxis, CommandType.AnalogFull, "REVERSER_FULLAXIS"),
			new CommandInfo(Command.DoorsLeft, CommandType.Digital, "DOORS_LEFT"),
			new CommandInfo(Command.DoorsRight, CommandType.Digital, "DOORS_RIGHT"),
			new CommandInfo(Command.HornPrimary, CommandType.Digital, "HORN_PRIMARY"),
			new CommandInfo(Command.HornSecondary, CommandType.Digital, "HORN_SECONDARY"),
			new CommandInfo(Command.HornMusic, CommandType.Digital, "HORN_MUSIC"),
			new CommandInfo(Command.DeviceConstSpeed, CommandType.Digital, "DEVICE_CONSTSPEED"),
			new CommandInfo(Command.SecurityS, CommandType.Digital, "SECURITY_S"),
			new CommandInfo(Command.SecurityA1, CommandType.Digital, "SECURITY_A1"),
			new CommandInfo(Command.SecurityA2, CommandType.Digital, "SECURITY_A2"),
			new CommandInfo(Command.SecurityB1, CommandType.Digital, "SECURITY_B1"),
			new CommandInfo(Command.SecurityB2, CommandType.Digital, "SECURITY_B2"),
			new CommandInfo(Command.SecurityC1, CommandType.Digital, "SECURITY_C1"),
			new CommandInfo(Command.SecurityC2, CommandType.Digital, "SECURITY_C2"),
			new CommandInfo(Command.SecurityD, CommandType.Digital, "SECURITY_D"),
			new CommandInfo(Command.SecurityE, CommandType.Digital, "SECURITY_E"),
			new CommandInfo(Command.SecurityF, CommandType.Digital, "SECURITY_F"),
			new CommandInfo(Command.SecurityG, CommandType.Digital, "SECURITY_G"),
			new CommandInfo(Command.SecurityH, CommandType.Digital, "SECURITY_H"),
			new CommandInfo(Command.SecurityI, CommandType.Digital, "SECURITY_I"),
			new CommandInfo(Command.SecurityJ, CommandType.Digital, "SECURITY_J"),
			new CommandInfo(Command.SecurityK, CommandType.Digital, "SECURITY_K"),
			new CommandInfo(Command.SecurityL, CommandType.Digital, "SECURITY_L"),
			new CommandInfo(Command.CameraInterior, CommandType.Digital, "CAMERA_INTERIOR"),
			new CommandInfo(Command.CameraExterior, CommandType.Digital, "CAMERA_EXTERIOR"),
			new CommandInfo(Command.CameraTrack, CommandType.Digital, "CAMERA_TRACK"),
			new CommandInfo(Command.CameraFlyBy, CommandType.Digital, "CAMERA_FLYBY"),
			new CommandInfo(Command.CameraMoveForward, CommandType.AnalogHalf, "CAMERA_MOVE_FORWARD"),
			new CommandInfo(Command.CameraMoveBackward, CommandType.AnalogHalf, "CAMERA_MOVE_BACKWARD"),
			new CommandInfo(Command.CameraMoveLeft, CommandType.AnalogHalf, "CAMERA_MOVE_LEFT"),
			new CommandInfo(Command.CameraMoveRight, CommandType.AnalogHalf, "CAMERA_MOVE_RIGHT"),
			new CommandInfo(Command.CameraMoveUp, CommandType.AnalogHalf, "CAMERA_MOVE_UP"),
			new CommandInfo(Command.CameraMoveDown, CommandType.AnalogHalf, "CAMERA_MOVE_DOWN"),
			new CommandInfo(Command.CameraRotateLeft, CommandType.AnalogHalf, "CAMERA_ROTATE_LEFT"),
			new CommandInfo(Command.CameraRotateRight, CommandType.AnalogHalf, "CAMERA_ROTATE_RIGHT"),
			new CommandInfo(Command.CameraRotateUp, CommandType.AnalogHalf, "CAMERA_ROTATE_UP"),
			new CommandInfo(Command.CameraRotateDown, CommandType.AnalogHalf, "CAMERA_ROTATE_DOWN"),
			new CommandInfo(Command.CameraRotateCCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CCW"),
			new CommandInfo(Command.CameraRotateCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CW"),
			new CommandInfo(Command.CameraZoomIn, CommandType.AnalogHalf, "CAMERA_ZOOM_IN"),
			new CommandInfo(Command.CameraZoomOut, CommandType.AnalogHalf, "CAMERA_ZOOM_OUT"),
			new CommandInfo(Command.CameraPreviousPOI, CommandType.Digital, "CAMERA_POI_PREVIOUS"),
			new CommandInfo(Command.CameraNextPOI, CommandType.Digital, "CAMERA_POI_NEXT"),
			new CommandInfo(Command.CameraReset, CommandType.Digital, "CAMERA_RESET"),
			new CommandInfo(Command.CameraRestriction, CommandType.Digital, "CAMERA_RESTRICTION"),
			new CommandInfo(Command.TimetableToggle, CommandType.Digital, "TIMETABLE_TOGGLE"),
			new CommandInfo(Command.TimetableUp, CommandType.AnalogHalf, "TIMETABLE_UP"),
			new CommandInfo(Command.TimetableDown, CommandType.AnalogHalf, "TIMETABLE_DOWN"),
			new CommandInfo(Command.MenuActivate, CommandType.Digital, "MENU_ACTIVATE"),
			new CommandInfo(Command.MenuUp, CommandType.Digital, "MENU_UP"),
			new CommandInfo(Command.MenuDown, CommandType.Digital, "MENU_DOWN"),
			new CommandInfo(Command.MenuEnter, CommandType.Digital, "MENU_ENTER"),
			new CommandInfo(Command.MenuBack, CommandType.Digital, "MENU_BACK"),
			new CommandInfo(Command.MiscClock, CommandType.Digital, "MISC_CLOCK"),
			new CommandInfo(Command.MiscSpeed, CommandType.Digital, "MISC_SPEED"),
			new CommandInfo(Command.MiscFps, CommandType.Digital, "MISC_FPS"),
			new CommandInfo(Command.MiscAI, CommandType.Digital, "MISC_AI"),
			new CommandInfo(Command.MiscFullscreen, CommandType.Digital, "MISC_FULLSCREEN"),
			new CommandInfo(Command.MiscMute, CommandType.Digital, "MISC_MUTE"),
			new CommandInfo(Command.MiscPause, CommandType.Digital, "MISC_PAUSE"),
			new CommandInfo(Command.MiscTimeFactor, CommandType.Digital, "MISC_TIMEFACTOR"),
			new CommandInfo(Command.MiscQuit, CommandType.Digital, "MISC_QUIT"),
			new CommandInfo(Command.MiscInterfaceMode, CommandType.Digital, "MISC_INTERFACE"),
			new CommandInfo(Command.MiscBackfaceCulling, CommandType.Digital, "MISC_BACKFACE"),
			new CommandInfo(Command.MiscCPUMode, CommandType.Digital, "MISC_CPUMODE"),
			new CommandInfo(Command.DebugWireframe, CommandType.Digital, "DEBUG_WIREFRAME"),
			new CommandInfo(Command.DebugNormals, CommandType.Digital, "DEBUG_NORMALS"),
			new CommandInfo(Command.DebugBrakeSystems, CommandType.Digital, "DEBUG_BRAKE"),
		};
		internal static Control[] CurrentControls = new Control[] { };

		// try get command info
		internal static bool TryGetCommandInfo(Command Value, out CommandInfo Info) {
			for (int i = 0; i < CommandInfos.Length; i++) {
				if (CommandInfos[i].Command == Value) {
					Info = CommandInfos[i];
					return true;
				}
			}
			Info.Command = Value;
			Info.Type = CommandType.Digital;
			Info.Name = "N/A";
			Info.Description = "N/A";
			return false;
		}

		// save controls
		internal static void SaveControls(string FileOrNull) {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			Builder.AppendLine("; Current control configuration");
			Builder.AppendLine("; =============================");
			Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
			Builder.AppendLine();
			for (int i = 0; i < CurrentControls.Length; i++) {
				CommandInfo Info;
				TryGetCommandInfo(CurrentControls[i].Command, out Info);
				Builder.Append(Info.Name + ", ");
				switch (CurrentControls[i].Method) {
					case ControlMethod.Keyboard:
						Builder.Append("keyboard, " + CurrentControls[i].Element.ToString(Culture) + ", " + ((int)CurrentControls[i].Modifier).ToString(Culture));
						break;
					case ControlMethod.Joystick:
						Builder.Append("joystick, " + CurrentControls[i].Device.ToString(Culture) + ", ");
						switch (CurrentControls[i].Component) {
							case JoystickComponent.Axis:
								Builder.Append("axis, " + CurrentControls[i].Element.ToString(Culture) + ", " + CurrentControls[i].Direction.ToString());
								break;
							case JoystickComponent.Hat:
								Builder.Append("hat, " + CurrentControls[i].Element.ToString(Culture) + ", " + CurrentControls[i].Direction.ToString());
								break;
							case JoystickComponent.Button:
								Builder.Append("button, " + CurrentControls[i].Element.ToString(Culture));
								break;
							default:
								Builder.Append("invalid");
								break;
						}
						break;
					default:
						break;
				}
				Builder.Append("\n");
			}
			string File;
			if (FileOrNull == null) {
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
			} else {
				File = FileOrNull;
			}
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}

		// load controls
		internal static void LoadControls(string FileOrNull, out Control[] Controls) {
			string File;
			if (FileOrNull == null) {
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
				if (!System.IO.File.Exists(File)) {
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls");
				}
			} else {
				File = FileOrNull;
			}
			Controls = new Control[256];
			int Length = 0;
			CultureInfo Culture = CultureInfo.InvariantCulture;
			if (System.IO.File.Exists(File)) {
				string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
				for (int i = 0; i < Lines.Length; i++) {
					Lines[i] = Lines[i].Trim();
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
						string[] Terms = Lines[i].Split(new char[] { ',' });
						for (int j = 0; j < Terms.Length; j++) {
							Terms[j] = Terms[j].Trim();
						}
						if (Terms.Length >= 2) {
							if (Length >= Controls.Length) {
								Array.Resize<Control>(ref Controls, Controls.Length << 1);
							}
							int j;
							for (j = 0; j < CommandInfos.Length; j++) {
								if (string.Compare(CommandInfos[j].Name, Terms[0], StringComparison.OrdinalIgnoreCase) == 0) break;
							}
							if (j == CommandInfos.Length) {
								Controls[Length].Command = Command.None;
								Controls[Length].InheritedType = CommandType.Digital;
								Controls[Length].Method = ControlMethod.Invalid;
								Controls[Length].Device = -1;
								Controls[Length].Component = JoystickComponent.Invalid;
								Controls[Length].Element = -1;
								Controls[Length].Direction = 0;
								Controls[Length].Modifier = KeyboardModifier.None;
							} else {
								Controls[Length].Command = CommandInfos[j].Command;
								Controls[Length].InheritedType = CommandInfos[j].Type;
								string Method = Terms[1].ToLowerInvariant();
								bool Valid = false;
								if (Method == "keyboard" & Terms.Length == 4) {
									int Element, Modifiers;
									if (int.TryParse(Terms[2], NumberStyles.Integer, Culture, out Element)) {
										if (int.TryParse(Terms[3], NumberStyles.Integer, Culture, out Modifiers)) {
											Controls[Length].Method = ControlMethod.Keyboard;
											Controls[Length].Device = -1;
											Controls[Length].Component = JoystickComponent.Invalid;
											Controls[Length].Element = Element;
											Controls[Length].Direction = 0;
											Controls[Length].Modifier = (KeyboardModifier)Modifiers;
											Valid = true;
										}
									}
								} else if (Method == "joystick" & Terms.Length >= 4) {
									int Device;
									if (int.TryParse(Terms[2], NumberStyles.Integer, Culture, out Device)) {
										string Component = Terms[3].ToLowerInvariant();
										if (Component == "axis" & Terms.Length == 6) {
											int Element, Direction;
											if (int.TryParse(Terms[4], NumberStyles.Integer, Culture, out Element)) {
												if (int.TryParse(Terms[5], NumberStyles.Integer, Culture, out Direction)) {
													Controls[Length].Method = ControlMethod.Joystick;
													Controls[Length].Device = Device;
													Controls[Length].Component = JoystickComponent.Axis;
													Controls[Length].Element = Element;
													Controls[Length].Direction = Direction;
													Controls[Length].Modifier = KeyboardModifier.None;
													Valid = true;
												}
											}
										} else if (Component == "hat" & Terms.Length == 6) {
											int Element, Direction;
											if (int.TryParse(Terms[4], NumberStyles.Integer, Culture, out Element)) {
												if (int.TryParse(Terms[5], NumberStyles.Integer, Culture, out Direction)) {
													Controls[Length].Method = ControlMethod.Joystick;
													Controls[Length].Device = Device;
													Controls[Length].Component = JoystickComponent.Hat;
													Controls[Length].Element = Element;
													Controls[Length].Direction = Direction;
													Controls[Length].Modifier = KeyboardModifier.None;
													Valid = true;
												}
											}
										} else if (Component == "button" & Terms.Length == 5) {
											int Element;
											if (int.TryParse(Terms[4], NumberStyles.Integer, Culture, out Element)) {
												Controls[Length].Method = ControlMethod.Joystick;
												Controls[Length].Device = Device;
												Controls[Length].Component = JoystickComponent.Button;
												Controls[Length].Element = Element;
												Controls[Length].Direction = 0;
												Controls[Length].Modifier = KeyboardModifier.None;
												Valid = true;

											}
										}
									}
								}
								if (!Valid) {
									Controls[Length].Method = ControlMethod.Invalid;
									Controls[Length].Device = -1;
									Controls[Length].Component = JoystickComponent.Invalid;
									Controls[Length].Element = -1;
									Controls[Length].Direction = 0;
									Controls[Length].Modifier = KeyboardModifier.None;
								}
							}
							Length++;
						}
					}
				}
			}
			Array.Resize<Control>(ref Controls, Length);
		}

		// add controls
		internal static void AddControls(ref Control[] Base, Control[] Add) {
			for (int i = 0; i < Add.Length; i++) {
				int j;
				for (j = 0; j < Base.Length; j++) {
					if (Add[i].Command == Base[j].Command) break;
				}
				if (j == Base.Length) {
					Array.Resize<Control>(ref Base, Base.Length + 1);
					Base[Base.Length - 1] = Add[i];
				}
			}
		}

    }
}

