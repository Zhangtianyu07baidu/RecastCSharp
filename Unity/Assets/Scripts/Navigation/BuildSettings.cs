
using System.Runtime.InteropServices;

namespace Recast.Navigation
{
	[StructLayout(LayoutKind.Sequential)]
	public struct BuildSettings
	{
		// Cell size in world units
		public float cellSize;
		// Cell height in world units
		public float cellHeight;
		// Agent height in world units
		public float agentHeight;
		// Agent radius in world units
		public float agentRadius;
		// Agent max climb in world units
		public float agentMaxClimb;
		// Agent max slope in degrees
		public float agentMaxSlope;
		// Region minimum size in voxels.
		// regionMinSize = sqrt(regionMinArea)
		public float regionMinSize;
		// Region merge size in voxels.
		// regionMergeSize = sqrt(regionMergeArea)
		public float regionMergeSize;
		// Edge max length in world units
		public float edgeMaxLen;
		// Edge max error in voxels
		public float edgeMaxError;
		public float vertsPerPoly;
		// Detail sample distance in voxels
		public float detailSampleDist;
		// Detail sample max error in voxel heights.
		public float detailSampleMaxError;
		// Partition type, see SamplePartitionType
		public int partitionType;
		// Bounds of the area to mesh
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public float[] navMeshBMin;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public float[] navMeshBMax;
		// Size of the tiles in voxels
		public float tileSize;

		public void SetDefaultValues()
		{
			this.cellSize = 0.3f;
			this.cellHeight = 0.2f;
			this.agentHeight = 2f;
			this.agentRadius = 0.5f;
			this.agentMaxClimb = 0.4f;
			this.agentMaxSlope = 45f;
			this.regionMinSize = 8f;
			this.regionMergeSize = 20f;

			this.edgeMaxLen = 12f;
			this.edgeMaxError = 1.3f;
			this.vertsPerPoly = 6f;

			this.detailSampleDist = 6f;
			this.detailSampleMaxError = 1f;

			this.partitionType = 0;

			this.tileSize = 48f;
		}
	}
}
