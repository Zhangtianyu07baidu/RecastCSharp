using System;
using UnityEngine;


namespace Recast.Navigation
{
	public class TileCacheRecast : RecastBase
	{
		public TileCacheRecast()
		{
			this.recastPtr = RecastDll.CreateTileCacheRecast();
		}

		public void Update(float deltaTime)
		{
			RecastDll.Update(this.recastPtr, deltaTime);
		}

		/// <summary>
		/// 添加圆柱形障碍
		/// </summary>
		/// <param name="pos">y轴要贴近地面</param>
		public IntPtr AddCylinderObstacle(Vector3 pos, float radius, float height)
		{
			this.Vector3ToArray(ref pos, this.startPos);
			IntPtr obstaclePtr;
			if (RecastDll.AddCylinderObstacle(this.recastPtr, this.startPos, radius, height, out obstaclePtr))
			{
				return obstaclePtr;
			}

			return IntPtr.Zero;
		}

		/// <summary>
		/// 添加矩形障碍
		/// </summary>
		public IntPtr AddBoxObstacle(Vector3 bMin, Vector3 bMax)
		{
			this.Vector3ToArray(ref bMin, this.startPos);
			this.Vector3ToArray(ref bMax, this.endPos);
			IntPtr obstaclePtr;
			if (RecastDll.AddBoxObstacle(this.recastPtr, this.startPos, this.endPos, out obstaclePtr))
			{
				return obstaclePtr;
			}

			return IntPtr.Zero;
		}

		public bool RemoveObstacle(IntPtr obstaclePtr)
		{
			return RecastDll.RemoveObstacle(this.recastPtr, obstaclePtr);
		}

		public void RemoveAllObstacles()
		{
			RecastDll.RemoveAllObstacles(this.recastPtr);
		}
	}

}


