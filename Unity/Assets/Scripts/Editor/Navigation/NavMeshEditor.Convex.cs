using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Recast.Navigation
{
	public partial class NavMeshEditor
	{
		private List<ConvexVolume> convexDatas;
		private List<ConvexVolume> ConvexDatas
		{
			get
			{
				if (this.convexDatas == null)
				{
					this.convexDatas = new List<ConvexVolume>();
				}
				return this.convexDatas;
			}
		}

		private bool isConvexFold = false;
		private ConvexVolume curSelectData;

		private void OnConvexInit()
		{
			
		}

		private void OnConvexEnable()
		{
		}

		private void OnConvexSecneGUI(SceneView sceneView)
		{
			foreach (var convexItem in this.ConvexDatas)
			{
				convexItem.OnSecneGUI();
			}
		}

		private void OnConvexGUI()
		{
			this.isConvexFold = EditorGUILayout.Foldout(this.isConvexFold, "Convex Volumes");
			if(this.isConvexFold)
			{
				if (!Application.isPlaying)
				{
					ShowNotification(new GUIContent("请播放，不播放时不能编辑多边形区域"));
					return;
				}

				for (int i = 0; i < this.ConvexDatas.Count; i++)
				{
					ConvexVolume convexData = this.ConvexDatas[i];
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField($"{i + 1}: {convexData.AreaType}");
						convexData.AreaType = (PolyAreas)EditorGUILayout.EnumPopup("", convexData.AreaType, GUILayout.MaxWidth(120));

						if (GUILayout.Button("移除"))
						{
							if (this.curSelectData == convexData)
							{
								this.curSelectData = null;
							}
							this.ConvexDatas.RemoveAt(i);
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				if (GUILayout.Button("添加区域"))
				{
					this.curSelectData = new ConvexVolume();
					this.ConvexDatas.Add(this.curSelectData);
				}
			}
		}

		private void OnConvexDestroy()
		{
			
		}

		private void LoadConvexDatasFromBin(RecastBase recast)
		{
			this.ConvexDatas.Clear();
			this.curSelectData = null;
			ConvexVolumeData[] datas = recast.GetVolumeDatas();
			foreach (var volumeData in datas)
			{
				this.ConvexDatas.Add(new ConvexVolume().Parse(volumeData));
			}
		}
	}

}


