using OpenTK.Graphics;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using OpenTK;

#nullable enable

namespace Terrascape.Windowing
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
	[SuppressMessage("ReSharper", "UnusedParameter.Global")]
	// TODO(LOGIX): Come back to this
	internal sealed class TerrascapeWindow : NativeWindow, IGameWindow
	{
		private readonly Stopwatch         watch_render = new Stopwatch();
		private readonly Stopwatch         watch_update = new Stopwatch();
		private readonly FrameEventArgs    update_args  = new FrameEventArgs();
		private readonly FrameEventArgs    render_args  = new FrameEventArgs();
		private          Thread?           update_thread;
		private readonly bool              is_single_threaded;
		private          IGraphicsContext? gl_context;
		private          bool              is_exiting;
		private          double            update_period;
		private          double            render_period;
		private          double            target_update_period;
		private          double            target_render_period;
		private          double            update_time;
		private          double            render_time;
		private          double            update_timestamp;
		private          double            render_timestamp;
		private          double            update_epsilon;
		private          bool              is_running_slowly;

		/// <summary>Constructs a new GameWindow with sensible default attributes.</summary>
		public TerrascapeWindow()
			: this(640, 480, GraphicsMode.Default, "OpenTK Game Window", GameWindowFlags.Default, DisplayDevice.Default)
		{
		}

		public TerrascapeWindow(int p_width, int p_height)
			: this(p_width, p_height, GraphicsMode.Default, "OpenTK Game Window", GameWindowFlags.Default, DisplayDevice.Default)
		{
		}

		public TerrascapeWindow(int p_width, int p_height, GraphicsMode p_mode)
			: this(p_width, p_height, p_mode, "OpenTK Game Window", GameWindowFlags.Default, DisplayDevice.Default)
		{
		}

		public TerrascapeWindow(int p_width, int p_height, GraphicsMode p_mode, string p_title)
			: this(p_width, p_height, p_mode, p_title, GameWindowFlags.Default, DisplayDevice.Default)
		{
		}

		public TerrascapeWindow(int p_width, int p_height, GraphicsMode p_mode, string p_title, GameWindowFlags p_options)
			: this(p_width, p_height, p_mode, p_title, p_options, DisplayDevice.Default)
		{
		}

#pragma warning disable CS8625
		public TerrascapeWindow(int p_width, int p_height, GraphicsMode p_mode, string p_title, GameWindowFlags p_options, DisplayDevice p_device, int p_major = 1, int p_minor = 0, GraphicsContextFlags p_flags = GraphicsContextFlags.Default)
			: this(p_width, p_height, p_mode, p_title, p_options, p_device, p_major, p_minor, p_flags, null)
#pragma warning restore CS8625
		{
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
#pragma warning disable CS8625
		public TerrascapeWindow(int p_width, int p_height, GraphicsMode p_mode, string p_title, GameWindowFlags p_options, DisplayDevice p_device, int p_major, int p_minor, GraphicsContextFlags p_flags, IGraphicsContext p_shared_context, bool p_is_single_threaded = true)
			: base(p_width, p_height, p_title, p_options, p_mode ?? GraphicsMode.Default, p_device ?? DisplayDevice.Default)
#pragma warning restore CS8625
		{
			try
			{
				this.is_single_threaded = p_is_single_threaded;
				this.gl_context        = new GraphicsContext(p_mode ?? GraphicsMode.Default, this.WindowInfo, p_major, p_minor, p_flags);
				this.gl_context.MakeCurrent(this.WindowInfo);
				(this.gl_context as IGraphicsContextInternal)?.LoadAll();
				this.VSync = VSyncMode.On;
			}
			catch (Exception)
			{
				base.Dispose();
				throw;
			}
		}
		
		[SuppressMessage("ReSharper", "GCSuppressFinalizeForTypeWithoutDestructor")]
		public override void Dispose()
		{
			try
			{
				Dispose(true);
			}
			finally
			{
				try
				{
					if (this.gl_context != null)
					{
						this.gl_context.Dispose();
						this.gl_context = null;
					}
				}
				finally
				{
					base.Dispose();
				}
			}

			GC.SuppressFinalize(this);
		}
		
		public void Exit()
		{
			Close();
		}
		
		public void MakeCurrent()
		{
			EnsureUndisposed();
			this.Context?.MakeCurrent(this.WindowInfo);
		}

		protected override void OnClosing(CancelEventArgs p_e)
		{
			base.OnClosing(p_e);
			if (p_e.Cancel)
				return;
			this.is_exiting = true;
			OnUnloadInternal(EventArgs.Empty);
		}

		private void OnLoad(EventArgs p_e)
		{
			Load(this, p_e);
		}
		
		private void OnUnload(EventArgs p_e)
		{
			Unload(this, p_e);
		}
		
		public void Run()
		{
			Run(0.0, 0.0);
		}
		
		public void Run(double p_update_rate)
		{
			Run(p_update_rate, 0.0);
		}
		
		[SuppressMessage("ReSharper", "RedundantEmptyFinallyBlock")]
		public void Run(double p_updates_per_second, double p_frames_per_second)
		{
			EnsureUndisposed();
			try
			{
				if (p_updates_per_second < 0.0 || p_updates_per_second > 200.0)
					throw new ArgumentOutOfRangeException(nameof(p_updates_per_second), p_updates_per_second, "Parameter should be inside the range [0.0, 200.0]");
				if (p_frames_per_second < 0.0 || p_frames_per_second > 200.0)
					throw new ArgumentOutOfRangeException(nameof(p_frames_per_second), p_frames_per_second, "Parameter should be inside the range [0.0, 200.0]");
				if (p_updates_per_second != 0.0)
					this.TargetUpdateFrequency = p_updates_per_second;
				if (p_frames_per_second != 0.0)
					this.TargetRenderFrequency = p_frames_per_second;
				this.Visible = true;
				OnLoadInternal(EventArgs.Empty);
				OnResize(EventArgs.Empty);
				if (!this.is_single_threaded)
				{
					this.update_thread = new Thread(UpdateThread);
					this.update_thread.Start();
				}

				this.watch_render.Start();
				while (true)
				{
					ProcessEvents();
					if (this.Exists && !this.IsExiting)
					{
						if (this.is_single_threaded)
							DispatchUpdateFrame(this.watch_render);
						DispatchRenderFrame();
					}
					else
						break;
				}
			}
			finally
			{
				
			}
		}

		private void UpdateThread()
		{
			OnUpdateThreadStarted(this, new EventArgs());
			this.watch_update.Start();
			while (this.Exists && !this.IsExiting)
				DispatchUpdateFrame(this.watch_update);
		}

		private static double ClampElapsed(double p_elapsed)
		{
			return MathHelper.Clamp(p_elapsed, 0.0, 1.0);
		}

		private void DispatchUpdateFrame(Stopwatch p_watch)
		{
			int    num          = 4;
			double total_seconds = p_watch.Elapsed.TotalSeconds;
			double elapsed      = ClampElapsed(total_seconds - this.update_timestamp);
			while (elapsed > 0.0 && elapsed + this.update_epsilon >= this.TargetUpdatePeriod)
			{
				RaiseUpdateFrame(p_watch, elapsed, ref total_seconds);
				this.update_epsilon += elapsed - this.TargetUpdatePeriod;
				elapsed             =  ClampElapsed(total_seconds - this.update_timestamp);
				if (this.TargetUpdatePeriod <= double.Epsilon)
					break;
				this.is_running_slowly = this.update_epsilon >= this.TargetUpdatePeriod;
				if (this.is_running_slowly && --num == 0)
					break;
			}
		}

		private void DispatchRenderFrame()
		{
			double total_seconds = this.watch_render.Elapsed.TotalSeconds;
			double elapsed      = ClampElapsed(total_seconds - this.render_timestamp);
			if (elapsed <= 0.0 || elapsed < this.TargetRenderPeriod)
				return;
			RaiseRenderFrame(elapsed, ref total_seconds);
		}

		private void RaiseUpdateFrame(Stopwatch p_watch, double p_elapsed, ref double p_timestamp)
		{
			this.update_args.Time = p_elapsed;
			OnUpdateFrameInternal(this.update_args);
			this.update_period    = p_elapsed;
			this.update_timestamp = p_timestamp;
			p_timestamp             = p_watch.Elapsed.TotalSeconds;
			this.update_time      = p_timestamp - this.update_timestamp;
		}

		private void RaiseRenderFrame(double p_elapsed, ref double p_timestamp)
		{
			this.render_args.Time = p_elapsed;
			OnRenderFrameInternal(this.render_args);
			this.render_period    = p_elapsed;
			this.render_timestamp = p_timestamp;
			p_timestamp             = this.watch_render.Elapsed.TotalSeconds;
			this.render_time      = p_timestamp - this.render_timestamp;
		}
		
		public void SwapBuffers()
		{
			EnsureUndisposed();
			this.Context?.SwapBuffers();
		}
		
		public IGraphicsContext? Context
		{
			get
			{
				EnsureUndisposed();
				return this.gl_context;
			}
		}
		
		public bool IsExiting
		{
			get
			{
				EnsureUndisposed();
				return this.is_exiting;
			}
		}
		
		public double RenderFrequency
		{
			get
			{
				EnsureUndisposed();
				return this.render_period == 0.0 ? 1.0 : 1.0 / this.render_period;
			}
		}
		
		public double RenderPeriod
		{
			get
			{
				EnsureUndisposed();
				return this.render_period;
			}
		}
		
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		public double RenderTime
		{
			get
			{
				EnsureUndisposed();
				return this.render_time;
			}
			private set
			{
				EnsureUndisposed();
				this.render_time = value;
			}
		}
		
		public double TargetRenderFrequency
		{
			get
			{
				EnsureUndisposed();
				return this.TargetRenderPeriod == 0.0 ? 0.0 : 1.0 / this.TargetRenderPeriod;
			}
			set
			{
				EnsureUndisposed();
				if (value < 1.0)
				{
					this.TargetRenderPeriod = 0.0;
				}
				else
				{
					if (value > 500.0)
						return;
					this.TargetRenderPeriod = 1.0 / value;
				}
			}
		}
		
		public double TargetRenderPeriod
		{
			get
			{
				EnsureUndisposed();
				return this.target_render_period;
			}
			set
			{
				EnsureUndisposed();
				if (value <= 0.002)
				{
					this.target_render_period = 0.0;
				}
				else
				{
					if (value > 1.0)
						return;
					this.target_render_period = value;
				}
			}
		}
		
		public double TargetUpdateFrequency
		{
			get
			{
				EnsureUndisposed();
				return this.TargetUpdatePeriod == 0.0 ? 0.0 : 1.0 / this.TargetUpdatePeriod;
			}
			set
			{
				EnsureUndisposed();
				if (value < 1.0)
				{
					this.TargetUpdatePeriod = 0.0;
				}
				else
				{
					if (value > 500.0)
						return;
					this.TargetUpdatePeriod = 1.0 / value;
				}
			}
		}
		
		public double TargetUpdatePeriod
		{
			get
			{
				EnsureUndisposed();
				return this.target_update_period;
			}
			set
			{
				EnsureUndisposed();
				if (value <= 0.002)
				{
					this.target_update_period = 0.0;
				}
				else
				{
					if (value > 1.0)
						return;
					this.target_update_period = value;
				}
			}
		}
		
		public double UpdateFrequency
		{
			get
			{
				EnsureUndisposed();
				return this.update_period == 0.0 ? 1.0 : 1.0 / this.update_period;
			}
		}

		public double UpdatePeriod
		{
			get
			{
				EnsureUndisposed();
				return this.update_period;
			}
		}

		public double UpdateTime
		{
			get
			{
				EnsureUndisposed();
				return this.update_time;
			}
		}

		public VSyncMode VSync
		{
			get
			{
				EnsureUndisposed();
				GraphicsContext.Assert();
				if (this.Context?.SwapInterval < 0)
					return VSyncMode.Adaptive;
				return this.Context?.SwapInterval == 0 ? VSyncMode.Off : VSyncMode.On;
			}
			set
			{
				EnsureUndisposed();
				GraphicsContext.Assert();
				Debug.Assert(this.Context != null);
				switch (value)
				{
					case VSyncMode.Off:
						this.Context.SwapInterval = 0;
						break;
					case VSyncMode.On:
						this.Context.SwapInterval = 1;
						break;
					case VSyncMode.Adaptive:
						this.Context.SwapInterval = -1;
						break;
				}
			}
		}

		public override WindowState WindowState
		{
			get => base.WindowState;
			set
			{
				base.WindowState = value;
				this.Context?.Update(this.WindowInfo);
			}
		}
		
		public event EventHandler<EventArgs> Load = (p_param1, p_param2) => { };
		
		public event EventHandler<FrameEventArgs> RenderFrame = (p_param1, p_param2) => { };
		
		public event EventHandler<EventArgs> Unload = (p_param1, p_param2) => { };

		public event EventHandler<FrameEventArgs> UpdateFrame = (p_param1, p_param2) => { };
		
		[SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")] public event EventHandler OnUpdateThreadStarted = (p_param1, p_param2) => { };
		
		internal static void Dispose(bool p_manual)
		{
		}
		
		internal void OnRenderFrame(FrameEventArgs p_e)
		{
			RenderFrame(this, p_e);
		}
		
		internal void OnUpdateFrame(FrameEventArgs p_e)
		{
			UpdateFrame(this, p_e);
		}
		
		internal static void OnWindowInfoChanged(EventArgs p_e)
		{
		}

		protected override void OnResize(EventArgs p_e)
		{
			base.OnResize(p_e);
			this.gl_context?.Update(this.WindowInfo);
		}

		private void OnLoadInternal(EventArgs p_e)
		{
			OnLoad(p_e);
		}

		private void OnRenderFrameInternal(FrameEventArgs p_e)
		{
			if (!this.Exists || this.is_exiting)
				return;
			OnRenderFrame(p_e);
		}

		private void OnUnloadInternal(EventArgs p_e)
		{
			OnUnload(p_e);
		}

		private void OnUpdateFrameInternal(FrameEventArgs p_e)
		{
			if (!this.Exists || this.is_exiting)
				return;
			OnUpdateFrame(p_e);
		}

		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private void OnWindowInfoChangedInternal(EventArgs p_e)
		{
			this.gl_context?.MakeCurrent(this.WindowInfo);
			OnWindowInfoChanged(p_e);
		}
	}
}