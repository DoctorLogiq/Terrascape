using System;

#nullable enable

namespace Terrascape.Exceptions
{
	public class IllegalStateException : TerrascapeException
	{
		public IllegalStateException(string p_message, Exception? p_inner = null, params string[] p_additional_details)
			: base($"Illegal state: {p_message}{(p_message.EndsWith("!") ? "" : "?")} This should not be possible.", p_inner, p_additional_details)
		{
			
		}
	}
}