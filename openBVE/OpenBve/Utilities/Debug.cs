using System;
using System.Windows.Forms;

namespace OpenBve
{
	/// <summary>
	/// Informative functions
	/// </summary>
    public static class Debug
	{
		public static readonly string DialogTitle = Application.ProductName;
		/// <summary>
		/// Shows a message box with given text
		/// </summary>
		/// <param name="text">Message box contents</param>
		/// <param name="caption">Title </param>
		public static void InfoMessage(string text){
			MessageBox.Show(text, DialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		/// <summary>
		/// Shows a message box with given text, caption is <see cref="DialogTitle"/>
		/// </summary>
		/// <param name="format">String passed to <see cref="String.Format"/></param>
		/// <param name="objects">Objects passed to <see cref="String.Format"/></param>
		public static void InfoMessage(string format,params object[] objects){
			MessageBox.Show(String.Format(format,objects), DialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
    }
}

