using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JointsPool : MonoBehaviour {

	public GameObject ItemPrefab;
	
	List<ParallaxJoint> pool;
	private static JointsPool instance = null;
	
	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.LogError("JointsPool duplicate!");
			DestroyObject(gameObject);
		}
	}
	
	public static JointsPool Get()
	{
		return instance;
	}
	
	public void Initialize(int tableSize)
	{
		if(pool == null)
			pool = new List<ParallaxJoint> ();
		else
		{
			Debug.LogError("You tried initialize pool second time!");
			return;
		}
		
		int count = tableSize*tableSize;
		for(int i = 0 ; i < count; i++)
		{
			GameObject obj = Instantiate(ItemPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			obj.transform.SetParent(transform);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = new Vector3(1f,1f,1f);
			pool.Add(obj.GetComponent<ParallaxJoint>());
		}
	}
	
	public ParallaxJoint Pull()
	{
		if(pool.Count == 0)
		{
			Debug.LogError("Pool is empty!");
			return null;
		}
		
		ParallaxJoint obj = pool [0];
		pool.Remove (obj);
		
		return obj;
	}
	
	public void Push(ParallaxJoint obj)
	{
		obj.StopJoint ();
		pool.Add (obj);
		obj.transform.SetParent(transform);
		obj.transform.localPosition = Vector3.zero;
	}
}
