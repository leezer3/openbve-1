using System;
using System.Globalization;
using OpenBveApi.Colors;

namespace OpenBve
{
    public static class Conversions
    {
		internal enum Encoding {
			Unknown = 0,
			Utf8 = 1,
			Utf16Le = 2,
			Utf16Be = 3,
			Utf32Le = 4,
			Utf32Be = 5,
		}
		internal static Encoding GetEncodingFromFile(string File) {
			try {
				byte[] Data = System.IO.File.ReadAllBytes(File);
				if (Data.Length >= 3) {
					if (Data[0] == 0xEF & Data[1] == 0xBB & Data[2] == 0xBF) return Encoding.Utf8;
				}
				if (Data.Length >= 2) {
					if (Data[0] == 0xFE & Data[1] == 0xFF) return Encoding.Utf16Be;
					if (Data[0] == 0xFF & Data[1] == 0xFE) return Encoding.Utf16Le;
				}
				if (Data.Length >= 4) {
					if (Data[0] == 0x00 & Data[1] == 0x00 & Data[2] == 0xFE & Data[3] == 0xFF) return Encoding.Utf32Be;
					if (Data[0] == 0xFF & Data[1] == 0xFE & Data[2] == 0x00 & Data[3] == 0x00) return Encoding.Utf32Le;
				}
				return Encoding.Unknown;
			} catch {
				return Encoding.Unknown;
			}
		}
		internal static Encoding GetEncodingFromFile(string Folder, string File) {
			return GetEncodingFromFile(OpenBveApi.Path.CombineFile(Folder, File));
		}

		// ================================

