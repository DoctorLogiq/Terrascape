#nullable enable

namespace Terrascape.Rendering
{
	public interface IRenderable
	{
		void Render(in double p_delta);
	}
}