using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using Terrascape.Debugging;
using Terrascape.GameObjects;
using Terrascape.Registry;
using static Terrascape.Debugging.Indentation;

#nullable enable

namespace Terrascape.Rendering.Interface
{
	[SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
	[SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
	[SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
	public class UiModel : GameObject, IRenderable
	{
		public            UiModelAnchorMode AnchorMode { get; private set; }
		public            PrimitiveType     Type       { get; private set; }
		public            float             XOffset    { get; internal set; } = 0f;
		public            float             YOffset    { get; internal set; } = 0f;
		internal readonly TextureCell       texture_diffuse;
		internal readonly int               vbo, ebo, vao;
		internal readonly int               indices_length;

		public UiModel(in string p_name, TextureAtlas p_atlas, Identifier p_texture_diffuse, float p_x_offset = 0f, float p_y_offset = 0f, UiModelAnchorMode p_anchor_mode = UiModelAnchorMode.Center)
			: base(in p_name)
		{
			Debug.LogDebug($"Creating interface model '{p_name}'", DebuggingLevel.Verbose, p_post: Indent);

			this.texture_diffuse = p_atlas.Get(p_texture_diffuse);

			UVRectangle uvs = p_atlas.Get(p_texture_diffuse).uvs;
			float[] vertices =
			{
				-0.5f, +0.5f, +0.0f, (float)uvs.x1, (float)uvs.y1,
				+0.5f, +0.5f, +0.0f, (float)uvs.x2, (float)uvs.y1,
				+0.5f, -0.5f, +0.0f, (float)uvs.x2, (float)uvs.y2,
				-0.5f, -0.5f, +0.0f, (float)uvs.x1, (float)uvs.y2
			};
			
			uint[] indices =
			{
				0, 1, 3,
				1, 2, 3
			};

			Renderer.CreateVBO(true, out this.vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			Renderer.CreateEBO(true, out this.ebo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

			Shader interface_shader = ShaderRegistry.Get("ui_shader");
			interface_shader.Use();

			Renderer.CreateVAO(true, out this.vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ebo);

			int vertex_location = interface_shader.GetAttributeLocation("inPosition");
			GL.EnableVertexAttribArray(vertex_location); // TODO(LOGIX): Maybe store all enabled attrib arrays in Renderer, to disable at program end?
			GL.VertexAttribPointer(vertex_location, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

			int uv_location = interface_shader.GetAttributeLocation("inUV");
			GL.EnableVertexAttribArray(uv_location);
			GL.VertexAttribPointer(uv_location, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			this.indices_length = indices.Length;
			this.XOffset    = p_x_offset;
			this.YOffset    = p_y_offset;
			this.AnchorMode = p_anchor_mode;
			this.Type       = PrimitiveType.Triangles;
			this.transform.SetScale((float) this.texture_diffuse.width, (float) this.texture_diffuse.height, 1f);
			UpdateAnchoredPostion();

			Debug.LogDebug("Interface model created", DebuggingLevel.Verbose, Unindent);
		}

		private void UpdateAnchoredPostion()
		{
			// TODO(LOGIX): Only if offsets or window size change
			double half_texture_width = this.texture_diffuse.width / 2D;
			double half_texture_height = this.texture_diffuse.height / 2D;
			
			switch (this.AnchorMode)
			{
				case UiModelAnchorMode.Center:
					this.transform.SetTranslation(this.XOffset, -this.YOffset, 0f);
					break;
				case UiModelAnchorMode.Left:
					this.transform.SetTranslation((float) ((-Terrascape.HalfWidth + half_texture_width) + this.XOffset), -this.YOffset, 0f);
					break;
				case UiModelAnchorMode.Right:
					this.transform.SetTranslation((float) ((Terrascape.HalfWidth - half_texture_width) + this.XOffset), -this.YOffset, 0f);
					break;
				case UiModelAnchorMode.Top:
					this.transform.SetTranslation(this.XOffset, (float) (Terrascape.HalfHeight - half_texture_height) + -this.YOffset, 0f);
					break;
				case UiModelAnchorMode.Bottom:
					this.transform.SetTranslation(this.XOffset, (float) (-Terrascape.HalfHeight + half_texture_height) + -this.YOffset, 0f);
					break;

				case UiModelAnchorMode.TopLeft:
					this.transform.SetTranslation(
						(float) ((-Terrascape.HalfWidth + half_texture_width) + this.XOffset),
						(float) (Terrascape.HalfHeight - half_texture_height) + -this.YOffset,
						0f);
					break;

				case UiModelAnchorMode.BottomLeft:
					this.transform.SetTranslation(
						(float) ((-Terrascape.HalfWidth + half_texture_width) + this.XOffset),
						(float) (-Terrascape.HalfHeight + half_texture_height) + -this.YOffset,
						0f);
					break;

				case UiModelAnchorMode.TopRight:
					this.transform.SetTranslation(
						(float) ((Terrascape.HalfWidth - half_texture_width) + this.XOffset),
						(float) (Terrascape.HalfHeight - half_texture_height) + -this.YOffset,
						0f);
					break;

				case UiModelAnchorMode.BottomRight:
					this.transform.SetTranslation(
						(float) ((Terrascape.HalfWidth - half_texture_width) + this.XOffset),
						(float) (-Terrascape.HalfHeight + half_texture_height) + -this.YOffset,
						0f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Render(in double p_delta)
		{
			UpdateAnchoredPostion();
			Shader.CurrentShader?.SetInt("inDiffuse", 0); // TODO(LOGIX): Optimise
			this.transform.SendTransformationMatrixToShader();

			GL.BindVertexArray(this.vao);
			GL.DrawElements(this.Type, this.indices_length, DrawElementsType.UnsignedInt, 0);
		}
	}

	public enum UiModelAnchorMode
	{
		Center,
		Left,
		Top,
		Right,
		Bottom,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}
}