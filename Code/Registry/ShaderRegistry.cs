using System.Diagnostics.CodeAnalysis;
using Terrascape.Rendering;

#nullable enable

namespace Terrascape.Registry
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "UnusedType.Global")]
	public sealed class ShaderRegistry : Registry<Shader>
	{
		private static readonly ShaderRegistry instance = new ShaderRegistry();

		public new static bool IsRegistered(in Identifier p_identifier)
		{
			return instance.ActualIsRegistered(p_identifier);
		}

		public new static bool IsRegistered(in Shader p_object)
		{
			return instance.ActualIsRegistered(p_object);
		}

		public new static void Register(in Shader p_object)
		{
			instance.ActualRegister(in p_object);
		}

		public new static Shader Get(in Identifier p_identifier)
		{
			return instance.ActualGet(p_identifier);
		}
		
		public new static Shader? GetOrNull(in Identifier p_identifier)
		{
			return instance.ActualGetOrNull(p_identifier);
		}
	}
}