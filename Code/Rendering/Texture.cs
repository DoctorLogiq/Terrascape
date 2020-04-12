using System.Diagnostics.CodeAnalysis;
using Terrascape.Debugging;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering
{
	[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
	public class Texture : GraphicsObject
	{
		private Texture(Identifier p_name, int p_id) : base(p_name, p_id)
		{
			Debug.LogDebug($"Created Texture '{p_name}' ({p_id})");
		}
		
		protected override void Delete()
		{
			// TODO(LOGIX): Implement!
		}
		
		public static Texture Load(Identifier p_name, bool p_register = true)
		{
			Texture texture = new Texture(p_name, 0);
			if (p_register) TextureRegistry.Register(texture);
			return texture;
		}
	}
}