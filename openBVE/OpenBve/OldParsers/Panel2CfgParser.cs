﻿using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve {
	internal static class Panel2CfgParser {

		// constants
		internal static double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		internal const double EyeDistance = 1.0;
		
		// parse panel config
		internal static void ParsePanel2Config(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train) {
			// read lines
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "panel2.cfg");
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++) {
				Lines[i] = Lines[i].Trim();
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).TrimEnd();
				}
			}
			// initialize
			double DriverX = Train.Cars[Train.DriverCar].DriverX;
			double DriverY = Train.Cars[Train.DriverCar].DriverY;
			double DriverZ = Train.Cars[Train.DriverCar].DriverZ;
			double PanelResolution = 1024.0;
			double PanelLeft = 0.0, PanelRight = 1024.0;
			double PanelTop = 0.0, PanelBottom = 1024.0;
			double PanelCenterX = 0.0, PanelCenterY = 512.0;
			double PanelOriginX = 0.0, PanelOriginY = 512.0;
			double PanelBitmapWidth = 1024.0, PanelBitmapHeight = 1024.0;
			string PanelDaytimeImage = null;
			string PanelNighttimeImage = null;
			Color24 PanelTransparentColor = new Color24(0, 0, 255);
			// parse lines for panel
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length > 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant()) {
								// panel
							case "this":
								i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
									int j = Lines[i].IndexOf('='); if (j >= 0) {
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										switch (Key.ToLowerInvariant()) {
											case "resolution":
												if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out PanelResolution)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "left":
												if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out PanelLeft)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line" + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "right":
												if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out PanelRight)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "top":
												if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out PanelTop)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "bottom":
												if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out PanelBottom)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "daytimeimage":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidPathChars(Value)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} else {
													PanelDaytimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(PanelDaytimeImage)) {
														Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + PanelDaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														PanelDaytimeImage = null;
													}
												}
												break;
											case "nighttimeimage":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidPathChars(Value)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} else {
													PanelNighttimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(PanelNighttimeImage)) {
														Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + PanelNighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														PanelNighttimeImage = null;
													}
												}
												break;
											case "transparentcolor":
												if (Value.Length != 0 && !Conversions.TryParseHexColor(Value, out PanelTransparentColor)) {
													Debug.AddMessage(Debug.MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "center":
												{
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out PanelCenterX)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out PanelCenterY)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												}
											case "origin":
												{
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out PanelOriginX)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out PanelOriginY)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												}
										}
									} i++;
								} i--; break;
						}
					}
				}
			}
			{ // camera restriction
				double WorldWidth, WorldHeight;
				if (Screen.Width >= Screen.Height) {
					WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
					WorldHeight = WorldWidth / World.AspectRatio;
				} else {
					WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance / World.AspectRatio;
					WorldWidth = WorldHeight * World.AspectRatio;
				}
				double x0 = (PanelLeft - PanelCenterX) / PanelResolution;
				double x1 = (PanelRight - PanelCenterX) / PanelResolution;
				double y0 = (PanelCenterY - PanelBottom) / PanelResolution * World.AspectRatio;
				double y1 = (PanelCenterY - PanelTop) / PanelResolution * World.AspectRatio;
				World.CameraRestrictionBottomLeft = new Vector3D(x0 * WorldWidth, y0 * WorldHeight, EyeDistance);
				World.CameraRestrictionTopRight = new Vector3D(x1 * WorldWidth, y1 * WorldHeight, EyeDistance);
				Train.Cars[Train.DriverCar].DriverYaw = Math.Atan((PanelCenterX - PanelOriginX) * WorldWidth / PanelResolution);
				Train.Cars[Train.DriverCar].DriverPitch = Math.Atan((PanelOriginY - PanelCenterY) * WorldWidth / PanelResolution);
			}
			// create panel
			if (PanelDaytimeImage != null) {
				if (!System.IO.File.Exists(PanelDaytimeImage)) {
					Debug.AddMessage(Debug.MessageType.Error, true, "The daytime panel bitmap could not be found in " + FileName);
					PanelDaytimeImage = null;
				} else {
					Textures.Texture tday;
					Textures.RegisterTexture(PanelDaytimeImage, new OpenBveApi.Textures.TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out tday);
					Textures.Texture tnight = null;
					if (PanelNighttimeImage != null) {
						if (!System.IO.File.Exists(PanelNighttimeImage)) {
							Debug.AddMessage(Debug.MessageType.Error, true, "The nighttime panel bitmap could not be found in " + FileName);
							PanelNighttimeImage = null;
						} else {
							Textures.RegisterTexture(PanelNighttimeImage, new OpenBveApi.Textures.TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out tnight);
						}
					}
					Textures.LoadTexture(tday, Textures.OpenGlTextureWrapMode.ClampClamp);
					PanelBitmapWidth = (double)tday.Width;
					PanelBitmapHeight = (double)tday.Height;
					CreateElement(Train, 0.0, 0.0, PanelBitmapWidth, PanelBitmapHeight, 0.5, 0.5, 0.0, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, DriverX, DriverY, DriverZ, tday, tnight, new Color32(255, 255, 255, 255), false);
				}
			}
			// parse lines for rest
			double invfac = Lines.Length == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)Lines.Length;
			for (int i = 0; i < Lines.Length; i++) {
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double)i;
				if ((i & 7) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				if (Lines[i].Length > 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant()) {
								// pilotlamp
							case "pilotlamp":
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color24 TransparentColor = new Color24(0, 0, 255);
									int Layer = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out LocationX)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out LocationY)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (OpenBveApi.Path.ContainsInvalidPathChars(Value)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(DaytimeImage)) {
															Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case "nighttimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (OpenBveApi.Path.ContainsInvalidPathChars(Value)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(NighttimeImage)) {
															Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case "transparentcolor":
													if (Value.Length != 0 && !Conversions.TryParseHexColor(Value, out TransparentColor)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !Conversions.TryParseIntVb6(Value, out Layer)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Debug.AddMessage(Debug.MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null) {
										Textures.Texture tday;
										Textures.RegisterTexture(DaytimeImage, new OpenBveApi.Textures.TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday);
										Textures.Texture tnight = null;
										if (NighttimeImage != null) {
											Textures.RegisterTexture(NighttimeImage, new OpenBveApi.Textures.TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
										}
										Textures.LoadTexture(tday, Textures.OpenGlTextureWrapMode.ClampClamp);
										int w = tday.Width;
										int h = tday.Height;
										int j = CreateElement(Train, LocationX, LocationY, w, h, 0.5, 0.5, (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, DriverX, DriverY, DriverZ, tday, tnight, new Color32(255, 255, 255, 255), false);
										string f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f + " 1 == --");
									}
								} break;
								// needle
							case "needle":
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color32 Color = new Color32(255, 255, 255, 255);
									Color24 TransparentColor = new Color24(0, 0, 255);
									double OriginX = -1.0, OriginY = -1.0;
									bool OriginDefined = false;
									double Layer = 0.0, Radius = 0.0;
									double InitialAngle = -2.0943951023932, LastAngle = 2.0943951023932;
									double Minimum = 0.0, Maximum = 1000.0;
									double NaturalFrequency = -1.0, DampingRatio = -1.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													{
														int k = Value.IndexOf(',');
														if (k >= 0) {
															string a = Value.Substring(0, k).TrimEnd();
															string b = Value.Substring(k + 1).TrimStart();
															if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out LocationX)) {
																Debug.AddMessage(Debug.MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
															if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out LocationY)) {
																Debug.AddMessage(Debug.MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
														} else {
															Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} break;
												case "radius":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Radius)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Radius == 0.0) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 16.0;
													} break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (OpenBveApi.Path.ContainsInvalidPathChars(Value)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(DaytimeImage)) {
															Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case "nighttimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (OpenBveApi.Path.ContainsInvalidPathChars(Value)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(NighttimeImage)) {
															Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case "color":
													if (Value.Length != 0 && !Conversions.TryParseHexColor(Value, out Color)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "transparentcolor":
													if (Value.Length != 0 && !Conversions.TryParseHexColor(Value, out TransparentColor)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "origin":
													{
														int k = Value.IndexOf(',');
														if (k >= 0) {
															string a = Value.Substring(0, k).TrimEnd();
															string b = Value.Substring(k + 1).TrimStart();
															if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out OriginX)) {
																Debug.AddMessage(Debug.MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
															if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out OriginY)) {
																Debug.AddMessage(Debug.MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
																OriginX = -OriginX;
															}
															OriginDefined = true;
														} else {
															Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} break;
												case "initialangle":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out InitialAngle)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "lastangle":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out LastAngle)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "minimum":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Minimum)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "maximum":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Maximum)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "naturalfreq":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out NaturalFrequency)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (NaturalFrequency < 0.0) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														NaturalFrequency = -NaturalFrequency;
													} break;
												case "dampingratio":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out DampingRatio)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (DampingRatio < 0.0) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														DampingRatio = -DampingRatio;
													} break;
												case "layer":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Layer)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Debug.AddMessage(Debug.MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null) {
										Textures.Texture tday;
										Textures.RegisterTexture(DaytimeImage, new OpenBveApi.Textures.TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday);
										Textures.Texture tnight = null;
										if (NighttimeImage != null) {
											Textures.RegisterTexture(NighttimeImage, new OpenBveApi.Textures.TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
										}
										Textures.LoadTexture(tday, Textures.OpenGlTextureWrapMode.ClampClamp);
										double w = (double)tday.Width;
										double h = (double)tday.Height;
										if (!OriginDefined) {
											OriginX = 0.5 * w;
											OriginY = 0.5 * h;
										}
										double ox = OriginX / w;
										double oy = OriginY / h;
										double n = Radius == 0.0 | OriginY == 0.0 ? 1.0 : Radius / OriginY;
										double nx = n * w;
										double ny = n * h;
										int j = CreateElement(Train, LocationX - ox * nx, LocationY - oy * ny, nx, ny, ox, oy, (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, DriverX, DriverY, DriverZ, tday, tnight, Color, false);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection = new Vector3D(0.0, 0.0, -1.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection = new Vector3D(1.0, 0.0, 0.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateYDirection = Vector3D.Cross(Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection);
										string f;
										switch (Subject.ToLowerInvariant()) {
											case "hour":
												f = "0.000277777777777778 time * floor";
												break;
											case "min":
												f = "0.0166666666666667 time * floor";
												break;
											case "sec":
												f = "time floor";
												break;
											default:
												f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
												break;
										}
										InitialAngle *= 0.0174532925199433;
										LastAngle *= 0.0174532925199433;
										double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
										double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
										f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
										if (NaturalFrequency >= 0.0 & DampingRatio >= 0.0) {
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDamping = new ObjectManager.Damping(NaturalFrequency, DampingRatio);
										}
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
									}
								} break;
								// digitalnumber
							case "digitalnumber":
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color24 TransparentColor = new Color24(0, 0, 255);
									double Layer = 0.0; int Interval = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out LocationX)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out LocationY)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (OpenBveApi.Path.ContainsInvalidPathChars(Value)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(DaytimeImage)) {
															Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case "nighttimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (OpenBveApi.Path.ContainsInvalidPathChars(Value)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(NighttimeImage)) {
															Debug.AddMessage(Debug.MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case "transparentcolor":
													if (Value.Length != 0 && !Conversions.TryParseHexColor(Value, out TransparentColor)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "interval":
													if (Value.Length != 0 && !Conversions.TryParseIntVb6(Value, out Interval)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Interval <= 0) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Height is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Layer)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Debug.AddMessage(Debug.MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									if (Interval <= 0) {
										Debug.AddMessage(Debug.MessageType.Error, false, "Interval is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null & Interval > 0) {
										int wday, hday;
										Program.CurrentHost.QueryTextureDimensions(DaytimeImage, out wday, out hday);
										if (wday > 0 & hday > 0) {
											int nday = hday / Interval;
											Textures.Texture[] tday = new Textures.Texture[nday];
											Textures.Texture[] tnight;
											for (int k = 0; k < nday; k++) {
												Textures.RegisterTexture(DaytimeImage, new OpenBveApi.Textures.TextureParameters(new OpenBveApi.Textures.TextureClipRegion(0, k * Interval, wday, Interval), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday[k]);
											}
											if (NighttimeImage != null) {
												int wnight, hnight;
												Program.CurrentHost.QueryTextureDimensions(NighttimeImage, out wnight, out hnight);
												int nnight = hnight / Interval;
												if (nnight > nday) nnight = nday;
												tnight = new Textures.Texture[nday];
												for (int k = 0; k < nnight; k++) {
													Textures.RegisterTexture(NighttimeImage, new OpenBveApi.Textures.TextureParameters(new OpenBveApi.Textures.TextureClipRegion(0, k * Interval, wday, Interval), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight[k]);
												}
												for (int k = nnight; k < nday; k++) {
													tnight[k] = null;
												}
											} else {
												tnight = new Textures.Texture[nday];
												for (int k = 0; k < nday; k++) {
													tnight[k] = null;
												}
											}
											int j = -1;
											for (int k = 0; k < tday.Length; k++) {
												int l = CreateElement(Train, LocationX, LocationY, (double)wday, (double)Interval, 0.5, 0.5, (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, DriverX, DriverY, DriverZ, tday[k], tnight[k], new Color32(255, 255, 255, 255), k != 0);
												if (k == 0) j = l;
											}
											string f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
										}
									}
								} break;
								// digitalgauge
							case "digitalgauge":
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									Color32 Color = new Color32(0, 0, 0, 255);
									double Radius = 0.0;
									int Layer = 0;
									double InitialAngle = -2.0943951023932, LastAngle = 2.0943951023932;
									double Minimum = 0.0, Maximum = 1000.0;
									double Step = 0.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out LocationX)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out LocationY)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "radius":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Radius)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Radius == 0.0) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 16.0;
													} break;
												case "color":
													if (Value.Length != 0 && !Conversions.TryParseHexColor(Value, out Color)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "initialangle":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out InitialAngle)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														InitialAngle *= 0.0174532925199433;
													} break;
												case "lastangle":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out LastAngle)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														LastAngle *= 0.0174532925199433;
													} break;
												case "minimum":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Minimum)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "maximum":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Maximum)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "step":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Step)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !Conversions.TryParseIntVb6(Value, out Layer)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (Radius == 0.0) {
										Debug.AddMessage(Debug.MessageType.Error, false, "Radius is required to be non-zero in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
									}
									if (Minimum == Maximum) {
										Debug.AddMessage(Debug.MessageType.Error, false, "Minimum and Maximum must not be equal in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										Radius = 0.0;
									}
									if (Math.Abs(InitialAngle - LastAngle) > 6.28318531) {
										Debug.AddMessage(Debug.MessageType.Warning, false, "The absolute difference between InitialAngle and LastAngle exceeds 360 degrees in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
									}
									if (Radius != 0.0) {
										// create element
										int j = CreateElement(Train, LocationX - Radius, LocationY - Radius, 2.0 * Radius, 2.0 * Radius, 0.5, 0.5, (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, DriverX, DriverY, DriverZ, null, null, Color, false);
										InitialAngle = InitialAngle + Math.PI;
										LastAngle = LastAngle + Math.PI;
										double x0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.X;
										double y0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Y;
										double z0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Z;
										double x1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.X;
										double y1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Y;
										double z1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Z;
										double x2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.X;
										double y2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Y;
										double z2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Z;
										double x3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.X;
										double y3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Y;
										double z3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Z;
										double cx = 0.25 * (x0 + x1 + x2 + x3);
										double cy = 0.25 * (y0 + y1 + y2 + y3);
										double cz = 0.25 * (z0 + z1 + z2 + z3);
										World.Vertex[] vertices = new World.Vertex[11];
										int[][] faces = new int[][] {
											new int[] { 0, 1, 2 },
											new int[] { 0, 3, 4 },
											new int[] { 0, 5, 6 },
											new int[] { 0, 7, 8 },
											new int[] { 0, 9, 10 }
										};
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh = new World.Mesh(vertices, faces, Color);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDClockwiseWinding = InitialAngle <= LastAngle;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDInitialAngle = InitialAngle;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDLastAngle = LastAngle;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDVectors = new Vector3D[] {
											new Vector3D(x0, y0, z0),
											new Vector3D(x1, y1, z1),
											new Vector3D(x2, y2, z2),
											new Vector3D(x3, y3, z3),
											new Vector3D(cx, cy, cz)
										};
										string f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
										double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
										double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
										if (Step == 1.0) {
											f += " floor";
										} else if (Step != 0.0) {
											string s = (1.0 / Step).ToString(Culture);
											string t = Step.ToString(Culture);
											f += " " + s + " * floor " + t + " *";
										}
										f += " " + a1.ToString(Culture) + " " + a0.ToString(Culture) + " fma";
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
									} else {
										Debug.AddMessage(Debug.MessageType.Error, false, "Radius is required to be specified in " + Section + " in " + FileName);
									}
								} break;
								// timetable
							case "timetable":
								{
									double LocationX = 0.0, LocationY = 0.0;
									double Width = 0.0, Height = 0.0;
									Color24 TransparentColor = new Color24(0, 0, 255);
									double Layer = 0.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !Conversions.TryParseDoubleVb6(a, out LocationX)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !Conversions.TryParseDoubleVb6(b, out LocationY)) {
															Debug.AddMessage(Debug.MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Debug.AddMessage(Debug.MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "width":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Width)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Width <= 0.0) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "height":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Height)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Height <= 0.0) {
														Debug.AddMessage(Debug.MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "transparentcolor":
													if (Value.Length != 0 && !Conversions.TryParseHexColor(Value, out TransparentColor)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !Conversions.TryParseDoubleVb6(Value, out Layer)) {
														Debug.AddMessage(Debug.MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									// create element
									if (Width <= 0.0) {
										Debug.AddMessage(Debug.MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
									}
									if (Height <= 0.0) {
										Debug.AddMessage(Debug.MessageType.Error, false, "Height is required to be specified in " + Section + " in " + FileName);
									}
									if (Width > 0.0 & Height > 0.0) {
										int j = CreateElement(Train, LocationX, LocationY, Width, Height, 0.5, 0.5, (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, DriverX, DriverY, DriverZ, null, null, new Color32(255, 255, 255, 255), false);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("timetable");
										Timetable.AddObjectForCustomTimetable(Train.Cars[Train.DriverCar].CarSections[0].Elements[j]);
									}
								} break;
						}
					}
				}
			}
		}

		// get stack language from subject
		private static string GetStackLanguageFromSubject(TrainManager.Train Train, string Subject, string ErrorLocation) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Suffix = "";
			{
				// detect d# suffix
				int i;
				for (i = Subject.Length - 1; i >= 0; i--) {
					int a = char.ConvertToUtf32(Subject, i);
					if (a < 48 | a > 57) break;
				}
				if (i >= 0 & i < Subject.Length - 1) {
					if (Subject[i] == 'd' | Subject[i] == 'D') {
						int n;
						if (int.TryParse(Subject.Substring(i + 1), System.Globalization.NumberStyles.Integer, Culture, out n)) {
							if (n == 0) {
								Suffix = " floor 10 mod";
							} else {
								string t0 = Math.Pow(10.0, (double)n).ToString(Culture);
								string t1 = Math.Pow(10.0, (double)-n).ToString(Culture);
								Suffix = " ~ " + t0 + " >= <> " + t1 + " * floor 10 mod 10 ?";
							}
							Subject = Subject.Substring(0, i);
							i--;
						}
					}
				}
			}
			// transform subject
			string Code;
			switch (Subject.ToLowerInvariant()) {
				case "acc":
					Code = "acceleration";
					break;
				case "motor":
					Code = "accelerationmotor";
					break;
				case "true":
					Code = "1";
					break;
				case "kmph":
					Code = "speedometer abs 3.6 *";
					break;
				case "mph":
					Code = "speedometer abs 2.2369362920544 *";
					break;
				case "ms":
					Code = "speedometer abs";
					break;
				case "bc":
					Code = "brakecylinder 0.001 *";
					break;
				case "mr":
					Code = "mainreservoir 0.001 *";
					break;
				case "sap":
					Code = "straightairpipe 0.001 *";
					break;
				case "bp":
					Code = "brakepipe 0.001 *";
					break;
				case "er":
					Code = "equalizingreservoir 0.001 *";
					break;
				case "door":
					Code = "1 doors -";
					break;
				case "csc":
					Code = "constSpeed";
					break;
				case "power":
					Code = "brakeNotchLinear 0 powerNotch ?";
					break;
				case "brake":
					Code = "brakeNotchLinear";
					break;
				case "rev":
					Code = "reverserNotch ++";
					break;
				case "hour":
					Code = "0.000277777777777778 time * 24 mod floor";
					break;
				case "min":
					Code = "0.0166666666666667 time * 60 mod floor";
					break;
				case "sec":
					Code = "time 60 mod floor";
					break;
				case "atc":
					Code = "271 pluginstate";
					break;
				default:
					{
						Code = "0";
						bool unsupported = true;
						if (Subject.StartsWith("ats", StringComparison.OrdinalIgnoreCase)) {
							string a = Subject.Substring(3);
							int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n)) {
								if (n >= 0 & n <= 255) {
									Code = n.ToString(Culture) + " pluginstate";
									unsupported = false;
								}
							}
						} else if (Subject.StartsWith("doorl", StringComparison.OrdinalIgnoreCase)) {
							string a = Subject.Substring(5);
							int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n)) {
								if (n >= 0 & n < Train.Cars.Length) {
									Code = n.ToString(Culture) + " leftdoorsindex ceiling";
									unsupported = false;
								} else {
									Code = "2";
									unsupported = false;
								}
							}
						} else if (Subject.StartsWith("doorr", StringComparison.OrdinalIgnoreCase)) {
							string a = Subject.Substring(5);
							int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n)) {
								if (n >= 0 & n < Train.Cars.Length) {
									Code = n.ToString(Culture) + " rightdoorsindex ceiling";
									unsupported = false;
								} else {
									Code = "2";
									unsupported = false;
								}
							}
						}
						if (unsupported) {
							Debug.AddMessage(Debug.MessageType.Error, false, "Invalid subject " + Subject + " encountered in " + ErrorLocation);
						}
					} break;
			}
			return Code + Suffix;
		}

		// create element
		private static int CreateElement(TrainManager.Train Train, double Left, double Top, double Width, double Height, double RelativeRotationCenterX, double RelativeRotationCenterY, double Distance, double PanelResolution, double PanelLeft, double PanelRight, double PanelTop, double PanelBottom, double PanelBitmapWidth, double PanelBitmapHeight, double PanelCenterX, double PanelCenterY, double PanelOriginX, double PanelOriginY, double DriverX, double DriverY, double DriverZ, Textures.Texture DaytimeTexture, Textures.Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement) {
			double WorldWidth, WorldHeight;
			if (Screen.Width >= Screen.Height) {
				WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / World.AspectRatio;
			} else {
				WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance / World.AspectRatio;
				WorldWidth = WorldHeight * World.AspectRatio;
			}
			double x0 = Left / PanelResolution;
			double x1 = (Left + Width) / PanelResolution;
			double y0 = (PanelBottom - Top) / PanelResolution * World.AspectRatio;
			double y1 = (PanelBottom - (Top + Height)) / PanelResolution * World.AspectRatio;
			double xd = 0.5 - PanelCenterX / PanelResolution;
			x0 += xd; x1 += xd;
			double yt = PanelBottom - PanelResolution / World.AspectRatio;
			double yd = (PanelCenterY - yt) / (PanelBottom - yt) - 0.5;
			y0 += yd; y1 += yd;
			x0 = (x0 - 0.5) * WorldWidth;
			x1 = (x1 - 0.5) * WorldWidth;
			y0 = (y0 - 0.5) * WorldHeight;
			y1 = (y1 - 0.5) * WorldHeight;
			double xm = x0 * (1.0 - RelativeRotationCenterX) + x1 * RelativeRotationCenterX;
			double ym = y0 * (1.0 - RelativeRotationCenterY) + y1 * RelativeRotationCenterY;
			Vector3D[] v = new Vector3D[4];
			v[0] = new Vector3D(x0 - xm, y1 - ym, 0);
			v[1] = new Vector3D(x0 - xm, y0 - ym, 0);
			v[2] = new Vector3D(x1 - xm, y0 - ym, 0);
			v[3] = new Vector3D(x1 - xm, y1 - ym, 0);
			World.Vertex t0 = new World.Vertex(v[0], new Vector2f(0.0f, 1.0f));
			World.Vertex t1 = new World.Vertex(v[1], new Vector2f(0.0f, 0.0f));
			World.Vertex t2 = new World.Vertex(v[2], new Vector2f(1.0f, 0.0f));
			World.Vertex t3 = new World.Vertex(v[3], new Vector2f(1.0f, 1.0f));
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Vertices = new World.Vertex[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new World.MeshFace[] { new World.MeshFace(new int[] { 0, 1, 2, 3 }) };
			Object.Mesh.Materials = new World.MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = (byte)(DaytimeTexture != null ? World.MeshMaterial.TransparentColorMask : 0);
			Object.Mesh.Materials[0].Color = Color;
			Object.Mesh.Materials[0].TransparentColor = new Color24(0, 0, 255);
			Object.Mesh.Materials[0].DaytimeTexture = DaytimeTexture;
			Object.Mesh.Materials[0].NighttimeTexture = NighttimeTexture;
			Object.Dynamic = true;
			// calculate offset
			Vector3D o;
			o.X = xm + DriverX;
			o.Y = ym + DriverY;
			o.Z = EyeDistance - Distance + DriverZ;
			// add object
			if (AddStateToLastElement) {
				int n = Train.Cars[Train.DriverCar].CarSections[0].Elements.Length - 1;
				int j = Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States.Length;
				Array.Resize<ObjectManager.AnimatedObjectState>(ref Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States, j + 1);
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[j].Position = o;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[j].Object = Object;
				return n;
			} else {
				int n = Train.Cars[Train.DriverCar].CarSections[0].Elements.Length;
				Array.Resize<ObjectManager.AnimatedObject>(ref Train.Cars[Train.DriverCar].CarSections[0].Elements, n + 1);
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n] = new ObjectManager.AnimatedObject();
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States = new ObjectManager.AnimatedObjectState[1];
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[0].Position = o;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[0].Object = Object;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].CurrentState = 0;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].ObjectIndex = ObjectManager.CreateDynamicObject();
				ObjectManager.Objects[Train.Cars[Train.DriverCar].CarSections[0].Elements[n].ObjectIndex] = ObjectManager.CloneObject(Object);
				return n;
			}
		}

	}
}