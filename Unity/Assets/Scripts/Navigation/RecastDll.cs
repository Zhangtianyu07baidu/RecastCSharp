using System;
using System.Runtime.InteropServices;

namespace Recast.Navigation
{
	public static class RecastDll
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		const string RECAST = "__Internal";
		const string PREFIX = "X_";
#else
		const string RECAST = "recast";
		const string PREFIX = "";
#endif
		public delegate void EnvLog(int category, string content);

		#region SoloRecast

		[DllImport(RECAST, EntryPoint = PREFIX + "CreateSoloRecast", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateSoloRecast();

		#endregion

		#region TileRecast

		[DllImport(RECAST, EntryPoint = PREFIX + "CreateTileRecast", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateTileRecast();

		[DllImport(RECAST, EntryPoint = PREFIX + "BuildTile", CallingConvention = CallingConvention.Cdecl)]
		public static extern void BuildTile(IntPtr tileRecast, float[] minPoint, float[] maxPoint);

		[DllImport(RECAST, EntryPoint = PREFIX + "RemoveTile", CallingConvention = CallingConvention.Cdecl)]
		public static extern void RemoveTile(IntPtr tileRecast, float[] minPoint, float[] maxPoint);

		#endregion

		#region TileCacheRecast

		[DllImport(RECAST, EntryPoint = PREFIX + "CreateTileCacheRecast", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateTileCacheRecast();

		[DllImport(RECAST, EntryPoint = PREFIX + "Update", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Update(IntPtr tileCacheRecast, float deltaTime);

		[DllImport(RECAST, EntryPoint = PREFIX + "AddCylinderObstacle", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool AddCylinderObstacle(IntPtr tileCacheRecast, float[] pos, float radius, float height, out IntPtr obstaclePtr);

		[DllImport(RECAST, EntryPoint = PREFIX + "AddBoxObstacle", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool AddBoxObstacle(IntPtr tileCacheRecast, float[] bMin, float[] bMax, out IntPtr obstaclePtr);

		[DllImport(RECAST, EntryPoint = PREFIX + "RemoveObstacle", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool RemoveObstacle(IntPtr tileCacheRecast, IntPtr obstaclePtr);

		[DllImport(RECAST, EntryPoint = PREFIX + "RemoveAllObstacles", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool RemoveAllObstacles(IntPtr tileCacheRecast);

		#endregion

		#region Common

		[DllImport(RECAST, EntryPoint = PREFIX + "SetEnvLog", CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetEnvLog(EnvLog log);

		[DllImport(RECAST, EntryPoint = PREFIX + "SetBuildParams", CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetBuildParams(IntPtr recastPtr, float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope,
			float cellSize, float cellHeight, float regionMinSize, float regionMergeSize, float edgeMaxLen,
			float edgeMaxError, float vertsPerPoly, float detailSampleDist, float detailSampleMaxError,
			int partitionType, float tileSize);

		[DllImport(RECAST, EntryPoint = PREFIX + "Release", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Release(IntPtr recastPtr);

		[DllImport(RECAST, EntryPoint = PREFIX + "LoadMeshGeometry", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool LoadMeshGeometry(IntPtr recastPtr, string objPath);

		[DllImport(RECAST, EntryPoint = PREFIX + "BuildBinary", CallingConvention = CallingConvention.Cdecl)]
		public static extern void BuildBinary(IntPtr recastPtr, string outputPath);

		[DllImport(RECAST, EntryPoint = PREFIX + "LoadMeshBin", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool LoadMeshBin(IntPtr recastPtr, string binPath);

		[DllImport(RECAST, EntryPoint = PREFIX + "GetConvexCount", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetConvexCount(IntPtr recastPtr);

		[DllImport(RECAST, EntryPoint = PREFIX + "GetConvexArray", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool GetConvexArray(IntPtr recastPtr, IntPtr p);

		[DllImport(RECAST, EntryPoint = PREFIX + "GetBuildSettings", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool GetBuildSettings(IntPtr recastPtr, out BuildSettings settings);

		[DllImport(RECAST, EntryPoint = PREFIX + "FindSmoothPath", CallingConvention = CallingConvention.Cdecl)]
		public static extern int FindSmoothPath(IntPtr recastPtr, float[] startPos, float[] endPos, float[] smoothPath, ref int nSmoothPath);

		[DllImport(RECAST, EntryPoint = PREFIX + "FindStraightPath", CallingConvention = CallingConvention.Cdecl)]
		public static extern int FindStraightPath(IntPtr recastPtr, float[] startPos, float[] endPos, float[] straightPath, ref int nstraight);

		[DllImport(RECAST, EntryPoint = PREFIX + "FindRandomPoint", CallingConvention = CallingConvention.Cdecl)]
		public static extern int FindRandomPoint(IntPtr recastPtr, float[] resultPos);

		[DllImport(RECAST, EntryPoint = PREFIX + "FindRandomPointAroundCircle", CallingConvention = CallingConvention.Cdecl)]
		public static extern int FindRandomPointAroundCircle(IntPtr recastPtr, float[] center, float radius, float[] resultPos);

		[DllImport(RECAST, EntryPoint = PREFIX + "Raycast", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Raycast(IntPtr recastPtr, float[] startPos, float[] endPos, float[] hitPos);

		[DllImport(RECAST, EntryPoint = PREFIX + "SetAreaCost", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool SetAreaCost(IntPtr recastPtr, int area, float cost);

		/// <summary>
		/// 设置flag指定区域是否在寻路中排除
		/// </summary>
		[DllImport(RECAST, EntryPoint = PREFIX + "SetFilterFlag", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool SetFilterFlag(IntPtr recastPtr, int flag, bool isIncluded);

		/// <summary>
		/// 添加划分区域的点
		/// </summary>
		[DllImport(RECAST, EntryPoint = PREFIX + "AddConvexPoint", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool AddConvexPoint(IntPtr recastPtr, float[] point);

		/// <summary>
		/// 将划分的点结合成区域areaType类型
		/// </summary>
		[DllImport(RECAST, EntryPoint = PREFIX + "MakeConvexPolygon", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool MakeConvexPolygon(IntPtr recastPtr, int areaType);

		/// <summary>
		/// 删除p位置的区域划分
		/// </summary>
		[DllImport(RECAST, EntryPoint = PREFIX + "DeleteConvexPolygon", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DeleteConvexPolygon(IntPtr recastPtr, float[] p);

		#endregion
	}

}


