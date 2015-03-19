using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Cell : MonoBehaviour {

	public Sprites[] SpritesKit;
	public Image img;
	[HideInInspector]
	public int posX;
	[HideInInspector]
	public int posY;
	[HideInInspector]
	public int[,] betweenCells = new int[1,1];
	[HideInInspector]
	public Type cellType;
	[HideInInspector]
	public RectTransform rectTransform;
	public enum Type
	{
		empty,
		groundBlock,
		block,
		box
	}

	[Serializable]
	public struct Sprites
	{
		public Type type;
		public DestroyType destroyType;
		public Sprite[] sprites;

		public enum DestroyType
		{
			notDestroy,
			destroy
		}
	}
	
	int lvl;
	Sprites kit;
	void Awake()
	{
		rectTransform = GetComponent<RectTransform> ();
	}

	public void SetType (Type t,float size = -1, int health = 1)
	{
		cellType = t;
		Sprite sp;
		kit = getKitByType (t);
		lvl = health;
		sp =  kit.sprites[kit.sprites.Length - health];
		img.sprite = sp;
		if(size != -1)
			rectTransform.sizeDelta = new Vector2 (size, size);
	}

	public Sprites getKitByType (Type t)
	{
		for(int i = 0; i < SpritesKit.Length; i++)
		{
			if(SpritesKit[i].type == t) return SpritesKit[i];
		}
		return default(Sprites);
	}

	public void GiveDamage()
	{
		if(kit.destroyType == Sprites.DestroyType.destroy && cellType != Type.empty)
		{
			lvl--;
			if(lvl == 0)
				SetType(Type.empty);
		}
	}


}
