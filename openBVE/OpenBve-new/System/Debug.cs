using System;
using System.Collections.Generic;

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
			public Message(MessageType Type, bool FileNotFound, string Text){
				this.Type = Type;
				this.FileNotFound = FileNotFound;
				this.Text = Text;
			}
		}
		internal static List<Message> Messages = null;
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			if (Type == MessageType.Warning && !Program.Options.ShowWarningMessages) return;
			if (Type == MessageType.Error && !Program.Options.ShowErrorMessages) return;
			if (Messages == null) {
				Messages = new List<Message>(16);
			}
			Message m = new Message(Type, FileNotFound, Text);
			Messages.Add(m);

			Program.AppendToLogFile(Text);

		}
		internal static void ClearMessages() {
			Messages.Clear();
		}
    }
}

