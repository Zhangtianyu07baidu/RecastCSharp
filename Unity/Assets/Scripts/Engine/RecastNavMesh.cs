using System.Collections.Generic;
using Recast.Navigation;


public static class RecastNavMesh
{
	public static List<string> Logs = new List<string>();

	private static TileCacheRecast tileCacheRecast;

	public static TileCacheRecast TileCacheRecast
	{
		get
		{
			if (tileCacheRecast == null)
			{
				tileCacheRecast = new TileCacheRecast();
			}

			return tileCacheRecast;
		}
		private set { tileCacheRecast = value; }
	}

	static RecastNavMesh()
	{
		RecastBase.OnRecastLog = (category, content) =>
		{
			Logs.Add(content);
		};
	}

	public static void Release()
	{
		TileCacheRecast.Release();
		TileCacheRecast = null;
	}

}
