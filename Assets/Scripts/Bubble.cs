using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Bubble : MonoBehaviour, IPointerDownHandler,IPointerEnterHandler, IPointerUpHandler{

	public List<Sprite> bubbleImages;
	public 	Animator animator;
	public 	Image img;
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
	
	[HideInInspector]
	public RectTransform rectTransform;
	public bool isRun = false;
	public List<KeyValuePair<float,Vector2>> whereMove =  new List<KeyValuePair<float,Vector2>>();
	void Awake () 
	{
		//img = GetComponent<Image> ();
		rectTransform = GetComponent<RectTransform> ();
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

	public void SetType(Type tp,float size)
	{
		type = tp;
		Sprite sprite = bubbleImages.Find(i => {return i.name == "bubble_"+type.ToString()? i : null;});
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
		rectTransform.sizeDelta = new Vector2 (size, size);
		RealeaseBubble ();
		SetNotChosed ();
	}

	public void playMovedAnim()
	{
		animator.Play ("Bounce", 0, 0f);
	}

	public void playChosedAnim()
	{
		animator.Play ("Scale", 0, 0f);
	}

	public void addMovePoints(List<KeyValuePair<float,Vector2>> list)
	{
		int count = list.Count;
		for(int i = 0; i < count; i++)
		{
			whereMove.Add(list[i]);
		}
	}

	public void HideBubble ()
	{
		Color c = img.color;
		c.a = 0.5f;
		img.color = c;
	}

	public void RealeaseBubble()
	{
		Color c = img.color;
		c.a = 1f;
		img.color = c;
	}

	public void SetChosed ()
	{
		Color c = img.color;
		c.a = 0.8f;
		img.color = c;
	}

	public void SetNotChosed()
	{
		Color c = img.color;
		c.a = 1f;
		img.color = c;
	}

}
