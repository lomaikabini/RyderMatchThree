﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Item : FieldItem {

	public List<Sprite> spritesIdle;

	[HideInInspector]
	public ItemType itemType;

	public enum ItemType
	{
		key,
		crystal,
		gold,
		bomb,
		tooth,
		jumper
	}
	public override void SetItemScript()
	{
		itemScript = this;
	}
	public override void SetType (Type tp, float size,Bubble.BoosterType bT)
	{
		if(tp != Type.item)
		{
			Debug.LogError("WTF! Not properly type for item!");
			return;
		}
		//SetItemScript ();
		this.enabled = true;
		this.GetComponent<Bubble> ().enabled = false;
		type = tp;
		Sprite sprite = FindSpriteByType (itemType); 
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
		rectTransform.sizeDelta = new Vector2 (size*0.9f, size*0.9f);
		RealeaseItem ();
		SetNotChosed ();
	}
	public Sprite FindSpriteByType(ItemType t)
	{
		return spritesIdle.Find(i => {return i.name == "item_"+t.ToString()? i : null;});
	}
	public override void SetChosed ()
	{
		playChosedAnim ();
		Color c = img.color;
		c.a = 0.8f;
		img.color = c;
	}

	public override void SetNotChosed ()
	{
		Color c = img.color;
		c.a = 1f;
		img.color = c;
	}

	public override void HideItem ()
	{
		//poxody ne ponadobitsya
	}

	public override void RealeaseItem ()
	{
		//poxody ne ponadobitsya
	}

}
