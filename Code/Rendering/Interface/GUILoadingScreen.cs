using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering.Interface
{
	internal sealed class GUILoadingScreen : GUI
	{
		public GUILoadingScreen() : base("loading_screen_interface")
		{
		}
		
		protected override void Construct()
		{
			Add(new UiModel("loading_model", RegistryKeys.Textures.Loading, UiModel.Rectangle, -50f, -50f, UiModelAnchorMode.BottomRight));
		}

		private byte alpha = 255;
		private bool decrease = true;

		protected override void PreRender(in double p_delta)
		{
			Terrascape.SetUiShaderChannelMix(p_alpha: this.alpha);
			
			if (this.decrease && this.alpha == 50)
				this.decrease = false;
			else if (!this.decrease && this.alpha == 255)
				this.decrease = true;

			if (this.decrease)
				this.alpha--;
			else
				this.alpha++;
		}
	}
}