using System.Runtime.InteropServices;


namespace Recast.Navigation
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ConvexVolumeData
	{
		const int MAX_CONVEXVOL_PTS = 12;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CONVEXVOL_PTS * 3)]
		public float[] Verts;

		public float Hmin;
		public float Hmax;
		public int VertsCount;
		public int Area;
	}
}
