using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class FactorView : MonoBehaviour {

	public Text tx;
	public static FactorView instance;
	[HideInInspector]
	public Dictionary<Vector2,float> factors = new Dictionary<Vector2, float>()
	{
		{new Vector2(6,10), 1.2f},
		{new Vector2(11,15), 1.5f},
		{new Vector2(16,20), 2f},
		{new Vector2(21,25), 2.5f},
		{new Vector2(26,99999), 3f}
	}; 
	void Awake()
	{
		instance = this;
	}

	public float SetView(int count)
	{
		float val = 1f;

		foreach(KeyValuePair<Vector2, float> k in factors)
		{
			if(count >= (int)k.Key.x && count <= (int)k.Key.y)
			{
				val = k.Value;
				break;
			}
		}
		if (val == 1f)
		{
			HideView();
		}
		else
			tx.text = "Combo! \n" + "x" + val.ToString ();

		return val; 
	}

	public void HideView()
	{
		tx.text = "";
	}
}
