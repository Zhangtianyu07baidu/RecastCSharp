

namespace Recast.Navigation
{
	/// <summary>
	/// 基本寻路方式
	/// </summary>
	public class SoloRecast : RecastBase
	{
		public SoloRecast()
		{
			this.recastPtr = RecastDll.CreateSoloRecast();
		}
	}
}
