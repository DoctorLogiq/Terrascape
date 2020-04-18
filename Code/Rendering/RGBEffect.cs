using System;

#nullable enable

namespace Terrascape.Rendering
{
	public class RGBEffect
	{
		public           byte   Red   { get; private set; } = 0;
		public           byte   Green { get; private set; } = 0;
		public           byte   Blue  { get; private set; } = 0;
		private          double angle = 0;
		private readonly double speed;

		public RGBEffect(double p_speed = 1D)
		{
			this.speed = p_speed;
		}

		public void Update(in double p_delta)
		{
			this.angle += this.speed * p_delta;
			if (this.angle > 360) this.angle = 0;

			this.Red   = (byte) (Math.Sin(this.angle + 0) * 127 + 128);
			this.Green = (byte) (Math.Sin(this.angle + 2) * 127 + 128);
			this.Blue  = (byte) (Math.Sin(this.angle + 4) * 127 + 128);

			Terrascape.SetUiShaderChannelMix(this.Red, this.Green, this.Blue);
		}
	}
}