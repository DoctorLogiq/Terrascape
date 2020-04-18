using Terrascape.Debugging;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering.Interface
{
	internal sealed class GUILoadingScreen : GUI
	{
		private readonly RGBEffect rgb = new RGBEffect();
		
		public GUILoadingScreen() : base("loading_screen_interface")
		{
		}
		
		protected override void Construct()
		{
			Debug.Assert(() => Terrascape.GUIAtlas != null, true);
			Add(new UiModel("loading_model", Terrascape.GUIAtlas, "gui_loading", -50f, -50f, UiModelAnchorMode.BottomRight));
		}

		protected override void PreRender(in double p_delta)
		{
			//this.rgb.Update(p_delta);
			//Terrascape.SetUiShaderChannelMix(this.rgb.Red, this.rgb.Green, this.rgb.Blue);
		}

		protected override void PostRender(in double p_delta)
		{
			Terrascape.SetUiShaderChannelMix();
		}
	}
}