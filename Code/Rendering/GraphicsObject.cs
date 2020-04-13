using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terrascape.Debugging;
using Terrascape.Helpers;
using Terrascape.Registry;

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
			Debug.LogDebugProcessStart("Cleaning up OpenGL objects", DebuggingLevel.Verbose);
			foreach (GraphicsObject obj in registry)
			{
				Debug.LogDebug($"{SpecialCharacters.Bullet} {obj.GetType().Name} '{obj.name}' {($"({obj.ID})")}",
					DebuggingLevel.Verbose, p_after_indentation: IndentationStyle.Indent);
				
				obj.Delete();
				Debug.Unindent(DebuggingLevel.Verbose);
			}
			Debug.LogDebugProcessEnd(true, DebuggingLevel.Verbose);
		}
	}
}