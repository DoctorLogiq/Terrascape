using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using Terrascape.Debugging;
using Terrascape.GameObjects;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering.Interface
{
	[SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
	[SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
	[SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
	public class UiModel : GameObject, IRenderable
	{
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		
		 public static readonly (float[], uint[], ModelPrimitiveType) Rectangle = (new[] {
					-0.5f, +0.5f, +0.0f, 0f, 0f,
					+0.5f, +0.5f, +0.0f, 1f, 0f,
					+0.5f, -0.5f, +0.0f, 1f, 1f,
					-0.5f, -0.5f, +0.0f, 0f, 1f
				}, new uint[] {
					0,1,3,
					1,2,3
				},
				ModelPrimitiveType.Triangles);

		 public UiModelAnchorMode AnchorMode { get; private set; }
		public PrimitiveType Type { get; private set; }
		public float XOffset { get; private set; } = 0f;
		public float YOffset { get; private set; } = 0f;
		private readonly Texture texture_diffuse;
		private readonly Texture texture_mask;
		internal readonly int vbo, ebo, vao;
		internal readonly int indices_length;
		
		/* Constructor which takes in model data, and doesn't take a mask texture */
		public UiModel(in string p_name, Identifier p_texture_diffuse, (float[], uint[], ModelPrimitiveType) p_model_data, float p_x_offset = 0f, float p_y_offset = 0f, UiModelAnchorMode p_anchor_mode = UiModelAnchorMode.Center)
			: this(p_name, TextureRegistry.Get(p_texture_diffuse), null, p_model_data.Item1, p_model_data.Item2, p_model_data.Item3, p_x_offset, p_y_offset, p_anchor_mode)
		{
			
		}
		
		/* Constructor which takes in model data, and takes identifiers and retrieves the textures from the TextureRegistry */
		public UiModel(in string p_name, Identifier p_texture_diffuse, Identifier? p_texture_mask, (float[], uint[], ModelPrimitiveType) p_model_data, float p_x_offset = 0f, float p_y_offset = 0f, UiModelAnchorMode p_anchor_mode = UiModelAnchorMode.Center)
			: this(p_name, TextureRegistry.Get(p_texture_diffuse), (!string.IsNullOrEmpty(p_texture_mask) ? TextureRegistry.Get(p_texture_mask) : null), p_model_data.Item1, p_model_data.Item2, p_model_data.Item3, p_x_offset, p_y_offset, p_anchor_mode)
		{
			
		}

		/* Constructor which takes identifiers and retrieves the textures from the TextureRegistry */
		public UiModel(in string p_name, Identifier p_texture_diffuse, Identifier? p_texture_mask, float[] p_vertices, uint[] p_indices, ModelPrimitiveType p_type, float p_x_offset = 0f, float p_y_offset = 0f, UiModelAnchorMode p_anchor_mode = UiModelAnchorMode.Center)
			: this(p_name, TextureRegistry.Get(p_texture_diffuse), (!string.IsNullOrEmpty(p_texture_mask) ? TextureRegistry.Get(p_texture_mask) : null), p_vertices, p_indices, p_type, p_x_offset, p_y_offset, p_anchor_mode)
		{
			
		}

		/* Constructor which takes in model data */
		public UiModel(in string p_name, Texture p_texture_diffuse, Texture? p_texture_mask, (float[], uint[], ModelPrimitiveType) p_model_data, float p_x_offset = 0f, float p_y_offset = 0f, UiModelAnchorMode p_anchor_mode = UiModelAnchorMode.Center)
			: this(p_name, p_texture_diffuse, p_texture_mask, p_model_data.Item1, p_model_data.Item2, p_model_data.Item3, p_x_offset, p_y_offset, p_anchor_mode)
		{
			
		}
		
		public UiModel(in string p_name, Texture p_texture_diffuse, Texture? p_texture_mask, float[] p_vertices, uint[] p_indices, ModelPrimitiveType p_type, float p_x_offset = 0f, float p_y_offset = 0f, UiModelAnchorMode p_anchor_mode = UiModelAnchorMode.Center) : base(in p_name)
		{
			Debug.LogDebug($"Creating interface model '{p_name}'", DebuggingLevel.Verbose, p_after_indentation: IndentationStyle.Indent);
			
			Renderer.CreateVBO(true, out this.vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, p_vertices.Length * sizeof(float), p_vertices, BufferUsageHint.StaticDraw);

			Renderer.CreateEBO(true, out this.ebo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, p_indices.Length * sizeof(uint), p_indices, BufferUsageHint.StaticDraw);
			
			Shader interface_shader = ShaderRegistry.Get(RegistryKeys.Shaders.InterfaceShader);
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

			this.indices_length = p_indices.Length;

			this.texture_diffuse = p_texture_diffuse;
			this.texture_mask = p_texture_mask ?? TextureRegistry.Get(RegistryKeys.Textures.FullMask);

			if (this.texture_mask.name != RegistryKeys.Textures.FullMask)
			{
				if (Math.Abs(this.texture_diffuse.width - this.texture_mask.width) > 0.5)
					Debug.LogWarning($"The diffuse and mask texture widths for the UiModel '{this.name}' do not match; undesired results may occur");
				if (Math.Abs(this.texture_diffuse.height - this.texture_mask.height) > 0.5)
					Debug.LogWarning($"The diffuse and mask texture heights for the UiModel '{this.name}' do not match; undesired results may occur");
			}

			this.XOffset = p_x_offset;
			this.YOffset = p_y_offset;
			this.AnchorMode = p_anchor_mode;
			this.Type = p_type == ModelPrimitiveType.Triangles ? PrimitiveType.Triangles : PrimitiveType.Quads;

			this.transform.SetScale((float)this.texture_diffuse.width, (float)this.texture_diffuse.height, 1f);

			UpdateAnchoredPostion();
			
			Debug.LogDebug("Interface model created", DebuggingLevel.Verbose, IndentationStyle.Unindent);
		}

		private void UpdateAnchoredPostion()
		{
			// TODO(LOGIX): Only if offsets or window size change
			switch (this.AnchorMode)
            {
            	case UiModelAnchorMode.Center:
	                this.transform.SetTranslation(this.XOffset, -this.YOffset, 0f);
	                break;
            	case UiModelAnchorMode.Left:
            		this.transform.SetTranslation((float)((-Terrascape.HalfWidth + this.texture_diffuse.half_width) + this.XOffset), -this.YOffset, 0f);
            		break;
            	case UiModelAnchorMode.Right:
            		this.transform.SetTranslation((float)((Terrascape.HalfWidth - this.texture_diffuse.half_width) + this.XOffset), -this.YOffset, 0f);
            		break;
                case UiModelAnchorMode.Top:
	                this.transform.SetTranslation(this.XOffset, (float)(Terrascape.HalfHeight - this.texture_diffuse.half_height) + -this.YOffset, 0f);
	                break;
                case UiModelAnchorMode.Bottom:
	                this.transform.SetTranslation(this.XOffset, (float)(-Terrascape.HalfHeight + this.texture_diffuse.half_height) + -this.YOffset, 0f);
	                break;
                
                case UiModelAnchorMode.TopLeft:
	                this.transform.SetTranslation(
		                (float)((-Terrascape.HalfWidth + this.texture_diffuse.half_width) + this.XOffset), 
		                (float)(Terrascape.HalfHeight - this.texture_diffuse.half_height) + -this.YOffset,
		                0f);
	                break;
                
                case UiModelAnchorMode.BottomLeft:
	                this.transform.SetTranslation(
		                (float)((-Terrascape.HalfWidth + this.texture_diffuse.half_width) + this.XOffset), 
		                (float)(-Terrascape.HalfHeight + this.texture_diffuse.half_height) + -this.YOffset,
		                0f);
	                break;
                
                case UiModelAnchorMode.TopRight:
	                this.transform.SetTranslation(
		                (float)((Terrascape.HalfWidth - this.texture_diffuse.half_width) + this.XOffset), 
		                (float)(Terrascape.HalfHeight - this.texture_diffuse.half_height) + -this.YOffset,
		                0f);
	                break;
                
                case UiModelAnchorMode.BottomRight:
	                this.transform.SetTranslation(
		                (float)((Terrascape.HalfWidth - this.texture_diffuse.half_width) + this.XOffset), 
		                (float)(-Terrascape.HalfHeight + this.texture_diffuse.half_height) + -this.YOffset,
		                0f);
	                break;
            }
		}

		public void Render(in double p_delta)
		{
			Shader? current_shader = Shader.CurrentShader;
			if (current_shader == null)
				return;
			
			Debug.Assert(() => current_shader != null);
			Debug.Assert(() => Shader.CurrentShader == ShaderRegistry.Get(RegistryKeys.Shaders.InterfaceShader));
			
			UpdateAnchoredPostion();

			current_shader.SetInt("texture1", 0); // TODO(LOGIX): Optimise
			current_shader.SetInt("texture2", 1);
			this.texture_diffuse.Use();
			this.texture_mask.Use(TextureUnit.Texture1);
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

	public enum ModelPrimitiveType
	{
		Triangles,
		Quads
	}
}