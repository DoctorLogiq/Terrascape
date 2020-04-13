using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Terrascape.Debugging;
using Terrascape.Helpers;
using Terrascape.Registry;
using Terrascape.Rendering;

#nullable enable

namespace Terrascape
{
	internal class Terrascape : Program
	{
		private Matrix4 view_matrix, projection_matrix;
		private UiModel? test_model;
		
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
		}

		[SuppressMessage("ReSharper", "UnusedVariable")] // TODO(LOGIX): Remove when implemented
		private static void CheckForUpdates()
		{
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
					// TODO(LOGIX): Implement
				}
			}
		}

		[SuppressMessage("ReSharper", "UnusedVariable")] // TODO(LOGIX): Remove when implemented
		private static void CheckForChanges()
		{
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
					// TODO(LOGIX): Implement
				}
			}
		}

		private void PreLoad()
		{
			Shader.Load(RegistryKeys.Shaders.InterfaceShader, "ui_shader");
			SetUiShaderChannelMix(); // NOTE(LOGIX): This is necessary to set the default channel mix to full white + alpha
            
			Texture.Load(RegistryKeys.Textures.FullMask, "GUI/full_mask");
			Texture.Load(RegistryKeys.Textures.Loading, "GUI/loading");
            
			float[] vertices =
			{
				-0.5f, +0.5f, +0.0f, 0f, 0f,
				+0.5f, +0.5f, +0.0f, 1f, 0f,
				+0.5f, -0.5f, +0.0f, 1f, 1f,
				-0.5f, -0.5f, +0.0f, 0f, 1f
			};

			uint[] indices =
			{
				0,1,3,
				1,2,3
			};

			this.test_model = new UiModel("test_model", TextureRegistry.Get(RegistryKeys.Textures.Loading), null, vertices, indices); 
		}
		
		protected override void Load()
		{
			
		}

		protected override void Update(in double p_delta)
		{
			
		}

		protected override void Render(in double p_delta)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			GL.Disable(EnableCap.DepthTest);
			{
				ShaderRegistry.Get(RegistryKeys.Shaders.InterfaceShader).Use();
				Shader.CurrentShader?.SetMatrix4("inView", this.view_matrix);
				Shader.CurrentShader?.SetMatrix4("inProjection", this.projection_matrix);
				this.test_model.Render(p_delta);
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
			Debug.LogDebug("Shutdown request");
		}

		protected override void Shutdown()
		{
			Debug.ResetIndentation();
			Debug.LogDebug("Shutting down", p_after_indentation: IndentationStyle.Indent);
			
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
			Debug.LogDebug("Shutdown complete", p_before_indentation: IndentationStyle.Unindent);
		}

		[SuppressMessage("ReSharper", "RedundantCast")]
		internal static void SetUiShaderChannelMix(byte p_red = 255, byte p_green = 255, byte p_blue = 255, byte p_alpha = 255)
		{
			Shader shader = ShaderRegistry.Get(RegistryKeys.Shaders.InterfaceShader);
			shader.Use();

			shader.SetFloat("inR", (float) p_red   / 255F);
			shader.SetFloat("inG", (float) p_green / 255F);
			shader.SetFloat("inB", (float) p_blue  / 255F);
			shader.SetFloat("inA", (float) p_alpha / 255F);
		}
	}
}