		// try parse vb6
		internal static bool TryParseDoubleVb6(string Expression, out double Value) {
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				double a;
				if (double.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a)) {
					Value = a;
					return true;
				}
			}
			Value = 0.0;
			return false;
		}
		internal static bool TryParseFloatVb6(string Expression, out float Value) {
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				float a;
				if (float.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a)) {
					Value = a;
					return true;
				}
			}
			Value = 0.0f;
			return false;
		}
		internal static bool TryParseIntVb6(string Expression, out int Value) {
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				double a;
				if (double.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a)) {
					if (a >= -2147483648.0 & a <= 2147483647.0) {
						Value = (int)Math.Round(a);
						return true;
					} else break;
				}
			}
			Value = 0;
			return false;
		}

		// try parse time
		internal static bool TryParseTime(string Expression, out double Value) {
			Expression = TrimInside(Expression);
			if (Expression.Length != 0) {
				CultureInfo Culture = CultureInfo.InvariantCulture;
				int i = Expression.IndexOf('.');
				if (i >= 1) {
					int h; if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture, out m)) {
								Value = 3600.0 * (double)h + 60.0 * (double)m;
								return true;
							}
						} else if (n == 3 | n == 4) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out m)) {
								uint s; if (uint.TryParse(Expression.Substring(i + 3, n - 2), NumberStyles.None, Culture, out s)) {
									Value = 3600.0 * (double)h + 60.0 * (double)m + (double)s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					int h; if (int.TryParse(Expression, NumberStyles.Integer, Culture, out h)) {
						Value = 3600.0 * (double)h;
						return true;
					}
				}
			}
			Value = 0.0;
			return false;
		}

		// try parse hex color
		internal static bool TryParseHexColor(string Expression, out Color24 Color) {
			if (Expression.StartsWith("#")) {
				string a = Expression.Substring(1).TrimStart();
				int x; if (int.TryParse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out x)) {
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
						Color = new Color24((byte)r, (byte)g, (byte)b);
						return true;
					} else {
						Color = new Color24(0, 0, 255);
						return false;
					}
				} else {
					Color = new Color24(0, 0, 255);
					return false;
				}
			} else {
				Color = new Color24(0, 0, 255);
				return false;
			}
		}
		internal static bool TryParseHexColor(string Expression, out Color32 Color) {
			if (Expression.StartsWith("#")) {
				string a = Expression.Substring(1).TrimStart();
				int x; if (int.TryParse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out x)) {
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
						Color = new Color32((byte)r, (byte)g, (byte)b, 255);
						return true;
					} else {
						Color = new Color32(0, 0, 255, 255);
						return false;
					}
				} else {
					Color = new Color32(0, 0, 255, 255);
					return false;
				}
			} else {
				Color = new Color32(0, 0, 255, 255);
				return false;
			}
		}

		// try parse with unit factors
		internal static bool TryParseDouble(string Expression, double[] UnitFactors, out double Value) {
			double a;
			if (double.TryParse(Expression, NumberStyles.Any, CultureInfo.InvariantCulture, out a)) {
				Value = a * UnitFactors[UnitFactors.Length - 1];
				return true;
			} else {
				string[] parameters = Expression.Split(':');
				if (parameters.Length <= UnitFactors.Length) {
					Value = 0.0;
					for (int i = 0; i < parameters.Length; i++) {
						if (double.TryParse(parameters[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out a)) {
							int j = i + UnitFactors.Length - parameters.Length;
							Value += a * UnitFactors[j];
						} else {
							return false;
						}
					}
					return true;
				} else {
					Value = 0.0;
					return false;
				}
			}
		}
		internal static bool TryParseDoubleVb6(string Expression, double[] UnitFactors, out double Value) {
			double a;
			if (double.TryParse(Expression, NumberStyles.Any, CultureInfo.InvariantCulture, out a)) {
				Value = a * UnitFactors[UnitFactors.Length - 1];
				return true;
			} else {
				string[] parameters = Expression.Split(':');
				Value = 0.0;
				if (parameters.Length <= UnitFactors.Length) {
					for (int i = 0; i < parameters.Length; i++) {
						if (TryParseDoubleVb6(parameters[i].Trim(), out a)) {
							int j = i + UnitFactors.Length - parameters.Length;
							Value += a * UnitFactors[j];
						} else {
							return false;
						}
					}
					return true;
				} else {
					return false;
				}
			}
		}

		// trim inside
		private static string TrimInside(string Expression) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Expression.Length);
			for (int i = 0; i < Expression.Length; i++) {
				char c = Expression[i];
				if (!char.IsWhiteSpace(c)) {
					Builder.Append(c);
				}
			} return Builder.ToString();
		}

		// unescape
		internal static string Unescape(string Text) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Text.Length);
			int Start = 0;
			for (int i = 0; i < Text.Length; i++) {
				if (Text[i] == '\\') {
					Builder.Append(Text, Start, i - Start);
					if (i + 1 < Text.Length) {
						switch (Text[i + 1]) {
							case 'a': Builder.Append('\a'); break;
							case 'b': Builder.Append('\b'); break;
							case 't': Builder.Append('\t'); break;
							case 'n': Builder.Append('\n'); break;
							case 'v': Builder.Append('\v'); break;
							case 'f': Builder.Append('\f'); break;
							case 'r': Builder.Append('\r'); break;
							case 'e': Builder.Append('\x1B'); break;
							case 'c':
								if (i + 2 < Text.Length) {
									int CodePoint = char.ConvertToUtf32(Text, i + 2);
									if (CodePoint >= 0x40 & CodePoint <= 0x5F) {
										Builder.Append(char.ConvertFromUtf32(CodePoint - 64));
									} else if (CodePoint == 0x3F) {
										Builder.Append('\x7F');
									} else {
										//Interface.AddMessage(MessageType.Error, false, "Unrecognized control character found in " + Text.Substring(i, 3));
										return Text;
									} i++;
								} else {
									//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode control character escape sequence");
									return Text;
								} break;
							case '"':
								Builder.Append('"');
								break;
							case '\\':
								Builder.Append('\\');
								break;
							case 'x':
								if (i + 3 < Text.Length) {
									Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 2), 16)));
									i += 2;
								} else {
									//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode hexadecimal escape sequence.");
									return Text;
								} break;
							case 'u':
								if (i + 5 < Text.Length) {
									Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 4), 16)));
									i += 4;
								} else {
									//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode hexadecimal escape sequence.");
									return Text;
								} break;
							default:
								//Interface.AddMessage(MessageType.Error, false, "Unrecognized escape sequence found in " + Text + ".");
								return Text;
						}
						i++;
						Start = i + 1;
					} else {
						//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode escape sequence.");
						return Text;
					}
				}
			}
			Builder.Append(Text, Start, Text.Length - Start);
			return Builder.ToString();
		}

		// ================================

		// convert newlines to crlf
		internal static string ConvertNewlinesToCrLf(string Text) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			for (int i = 0; i < Text.Length; i++) {
				int a = char.ConvertToUtf32(Text, i);
				if (a == 0xD & i < Text.Length - 1) {
					int b = char.ConvertToUtf32(Text, i + 1);
					if (b == 0xA) {
						Builder.Append("\r\n");
						i++;
					} else {
						Builder.Append("\r\n");
					}
				} else if (a == 0xA | a == 0xC | a == 0xD | a == 0x85 | a == 0x2028 | a == 0x2029) {
					Builder.Append("\r\n");
				} else if (a < 0x10000) {
					Builder.Append(Text[i]);
				} else {
					Builder.Append(Text.Substring(i, 2));
					i++;
				}
			} return Builder.ToString();
		}
    }
}

