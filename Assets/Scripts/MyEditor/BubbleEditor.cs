using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class BubbleEditor : MonoBehaviour,IPointerClickHandler {
	
	public Image img;
	public Text tx;
	public static List<Sprite> bubbleImages;
	[HideInInspector]
	public RectTransform rectTransform;
	public BubbleEssence bubbleConfig = new BubbleEssence ();
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
		bubbleConfig.type = tp;
		Sprite sprite = bubbleImages.Find(i => {return i.name == "bubble_"+bubbleConfig.type.ToString()? i : null;});
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
		rectTransform.sizeDelta = new Vector2 (size, size);
	}

}
