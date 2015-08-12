using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Vector2D = OpenBveApi.Math.Vector2D;
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
			int width = Options.Current.FullscreenMode ? 
				Options.Current.FullscreenWidth : Options.Current.WindowWidth;
			int height = Options.Current.FullscreenMode ? 
				Options.Current.FullscreenHeight : Options.Current.WindowHeight;
			var colors = new ColorFormat(8,8,8,0);
			int antialias = Options.Current.AntiAliasingLevel;
			var accum = new ColorFormat(0);
			var mode = new GraphicsMode(colors,24,0,antialias,accum,2,false);
			string title = Application.ProductName;
			var flags = Options.Current.FullscreenMode ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow;
			Program.UI = new GameWindow(width,height,mode,title,flags,DisplayDevice.Default,1,5,GraphicsContextFlags.Default);
			Program.UI.WindowBorder = Options.Current.FullscreenMode ? WindowBorder.Hidden : WindowBorder.Fixed;
			Program.UI.Icon = new Icon(OpenBveApi.Path.CombineFile(Program.FileSystem.DataFolder, "icon.ico"));
			Program.UI.VSync = Options.Current.VerticalSynchronization ? VSyncMode.On /*Adaptive?*/ : VSyncMode.Off;
			Program.UI.CursorVisible = true;


			Options.Current.AnisotropicFilteringMaximum = 0;
			string[] extensions = GL.GetString(StringName.Extensions).Split(new []{' '});
			for (int i = 0; i < extensions.Length; i++) {
				if (extensions[i] == "GL_EXT_texture_filter_anisotropic") {
					float n;
					GL.GetFloat((GetPName) ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out n);
					int m = (int)Math.Round(n);
					Options.Current.AnisotropicFilteringMaximum = Math.Max(0, m);
					break;
				}
			}
			if (Options.Current.AnisotropicFilteringLevel <= 0) {
				Options.Current.AnisotropicFilteringLevel = Options.Current.AnisotropicFilteringMaximum;
			} else if (Options.Current.AnisotropicFilteringLevel > Options.Current.AnisotropicFilteringMaximum) {
				Options.Current.AnisotropicFilteringLevel = Options.Current.AnisotropicFilteringMaximum;
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
				Program.UI.ClientSize = new Size(Options.Current.FullscreenWidth,
					Options.Current.FullscreenHeight);
				Program.UI.WindowState = WindowState.Fullscreen;
				Program.UI.WindowBorder = WindowBorder.Hidden;
			} else {
				Program.UI.ClientSize = new Size(Options.Current.WindowWidth,
					Options.Current.WindowHeight);
				Program.UI.WindowState = WindowState.Normal;
				Program.UI.WindowBorder = WindowBorder.Fixed;
			}
			Renderer.InitializeLighting();
			MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
			MainLoop.InitializeMotionBlur();
			Timetable.CreateTimetable();
			Timetable.UpdateCustomTimetable(null, null);
			World.MouseGrabTarget = new Vector2D(0.0, 0.0);
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