using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Terrascape.Debugging;
using Terrascape.Registry;
using Terrascape.Rendering;
using Terrascape.Rendering.Interface;
using static Terrascape.Debugging.Indentation;

#nullable enable

namespace Terrascape
{
	internal class Terrascape : Program
	{
		private static Shader? ui_shader;
		internal static TextureAtlas? GUIAtlas { get; private set; }
		internal static TextureAtlas? BlocksAtlas { get; private set; }

		//internal static bool GUIDebug { get; private set; } = false;
			
		private Matrix4 view_matrix, projection_matrix;
		private GUI? current_gui = null;

		protected override void Initialize()
		{
			GL.ClearColor(.1f, .1f, .1f, 0f);
			
			GL.FrontFace(FrontFaceDirection.Cw);
			GL.CullFace(CullFaceMode.Back);
			
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			
			GL.Enable(EnableCap.DepthTest);
			
			GL.Enable(EnableCap.LineSmooth);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			
			CheckForUpdates();
			CheckForChanges();

			this.view_matrix = Matrix4.Identity;
			this.view_matrix *= Matrix4.CreateTranslation(0f, 0f, -1f);
			this.projection_matrix = Matrix4.CreateOrthographic(this.window.Width, this.window.Height, .01f, 100f); // TODO(LOGIX): Update values

			PreLoad();
			SetUiShaderChannelMix(); // NOTE(LOGIX): This is necessary to set the default channel mix to full white + alpha
			ChangeGUI<GUILoadingScreen>();
		}

		private static void CheckForUpdates()
		{
			/* TODO(LOGIX): Implement
			Debug.LogInfo("Checking for updates");
			string? latest = GitHelper.ReadResource("https://raw.githubusercontent.com/DoctorLogiq/Terrascape/master/Resources/LATEST.json", true);
			if (string.IsNullOrEmpty(latest))
			{
				Debug.LogError("Update check failed");
			}
			else
			{
				foreach ((string key, object value) in JsonHelper.ParseJsonBasic(latest))
				{
					
				}
			}
			*/
		}

		private static void CheckForChanges()
		{
			/* TODO(LOGIX): Implement
			Debug.LogInfo("Fetching changelog");
			string? changelog = GitHelper.ReadResource("https://raw.githubusercontent.com/DoctorLogiq/Terrascape/master/Resources/CHANGES.json", true);
			if (string.IsNullOrEmpty(changelog))
			{
				Debug.LogError("Failed to fetch the changelog");
			}
			else
			{
				foreach (string str in Array.ConvertAll((object[])JsonHelper.ParseJsonBasic(changelog)[0].Item2, (p_input => p_input?.ToString() ?? string.Empty)))
				{
					
				}
			}
			*/
		}

		private static void PreLoad()
		{
			Debug.LogInfo("Loading shaders", p_post: Indent);
			{
				Shader.Load("ui_shader", "ui_shader");
				ui_shader = ShaderRegistry.Get("ui_shader");
			}
			Debug.LogInfo("Shaders loaded", p_pre: Unindent);
			
			GUIAtlas    = TextureAtlas.Build("gui_texture_atlas", "GUI", "gui");
			BlocksAtlas = TextureAtlas.Build("blocks_texture_atlas", "Blocks", "block");
		}
		
		protected override void Load()
		{
 
		}

		internal void ChangeGUI<T>() where T : GUI, new()
		{
			this.current_gui?.Dispose();
			this.current_gui = new T();
		}

		private KeyboardState last_keyboard_state = Keyboard.GetState();

		protected override void Update(in double p_delta)
		{
			KeyboardState current_keyboard_state = Keyboard.GetState();
			
			if (current_keyboard_state.IsKeyUp(Key.Escape) && this.last_keyboard_state.IsKeyDown(Key.Escape))
				this.window.Close(); // TODO(LOGIX): Request shutdown
			
			this.last_keyboard_state = current_keyboard_state;
		}

		protected override void Render(in double p_delta)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			GL.Disable(EnableCap.DepthTest);
			{
				Debug.Assert(() => ui_shader != null, true, "The 'ui_shader' was somehow null during Render()");

				ui_shader.Use();
				GUIAtlas.Use();
				Shader.CurrentShader?.SetMatrix4("inView", this.view_matrix);
				Shader.CurrentShader?.SetMatrix4("inProjection", this.projection_matrix);
				this.current_gui?.Render(p_delta);
			}
			GL.Enable(EnableCap.DepthTest);

			this.window.SwapBuffers();
			GL.Flush();
		}

		protected override void Resize()
		{
			GL.Viewport(0, 0, this.window.Width, this.window.Height);

			this.projection_matrix = Matrix4.CreateOrthographic(this.window.Width, this.window.Height, .01f, 100f);
		}

		protected override void RequestShutdown(ref bool p_cancel)
		{
			Debug.LogDebug("Shutdown request received; accepting");
		}

		protected override void Shutdown()
		{
			Debug.ResetIndentation();
			Debug.LogDebug("Shutting down", p_post: Indent);
			
			this.current_gui?.Dispose();
			
			foreach (TextureUnit unit in Enum.GetValues(typeof(TextureUnit)))
			{
				GL.ActiveTexture(unit);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
			GL.DisableVertexAttribArray(0);
			GL.DisableVertexAttribArray(1);
			GL.UseProgram(0);
			
			Renderer.Cleanup();
			GraphicsObject.Cleanup();
			Debug.LogDebug("Shutdown complete", p_pre: Unindent);
		}

		[SuppressMessage("ReSharper", "RedundantCast")]
		internal static void SetUiShaderChannelMix(byte p_red = 255, byte p_green = 255, byte p_blue = 255, byte p_alpha = 255)
		{
			Shader shader = ShaderRegistry.Get("ui_shader");
			shader.Use();

			shader.SetFloat("inR", (float) p_red   / 255F);
			shader.SetFloat("inG", (float) p_green / 255F);
			shader.SetFloat("inB", (float) p_blue  / 255F);
			shader.SetFloat("inA", (float) p_alpha / 255F);
		}
	}
}