﻿using System;
using System.Reflection;
using System.Collections.Generic;
using OpenTK.Input;

namespace OpenBve
{
	/// <summary>Provides functions for dealing with joysticks.</summary>
	internal static class Joysticks
	{
		// --- structures ---
		
		/// <summary>Represents a joystick.</summary>
		internal struct Joystick
		{
			// --- members ---
			/// <summary>
			/// Positions of hats on the joystick obtained from the last <see cref="Joystick.Update"/> call.
			/// </summary>
			private HatPosition[] hats;
			/// <summary>
			/// Positions of axes on the joystick obtained from the last <see cref="Joystick.Update"/> call.
			/// </summary>
			private float[] axes;
			/// <summary>
			/// States of buttons on the joystick obtained from the last <see cref="Joystick.Update"/> call.
			/// </summary>
			private ButtonState[] buttons;
			/// <summary>The textual representation of the joystick.</summary>
			internal string Name;
			/// <summary>The OpenTK index of this joystick.</summary>
			internal int Index;
			// --- constructors ---
			/// <summary>Creates a new joystick.</summary>
			/// <param name="name">The textual representation of the joystick.</param>
			/// <param name="index">The OpenTK index of this joystick.</param>
			internal Joystick(string name, int index) {
				this.Name = name;
				this.Index = index;
				var caps = OpenTK.Input.Joystick.GetCapabilities(index);
				hats = new HatPosition[JoystickHat.Last+1];
				axes = new float[JoystickAxis.Last+1];
				buttons = new ButtonState[JoystickButton.Last+1];
			}

			// --- public API methods ---
			/// <summary>
			/// Update internal joystick state.
			/// </summary>
			internal void Update() {
				var joyState = OpenTK.Input.Joystick.GetState(Index);
				for (int i = 0; i < hats.Length; i++) {
					hats[i] = joyState.GetHat(EnumFromIndex<JoystickHat>(i,"Hat")).Position;
				}
				for (int j = 0; j < axes.Length; j++) {
					axes[j] = joyState.GetAxis(EnumFromIndex<JoystickAxis>(j,"Axis"));
				}
				for (int k = 0; k < buttons.Length; k++) {
					buttons[k] = joyState.GetButton(EnumFromIndex<JoystickButton>(k,"Button"));
				}
			}
			/// <summary>
			/// Get the position of hat specified by it's index.
			/// </summary>
			/// <returns>The hat position.</returns>
			/// <param name="index">The hat index (0 - (<see cref="JoystickHat.Last"/>+1)).</param>
			internal HatPosition GetHat(int index) {
				return hats[index];
			}

			/// <summary>
			/// Get the position of hat specified by it's name.
			/// </summary>
			/// <returns>The hat position.</returns>
			/// <param name="hat">The hat name.</param>
			internal HatPosition GetHat(JoystickHat hat) {
				return hats[EnumToIndex<JoystickHat>(hat,"Hat")];
			}

			/// <summary>
			/// Get the position of axis specified by it's index.
			/// </summary>
			/// <returns>The normalized float position (-1f - 1f) of the axis</returns>
			/// <param name="index">The axis index (0 - (<see cref="JoystickAxis.Last"/>+1)).</param>
			internal float GetAxis(int index) {
				return axes[index];
			}

			/// <summary>
			/// Get the position of axis specified by it's name.
			/// </summary>
			/// <returns>The normalized float position (-1f - 1f) of the axis</returns>
			/// <param name="axis">The axis name.</param>
			internal float GetAxis(JoystickAxis axis) {
				return axes[EnumToIndex<JoystickAxis>(axis,"Axis")];
			}

			/// <summary>
			/// Get the state of button specified by it's index.
			/// </summary>
			/// <returns>The button state (pressed, released)</returns>
			/// <param name="index">The button index (0 - (<see cref="JoystickButton.Last"/>+1)).</param>
			internal ButtonState GetButton(int index) {
				return buttons[index];
			}

			/// <summary>
			/// Get the state of button specified by it's name.
			/// </summary>
			/// <returns>The button state (pressed, released)</returns>
			/// <param name="button">The button name.</param>
			internal ButtonState GetButton(JoystickButton button) {
				return buttons[EnumToIndex<JoystickButton>(button,"Button")];
			}

			// --- private helper methods ---
			/// <summary>
			/// Converts enum member name to instance of it.
			/// </summary>
			/// <returns>Enum member with name <paramref name="basename"/>+<paramref name="index"/>.</returns>
			/// <param name="index">Index of member.</param>
			/// <param name="basename">Base enum name (e.g. without index).</param>
			/// <typeparam name="T">Type of enum.</typeparam>
			private T EnumFromIndex<T>(int index, string basename) {
				return (T)Enum.Parse(typeof(T), basename + index);
			}
			/// <summary>
			/// Converts enum member instance to it's index extracted from it's name.
			/// </summary>
			/// <returns>Index created by cutting out the <paramref name="basename"/> from enum member's name.</returns>
			/// <param name="enumVal">Enum member instance.</param>
			/// <param name="basename">Base enum name (e.g. without index).</param>
			/// <typeparam name="T">Type of enum.</typeparam>
			private int EnumToIndex<T>(T enumVal, string basename) {
				return Int32.Parse(Enum.GetName(typeof(T), enumVal).Substring(basename.Length));
			}
		}
		
		
		// --- members ---

		/// <summary>Whether joystick subsystem is initialized.</summary>
		private static bool Initialized = false;
		
		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal static Joystick[] AttachedJoysticks = new Joystick[] { };
		
		
		// --- functions ---
		
		/// <summary>Initializes joysticks. A call to Deinitialize should be made when terminating the program.</summary>
		/// <returns>Whether initializing joysticks was successful.</returns>
		internal static bool Initialize()
		{
			List<Joystick> joys = new List<Joystick>();
			for (int i = 0; i < 8; i++) {
				var state = OpenTK.Input.Joystick.GetState(i);
				var caps = OpenTK.Input.Joystick.GetCapabilities(i);
				if (state.IsConnected) {
					string description = String.Format(
						                     "Joystick #{0} ({1} axes, {2} buttons, {3} hats)", 
						                     joys.Count + 1, caps.AxisCount, caps.ButtonCount, caps.HatCount);
					joys.Add(new Joystick("Joystick " + (joys.Count + 1), i));
				}
			}
			AttachedJoysticks = joys.ToArray();
			Initialized = true;
			return true;
		}

		/// <summary>Deinitializes joysticks.</summary>
		internal static void Deinitialize()
		{
			if (Initialized) {
				AttachedJoysticks = null;
				Initialized = false;
			}
		}
		
	}
}