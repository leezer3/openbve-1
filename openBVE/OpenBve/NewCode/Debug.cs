using System;

namespace OpenBve
{
	internal static class Debug
    {
		// messages
		internal enum MessageType {
			Warning,
			Error,
			Critical
		}
		internal struct Message {
			internal MessageType Type;
			internal bool FileNotFound;
			internal string Text;
		}
		internal static Message[] Messages = new Message[] { };
		internal static int MessageCount = 0;
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			if (Type == MessageType.Warning & !Options.Current.ShowWarningMessages) return;
			if (Type == MessageType.Error & !Options.Current.ShowErrorMessages) return;
			if (MessageCount == 0) {
				Messages = new Message[16];
			} else if (MessageCount >= Messages.Length) {
				Array.Resize<Message>(ref Messages, Messages.Length << 1);
			}
			Messages[MessageCount].Type = Type;
			Messages[MessageCount].FileNotFound = FileNotFound;
			Messages[MessageCount].Text = Text;
			MessageCount++;

			Program.AppendToLogFile(Text);

		}
		internal static void ClearMessages() {
			Messages = new Message[] { };
			MessageCount = 0;
		}
    }
}

