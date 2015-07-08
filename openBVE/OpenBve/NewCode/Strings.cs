using System;

namespace OpenBve
{
    internal static class Strings
	{
		internal struct InterfaceQuickReference {
			internal string HandleForward;
			internal string HandleNeutral;
			internal string HandleBackward;
			internal string HandlePower;
			internal string HandlePowerNull;
			internal string HandleBrake;
			internal string HandleBrakeNull;
			internal string HandleRelease;
			internal string HandleLap;
			internal string HandleService;
			internal string HandleEmergency;
			internal string HandleHoldBrake;
			internal string DoorsLeft;
			internal string DoorsRight;
			internal string Score;
		}
		internal static InterfaceQuickReference QuickReferences;
		internal static int RatingsCount = 10;
		private struct InterfaceString {
			internal string Name;
			internal string Text;
		}
		private static InterfaceString[] InterfaceStrings = new InterfaceString[16];
		private static int InterfaceStringCount = 0;
		private static int CurrentInterfaceStringIndex = 0;
		private static void AddInterfaceString(string Name, string Text) {
			if (InterfaceStringCount >= InterfaceStrings.Length) {
				Array.Resize<InterfaceString>(ref InterfaceStrings, InterfaceStrings.Length << 1);
			}
			InterfaceStrings[InterfaceStringCount].Name = Name;
			InterfaceStrings[InterfaceStringCount].Text = Text;
			InterfaceStringCount++;
		}
		internal static string GetInterfaceString(string Name) {
			int n = Name.Length;
			for (int k = 0; k < InterfaceStringCount; k++) {
				int i;
				if ((k & 1) == 0) {
					i = (CurrentInterfaceStringIndex + (k >> 1) + InterfaceStringCount) % InterfaceStringCount;
				} else {
					i = (CurrentInterfaceStringIndex - (k + 1 >> 1) + InterfaceStringCount) % InterfaceStringCount;
				}
				if (InterfaceStrings[i].Name.Length == n) {
					if (InterfaceStrings[i].Name == Name) {
						CurrentInterfaceStringIndex = (i + 1) % InterfaceStringCount;
						return InterfaceStrings[i].Text;
					}
				}
			}
			return Name;
		}

		// load language
		internal static void LoadLanguage(string File) {
			string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
			string Section = "";
			InterfaceStrings = new InterfaceString[16];
			InterfaceStringCount = 0;
			QuickReferences.HandleForward = "F";
			QuickReferences.HandleNeutral = "N";
			QuickReferences.HandleBackward = "B";
			QuickReferences.HandlePower = "P";
			QuickReferences.HandlePowerNull = "N";
			QuickReferences.HandleBrake = "B";
			QuickReferences.HandleBrakeNull = "N";
			QuickReferences.HandleRelease = "RL";
			QuickReferences.HandleLap = "LP";
			QuickReferences.HandleService = "SV";
			QuickReferences.HandleEmergency = "EM";
			QuickReferences.HandleHoldBrake = "HB";
			QuickReferences.DoorsLeft = "L";
			QuickReferences.DoorsRight = "R";
			QuickReferences.Score = "Score: ";
			for (int i = 0; i < Lines.Length; i++) {
				Lines[i] = Lines[i].Trim();
				if (!Lines[i].StartsWith(";")) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
					} else {
						int j = Lines[i].IndexOf('=');
						if (j >= 0) {
							string a = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
							string b = Conversions.Unescape(Lines[i].Substring(j + 1).TrimStart());
							switch (Section) {
								case "handles":
									switch (a) {
										case "forward": QuickReferences.HandleForward = b; break;
										case "neutral": QuickReferences.HandleNeutral = b; break;
										case "backward": QuickReferences.HandleBackward = b; break;
										case "power": QuickReferences.HandlePower = b; break;
										case "powernull": QuickReferences.HandlePowerNull = b; break;
										case "brake": QuickReferences.HandleBrake = b; break;
										case "brakenull": QuickReferences.HandleBrakeNull = b; break;
										case "release": QuickReferences.HandleRelease = b; break;
										case "lap": QuickReferences.HandleLap = b; break;
										case "service": QuickReferences.HandleService = b; break;
										case "emergency": QuickReferences.HandleEmergency = b; break;
										case "holdbrake": QuickReferences.HandleHoldBrake = b; break;
									} break;
								case "doors":
									switch (a) {
										case "left": QuickReferences.DoorsLeft = b; break;
										case "right": QuickReferences.DoorsRight = b; break;
									} break;
								case "misc":
									switch (a) {
										case "score": QuickReferences.Score = b; break;
									} break;
								case "commands":
									{
										for (int k = 0; k < Controls.CommandInfos.Length; k++) {
											if (string.Compare(Controls.CommandInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0) {
												Controls.CommandInfos[k].Description = b;
												break;
											}
										}
									} break;
								case "keys":
									{
										for (int k = 0; k < Controls.Keys.Length; k++) {
											if (string.Compare(Controls.Keys[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0) {
												Controls.Keys[k].Description = b;
												break;
											}
										}
									} break;
								default:
									AddInterfaceString(Section + "_" + a, b);
									break;
							}
						}
					}
				}
			}
		}


		// is japanese
		internal static bool IsJapanese(string Name) {
			for (int i = 0; i < Name.Length; i++) {
				int a = char.ConvertToUtf32(Name, i);
				if (a < 0x10000) {
					bool q = false;
					while (true) {
						if (a >= 0x2E80 & a <= 0x2EFF) break;
						if (a >= 0x3000 & a <= 0x30FF) break;
						if (a >= 0x31C0 & a <= 0x4DBF) break;
						if (a >= 0x4E00 & a <= 0x9FFF) break;
						if (a >= 0xF900 & a <= 0xFAFF) break;
						if (a >= 0xFE30 & a <= 0xFE4F) break;
						if (a >= 0xFF00 & a <= 0xFFEF) break;
						q = true; break;
					} if (q) return false;
				} else {
					return false;
				}
			} return true;
		}
    }
}

