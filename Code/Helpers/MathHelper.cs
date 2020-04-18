using System;

#nullable enable

namespace Terrascape.Helpers
{
	public static class MathHelper
	{
		public static int FindNextPowerOf2(int p_value)
		{
			return (int) Math.Round(Math.Pow(2, Math.Ceiling(Math.Log(p_value, 2))));
		}
	}
}