using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticlesPool : MonoBehaviour {

	public GameObject ItemPrefab;
	
	List<ParticleEmitter> pool;
	private static ParticlesPool instance = null;
	
	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.LogError("BubblePool duplicate!");
			DestroyObject(gameObject);
		}
	}
	
	public static ParticlesPool Get()
	{
		return instance;
	}
	
	public void Initialize(int count)
	{
		if(pool == null)
			pool = new List<ParticleEmitter> ();
		else
		{
			Debug.LogError("You tried initialize pool second time!");
			return;
		}

		for(int i = 0 ; i < count; i++)
		{
			GameObject obj = Instantiate(ItemPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			obj.transform.SetParent(transform);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = new Vector3(1f,1f,1f);
			pool.Add(obj.GetComponent<ParticleEmitter>());
		}
	}
	
	public ParticleEmitter Pull()
	{
		if(pool.Count == 0)
		{
			Debug.LogError("Pool is empty!");
			return null;
		}
		
		ParticleEmitter obj = pool [0];
		pool.Remove (obj);
		
		return obj;
	}
	
	public void Push(ParticleEmitter obj)
	{
		pool.Add (obj);
		obj.emit = false;
		obj.transform.SetParent(transform);
		obj.transform.localPosition = Vector3.zero;
	}
}
