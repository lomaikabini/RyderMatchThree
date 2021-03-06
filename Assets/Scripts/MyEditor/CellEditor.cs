﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CellEditor : MonoBehaviour,IPointerClickHandler {

	public Image img;
	public Text tx;
	public bool isMenu = false;
	[HideInInspector]
	public static Cell.Sprites[] SpritesKit;
	public CellEssence cellInfo = new CellEssence();
	[HideInInspector]
	public RectTransform rectTransform;
	void Awake()
	{
		rectTransform = GetComponent<RectTransform> ();
	}
	public void OnPointerClick (PointerEventData eventData)
	{
		if (isMenu)
			MyEditor.instance.OnMenuCellClick (this);
		else
			MyEditor.instance.OnCellClick (this);
	}
	public void SetType (Cell.Type t,float size,int health)
	{
		cellInfo.type = t;
		Sprite sp;
		Cell.Sprites kit = getKitByType (t);
		cellInfo.lives = health;
		sp = kit.sprites[kit.sprites.Length - health];
		img.sprite = sp;
		if(size != -1)
			rectTransform.sizeDelta = new Vector2 (size, size);
	}
	public Cell.Sprites getKitByType (Cell.Type t)
	{
		for(int i = 0; i < SpritesKit.Length; i++)
		{
			if(SpritesKit[i].type == t) return SpritesKit[i];
		}
		return default(Cell.Sprites);
	}
}
