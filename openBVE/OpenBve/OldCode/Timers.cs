using System;

namespace OpenBve {
	internal static class Timers {

		// members
		private static double SdlTime = 0.0;

		// initialize
		internal static void Initialize() {
			SdlTime = 0.001 * (double)Environment.TickCount;
		}

		// get elapsed time
		internal static double GetElapsedTime() {
			double a = 0.001 * (double)Environment.TickCount;
			double d = a - SdlTime;
			SdlTime = a;
			return d;
		}

	}
}