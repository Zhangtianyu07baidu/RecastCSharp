using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Recast.Navigation
{
	[Flags]
	public enum NavimeshStatus
	{
		DT_FAILURE = 1 << 31, // Operation failed.
		DT_SUCCESS = 1 << 30, // Operation succeed.
		DT_IN_PROGRESS = 1 << 29, // Operation still in progress.
		// Detail information for status.
		DT_WRONG_MAGIC = 1 << 0, // Input data is not recognized.
		DT_WRONG_VERSION = 1 << 1, // Input data is in wrong version.
		DT_OUT_OF_MEMORY = 1 << 2, // Operation ran out of memory.
		DT_INVALID_PARAM = 1 << 3, // An input parameter was invalid.
		DT_BUFFER_TOO_SMALL = 1 << 4, // Result buffer for the query was too small to store all results.
		DT_OUT_OF_NODES = 1 << 5, // Query ran out of nodes during search.
		DT_PARTIAL_RESULT = 1 << 6 // Query did not reach the end location, returning best guess. 
	}

	[Flags]
	public enum NavimeshCategory
	{
		PROGRESS = 1,
		WARNING = 2,
		ERROR = 3,
	}

	[Flags]
	public enum PolyAreas
	{
		POLYAREA_GROUND = 0,
		POLYAREA_WATER = 1,
		POLYAREA_ROAD = 2,
		POLYAREA_DOOR = 3,
		POLYAREA_GRASS = 4,
		POLYAREA_JUMP = 5,
	}

	[Flags]
	public enum PolyFlags
	{
		POLYFLAGS_WALK = 0x01,       // Ability to walk (ground, grass, road)
		POLYFLAGS_SWIM = 0x02,       // Ability to swim (water).
		POLYFLAGS_DOOR = 0x04,       // Ability to move through doors.
		POLYFLAGS_JUMP = 0x08,       // Ability to jump.
		POLYFLAGS_DISABLED = 0x10,       // Disabled polygon
		POLYFLAGS_ALL = 0xffff   // All abilities.
	}

	public abstract class RecastBase
	{
		private const int MAX_POLYS = 256;
		private const int MAX_SMOOTH = 2048;
		public delegate void RecastLog(NavimeshCategory category, string content);
		public static RecastLog OnRecastLog;
		private static readonly object LogLockObj = new object();
		private static readonly RecastDll.EnvLog Log = ((category, content) =>
		{
			//may be used at multi threads
			lock (LogLockObj)
			{
				OnRecastLog?.Invoke((NavimeshCategory)category, content);
			}
		});

		protected readonly float[] startPos = new float[3];
		protected readonly float[] endPos = new float[3];
		protected readonly float[] resultPos = new float[3];
		protected readonly float[] straightPath = new float[MAX_POLYS * 3];
		protected readonly float[] smoothPath = new float[MAX_SMOOTH * 3];
		protected int smoothCount;
		protected int straigntCount;

		protected IntPtr recastPtr;

		static RecastBase()
		{
			RecastDll.SetEnvLog(Log);
		}

		protected RecastBase()
		{
			
		}

		public bool LoadMeshBin(string binPath)
		{
			return RecastDll.LoadMeshBin(this.recastPtr, binPath);
		}

		public void SetFilterFlag(PolyFlags flag, bool isIncluded)
		{
			RecastDll.SetFilterFlag(this.recastPtr, (int)flag, isIncluded);
		}

		public void SetAreaCost(PolyAreas area, float cost)
		{
			RecastDll.SetAreaCost(this.recastPtr, (int)area, cost);
		}

		/// <summary>
		/// 路径点多，路径平滑
		/// </summary>
		public bool FindSmoothPath(Vector3 startPos, Vector3 endPos, List<Vector3> path)
		{
			//startPos.x = -startPos.x;
			//endPos.x = -endPos.x;
			this.Vector3ToArray(ref startPos, this.startPos);
			this.Vector3ToArray(ref endPos, this.endPos);
			NavimeshStatus status = (NavimeshStatus)RecastDll.FindSmoothPath(this.recastPtr, this.startPos, this.endPos, this.smoothPath, ref this.smoothCount);
			if (this.IsSuccessStatus(status))
			{
				path.Clear();
				int count = this.smoothCount * 3;
				Debug.Assert(this.smoothPath.Length >= count);
				for (int i = 0; i < count; i += 3)
				{
					Vector3 point = new Vector3(this.smoothPath[i], this.smoothPath[i + 1], this.smoothPath[i + 2]);
					//point.x = -point.x;
					path.Add(point);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// 路径点少，无平滑
		/// </summary>
		public bool FindStraightPath(Vector3 startPos, Vector3 endPos, List<Vector3> path)
		{
			this.Vector3ToArray(ref startPos, this.startPos);
			this.Vector3ToArray(ref endPos, this.endPos);
			NavimeshStatus status = (NavimeshStatus)RecastDll.FindStraightPath(this.recastPtr, this.startPos, this.endPos, this.straightPath, ref this.straigntCount);
			if (this.IsSuccessStatus(status))
			{
				path.Clear();
				int count = this.straigntCount * 3;
				Debug.Assert(this.straightPath.Length >= count);
				for (int i = 0; i < count; i += 3)
				{
					Vector3 point = new Vector3(this.straightPath[i], this.straightPath[i + 1], this.straightPath[i + 2]);
					path.Add(point);
				}
				return true;
			}
			return false;
		}

		public bool FindRandomPoint(ref Vector3 resultPoint)
		{
			NavimeshStatus status = (NavimeshStatus)RecastDll.FindRandomPoint(this.recastPtr, this.resultPos);
			if (this.IsSuccessStatus(status))
			{
				resultPoint.Set(this.resultPos[0], this.resultPos[1], this.resultPos[2]);
				return true;
			}

			return false;
		}

		public bool FindRandomPointAroundCircle(ref Vector3 center, float radius, ref Vector3 resultPoint)
		{
			this.Vector3ToArray(ref center, this.startPos);
			NavimeshStatus status = (NavimeshStatus)RecastDll.FindRandomPointAroundCircle(this.recastPtr, this.startPos, radius, this.resultPos);
			if (this.IsSuccessStatus(status))
			{
				resultPoint.Set(this.resultPos[0], this.resultPos[1], this.resultPos[2]);
				return true;
			}

			return false;
		}

		public bool Raycast(ref Vector3 startPoint, ref Vector3 endPoint, ref Vector3 hitPoint)
		{
			this.Vector3ToArray(ref startPoint, this.startPos);
			this.Vector3ToArray(ref endPoint, this.endPos);
			NavimeshStatus status = (NavimeshStatus)RecastDll.Raycast(this.recastPtr, this.startPos, this.endPos, this.resultPos);
			if (this.IsSuccessStatus(status))
			{
				hitPoint.Set(this.resultPos[0], this.resultPos[1], this.resultPos[2]);
				return true;
			}

			return false;
		}

#if UNITY_EDITOR

		public ConvexVolumeData[] GetVolumeDatas()
		{
			int length = RecastDll.GetConvexCount(this.recastPtr);
			var array = new ConvexVolumeData[length];
			var size = Marshal.SizeOf(typeof(ConvexVolumeData)) * length;
			var ptr = Marshal.AllocHGlobal(size);
			RecastDll.GetConvexArray(this.recastPtr, ptr);
			for (var i = 0; i < length; i++)
			{
				var p = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(ConvexVolumeData)) * i);
				array[i] = (ConvexVolumeData)Marshal.PtrToStructure(p, typeof(ConvexVolumeData));
			}
			Marshal.FreeHGlobal(ptr); // 释放内存
			return array;
		}

		public bool LoadGeometry(string inObjPath)
		{
			return RecastDll.LoadMeshGeometry(this.recastPtr, inObjPath);
		}

		public void SetBuildingParams(float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope,
			float cellSize, float cellHeight, float regionMinSize, float regionMergeSize, float edgeMaxLen,
			float edgeMaxError, float vertsPerPoly, float detailSampleDist, float detailSampleMaxError,
			int partitionType, float tileSize)
		{
			RecastDll.SetBuildParams(this.recastPtr, agentHeight, agentRadius, agentMaxClimb, agentMaxSlope, 
				cellSize, cellHeight, regionMinSize, regionMergeSize, edgeMaxLen,
				edgeMaxError, vertsPerPoly, detailSampleDist, detailSampleMaxError,
				partitionType, tileSize);
		}

		public void Build(string outBinPath)
		{
			RecastDll.BuildBinary(this.recastPtr, outBinPath);
		}

		public void AddConvexPoint(Vector3 point)
		{
			//point.x = -point.x;
			this.Vector3ToArray(ref point, this.startPos);
			RecastDll.AddConvexPoint(this.recastPtr, this.startPos);
		}

		public bool MakeConvexPolygon(PolyAreas areaType)
		{
			return RecastDll.MakeConvexPolygon(this.recastPtr, (int)areaType);
		}

		public bool DeleteConvexPolygon(Vector3 point)
		{
			//point.x = -point.x;
			this.Vector3ToArray(ref point, this.startPos);
			return RecastDll.DeleteConvexPolygon(this.recastPtr, this.startPos);
		}

		public void GetBuildSettings(out BuildSettings settings)
		{
			RecastDll.GetBuildSettings(this.recastPtr, out settings);
		}
#endif

		public virtual void Release()
		{
			RecastDll.Release(this.recastPtr);
		}

		protected void Vector3ToArray(ref Vector3 srcVector3, float[] targetArray)
		{
			targetArray[0] = srcVector3.x;
			targetArray[1] = srcVector3.y;
			targetArray[2] = srcVector3.z;
		}

		private bool IsSuccessStatus(NavimeshStatus status)
		{
			return (status & NavimeshStatus.DT_SUCCESS) != 0;
		}
	}

}


