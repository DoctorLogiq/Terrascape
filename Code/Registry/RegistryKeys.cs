using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Terrascape.Registry
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class RegistryKeys
	{
		public static class Shaders
		{
			public const string InterfaceShader = "interface_shader";
		}

		public static class Textures
		{
			public const string Missing = "missing_texture";
			public const string FullMask = "full_mask";
			public const string Loading = "loading_texture";
		}
	}
}