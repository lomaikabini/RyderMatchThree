﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Bubble : FieldItem, IPointerDownHandler,IPointerEnterHandler, IPointerUpHandler{

	public List<Sprite> bubbleImages;
	public List<Sprite> boosterImages;
	public BoosterType boosterType;
	public enum BoosterType
	{
		none,
		threeQuad,
		diagonals,
		horizontal,
		vertical,
		rnd
	}
	public override void SetBubbleScript()
	{
		bubbleScript = this;
	}
	public void OnPointerUp (PointerEventData eventData)
	{
		Game.Get ().BubblePointerUp (this);
	}

	public void OnPointerDown (PointerEventData eventData)
	{
		Game.Get ().BubblePress (this);
	}
	
	public void OnPointerEnter (PointerEventData eventData)
	{
		Game.Get ().BubblePointerEnter (this);
	}

	public override void SetType(Type tp,float size,BoosterType bT)
	{
		//SetBubbleScript ();
		this.enabled = true;
		this.GetComponent<Item> ().enabled = false;
		type = tp;
		boosterType = bT;
		rectTransform.sizeDelta = new Vector2 (size*0.9f, size*0.9f);
		RealeaseItem ();
		SetNotChosed ();

		Sprite sprite;
		if(boosterType == BoosterType.none)
		{
			sprite = GetBubbleImageByType(type);
		}
		else
		{
			sprite = GetBoosterSpriteByType(type,boosterType);
		}
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
	}
	public Sprite GetBubbleImageByType(Type t)
	{
		return bubbleImages.Find(i => {return i.name == "bubble_"+t.ToString()? i : null;});
	}
	public Sprite GetBoosterSpriteByType(Type t,BoosterType bT)
	{
		return boosterImages.Find(i => {return i.name == "booster_"+t.ToString()+"_"+bT.ToString()? i : null;});
	}
	public override void HideItem ()
	{
		Color c = img.color;
		c.a = 0.5f;
		img.color = c;
	}

	public override void RealeaseItem()
	{
		Color c = img.color;
		c.a = 1f;
		img.color = c;
	}

	public override void SetChosed ()
	{
		Color c = img.color;
		c.a = 0.8f;
		img.color = c;
	}

	public override void SetNotChosed()
	{
		Color c = img.color;
		c.a = 1f;
		img.color = c;
	}

}
