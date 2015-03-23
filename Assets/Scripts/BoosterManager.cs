using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterManager : MonoBehaviour {
	
	public int[] itemsRequire = {12,12,12,12,12};
	[HideInInspector]
	public int[] itemsCollect = {0,0,0,0,0};

	public static Dictionary<Bubble.BoosterType,int> boosterSizes = new Dictionary<Bubble.BoosterType, int> ();
	public static BoosterManager instance;

	void Awake()
	{
		instance = this;
		boosterSizes.Add (Bubble.BoosterType.diagonals, 5);
		boosterSizes.Add (Bubble.BoosterType.horizontal, 5);
		boosterSizes.Add (Bubble.BoosterType.rnd, 5);
		boosterSizes.Add (Bubble.BoosterType.threeQuad, 3);
		boosterSizes.Add (Bubble.BoosterType.vertical, 5);
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
