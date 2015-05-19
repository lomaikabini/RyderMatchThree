using UnityEngine;
using System.Collections;

public class LoopScale : MonoBehaviour {

	public Vector3 firstState;
	public Vector3 secondState;
	public float timeForAnim;
	public float phase;

	RectTransform rectTransform;

	void Start () 
	{
		rectTransform = gameObject.GetComponent<RectTransform> ();
	}

	void Update () 
	{
		Vector3 val =  Vector3.Lerp(firstState,secondState,(Mathf.Sin((Time.time/timeForAnim)*2f + Mathf.Deg2Rad*phase ) + 1.0f) / 2.0f);
		rectTransform.localScale = val;
	}
}
