using System;
using System.Collections.Generic;
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
				List<Joystick> joys = new List<Joystick>();
				for (int i = 0; i < 8; i++) {
					var state = OpenTK.Input.Joystick.GetState(i);
					if (state.IsConnected) {
						joys.Add(new Joystick("Joystick "+(joys.Count+1),i));
					}
				}
				AttachedJoysticks = joys.ToArray();
				MainLoop.OldJoyStates = new MainLoop.JoyState[joys.Count];
				for(int j = 0; j < joys.Count; j++){
					var caps = OpenTK.Input.Joystick.GetCapabilities(joys[j].Index);
					int buttons	= caps.ButtonCount;
					int axes	= caps.AxisCount;
					int hats	= caps.HatCount;
					MainLoop.OldJoyStates[j] = new MainLoop.JoyState(buttons,axes,hats);
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