using System;
using System.Collections.Generic;
using System.IO;
using Kernel.Lang.Extension;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


namespace Recast.Navigation
{
	public partial class NavMeshEditor
	{
		private static List<Geometry> GetGeometries(GameObject gameObject, int layerMasks)
		{
			int triangleOffset = 0;
			List<Geometry> geometries = new List<Geometry>();
			Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(false);
			foreach (Collider collider in colliders)
			{
				GameObject go = collider.gameObject;
				int mask = 1 << go.layer;
				if ((mask & layerMasks) != 0)
				{
					if (collider is BoxCollider)
					{
						BoxCollider boxCollider = collider as BoxCollider;
						if (!boxCollider.size.EqualsEx(Vector3.one))
						{
							throw new Exception("BoxCollider " + go.name + " 的Size必须是1，1，1 ; " + boxCollider.size);
						}
						MeshFilter filter = go.GetComponent<MeshFilter>();
						if (filter != null)
						{
							Geometry geom = MeshToGeom(filter, ref triangleOffset);
							geometries.Add(geom);
						}
					}
					else if (collider is CapsuleCollider)
					{
						CapsuleCollider capsuleCollider = collider as CapsuleCollider;
						if (!capsuleCollider.height.EqualsEx(2) || !capsuleCollider.radius.EqualsEx(0.5f))
						{
							throw new Exception("CapsuleCollider " + go.name + " 的Height必须是2，Radius必须是0.5");
						}
						MeshFilter mesh = go.GetComponent<MeshFilter>();
						Geometry geom = MeshToGeom(mesh, ref triangleOffset);
						geometries.Add(geom);
					}
					else if (collider is SphereCollider)
					{
						SphereCollider sphereCollider = (SphereCollider)collider;
						if (!sphereCollider.radius.EqualsEx(0.5f))
						{
							throw new Exception("SphereCollider " + go.name + " 的Radius必须是0.5");
						}
						MeshFilter mesh = go.GetComponent<MeshFilter>();
						/*if (mesh == null)
						{
							mesh = co.AddComponent<MeshFilter>();
							var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							mesh.mesh = sphere.GetComponent<MeshFilter>().mesh;
							sphere.DestroyEx();
						}
						else
						{
							Logger.Error("@美术，本场景的SphereCollider上有mesh，这是不允许的 {0}", co.name);
						}*/
						Geometry geom = MeshToGeom(mesh, ref triangleOffset);
						geometries.Add(geom);
						//Object.Destroy(mesh);
					}
					else if (collider is MeshCollider)
					{
						MeshFilter mesh = go.GetComponent<MeshFilter>();
						if (mesh != null && mesh.sharedMesh != null)
						{
							Geometry geom = MeshToGeom(mesh, ref triangleOffset);
							geometries.Add(geom);
						}
					}
				}
			}
			return geometries;
		}

		private static Geometry MeshToGeom(MeshFilter meshFilter, ref int triangleOffset)
		{
			if (meshFilter != null && meshFilter.sharedMesh != null)
			{
				Geometry geom = new Geometry();
				Mesh m = meshFilter.sharedMesh;
				geom.Name = meshFilter.gameObject.name;
				UnityEngine.Vector3[] vertices = m.vertices;
				for (int i = 0; i < vertices.Length; ++i)
				{
					vertices[i] = meshFilter.transform.TransformPoint(vertices[i]);
				}
				int[] triangles = m.triangles;
				for (int i = 0; i < triangles.Length; ++i)
				{
					triangles[i] = triangles[i] + 1 + triangleOffset;
				}
				geom.Vertices = vertices;
				geom.Triangles = triangles;
				triangleOffset += vertices.Length;
				return geom;
			}
			return null;
		}

		private static void WriteObjFile(List<Geometry> geoms, string filename)
		{
			string path = Path.GetDirectoryName(filename);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			using (StreamWriter sw = new StreamWriter(filename))
			{
				foreach (Geometry geom in geoms)
				{
					sw.Write(geom);
				}
			}
		}

		private static void ExportNavMesh(string path)
		{
			NavMeshTriangulation tmpNavMeshTriangulation = NavMesh.CalculateTriangulation();
			using (StreamWriter tmpStreamWriter = new StreamWriter(path))
			{
				//顶点
				for (int i = 0; i < tmpNavMeshTriangulation.vertices.Length; i++)
				{
					tmpStreamWriter.WriteLine("v  " + tmpNavMeshTriangulation.vertices[i].x + " " + tmpNavMeshTriangulation.vertices[i].y + " " + tmpNavMeshTriangulation.vertices[i].z);
				}

				tmpStreamWriter.WriteLine("g pPlane1");

				//索引
				for (int i = 0; i < tmpNavMeshTriangulation.indices.Length;)
				{
					tmpStreamWriter.WriteLine("f " + (tmpNavMeshTriangulation.indices[i] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 1] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 2] + 1));
					i = i + 3;
				}
			}
			Debug.Log("ExportNavMesh Success");
		}
	}
}



