using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using Terrascape.Debugging;
using Terrascape.GameObjects;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering
{
	[SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
	public class UiModel : GameObject, IRenderable
	{
		private readonly Texture texture_diffuse;
		private readonly Texture texture_mask;
		private readonly int vbo, ebo, vao;
		internal readonly int indices_length;
		
		public UiModel(in string p_name, Texture p_texture_diffuse, Texture? p_texture_mask, float[] p_vertices, uint[] p_indices) : base(in p_name)
		{
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
			
			this.transform.SetScale((float)this.texture_diffuse.width, (float)this.texture_diffuse.height, 1f);
		}

		public void Render(in double p_delta)
		{
			Shader? current_shader = Shader.CurrentShader;
			if (current_shader == null)
				return;
			
			// TODO(LOGIX): Debug.Assert(current_shader is not null, is the interface_shader and is already in use);

			current_shader.SetInt("texture1", 0);
			current_shader.SetInt("texture2", 1);
			this.texture_diffuse.Use();
			this.texture_mask.Use(TextureUnit.Texture1);
			this.transform.SendTransformationMatrixToShader();
			
			GL.BindVertexArray(this.vao);
			GL.DrawElements(PrimitiveType.Triangles, this.indices_length, DrawElementsType.UnsignedInt, 0);
		}
	}
}