using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terrascape.Debugging;
using Terrascape.Helpers;
using Terrascape.Registry;
using static Terrascape.Debugging.Indentation;

#nullable enable

namespace Terrascape.Rendering
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public abstract class GraphicsObject : IIdentifiable
	{
		private static readonly List<GraphicsObject> registry = new List<GraphicsObject>();
		
		public Identifier name { get; }
		public readonly int ID;

		protected GraphicsObject(Identifier p_name, int p_id = 0)
		{
			this.name = p_name;
			this.ID   = p_id;
			registry.Add(this);
		}

		protected abstract void Delete();

		internal static void Cleanup()
		{
			Debug.LogDebug("Cleaning up OpenGL objects", DebuggingLevel.Verbose, p_post: Indent);
			foreach (GraphicsObject obj in registry)
			{
				Debug.LogDebug($"{SpecialCharacters.Bullet} {obj.GetType().Name} '{obj.name}' {($"({obj.ID})")}",
					DebuggingLevel.Verbose, p_post: Indent);
				
				obj.Delete();
				Debug.Unindent(DebuggingLevel.Verbose);
			}
			Debug.LogDebug("OpenGL object cleanup complete", DebuggingLevel.Verbose, Unindent);
		}
	}
}