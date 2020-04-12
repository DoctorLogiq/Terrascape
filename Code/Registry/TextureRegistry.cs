using System.Diagnostics.CodeAnalysis;
using Terrascape.Rendering;

#nullable enable

namespace Terrascape.Registry
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "UnusedType.Global")]
	public sealed class TextureRegistry : Registry<Texture>
	{
		private static readonly TextureRegistry instance = new TextureRegistry();

		public new static bool IsRegistered(in Identifier p_identifier)
		{
			return instance.ActualIsRegistered(p_identifier);
		}

		public new static bool IsRegistered(in Texture p_object)
		{
			return instance.ActualIsRegistered(p_object);
		}

		public new static void Register(in Texture p_object)
		{
			instance.ActualRegister(in p_object);
		}

		public new static Texture Get(in Identifier p_identifier)
		{
			return instance.ActualGet(p_identifier);
		}
		
		public new static Texture? GetOrNull(in Identifier p_identifier)
		{
			return instance.ActualGetOrNull(p_identifier);
		}
	}
}