using System;
using System.Collections.Generic;
using Terrascape.Debugging;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering.Interface
{
	// TODO(LOGIX): Implement disposal, clean up model vao, vbo and ebos
	public abstract class GUI : IRenderable, IIdentifiable, IDisposable
	{
		private readonly List<UiModel> models = new List<UiModel>();
		public Identifier name { get; }

		protected GUI(Identifier p_name)
		{
			this.name = p_name;
			Debug.LogDebug($"Creating interface '{p_name}'", DebuggingLevel.Verbose, p_after_indentation: IndentationStyle.Indent);
			Construct();
			Debug.LogDebug("Interface created", DebuggingLevel.Verbose, IndentationStyle.Unindent);
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
			Debug.LogDebug($"Cleaning up model data for the '{this.name}' GUI", DebuggingLevel.Verbose, p_after_indentation: IndentationStyle.Indent);
			foreach (UiModel model in this.models)
			{
				Debug.LogDebug($"Cleaning up model data for model '{model.name}'", DebuggingLevel.Verbose, p_after_indentation: IndentationStyle.Indent);
				Renderer.DeleteEBO(model.ebo);
				Renderer.DeleteVBO(model.vbo);
				Renderer.DeleteVAO(model.vao);
				Debug.LogDebug($"Model data cleaned up for model '{model.name}'", DebuggingLevel.Verbose, IndentationStyle.Unindent);
			}
			Debug.LogDebug("Cleanup complete", DebuggingLevel.Verbose, IndentationStyle.Unindent);
		}
	}
}