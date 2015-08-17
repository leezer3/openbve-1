using System;
using System.Diagnostics;
namespace OpenBve {
	internal static class Timers {

		// members
		private static Stopwatch timer;

		// initialize
		internal static void Initialize() {
			timer = new Stopwatch();
			timer.Start();
		}

		// get elapsed time
		internal static double GetElapsedTime() {
			long actual = timer.ElapsedMilliseconds;
			timer.Restart();
			return actual*0.001;
		}

	}
}