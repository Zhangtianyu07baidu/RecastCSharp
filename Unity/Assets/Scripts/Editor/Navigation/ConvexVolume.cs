using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Recast.Navigation
{
	public class ConvexVolume
	{
		public PolyAreas AreaType;

		public List<Vector3> Verts = new List<Vector3>();
		private bool isCreatedFromBin;
		private const int MAX_PTS = 12;
		private float[] pointArray = new float[MAX_PTS * 3];
		private int pointCount;
		private readonly int[] hullArray = new int[MAX_PTS];
		private int hullCount;
		private float boxHeight = 6.0f;
		private float boxDescent = 1.0f;

		public ConvexVolume Parse(ConvexVolumeData data)
		{
			for (int i = 0; i < data.VertsCount; i++)
			{
				int index = i * 3;
				Vector3 point = new Vector3(data.Verts[index], data.Verts[index + 1], data.Verts[index + 2]);
				this.Verts.Add(point);
			}

			this.AreaType = (PolyAreas)data.Area;
			this.isCreatedFromBin = true;
			return this;
		}

		public void OnSecneGUI()
		{
			if (this.isCreatedFromBin)
			{
				this.DrawByBinary();
			}
			else
			{
				this.DrawByRuntime();
			}
		}

		/// <summary>
		/// 根据二进制文件绘制
		/// </summary>
		private void DrawByBinary()
		{
			int vertsCount = this.Verts.Count;
			for (int i = 0, j = vertsCount - 1; i < vertsCount; j = i++)
			{
				Vector3 vi = this.Verts[j];
				Vector3 vj = this.Verts[i];
				Handles.color = Color.red;
				Handles.DrawLine(new Vector3(vj[0], vj[1], vj[2]), new Vector3(vi[0], vi[1], vi[2]));
			}
		}

		/// <summary>
		/// 根据编辑绘制
		/// </summary>
		private void DrawByRuntime()
		{
			// Find height extent of the shape.
			float minh = float.MaxValue, maxh = 0;
			for (int i = 0; i < this.pointCount; ++i)
				minh = RcMin(minh, this.pointArray[i * 3 + 1]);
			minh -= this.boxDescent;
			maxh = minh + this.boxHeight;

			for (int i = 0, j = this.hullCount - 1; i < this.hullCount; j = i++)
			{
				int viIndex = this.hullArray[j] * 3;
				Vector3 vi = new Vector3(this.pointArray[viIndex], this.pointArray[viIndex + 1], this.pointArray[viIndex + 2]);
				int vjIndex = this.hullArray[i] * 3;
				Vector3 vj = new Vector3(this.pointArray[vjIndex], this.pointArray[vjIndex + 1], this.pointArray[vjIndex + 2]);

				//this.pointList.Add(new Vector3(vj[0], minh, vj[2]));
				//this.pointList.Add(new Vector3(vi[0], minh, vi[2]));
				//下
				//Handles.DrawLine(new Vector3(vj[0], minh, vj[2]), new Vector3(vi[0], minh, vi[2]));
				//this.pointList.Add(new Vector3(vj[0], maxh, vj[2]));
				//this.pointList.Add(new Vector3(vi[0], maxh, vi[2]));
				//上
				//Handles.DrawLine(new Vector3(vj[0], maxh, vj[2]), new Vector3(vi[0], maxh, vi[2]));
				//this.pointList.Add(new Vector3(vj[0], minh, vj[2]));
				//this.pointList.Add(new Vector3(vj[0], maxh, vj[2]));
				//中
				//Handles.DrawLine(new Vector3(vj[0], minh, vj[2]), new Vector3(vj[0], maxh, vj[2]));
				Handles.color = Color.red;
				Handles.DrawLine(new Vector3(vj[0], vj[1], vj[2]), new Vector3(vi[0], vi[1], vi[2]));
			}
		}

		public void AddPoint(Vector3 vector3)
		{
			float[] point = new float[3] {vector3.x, vector3.y, vector3.z};
			if (this.pointCount > 0 && this.RcVdistSqr(point, 0, this.pointArray, (this.pointCount - 1) * 3) < this.RcSqr(0.2f))
			{
				// If clicked on that last pt, create the shape.

			}
			else
			{
				// Add new point 
				if (this.pointCount < MAX_PTS)
				{
					this.RcVcopy(this.pointArray, this.pointCount * 3, point, 0);
					this.pointCount++;
					// Update hull.
					if (this.pointCount > 1)
					{
						this.hullCount = this.Convexhull(this.pointArray, this.pointCount, this.hullArray);
					}
					else
					{
						this.hullCount = 0;
					}
				}
			}
		}

		private float RcVdistSqr(float[] v1, int v1Index, float[] v2, int v2Index)
		{
			float dx = v2[0 + v2Index] - v1[0 + v1Index];
			float dy = v2[1 + v2Index] - v1[1 + v1Index];
			float dz = v2[2 + v2Index] - v1[2 + v1Index];
			return dx * dx + dy * dy + dz * dz;
		}

		private float RcSqr(float a)
		{
			return a * a;
		}

		private void RcVcopy(float[] dest, int destIndex, float[] v, int vIndex)
		{
			dest[0 + destIndex] = v[0 + vIndex];
			dest[1 + destIndex] = v[1 + vIndex];
			dest[2 + destIndex] = v[2 + vIndex];
		}

		private int Convexhull(float[] points, int pointNum, int[] hulls)
		{
			// Find lower-leftmost point.
			int hull = 0;
			for (int i = 1; i < pointNum; ++i)
				if (this.Cmppt(points, i * 3, points, hull * 3))
					hull = i;

			// Gift wrap hull.
			int endpt = 0;
			int index = 0;
			do
			{
				hulls[index++] = hull;
				endpt = 0;
				for (int j = 1; j < pointNum; ++j)
				{
					if (hull == endpt || this.Left(points, hull * 3, points, endpt * 3, points, j * 3))
						endpt = j;
				}
				hull = endpt;

			} while (endpt != hulls[0]);

			return index;
		}

		// Returns true if 'a' is more lower-left than 'b'.
		private bool Cmppt(float[] a, int aIndex, float[] b, int bIndex)
		{
			if (a[aIndex] < b[bIndex]) return true;
			if (a[aIndex] > b[bIndex]) return false;
			if (a[aIndex + 2] < b[bIndex + 2]) return true;
			if (a[aIndex + 2] > b[bIndex + 2]) return false;
			return false;
		}

		// Returns true if 'c' is left of line 'a'-'b'.
		private bool Left(float[] a, int aIndex, float[] b, int bIndex, float[] c, int cIndex)
		{
			float u1 = b[bIndex] - a[aIndex];
			float v1 = b[bIndex + 2] - a[aIndex + 2];
			float u2 = c[cIndex] - a[aIndex];
			float v2 = c[cIndex + 2] - a[aIndex + 2];
			return u1 * v2 - v1 * u2 < 0;
		}

		private float RcMin(float a, float b)
		{
			return a < b ? a : b;
		}
	}

}


