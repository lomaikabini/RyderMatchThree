using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Bubble : MonoBehaviour, IPointerDownHandler,IPointerEnterHandler{

	public List<Sprite> bubbleImages;
	public enum Type
	{
		blue,
		green,
		purple,
		red,
		yellow
	};

	[HideInInspector]
	public Type type;
	[HideInInspector]
	public int posX;
	[HideInInspector]
	public int posY;

	Image img;
	[HideInInspector]
	public RectTransform rectTransform;

	void Awake () 
	{
		img = GetComponent<Image> ();
		rectTransform = GetComponent<RectTransform> ();
	}

	public void OnPointerDown (PointerEventData eventData)
	{
		Game.Get ().BubblePress (this);
	}
	
	public void OnPointerEnter (PointerEventData eventData)
	{
		Game.Get ().BubblePointerEnter (this);
	}

	public void SetType(Type tp,float size)
	{
		type = tp;
		Sprite sprite = bubbleImages.Find(i => {return i.name == "bubble_"+type.ToString()? i : null;});
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
		rectTransform.sizeDelta = new Vector2 (size, size);
	}
}
