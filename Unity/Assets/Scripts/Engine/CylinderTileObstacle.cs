using System;
using UnityEngine;


namespace Recast.Tile
{

	[RequireComponent(typeof(CapsuleCollider))]
	public class CylinderTileObstacle : MonoBehaviour
	{
		private CapsuleCollider capsuleCollider;
		private IntPtr obstaclePtr;

		void Awake()
		{
			this.capsuleCollider = this.GetComponent<CapsuleCollider>();
		}

		private void OnEnable()
		{
			this.obstaclePtr = RecastNavMesh.TileCacheRecast.AddCylinderObstacle(this.transform.position, this.capsuleCollider.radius, this.capsuleCollider.height);
			if (this.obstaclePtr != IntPtr.Zero)
			{
				Debug.Log("CylinderTileObstacle Add success");
			}
		}

		private void OnDisable()
		{
			if (RecastNavMesh.TileCacheRecast.RemoveObstacle(this.obstaclePtr))
			{
				Debug.Log("CylinderTileObstacle Remove success");
			}
		}
	}

}

