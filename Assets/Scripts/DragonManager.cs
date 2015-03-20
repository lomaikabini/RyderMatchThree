using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonManager : MonoBehaviour {

	public List<Dragon> dragons;
	public static DragonManager instance;

	void Awake()
	{
		instance = this;
	}

	public void GiveMeBooster(FieldItem.Type bubbleType)
	{
		Dragon dragon = dragons.Find (o => o.type == bubbleType);
		if(dragon == null)
			Debug.LogError("Dragon didn't find by type!");
		Game.instance.CreateBooster (dragon);
	}
	public void GetDragonItems(ref List<FieldItem> list)
	{

	}
}
