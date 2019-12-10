

using UnityEngine;

namespace Recast.Navigation
{
	/// <summary>
	/// 瓦片寻路方式
	/// </summary>
	public class TileRecast : RecastBase
	{

		public TileRecast()
		{
			this.recastPtr = RecastDll.CreateTileRecast();
		}

		public void BuildTile(Vector3 minPoint, Vector3 maxPoint)
		{
			this.Vector3ToArray(ref minPoint, this.startPos);
			this.Vector3ToArray(ref maxPoint, this.endPos);
			RecastDll.BuildTile(this.recastPtr, this.startPos, this.endPos);
		}

		public void RemoveTile(Vector3 minPoint, Vector3 maxPoint)
		{
			this.Vector3ToArray(ref minPoint, this.startPos);
			this.Vector3ToArray(ref maxPoint, this.endPos);
			RecastDll.RemoveTile(this.recastPtr, this.startPos, this.endPos);
		}
	}
}
