using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using Terrascape.Debugging;

#nullable enable

namespace Terrascape.Rendering
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Renderer
	{
		private static readonly List<(int, bool)> VAOs = new List<(int, bool)>();
        private static readonly List<(int, bool)> VBOs = new List<(int, bool)>();
        private static readonly List<(int, bool)> EBOs = new List<(int, bool)>();

        internal static int CreateVAO(bool p_bind, bool p_is_test = false)
        {
            int vao = GL.GenVertexArray();
            if (!p_is_test) Debug.LogDebug($"Created VAO ({vao})", DebuggingLevel.Verbose);
            else Debug.LogTest($"Created test VAO ({vao})", DebuggingLevel.Verbose);
            VAOs.Add((vao, p_is_test));

            if (p_bind) GL.BindVertexArray(vao);
            return vao;
        }

        internal static void CreateVAO(bool p_bind, out int p_vao, bool p_is_test = false)
        {
            p_vao = GL.GenVertexArray();
            if (!p_is_test) Debug.LogDebug($"Created VAO ({p_vao})", DebuggingLevel.Verbose);
            else Debug.LogTest($"Created test VAO ({p_vao})", DebuggingLevel.Verbose);
            VAOs.Add((p_vao, p_is_test));

            if (p_bind) GL.BindVertexArray(p_vao);
        }

        internal static int CreateVBO(bool p_bind, bool p_is_test = false)
        {
            int vbo = GL.GenBuffer();
            if (!p_is_test) Debug.LogDebug($"Created VBO ({vbo})", DebuggingLevel.Verbose);
            else Debug.LogTest($"Created test VBO ({vbo})", DebuggingLevel.Verbose);
            VBOs.Add((vbo, p_is_test));

            if (p_bind) GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            return vbo;
        }

        internal static void CreateVBO(bool p_bind, out int p_vbo, bool p_is_test = false)
        {
            p_vbo = GL.GenBuffer();
            if (!p_is_test) Debug.LogDebug($"Created VBO ({p_vbo})", DebuggingLevel.Verbose);
            else Debug.LogTest($"Created test VBO ({p_vbo})", DebuggingLevel.Verbose);
            VBOs.Add((p_vbo, p_is_test));

            if (p_bind) GL.BindBuffer(BufferTarget.ArrayBuffer, p_vbo);
        }

        internal static int CreateEBO(bool p_bind, bool p_is_test = false)
        {
            int ebo = GL.GenBuffer();
            if (!p_is_test) Debug.LogDebug($"Created EBO ({ebo})", DebuggingLevel.Verbose);
            else Debug.LogTest($"Created test EBO ({ebo})", DebuggingLevel.Verbose);
            EBOs.Add((ebo, p_is_test));

            if (p_bind) GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            return ebo;
        }

        internal static void CreateEBO(bool p_bind, out int p_ebo, bool p_is_test = false)
        {
            p_ebo = GL.GenBuffer();
            if (!p_is_test) Debug.LogDebug($"Created EBO ({p_ebo})", DebuggingLevel.Verbose);
            else Debug.LogTest($"Created test EBO ({p_ebo})", DebuggingLevel.Verbose);
            EBOs.Add((p_ebo, p_is_test));

            if (p_bind) GL.BindBuffer(BufferTarget.ElementArrayBuffer, p_ebo);
        }

        internal static void DeleteVBO(int p_vbo)
        {
            foreach ((int, bool) vbo in VBOs)
            {
                if (vbo.Item1 != p_vbo) continue;

                if (!vbo.Item2) Debug.LogDebug($"Deleting VBO ({vbo.Item1})", DebuggingLevel.Verbose);
                else Debug.LogTest($"Deleting test VBO ({vbo.Item1})", DebuggingLevel.Verbose);

                GL.DeleteBuffer(vbo.Item1);
                VBOs.Remove(vbo);
                return;
            }
        }

        internal static void DeleteVAO(int p_vao)
        {
            foreach ((int, bool) vao in VAOs)
            {
                if (vao.Item1 != p_vao) continue;

                if (!vao.Item2) Debug.LogDebug($"Deleting VAO ({vao.Item1})", DebuggingLevel.Verbose);
                else Debug.LogTest($"Deleting test VAO ({vao.Item1})", DebuggingLevel.Verbose);

                GL.DeleteVertexArray(vao.Item1);
                VAOs.Remove(vao);
                return;
            }
        }

        internal static void DeleteEBO(int p_ebo)
        {
            foreach ((int, bool) ebo in EBOs)
            {
                if (ebo.Item1 != p_ebo) continue;

                if (!ebo.Item2) Debug.LogDebug($"Deleting EBO ({ebo.Item1})", DebuggingLevel.Verbose);
                else Debug.LogTest($"Deleting test EBO ({ebo.Item1})", DebuggingLevel.Verbose);

                GL.DeleteBuffer(ebo.Item1);
                EBOs.Remove(ebo);
                return;
            }
        }

        internal static void Cleanup()
        {
            Debug.DoIfAssertionPasses(() => VBOs.Count > 0 || EBOs.Count > 0 || VAOs.Count > 0, () =>
            {
                Debug.LogDebugProcessStart("Cleaning up VAOs, VBOs and EBOs", DebuggingLevel.Verbose);
                foreach ((int id, bool is_test) in VBOs)
                {
                    if (!is_test) Debug.LogDebug($"Deleting VBO ({id})", DebuggingLevel.Verbose);
                    else Debug.LogTest($"Deleting test VBO ({id})", DebuggingLevel.Verbose);

                    GL.DeleteBuffer(id);
                }
                foreach ((int id, bool is_test) in EBOs)
                {
                    if (!is_test) Debug.LogDebug($"Deleting EBO ({id})", DebuggingLevel.Verbose);
                    else Debug.LogTest($"Deleting test EBO ({id})", DebuggingLevel.Verbose);

                    GL.DeleteBuffer(id);
                }
                foreach ((int id, bool is_test) in VAOs)
                {
                    if (!is_test) Debug.LogDebug($"Deleting VAO ({id})", DebuggingLevel.Verbose);
                    else Debug.LogTest($"Deleting test VAO ({id})", DebuggingLevel.Verbose);

                    GL.DeleteVertexArray(id);
                }
                Debug.LogDebugProcessEnd(true, DebuggingLevel.Verbose);
            });
        }
	}
}