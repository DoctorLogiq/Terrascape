using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Terrascape.Debugging;
using Terrascape.Exceptions;
using Terrascape.Helpers;
using Terrascape.Registry;
using static Terrascape.Debugging.Indentation;
using static Terrascape.Helpers.SpecialCharacters;

#nullable enable

namespace Terrascape.Rendering
{
	public class TextureAtlas : IIdentifiable
	{
		public Identifier name { get; }
		internal readonly Texture texture;
		internal readonly double cell_size;
		internal readonly int row_column_count;
		internal readonly Dictionary<Identifier, TextureCell> cells;

		private TextureAtlas(Identifier p_name, Texture p_texture, double p_cell_size, int p_row_column_count, Dictionary<Identifier, TextureCell> p_cell_data)
		{
			this.name = p_name;
			this.texture = p_texture;
			this.cell_size = p_cell_size;
			this.row_column_count = p_row_column_count;
			this.cells = p_cell_data;
		}

		internal void Use()
		{
			this.texture.Use();
		}
		
		internal TextureCell Get(Identifier p_name)
		{
			if (this.cells.ContainsKey(p_name))
				return this.cells[p_name];

			throw new TerrascapeException($"Could not find TextureCell '{p_name}' in the texture atlas '{this.name}'");
		}
		
		internal TextureCell? GetOrNull(Identifier p_name)
		{
			if (this.cells.ContainsKey(p_name))
				return this.cells[p_name];

			Debug.LogWarning($"Could not find TextureCell '{p_name}' in the texture atlas '{this.name}'");
			return null;
		}

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		internal static TextureAtlas Build(Identifier p_name, string p_subfolder, string p_prefix = "", string p_suffix = "")
		{
			// TODO(LOGIX): Multi-thread this!
			if (!p_name.ToString().EndsWith("atlas")) p_name = $"{p_name}_texture_atlas";
			Debug.LogInfo($"Stitching texture atlas '{p_name}'", p_post: Indent);

			int texture_count = 0;
			List<(Identifier, Image<Rgba32>, int, int)> texture_data = new List<(Identifier, Image<Rgba32>, int, int)>();

			// Find all the .png files in the given asset folder
			string[] texture_files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Assets/Textures/{p_subfolder}", "*.png");
			for (int i = 0; i < texture_files.Length; ++i)
			{
				texture_files[i] = texture_files[i].Replace('\\', '/');
			}
			
			// Load each texture as an Image<Rgba32> and store it in the list
			foreach (string file in texture_files)
			{
				string identifier = $"{p_prefix}{(!string.IsNullOrEmpty(p_prefix) ? "_" : "")}{file.Substring(file.LastIndexOf('/') + 1).Replace(".png", "")}{(!string.IsNullOrEmpty(p_suffix) ? "_" : "")}{p_suffix}";
				Debug.LogDebug($"Loading '{identifier}' from '{file.Substring(file.LastIndexOf("Assets", StringComparison.Ordinal))}'", DebuggingLevel.Verbose);
				
				if (!File.Exists(file))
					throw new TerrascapeException($"Cannot load texture file '{file}'");
			
				if (!(Image.Load(file) is Image<Rgba32> image))
					throw new TerrascapeException("Failed to read texture");

				texture_data.Add((identifier, image, image.Width, image.Height));
				texture_count++;
			}
			
			Debug.LogInfo($"Loaded {texture_count} textures");

			// Calculate the cell size and make sure it's a power-of-two number. Also calculate the row count
			int cell_size = 0;
			foreach ((Identifier _, Image<Rgba32> _, int width, int height) in texture_data)
			{
				if (width > cell_size)
					cell_size = width;
				if (height > cell_size)
					cell_size = height;
			}
			
			cell_size = MathHelper.FindNextPowerOf2(cell_size);
			int row_count = (int)Math.Ceiling(Math.Sqrt(texture_count));

			// Create the atlas texture
			Image<Rgba32> atlas_texture = new Image<Rgba32>(Configuration.Default, cell_size * row_count, cell_size * row_count);
			Dictionary<Identifier, TextureCell> cell_data = new Dictionary<Identifier, TextureCell>();
			
			double d_atlas_texture_width = atlas_texture.Width;
			double d_atlas_texture_height = atlas_texture.Height;
			Debug.Assert(() => Math.Abs(d_atlas_texture_width - d_atlas_texture_height) < .1);
			double d_row_count = row_count;
			double d_cell_size = d_atlas_texture_width / d_row_count;
			double d_atlas_size_scalar = 1.0 / d_atlas_texture_width;

			// Write the pixels to the texture
			int texture_index = 0;
			for (int column = 0; column < row_count; ++column)
			{
				for (int row = 0; row < row_count; ++row)
				{
					/* Write texture pixels if we haven't passed the texture count. This is because it's not guaranteed (in fact, it's unlikely)
					 that the number of textures just happens to match the number of cells, since if there are, for example, 11 textures, there 
					 will be 16 cells, due to the fact that the texture atlas must be a square, not a rectangle (in order for cell indexing to work). */
					if (texture_index < texture_data.Count)
					{
						// Grab the texture data from the list
						Image<Rgba32> tex_data = texture_data[texture_index].Item2;
						int tex_width = texture_data[texture_index].Item3, tex_height = texture_data[texture_index].Item4;

						// Calculate UVs using double precision
						double d_column = column;
						double d_row    = row;
						double d_texture_width = tex_width;
						double d_texture_height = tex_height;
						
						double uv_y1 = d_atlas_size_scalar * (d_cell_size * d_column);
						double uv_x1 = d_atlas_size_scalar * (d_cell_size * d_row);
						double uv_x2 = uv_x1 + (d_atlas_size_scalar * d_texture_width);
						double uv_y2 = uv_y1 + (d_atlas_size_scalar * d_texture_height);
						
						Debug.Assert(() => uv_x1 >= 0.0 && uv_x1 <= 1.0);
						Debug.Assert(() => uv_x2 >= 0.0 && uv_x2 <= 1.0);
						Debug.Assert(() => uv_y1 >= 0.0 && uv_y1 <= 1.0);
						Debug.Assert(() => uv_y2 >= 0.0 && uv_y2 <= 1.0);
						
						// Create cell data
						cell_data.Add(texture_data[texture_index].Item1, new TextureCell(texture_index, texture_data[texture_index].Item1, tex_width, tex_height, 
							new UVRectangle(uv_x1, uv_y1, uv_x2, uv_y2)));
						
						// Loop through each pixel in the CELL
						for (int y = 0; y < cell_size; ++y)
						{
							for (int x = 0; x < cell_size; ++x)
							{
								// If the pixel is still within the bounds of the texture, write the texture pixels
								if (x < tex_width && y < tex_height)
								{
									atlas_texture[(row * cell_size) + x, (column * cell_size) + y] = tex_data[x, y];
								}
								// Otherwise, write transparency (or red, if it's on a cell boundary)
								else
								{
									if (x == cell_size - 1 || y == cell_size - 1)
										atlas_texture[(row * cell_size) + x, (column * cell_size) + y] = Rgba32.Red;
									else
										atlas_texture[(row * cell_size) + x, (column * cell_size) + y] = Rgba32.Transparent;
								}

								// NOTE: Red won't be written at a cell boundary if the texture is exactly the same width/height as the cell
								// NOTE: In other words we won't overwrite texture pixels at the cell boundaries 
							}	
						}
					}
					// If we have passed the texture count, then just write transparency, or red at cell boundaries
					else
					{
						for (int y = 0; y < cell_size; ++y)
						{
							for (int x = 0; x < cell_size; ++x)
							{
								if (x == cell_size - 1 || y == cell_size - 1)
									atlas_texture[(row * cell_size) + x, (column * cell_size) + y] = Rgba32.Red;
								else
									atlas_texture[(row * cell_size) + x, (column * cell_size) + y] = Rgba32.Transparent;
							}	
						}
					}

					texture_index++;
				}
			}
			
			// Output the file for examination
			// TODO(LOGIX): Determine if this should be removed
			string temp_file = $"{Directory.GetCurrentDirectory()}/Temp/{p_name}.png";
			if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/Temp/"))
				Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/Temp/");
			if (File.Exists(temp_file))
				File.Delete(temp_file);

			atlas_texture.Save(temp_file);
			
			// Create a list of bytes to store the pixel data as a continuous stream
			List<byte> pixels = new List<byte>();
			foreach (Rgba32 pixel in atlas_texture.GetPixelSpan().ToArray())
			{
				pixels.Add(pixel.R);
				pixels.Add(pixel.G);
				pixels.Add(pixel.B);
				pixels.Add(pixel.A);
			}

			// Create the actual texture
			Texture atlas_texture_final = Texture.Create($"{p_name}_texture", pixels, atlas_texture.Width, atlas_texture.Height);

			Debug.LogInfo("Complete", p_pre: Unindent);
			return new TextureAtlas(p_name, atlas_texture_final, cell_size, row_count, cell_data);
		}
	}

