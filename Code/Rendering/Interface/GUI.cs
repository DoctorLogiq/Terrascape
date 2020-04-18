using System;
using System.Collections.Generic;
using Terrascape.Debugging;
using Terrascape.Registry;
using static Terrascape.Debugging.Indentation;
using static Terrascape.Debugging.DebuggingLevel;

#nullable enable

namespace Terrascape.Rendering.Interface
{
	// TODO(LOGIX): Possibly make a function to alter offset (non-internally)
	public abstract class GUI : IRenderable, IIdentifiable, IDisposable
	{
		private readonly List<UiModel> models = new List<UiModel>();
		public Identifier name { get; }

		protected GUI(Identifier p_name)
		{
			this.name = p_name;
			Debug.LogDebug($"Creating interface '{p_name}'", Verbose, p_post: Indent);
			Construct();
			Debug.LogDebug("Interface created", Verbose, Unindent);
		}

		protected abstract void Construct();

		protected void Add(UiModel p_model)
		{
			this.models.Add(p_model);
		}

		public void Render(in double p_delta)
		{
			PreRender(p_delta);
			
			foreach (UiModel model in this.models)
			{
				model.Render(p_delta);
			}
			
			PostRender(p_delta);
		}

		protected virtual void PreRender(in double p_delta)
		{
			
		}

		protected virtual void PostRender(in double p_delta)
		{
			
		}

		public void Dispose()
		{
			Debug.LogDebug($"Cleaning up model data for the '{this.name}' GUI", Verbose, p_post: Indent);
			foreach (UiModel model in this.models)
			{
				Debug.LogDebug($"Cleaning up model data for model '{model.name}'", Verbose, p_post: Indent);
				Renderer.DeleteEBO(model.ebo);
				Renderer.DeleteVBO(model.vbo);
				Renderer.DeleteVAO(model.vao);
				Debug.LogDebug($"Model data cleaned up for model '{model.name}'", Verbose, Unindent);
			}
			Debug.LogDebug("Cleanup complete", Verbose, Unindent);
		}
	}
}