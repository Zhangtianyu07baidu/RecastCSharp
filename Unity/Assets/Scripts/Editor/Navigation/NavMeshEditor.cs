using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Recast.Navigation
{
	public enum PartitionType
	{
		PARTITION_WATERSHED = 0,
		PARTITION_MONOTONE = 1,
		PARTITION_LAYERS = 2,
	}

	public partial class NavMeshEditor : EditorWindow
	{
		private const string NAV_OBJ_FIEL_PATH = "NAV_OBJ_FIEL_PATH";
		private const string NAV_BIN_FILE_PATH = "NAV_BIN_FILE_PATH";
		private Vector2 scroll;
		private GameObject gameObject;
		private string navMeshName;
		private string navObjBasePath = "";
		private string binBasePath = "";

		private BuildSettings settings;

		[MenuItem("Editor/Navimesh Editor %#N", false, 501)]
		public static void Open()
		{
			var window = EditorWindow.GetWindow<NavMeshEditor>(false, typeof(NavMeshEditor).Name, true);
			if (window != null)
			{
				window.Show();
			}
		}

		public NavMeshEditor()
		{
			SceneView.onSceneGUIDelegate -= this.OnSecneGUI;
			SceneView.onSceneGUIDelegate += this.OnSecneGUI;

			this.settings.SetDefaultValues();

			this.OnConvexInit();
		}

		void OnEnable()
		{
			this.navMeshName = SceneManager.GetActiveScene().name;
			if (PlayerPrefs.HasKey(NAV_OBJ_FIEL_PATH))
			{
				this.navObjBasePath = PlayerPrefs.GetString(NAV_OBJ_FIEL_PATH);
			}
			if (PlayerPrefs.HasKey(NAV_BIN_FILE_PATH))
			{
				this.binBasePath = PlayerPrefs.GetString(NAV_BIN_FILE_PATH);
			}
			this.OnConvexEnable();
			this.LoadDatasFromBin();
		}

		void OnDestroy()
		{
			SceneView.onSceneGUIDelegate -= this.OnSecneGUI;
			this.OnConvexDestroy();
		}

		void OnSecneGUI(SceneView sceneView)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				Debug.DrawRay(hit.point, Vector3.up * 50, Color.red);
				//Handles.PositionHandle(hit.point, Quaternion.identity);
				Handles.Label(hit.point + Vector3.down, hit.point.ToString(), GUIStyle.none);

				if (Event.current.keyCode == KeyCode.Space)
				{
					this.curSelectData.AddPoint(hit.point);
				}
			}
			this.OnConvexSecneGUI(sceneView);
		}

		void OnGUI()
		{
			EditorGUILayout.BeginVertical("window");
			{
				this.scroll = EditorGUILayout.BeginScrollView(this.scroll);
				{
					EditorGUILayout.LabelField("Obj File Base Path: ", this.navObjBasePath);
					if (GUILayout.Button("Set Obj File Base Path"))
					{
						string result = EditorUtility.OpenFolderPanel("Obj File Path Select", this.navObjBasePath, Application.dataPath);
						if (!string.IsNullOrEmpty(result) && this.navObjBasePath != result)
						{
							this.navObjBasePath = result;
							PlayerPrefs.SetString(NAV_OBJ_FIEL_PATH, this.navObjBasePath);
						}
					}
					EditorGUILayout.LabelField("Bin Path: ", this.binBasePath);
					if (GUILayout.Button("Set Bin File Base Path"))
					{
						string result = EditorUtility.OpenFolderPanel("Bin File Path Select", this.binBasePath, Application.dataPath);
						if (!string.IsNullOrEmpty(result) && this.binBasePath != result)
						{
							this.binBasePath = result;
							PlayerPrefs.SetString(NAV_BIN_FILE_PATH, this.binBasePath);
						}
					}
					
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("请选择场景要计算导航的节点（注意是场景中的）");
					this.gameObject = (GameObject)EditorGUILayout.ObjectField(this.gameObject, typeof(GameObject), true);

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("导航名称", this.navMeshName);
						if (GUILayout.Button("Load GSet File"))
						{
							this.LoadDatasFromBin();
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginVertical("Box");
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Params Setting");
							if (GUILayout.Button("Reset"))
							{
								this.settings.SetDefaultValues();
							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();
						{
							//光栅化
							EditorGUILayout.LabelField("Agent");
							if (GUILayout.Button("Reset"))
							{
								this.ResetAgentParams();
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginVertical("Box");
						{
							this.settings.agentHeight = EditorGUILayout.Slider("Height", this.settings.agentHeight, 0.1f, 5.0f);
							this.settings.agentHeight = (float)Math.Round(this.settings.agentHeight, 1);
							this.settings.agentRadius = EditorGUILayout.Slider("Radius", this.settings.agentRadius, 0.0f, 5.0f);
							this.settings.agentRadius = (float)Math.Round(this.settings.agentRadius, 1);
							this.settings.agentMaxClimb = EditorGUILayout.Slider("Max Climb", this.settings.agentMaxClimb, 0.1f, 5.0f);
							this.settings.agentMaxClimb = (float)Math.Round(this.settings.agentMaxClimb, 1);
							this.settings.agentMaxSlope = EditorGUILayout.Slider("Max Slope", this.settings.agentMaxSlope, 0f, 90f);
							this.settings.agentMaxSlope = (float)Math.Round(this.settings.agentMaxSlope, 0);
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginHorizontal();
						{
							//光栅化
							EditorGUILayout.LabelField("Rasterization");
							if (GUILayout.Button("Reset"))
							{
								this.ResetRasterizationParams();
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginVertical("Box");
						{
							this.settings.cellSize = EditorGUILayout.Slider("Cell Size", this.settings.cellSize, 0.10f, 1.00f);
							this.settings.cellSize = (float)Math.Round(this.settings.cellSize, 2);
							this.settings.cellHeight = EditorGUILayout.Slider("Cell Height", this.settings.cellHeight, 0.10f, 1.00f);
							this.settings.cellHeight = (float)Math.Round(this.settings.cellHeight, 2);
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Region");
							if (GUILayout.Button("Reset"))
							{
								this.ResetRegionParams();
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginVertical("Box");
						{
							this.settings.regionMinSize = EditorGUILayout.Slider("Min Region Size", this.settings.regionMinSize, 0f, 150f);
							this.settings.regionMinSize = (float)Math.Round(this.settings.regionMinSize, 0);
							this.settings.regionMergeSize = EditorGUILayout.Slider("Merged Region Height", this.settings.regionMergeSize, 0f, 150f);
							this.settings.regionMergeSize = (float)Math.Round(this.settings.regionMergeSize, 0);
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginHorizontal();
						{
							//多边形区域划分
							EditorGUILayout.LabelField("Polygonization");
							if (GUILayout.Button("Reset"))
							{
								this.ResetPolygonizationParams();
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginVertical("Box");
						{
							this.settings.edgeMaxLen = EditorGUILayout.Slider("Max Edge Length", this.settings.edgeMaxLen, 0f, 50f);
							this.settings.edgeMaxLen = (float)Math.Round(this.settings.edgeMaxLen, 0);
							this.settings.edgeMaxError = EditorGUILayout.Slider("Max Edge Error", this.settings.edgeMaxError, 0.1f, 3.0f);
							this.settings.edgeMaxError = (float)Math.Round(this.settings.edgeMaxError, 1);
							this.settings.vertsPerPoly = EditorGUILayout.Slider("Verts Per Poly", this.settings.vertsPerPoly, 3f, 12f);
							this.settings.vertsPerPoly = (float)Math.Round(this.settings.vertsPerPoly, 0);
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Detail Mesh");
							if (GUILayout.Button("Reset"))
							{
								this.ResetDetailMeshParams();
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginVertical("Box");
						{
							this.settings.detailSampleDist = EditorGUILayout.Slider("Sample Distance", this.settings.detailSampleDist, 0f, 16f);
							this.settings.detailSampleDist = (float)Math.Round(this.settings.detailSampleDist, 0);
							this.settings.detailSampleMaxError = EditorGUILayout.Slider("Max Sample Error", this.settings.detailSampleMaxError, 0f, 16f);
							this.settings.detailSampleMaxError = (float)Math.Round(this.settings.detailSampleMaxError, 0);
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Tiling");
							if (GUILayout.Button("Reset"))
							{
								this.ResetTilingParams();
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginVertical("Box");
						{
							this.settings.tileSize = EditorGUILayout.Slider("TileSize", this.settings.tileSize, 16f, 128f);
							this.settings.tileSize = (float)Math.Round(this.settings.tileSize, 0);
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Partitioning");
							if (GUILayout.Button("Reset"))
							{
								this.ResetPartitioningParams();
							}
						}
						EditorGUILayout.EndHorizontal();
						PartitionType type = (PartitionType)this.settings.partitionType;
						type = (PartitionType)EditorGUILayout.EnumPopup(type);
						this.settings.partitionType = (int)type;
					}
					EditorGUILayout.EndVertical();

					try
					{
						if (GUILayout.Button("生成 Obj 文件"))
						{

							if (string.IsNullOrEmpty(this.navMeshName))
							{
								ShowNotification(new GUIContent("不能使用空的导航文件名"));
								return;
							}

							if (this.gameObject == null)
							{
								ShowNotification(new GUIContent("请选择场景要计算导航的节点"));
								return;
							}

							string navObjPath = Path.Combine(this.navObjBasePath, $"{this.navMeshName.Trim()}.obj");
							this.BuildObjFile(navObjPath);
						}
						EditorGUILayout.Space();

						//多边形划分区域
						this.OnConvexGUI();
						if (GUILayout.Button("生成BIn文件"))
						{
							string navObjPath = Path.Combine(this.navObjBasePath, $"{this.navMeshName.Trim()}.obj");
							string binPath = Path.Combine(this.binBasePath, this.navMeshName.Trim());
							if (File.Exists(navObjPath))
							{
								this.BuildBinary(navObjPath, binPath, this.ConvexDatas);
							}
							else
							{
								ShowNotification(new GUIContent("未找到Obj文件在目录：" + navObjPath));
							}
						}
					}
					catch (Exception e)
					{
						EditorUtility.DisplayDialog("错误", e.ToString(), "我知道了");
					}
				}
				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.EndVertical();
		}

		private void ResetAgentParams()
		{
			this.settings.agentHeight = 2.0f;
			this.settings.agentRadius = 0.5f;
			this.settings.agentMaxClimb = 0.4f;
			this.settings.agentMaxSlope = 45f;
		}

		private void ResetRasterizationParams()
		{
			this.settings.cellSize = 0.3f;
			this.settings.cellHeight = 0.2f;
		}

		private void ResetRegionParams()
		{
			this.settings.regionMinSize = 8f;
			this.settings.regionMergeSize = 20f;
		}

		private void ResetPolygonizationParams()
		{
			this.settings.edgeMaxLen = 12f;
			this.settings.edgeMaxError = 1.3f;
			this.settings.vertsPerPoly = 6f;
		}

		private void ResetDetailMeshParams()
		{
			this.settings.detailSampleDist = 6f;
			this.settings.detailSampleMaxError = 1f;
		}

		private void ResetTilingParams()
		{
			this.settings.tileSize = 48f;
		}

		private void ResetPartitioningParams()
		{
			this.settings.partitionType = (int)PartitionType.PARTITION_WATERSHED;
		}

		private void LoadDatasFromBin()
		{
			string binPath = Path.Combine(this.binBasePath, this.navMeshName.Trim());
			if (File.Exists(binPath + ".bin"))
			{
				TileCacheRecast recast = new TileCacheRecast();
				recast.LoadMeshBin(binPath);
				//加载配置数据
				this.LoadBuildSettings(recast);
				//加载顶点区域数据
				this.LoadConvexDatasFromBin(recast);

				recast.Release();
			}
			else
			{
				Debug.LogWarning($"Can't load bin file by " + binPath);
			}
		}

		private void LoadBuildSettings(RecastBase recast)
		{
			recast.GetBuildSettings(out this.settings);
		}

		private void BuildObjFile(string objPath)
		{
			List<Geometry> geometries = GetGeometries(this.gameObject, -1);
			WriteObjFile(geometries, objPath);
			//ExportNavMesh(objPath);
			Debug.Log("Nav Obj File Export Success");
			Debug.Log(objPath);
		}

		private void BuildBinary(string objPath, string binPath, List<ConvexVolume> convexVolumes)
		{
			TileCacheRecast recast = new TileCacheRecast();

			//Set Building Params
			recast.SetBuildingParams(this.settings.agentHeight, this.settings.agentRadius, 
				this.settings.agentMaxClimb, this.settings.agentMaxSlope,
				this.settings.cellSize, this.settings.cellHeight, this.settings.regionMinSize, 
				this.settings.regionMergeSize, this.settings.edgeMaxLen, this.settings.edgeMaxError,
				this.settings.vertsPerPoly, this.settings.detailSampleDist, this.settings.detailSampleMaxError, 
				this.settings.partitionType, this.settings.tileSize);

			if (recast.LoadGeometry(objPath))
			{
				//添加特殊多边形区域
				foreach (var data in convexVolumes)
				{
					if (data.Verts.Count > 2)
					{
						//顶点数必须大于两个
						foreach (var p in data.Verts)
						{
							recast.AddConvexPoint(p);
						}
						if (recast.MakeConvexPolygon(data.AreaType))
						{
							Debug.Log($"{data.AreaType.ToString()}: 添加成功");
						}
					}
				}
				
				recast.Build(binPath);
				Debug.Log("Nav Bin File Build Success!");
				Debug.Log(binPath);
			}
			else
			{
				Debug.LogError("Parse Obj File Failed at " + objPath);
			}
			recast.Release();
		}
	}
}


