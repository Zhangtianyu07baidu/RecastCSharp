using UnityEngine;
using Recast;

public class NavController : MonoBehaviour
{
	private NavMeshAgent agent;
	public Transform Target;

	private bool isRecast;
	RaycastHit hitInfo;

	private Vector2 scrollVector2;

	// Start is called before the first frame update
	void Start()
    {
	    this.agent = this.GetComponent<NavMeshAgent>();

		this.transform.position = new Vector3(43.552830f, 9.998184f, 1.683649f);
    }

    // Update is called once per frame
    void Update()
    {
	    if (Input.GetMouseButton(0))
	    {
		    //从摄像机发出到点击坐标的射线
		    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		    
		    this.isRecast = Physics.Raycast(ray, out hitInfo);
			if (this.isRecast)
		    {
			    this.agent.SetDestination(hitInfo.point);
		    }

		    /*if (this.Target != null)
		    {
			    this.agent.SetDestination(this.Target.position);
			}*/
	    }
	}

	void OnGUI()
	{
		//if (this.isRecast)
		{
			this.scrollVector2 = GUILayout.BeginScrollView(this.scrollVector2);
			GUILayout.BeginVertical();
			GUIStyle style = new GUIStyle();
			style.fontSize = 20;
			GUI.color = Color.black;

			GUILayout.Label(this.hitInfo.point.ToString(), style);

			GUILayout.Label("IsLoadSuccess: " + this.agent.IsLoadSuccess, style);
			GUILayout.Label("Bin Path: " + this.agent.outBinPath, style);

			foreach (var log in RecastNavMesh.Logs)
			{
				GUILayout.Label(log, style);
			}

			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			
		}
	}
}
