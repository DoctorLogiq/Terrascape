using OpenTK.Graphics.OpenGL4;
using Terrascape.Debugging;
using Terrascape.Rendering;

#nullable enable

namespace Terrascape
{
	internal class Terrascape : Program
	{
		protected override void Initialize()
		{
			GL.ClearColor(.1f, .1f, .1f, 0f);
			
			Texture.Load("test_texture");
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

			this.window.SwapBuffers();
			GL.Flush();
		}

		protected override void Resize()
		{
			
		}

		protected override void RequestShutdown(ref bool p_cancel)
		{
			Debug.LogDebug("Shutdown request");
		}

		protected override void Shutdown()
		{
			Debug.ResetIndentation();
			Debug.LogDebug("Shutting down", p_after_indentation: IndentationStyle.Indent);
			GraphicsObject.Cleanup();
			Debug.Unindent();
		}
	}
}