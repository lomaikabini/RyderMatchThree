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
		separatorDestroyLeft,
		separatorDestroyRight,
		separatorDestroyTop,
		separatorDestroyDown,
		separatorLeft,
		separatorRight,
		separatorTop,
		separatorDown
	}

	[Serializable]
	public struct Sprites
	{
		public Type type;
		public Sprite[] sprites;
	}
	
	int lvl;
	void Awake()
	{
		rectTransform = GetComponent<RectTransform> ();
	}

	public void SetType (Type t,float size)
	{
		cellType = t;
		Sprite sp;
		try
		{
			Sprites kit = SpritesKit[(int) t];
			lvl = kit.sprites.Length;
			sp = SpritesKit[(int)t].sprites[0];
		}
		catch(Exception e)
		{
			Debug.LogError("Can't find sprite by cell Type! " + e);
			return;
		}
		img.sprite = sp;
		rectTransform.sizeDelta = new Vector2 (size, size);
	}



}
