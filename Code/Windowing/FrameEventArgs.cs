using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Terrascape.Windowing
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class FrameEventArgs : EventArgs
	{
		private double elapsed;

		/// <summary>Constructs a new FrameEventArgs instance.</summary>
		public FrameEventArgs()
		{
		}

		/// <summary>Constructs a new FrameEventArgs instance.</summary>
		/// <param name="elapsed">The amount of time that has elapsed since the previous event, in seconds.</param>
		public FrameEventArgs(double elapsed)
		{
			this.Time = elapsed;
		}

		/// <summary>
		/// Gets a <see cref="T:System.Double" /> that indicates how many seconds of time elapsed since the previous event.
		/// </summary>
		public double Time
		{
			get => this.elapsed;
			internal set
			{
				if (value <= 0.0)
					throw new ArgumentOutOfRangeException();
				this.elapsed = value;
			}
		}
	}
}