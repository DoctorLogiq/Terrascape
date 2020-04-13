#nullable enable

namespace Terrascape.GameObjects
{
	public class GameObject
	{
		public readonly string name;
		public readonly Transform transform = new Transform();

		public GameObject(in string p_name)
		{
			this.name = p_name;
		}
	}
}