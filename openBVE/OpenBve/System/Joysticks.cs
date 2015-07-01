using System;
using OpenTK;
using OpenTK.Input;

namespace OpenBve {
	/// <summary>Provides functions for dealing with joysticks.</summary>
	internal static class Joysticks {
		
		// --- structures ---
		
		/// <summary>Represents a joystick.</summary>
		internal struct Joystick {
			// --- members ---
			/// <summary>The textual representation of the joystick.</summary>
			internal string Name;
			/// <summary>The SDL handle to the joystick.</summary>
			internal int Index;
			// --- constructors ---
			/// <summary>Creates a new joystick.</summary>
			/// <param name="name">The textual representation of the joystick.</param>
			/// <param name="index">The OpenTK index of the joystick.</param>
			internal Joystick(string name, int index) {
				this.Name = name;
				this.Index = index;
			}
		}
		
		
		// --- members ---
		
		/// <summary>Whether joysticks are initialized.</summary>
		private static bool Initialized = false;
		
		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal static Joystick[] AttachedJoysticks = new Joystick[] { };
		
		
		// --- functions ---
		
		/// <summary>Initializes joysticks. A call to Screen.Initialize must have been made before calling this function. A call to Deinitialize must be made when terminating the program.</summary>
		/// <returns>Whether initializing joysticks was successful.</returns>
		internal static bool Initialize() {
			if (!Initialized) {
				int count = Program.UI.Joysticks.Count;
				AttachedJoysticks = new Joystick[count];

				for (int i = 0; i < count; i++) {
					AttachedJoysticks[i].Name = Program.UI.Joysticks[i].Description;
					AttachedJoysticks[i].Index = i;
				}
				Initialized = true;
				return true;
			} else {
				return true;
			}
		}
		
		/// <summary>Deinitializes joysticks.</summary>
		internal static void Deinitialize() {
			if (Initialized) {
				AttachedJoysticks = new Joystick[] { };
				Initialized = false;
			}
		}
		
	}
}