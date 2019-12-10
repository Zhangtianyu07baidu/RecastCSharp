using System.Text;
using UnityEngine;

public class Geometry
{
	public string Name;
	public Vector3[] Vertices;
	public int[] Triangles;

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(string.Format("g {0}\n", Name));
		foreach (Vector3 wv in Vertices)
		{
			sb.Append(string.Format("v {0} {1} {2}\n", wv.x, wv.y, wv.z));
		}
		sb.Append("\n");
		for (int i = 0; i < Triangles.Length; i += 3)
		{
			sb.Append(string.Format("f {0} {1} {2}\n", Triangles[i], Triangles[i + 1], Triangles[i + 2]));
		}
		sb.Append("\n");
		return sb.ToString();
	}
}
