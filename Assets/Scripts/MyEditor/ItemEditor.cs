using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ItemEditor : MonoBehaviour,IPointerClickHandler {

	public Image img;
	public Text tx;
	[HideInInspector]
	public Item.ItemType type;
	[HideInInspector]
	public RectTransform rectTransform;
	[HideInInspector]
	public int posX;
	[HideInInspector]
	public int posY;
	public bool isMenu = false;
	public static List<Sprite> spritesIdle;
	void Awake () 
	{
		rectTransform = GetComponent<RectTransform> ();
	}
	public void OnPointerClick (PointerEventData eventData)
	{
		if (isMenu)
			MyEditor.instance.OnMenuItemClick (this);
		else
			MyEditor.instance.OnItemClick (this);
	}
	public void SetType (Item.ItemType tp, float size)
	{
		type = tp;
		Sprite sprite = spritesIdle.Find(i => {return i.name == "item_"+type.ToString()? i : null;});
		if(sprite == null)
			Debug.LogError("Sprite didn't find!");
		img.sprite = sprite;
		rectTransform.sizeDelta = new Vector2 (size, size);
	}
}
