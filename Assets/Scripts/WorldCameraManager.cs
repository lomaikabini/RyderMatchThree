using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldCameraManager : MonoBehaviour {

	public Transform worldCamera;
	public CurvySpline spline;
	public List<CurvySplineSegment> endOfPathList;
	public static WorldCameraManager instance;
	private List<List<Vector3>> path = new List<List<Vector3>>();
	private int pathPart = 0;
	private float cameraSpeed = 200f;

	void Awake()
	{
		instance = this;
	}

	void Update()
	{
		if(path.Count == 0)
		{
			List<Vector3> routePathSegment = new List<Vector3>();
			foreach(CurvySplineSegment cr in spline.Segments)
			{
				Vector3[] apr = cr.Approximation;
				for(int i =0;i<apr.Length;i++)
				{
					routePathSegment.Add(apr[i]);
				}
				if(endOfPathList.Exists(o=> o == cr))
				{
					path.Add(new List<Vector3>(routePathSegment));
					routePathSegment.RemoveRange(0,routePathSegment.Count);
					worldCamera.transform.position = path[0][0];
				}
			}
			if(path.Count > 0)
				Run(1);
		}
	}

	public void Run (int count)
	{
		if (count <= 0)
			return;
		StartCoroutine (moveCamera (count));
	}
	IEnumerator moveCamera(int count)
	{
		yield return new WaitForEndOfFrame ();
		if (pathPart + 1 > path.Count)
			yield break;
		count--;
		List<Vector3> p = path [pathPart];
		for(int i = 0; i < p.Count; i++)
		{
			Vector3 startPos = worldCamera.transform.position;
			Vector3 endPos = p[i];
			float cof = 0f;
			while(cof < 1f)
			{
				cof +=Time.deltaTime * cameraSpeed;
				cof = Mathf.Min(1f,cof);
				worldCamera.transform.position = Vector3.Lerp(startPos,endPos,cof);

				yield return null;
			}
			yield return new WaitForEndOfFrame();
		}
		Run (count);
		pathPart++;
	}
}
