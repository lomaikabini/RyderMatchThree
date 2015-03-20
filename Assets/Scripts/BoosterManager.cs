using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterManager : MonoBehaviour {
	[HideInInspector]
	public int[] itemsRequire = {16,16,16,16,16};
	[HideInInspector]
	public int[] itemsCollect = {0,0,0,0,0};

	public static BoosterManager instance;

	void Awake()
	{
		instance = this;
	}

	public void AddCollectItems(List<FieldItem> items)
	{
		for(int i = 0; i < items.Count; i++)
		{
			itemsCollect[(int)items[i].type]++;
		}
		for(int i = 0; i < itemsCollect.Length;i++)
		{
			if(itemsCollect[i] >= itemsRequire[i])
			{
				DragonManager.instance.GiveMeBooster((FieldItem.Type)i);
				itemsCollect[i] = 0;
			}
		}
	}
}
