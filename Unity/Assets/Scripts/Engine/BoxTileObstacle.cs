using System;
using UnityEngine;


namespace Recast.Tile
{
	[RequireComponent(typeof(BoxCollider))]
	public class BoxTileObstacle : MonoBehaviour
	{
		private BoxCollider boxCollider;
		private IntPtr obstaclePtr;

		private void Awake()
		{
			this.boxCollider = this.GetComponent<BoxCollider>();
		}

		private void OnEnable()
		{
			Bounds bounds = this.boxCollider.bounds;
			this.obstaclePtr = RecastNavMesh.TileCacheRecast.AddBoxObstacle(bounds.min, bounds.max);
			if (this.obstaclePtr != IntPtr.Zero)
			{
				Debug.Log("BoxTileObstacle Add success");
			}
		}

		private void OnDisable()
		{
			if (RecastNavMesh.TileCacheRecast.RemoveObstacle(this.obstaclePtr))
			{
				Debug.Log("BoxTileObstacle Remove success");
			}
		}

	}

}


