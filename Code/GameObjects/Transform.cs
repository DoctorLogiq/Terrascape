using OpenTK;
using Terrascape.Rendering;

// ReSharper disable All
#nullable enable

namespace Terrascape.GameObjects
{
	public class Transform
	{
		private Vector3 _position = Vector3.Zero;
		private Vector3 _rotation = Vector3.Zero;
		private Vector3 _scale    = Vector3.One;
		private Matrix4 transformation_matrix = Matrix4.Identity;

		public Vector3 position => this._position;
		public Vector3 rotation => this._rotation;
		public Vector3 scale    => this._scale;

		public bool is_dirty { get; private set; } = true;

		public Transform(float p_uniform_scale = 1F)
		{
			this._scale.X = p_uniform_scale;
			this._scale.Y = p_uniform_scale;
			this._scale.Z = p_uniform_scale;
		}

		public void Translate(float p_x, float p_y, float p_z)
		{
			this._position.X += p_x;
			this._position.Y += p_y;
			this._position.Z += p_z;

			this.is_dirty = true;
		}

		public void Rotate(float p_pitch, float p_yaw, float p_roll)
		{
			this._rotation.X += p_pitch;
			this._rotation.Y += p_yaw;
			this._rotation.Z += p_roll;
			
			this.is_dirty = true;
		}
		
		public void Scale(float p_x, float p_y, float p_z)
		{
			this._scale.X += p_x;
			this._scale.Y += p_y;
			this._scale.Z += p_z;

			this.is_dirty = true;
		}

		public void Scale(float p_all)
		{
			Scale(p_all, p_all, p_all);
		}

		public void SetTranslation(float p_x, float p_y, float p_z)
		{
			this._position.X = p_x;
			this._position.Y = p_y;
			this._position.Z = p_z;

			this.is_dirty = true;
		}
		
		public void SetRotation(float p_pitch, float p_yaw, float p_roll)
		{
			this._rotation.X = p_pitch;
			this._rotation.Y = p_yaw;
			this._rotation.Z = p_roll;

			this.is_dirty = true;
		}
		
		public void SetScale(float p_x, float p_y, float p_z)
		{
			this._scale.X = p_x;
			this._scale.Y = p_y;
			this._scale.Z = p_z;

			this.is_dirty = true;
		}

		public void SetScale(float p_all)
		{
			SetScale(p_all, p_all, p_all);
		}

		internal Matrix4 TransformationMatrix
		{
			get
			{
				if (is_dirty)
				{
					transformation_matrix = Matrix4.Identity;

					transformation_matrix *= Matrix4.CreateScale(this._scale);
					transformation_matrix *= Matrix4.CreateRotationX(this._rotation.X);
					transformation_matrix *= Matrix4.CreateRotationY(this._rotation.Y);
					transformation_matrix *= Matrix4.CreateRotationZ(this._rotation.Z);
					transformation_matrix *= Matrix4.CreateTranslation(this._position);
				}

				return transformation_matrix;
			}
		}

		internal void SendTransformationMatrixToShader()
		{
			Shader.CurrentShader?.SetMatrix4("inModel", TransformationMatrix);
		}
	}
}