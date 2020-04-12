using System.Diagnostics.CodeAnalysis;
using Terrascape.Debugging;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class Shader : GraphicsObject
	{
		private Shader(Identifier p_name, int p_id) : base(p_name, p_id)
		{
			Debug.LogDebug($"Created Shader '{p_name}' ({p_id})");
		}
		
		protected override void Delete()
		{
			// TODO(LOGIX): Implement!
		}

		public static Shader Load(Identifier p_name, bool p_register = true)
		{
			Shader shader = new Shader(p_name, 0);
			if (p_register) ShaderRegistry.Register(shader);
			return shader;
		}
	}
}