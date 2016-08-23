﻿﻿#if UNITY
using UnityEngine;

namespace Lidgren.Network
{
	public partial class NetBuffer
	{
		public void Write(Vector2 vector)
		{
			Write(vector.x);
			Write(vector.y);
		}

		public Vector2 ReadVector2()
		{
			return new Vector2(
				x: ReadSingle(),
				y: ReadSingle());
		}

		public NetBuffer Write(Vector3 vector)
		{
			Write(vector.x);
			Write(vector.y);
			Write(vector.z);
			return this;
		}

		public Vector3 ReadVector3()
		{
			return new Vector3(
				x: ReadSingle(),
				y: ReadSingle(),
				z: ReadSingle());
		}

		public NetBuffer Write(Vector4 vector)
		{
			Write(vector.x);
			Write(vector.y);
			Write(vector.z);
			Write(vector.w);
			return this;
		}

		public Vector4 ReadVector4()
		{
			return new Vector4(
				x: ReadSingle(),
				y: ReadSingle(),
				z: ReadSingle(),
				w: ReadSingle());
		}

		public NetBuffer Write(Quaternion quaternion)
		{
			Write(quaternion.x);
			Write(quaternion.y);
			Write(quaternion.z);
			Write(quaternion.w);
			return this;
		}

		public Quaternion ReadQuaternion()
		{
			return new Quaternion(
				x: ReadFloat(),
				y: ReadFloat(),
				z: ReadFloat(),
				w: ReadFloat());
		}

		public NetBuffer WriteRgbColor(Color32 color)
		{
			Write(color.r);
			Write(color.g);
			Write(color.b);
			return this;
		}

		public Color32 ReadRgbColor()
		{
			return new Color32(
				r: ReadByte(),
				g: ReadByte(),
				b: ReadByte(),
				a: byte.MaxValue);
		}

		public NetBuffer WriteRgbaColor(Color32 color)
		{
			Write(color.r);
			Write(color.g);
			Write(color.b);
			Write(color.a);
			return this;
		}

		public Color32 ReadRgbaColor()
		{
			return new Color32(
				r: ReadByte(),
				g: ReadByte(),
				b: ReadByte(),
				a: ReadByte());
		}

		public NetBuffer Write(Ray ray)
		{
			Write(ray.direction);
			Write(ray.origin);
			return this;
		}

		public Ray ReadRay()
		{
			return new Ray(
				direction: ReadVector3(),
				origin: ReadVector3());
		}

		public NetBuffer Write(Plane plane)
		{
			Write(plane.normal);
			Write(plane.distance);
			return this;
		}

		public Plane ReadPlane()
		{
			return new Plane(
				inNormal: ReadVector3(),
				d: ReadSingle());
		}

		public NetBuffer Write(Matrix4x4 matrix)
		{
			Write(matrix.m00);
			Write(matrix.m01);
			Write(matrix.m02);
			Write(matrix.m03);
			Write(matrix.m10);
			Write(matrix.m11);
			Write(matrix.m12);
			Write(matrix.m13);
			Write(matrix.m20);
			Write(matrix.m21);
			Write(matrix.m22);
			Write(matrix.m23);
			Write(matrix.m30);
			Write(matrix.m31);
			Write(matrix.m32);
			Write(matrix.m33);
			return this;
		}

		public Matrix4x4 ReadMatrix4X4()
		{
			return new Matrix4x4
			{
				m00 = ReadSingle(),
				m01 = ReadSingle(),
				m02 = ReadSingle(),
				m03 = ReadSingle(),
				m10 = ReadSingle(),
				m11 = ReadSingle(),
				m12 = ReadSingle(),
				m13 = ReadSingle(),
				m20 = ReadSingle(),
				m21 = ReadSingle(),
				m22 = ReadSingle(),
				m23 = ReadSingle(),
				m30 = ReadSingle(),
				m31 = ReadSingle(),
				m32 = ReadSingle(),
				m33 = ReadSingle()
			};
		}

		public NetBuffer Write(Rect rect)
		{
			Write(rect.xMin);
			Write(rect.yMin);
			Write(rect.width);
			Write(rect.height);
			return this;
		}

		public Rect ReadRect()
		{
			return new Rect(
				x: ReadSingle(),
				y: ReadSingle(),
				width: ReadSingle(),
				height: ReadSingle());
		}
	}
}
#endif
