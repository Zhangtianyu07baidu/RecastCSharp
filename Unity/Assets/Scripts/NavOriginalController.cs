using UnityEngine;
using UnityEngine.AI;

public class NavOriginalController : MonoBehaviour
{
	private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
	    this.agent = this.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
	    if (Input.GetMouseButton(0))
	    {
		    //从摄像机发出到点击坐标的射线
		    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		    RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo))
		    {
			    this.agent.SetDestination(hitInfo.point);
		    }
	    }
	}
}
