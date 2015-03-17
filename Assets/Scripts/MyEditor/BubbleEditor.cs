using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class BubbleEditor : MonoBehaviour,IPointerClickHandler {
	
	public Image img;
	public Text tx;
	[HideInInspector]
	public FieldItem.Type type;
	public static List<Sprite> bubbleImages;
	[HideInInspector]
	public RectTransform rectTransform;
	[HideInInspector]
	public int posX;
	[HideInInspector]
	public int posY;
	public bool isMenu = false;

	void Awake () 
	{
		rectTransform = GetComponent<RectTransform> ();
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		if(isMenu)
			MyEditor.instance.OnMenuBubbleClick (this);
		else
			MyEditor.instance.OnBubbleClick (this);

	}
	public void SetType(Bubble.Type tp,float size)
	{
		type = tp;
		Sprite sprite = bubbleImages.Find(i => {return i.name == "bubble_"+type.ToString()? i : null;});
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
		rectTransform.sizeDelta = new Vector2 (size, size);
	}

}
