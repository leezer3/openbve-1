using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Vector2 = OpenBveApi.Math.Vector2;
namespace OpenBve {
	internal static class Screen {
		
		// --- members ---
		
		/// <summary>Whether the screen is initialized.</summary>
		private static bool Initialized = false;
		
		/// <summary>The fixed width of the screen.</summary>
		internal static int Width{
			get{ return Program.UI.ClientSize.Width; }
			set{
				Size temp = new Size(value,Program.UI.ClientSize.Height);
				Program.UI.ClientSize = temp;
			}
		}
		
		/// <summary>The fixed height of the screen.</summary>
		internal static int Height{
			get{ return Program.UI.ClientSize.Height; }
			set{
				Size temp = new Size(Program.UI.ClientSize.Width,value);
				Program.UI.ClientSize = temp;
			}
		}
		
		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		internal static bool Fullscreen{
			get{ return Program.UI.WindowState == WindowState.Fullscreen; }
		}

		/// <summary>The fixed size of the screen.</summary>
		internal static Size Size {
			get{ return Program.UI.ClientSize; }
			set{ Program.UI.ClientSize = value; }
		}

		// --- functions ---
		
		/// <summary>Initializes the screen. A call to SDL_Init must have been made before calling this function. A call to Deinitialize must be made when terminating the program.</summary>
		/// <returns>Whether initializing the screen was successful.</returns>
		internal static bool Initialize() {
			int width = Interface.CurrentOptions.FullscreenMode ? 
				Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
			int height = Interface.CurrentOptions.FullscreenMode ? 
				Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
			var colors = new ColorFormat(8,8,8,0);
			int antialias = Interface.CurrentOptions.AntiAliasingLevel;
			var accum = new ColorFormat(0);
			var mode = new GraphicsMode(colors,24,0,antialias,accum,2,false);
			string title = Application.ProductName;
			var flags = Interface.CurrentOptions.FullscreenMode ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow;
			Program.UI = new GameWindow(width,height,mode,title,flags,DisplayDevice.Default,1,5,GraphicsContextFlags.Default);
			Program.UI.WindowBorder = Interface.CurrentOptions.FullscreenMode ? WindowBorder.Hidden : WindowBorder.Fixed;
			Program.UI.Icon = new Icon(OpenBveApi.Path.CombineFile(Program.FileSystem.DataFolder, "icon.ico"));
			Program.UI.VSync = Interface.CurrentOptions.VerticalSynchronization ? VSyncMode.On /*Adaptive?*/ : VSyncMode.Off;
			Program.UI.CursorVisible = true;


			Interface.CurrentOptions.AnisotropicFilteringMaximum = 0;
			string[] extensions = GL.GetString(StringName.Extensions).Split(new []{' '});
			for (int i = 0; i < extensions.Length; i++) {
				if (extensions[i] == "GL_EXT_texture_filter_anisotropic") {
					float n;
					GL.GetFloat((GetPName) ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out n);
					int m = (int)Math.Round(n);
					Interface.CurrentOptions.AnisotropicFilteringMaximum = Math.Max(0, m);
					break;
				}
			}
			if (Interface.CurrentOptions.AnisotropicFilteringLevel <= 0) {
				Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
			} else if (Interface.CurrentOptions.AnisotropicFilteringLevel > Interface.CurrentOptions.AnisotropicFilteringMaximum) {
				Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
			}
			// --- done ---
			Initialized = true;
			return true;
		}
		
		/// <summary>Deinitializes the screen.</summary>
		internal static void Deinitialize() {
			if (Initialized) {
				Program.UI.Close();
				Program.UI.Dispose();
				Initialized = false;
			}
		}
		
		/// <summary>Changes to or from fullscreen mode.</summary>
		internal static void ToggleFullscreen() {
			Renderer.ClearDisplayLists();
			GL.Disable(EnableCap.Fog);
			Renderer.FogEnabled = false;
			GL.Disable(EnableCap.Lighting);
			Renderer.LightingEnabled = false;
			Textures.UnloadAllTextures();
			if (Fullscreen) {
				Program.UI.ClientSize = new Size(Interface.CurrentOptions.FullscreenWidth,
					Interface.CurrentOptions.FullscreenHeight);
				Program.UI.WindowState = WindowState.Fullscreen;
				Program.UI.WindowBorder = WindowBorder.Hidden;
			} else {
				Program.UI.ClientSize = new Size(Interface.CurrentOptions.WindowWidth,
					Interface.CurrentOptions.WindowHeight);
				Program.UI.WindowState = WindowState.Normal;
				Program.UI.WindowBorder = WindowBorder.Fixed;
			}
			Renderer.InitializeLighting();
			MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
			MainLoop.InitializeMotionBlur();
			Timetable.CreateTimetable();
			Timetable.UpdateCustomTimetable(null, null);
			World.MouseGrabTarget = new Vector2(0.0, 0.0);
			World.MouseGrabIgnoreOnce = true;
			World.InitializeCameraRestriction();
			if (Renderer.OptionBackfaceCulling) {
				GL.Enable(EnableCap.CullFace);
			} else {
				GL.Disable(EnableCap.CullFace);
			}
			Renderer.ReAddObjects();
		}
	}
}