using System.Collections;
using System.Collections.Generic;
using System.IO;
using Recast.Navigation;
using UnityEngine;

namespace Recast
{
	public class NavMeshAgent : MonoBehaviour
	{
		public float Speed = 3.5f;

		private readonly List<Vector3> pathPoints = new List<Vector3>();
		private int index;

		public string outBinPath = "";

		public bool IsLoadSuccess;


		IEnumerator MoveFile(string fileName)
		{
			//Application.streamingAssetsPath这个File的一般操作不支持，只允许WWW或AssetBundle.LoadFromFile
			string persistentDataPath = Application.persistentDataPath + "/" + fileName;
			string path = Application.streamingAssetsPath + "/" + fileName;
			WWW www = new WWW(path);
			yield return www;
			if (www.isDone)
			{
				File.WriteAllBytes(persistentDataPath, www.bytes);
				string log = persistentDataPath + " load done";
				Debug.Log(log);
				RecastNavMesh.Logs.Add(log);
			}
			yield return null;
		}

		// Start is called before the first frame update
		IEnumerator Start()
		{
			yield return this.MoveFile("dungeon.bin");
			yield return this.MoveFile("dungeon.gset");
			this.outBinPath = Path.Combine(Application.persistentDataPath, "dungeon");
			this.IsLoadSuccess = RecastNavMesh.TileCacheRecast.LoadMeshBin(this.outBinPath);
			RecastNavMesh.TileCacheRecast.SetAreaCost(PolyAreas.POLYAREA_GROUND, 1f);
			RecastNavMesh.TileCacheRecast.SetAreaCost(PolyAreas.POLYAREA_DOOR, 1f);
			RecastNavMesh.TileCacheRecast.SetAreaCost(PolyAreas.POLYAREA_GRASS, 1f);
			RecastNavMesh.TileCacheRecast.SetAreaCost(PolyAreas.POLYAREA_JUMP, 1f);
			RecastNavMesh.TileCacheRecast.SetAreaCost(PolyAreas.POLYAREA_ROAD, 1f);
			RecastNavMesh.TileCacheRecast.SetAreaCost(PolyAreas.POLYAREA_WATER, 1f);
			//RecastNavMesh.TileCacheRecast.SetFilterFlag(PolyFlags.POLYFLAGS_SWIM, true);
		}

		// Update is called once per frame
		void Update()
		{
			RecastNavMesh.TileCacheRecast.Update(Time.deltaTime);
			if (this.index < this.pathPoints.Count)
			{
				Vector3 pos = this.transform.position;
				Vector3 targetPoint = this.pathPoints[this.index];
				float moveDistance = this.Speed * Time.deltaTime;
				float restDistance = Vector3.Distance(pos, targetPoint);
				if (moveDistance >= restDistance)
				{
					this.transform.position = targetPoint;
					this.index++;
				}
				else
				{
					this.transform.position = pos + (targetPoint - pos).normalized * moveDistance;
				}
			}


		}

		public void SetDestination(Vector3 point)
		{
			Vector3 startPosition = this.transform.position;
			if (RecastNavMesh.TileCacheRecast.FindStraightPath(startPosition, point, this.pathPoints))
			{
				this.index = 0;
			}
		}

		void OnDestroy()
		{
			RecastNavMesh.Release();
		}
	}

}


