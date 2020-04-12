using System;
using OpenTK;

// ReSharper disable All
#nullable enable

namespace Terrascape.Windowing
{
	public interface IGameWindow : INativeWindow
	{
		void Run();
		
		void Run(double updateRate);
		
		void MakeCurrent();
		
		void SwapBuffers();
		
		event EventHandler<EventArgs> Load;
		
		event EventHandler<EventArgs> Unload;

		event EventHandler<FrameEventArgs> UpdateFrame;

		event EventHandler<FrameEventArgs> RenderFrame;
	}
}