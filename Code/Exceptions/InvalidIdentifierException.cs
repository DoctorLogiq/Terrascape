using System;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Exceptions
{
	public class InvalidIdentifierException : TerrascapeException
	{
		public InvalidIdentifierException(Identifier p_identifier, Exception? p_inner = null, params string[] p_additional_details)
			: base($"Invalid identifier: {p_identifier}", p_inner, p_additional_details)
		{
			
		}
	}
}