	internal struct TextureCell
	{
		public readonly int id;
		public readonly Identifier name;
		public readonly double width;
		public readonly double height;
		public readonly UVRectangle uvs;

		public TextureCell(int p_id, Identifier p_name, double p_width, double p_height, UVRectangle p_uvs)
		{
			this.id = p_id;
			this.name = p_name;
			this.width = p_width;
			this.height = p_height;
			this.uvs = p_uvs;
		}
	}

	public struct SizeRectangle
	{
		public readonly double x, y, width, height;

		public SizeRectangle(double p_x = 0D, double p_y = 0D, double p_width = 0D, double p_height = 0D)
		{
			this.x = p_x;
			this.y= p_y;
			this.width = p_width;
			this.height = p_height;
		}

		public override string ToString()
		{
			return $"{this.x}, {this.y}, {this.width}{Multiply}{this.height}";
		}
	}
	
	public struct UVRectangle
	{
		public readonly double x1, y1, x2, y2;

		public UVRectangle(double p_x1 = 0D, double p_y1 = 0D, double p_x2 = 0D, double p_y2 = 0D)
		{
			this.x1 = p_x1;
			this.y1 = p_y1;
			this.x2 = p_x2;
			this.y2 = p_y2;
		}

		public override string ToString()
		{
			return $"{this.x1}, {this.y1}, {this.x2}, {this.y2}";
		}
	}
}