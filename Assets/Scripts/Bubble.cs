using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Bubble : FieldItem, IPointerDownHandler,IPointerEnterHandler, IPointerUpHandler{

	public List<Sprite> bubbleImages;

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

	public override void SetType(Type tp,float size)
	{
		type = tp;
		Sprite sprite = bubbleImages.Find(i => {return i.name == "bubble_"+type.ToString()? i : null;});
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
		rectTransform.sizeDelta = new Vector2 (size, size);
		RealeaseItem ();
		SetNotChosed ();
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
