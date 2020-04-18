using System;

#nullable enable

namespace Terrascape.Exceptions
{
	public class TerrascapeException : Exception
	{
		public readonly string[]? additional_details;
		
		public TerrascapeException(string p_message, Exception? p_inner = null, params string[] p_additional_details) : base(p_message, p_inner)
		{
			this.additional_details = p_additional_details;
		}
	}
}