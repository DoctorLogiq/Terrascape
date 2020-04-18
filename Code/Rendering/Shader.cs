using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Terrascape.Debugging;
using Terrascape.Exceptions;
using Terrascape.Helpers;
using Terrascape.Registry;

#nullable enable

namespace Terrascape.Rendering
{
	public class Shader : GraphicsObject
	{
		internal static Shader? CurrentShader { get; private set; } = null;
		
		private readonly Dictionary<string, int> uniform_locations = new Dictionary<string, int>();
		private bool in_use = false;
		
		private Shader(Identifier p_name, int p_id) : base(p_name, p_id)
		{
			Debug.LogDebug($"Created Shader '{p_name}' ({p_id})");
		}

		public void Use()
		{
			if (!this.in_use)
			{
				CurrentShader?.StopUsing();
				
				GL.UseProgram(this.ID);
				this.in_use = true;
				CurrentShader = this;
			}
		}

		public void StopUsing()
		{
			if (this.in_use)
			{
				GL.UseProgram(0);
				this.in_use = false;
				CurrentShader = null;
			}
		}

		internal int GetAttributeLocation(string p_name)
		{
			return GL.GetAttribLocation(this.ID, p_name);
		}
		
		#region Uniform Setters

		public void SetInt(string p_name, int p_value)
		{
			Use();
			
			if (!this.uniform_locations.ContainsKey(p_name))
				throw new TerrascapeException($"Could not find int shader uniform '{p_name}' in the '{this.name}' shader");
			
			GL.Uniform1(this.uniform_locations[p_name], p_value);
		}
		
		public void SetFloat(string p_name, float p_value)
		{
			Use();
			
			if (!this.uniform_locations.ContainsKey(p_name))
				throw new TerrascapeException($"Could not find float shader uniform '{p_name}' in the '{this.name}' shader");
			
			GL.Uniform1(this.uniform_locations[p_name], p_value);
		}
		
		public void SetMatrix4(string p_name, Matrix4 p_value)
		{
			Use();
			
			if (!this.uniform_locations.ContainsKey(p_name))
				throw new TerrascapeException($"Could not find Matrix4 shader uniform '{p_name}' in the '{this.name}' shader");
			
			GL.UniformMatrix4(this.uniform_locations[p_name], true, ref p_value);
		}
		
		public void SetVector2(string p_name, Vector2 p_value)
		{
			Use();
			
			if (!this.uniform_locations.ContainsKey(p_name))
				throw new TerrascapeException($"Could not find Vector2 shader uniform '{p_name}' in the '{this.name}' shader");
			
			GL.Uniform2(this.uniform_locations[p_name], p_value);
		}
		
		public void SetVector3(string p_name, Vector3 p_value)
		{
			Use();
			
			if (!this.uniform_locations.ContainsKey(p_name))
				throw new TerrascapeException($"Could not find Vector3 shader uniform '{p_name}' in the '{this.name}' shader");
			
			GL.Uniform3(this.uniform_locations[p_name], p_value);
		}
		
		#endregion
		
		protected override void Delete()
		{
			GL.DeleteProgram(this.ID);
		}
		
		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public static Shader Load(Identifier p_name, string p_filename, bool p_register = true)
		{
			return Load(p_name, p_filename, p_filename, p_register);
		}

		[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
		public static Shader Load(Identifier p_name, string p_vertex_filename, string p_fragment_filename, bool p_register = true)
		{
            string vertex_source = AssetHelper.ReadTextAsset($"/Shaders/{(p_vertex_filename.Contains(".") ? p_vertex_filename : $"{p_vertex_filename}.vert")}");
            if (string.IsNullOrEmpty(vertex_source))
                throw new TerrascapeException("The vertex shader source code was null or empty");

            string fragment_source = AssetHelper.ReadTextAsset($"Shaders/{(p_fragment_filename.Contains(".") ? p_fragment_filename : $"{p_fragment_filename}.frag")}");
            if (string.IsNullOrEmpty(fragment_source))
                throw new TerrascapeException("The vertex vertex source code was null or empty");
            
            int vertex_shader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex_shader, vertex_source);
            GL.CompileShader(vertex_shader);
            string vertex_info = GL.GetShaderInfoLog(vertex_shader);
            if (!string.IsNullOrEmpty(vertex_info))
            {
	            GL.DeleteShader(vertex_shader);

                throw new TerrascapeException("vertex shader compilation failed; " + vertex_info);
            }

            int fragment_shader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment_shader, fragment_source);
            GL.CompileShader(fragment_shader);
            string fragment_info = GL.GetShaderInfoLog(fragment_shader);
            if (!string.IsNullOrEmpty(fragment_info))
            {
                GL.DeleteShader(vertex_shader);
                GL.DeleteShader(fragment_shader);

                throw new TerrascapeException("fragment shader compilation failed; " + fragment_info);
            }
            
            // ReSharper disable once InconsistentNaming
            int program_ID = GL.CreateProgram();
            GL.AttachShader(program_ID, vertex_shader);
            GL.AttachShader(program_ID, fragment_shader);
            GL.LinkProgram(program_ID);
            string program_info = GL.GetProgramInfoLog(program_ID);
            if (!string.IsNullOrEmpty(program_info))
            {
                GL.DetachShader(program_ID, vertex_shader);
                GL.DeleteShader(vertex_shader);

                GL.DetachShader(program_ID, fragment_shader);
                GL.DeleteShader(fragment_shader);

                GL.DeleteShader(program_ID);

                throw new TerrascapeException("Shader program linking failed; " + program_info);
            }

            GL.DetachShader(program_ID, vertex_shader);
            GL.DetachShader(program_ID, fragment_shader);
            GL.DeleteShader(vertex_shader);
            GL.DeleteShader(vertex_shader);
			
			Shader shader = new Shader(p_name, program_ID);
			
			/* Programatically retrieve all shader uniforms and store them */
			GL.GetProgram(program_ID, GetProgramParameterName.ActiveUniforms, out int uniform_count);
			for (int i = 0; i < uniform_count; ++i)
			{
				string key = GL.GetActiveUniform(program_ID, i, out _, out _);
				int location = GL.GetUniformLocation(program_ID, key);
				
				shader.uniform_locations.Add(key, location);
			}
			
			if (p_register) ShaderRegistry.Register(shader);
			return shader;
		}
	}
}