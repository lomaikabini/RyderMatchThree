using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemEditor : MonoBehaviour,IPointerClickHandler {

	public Image img;
	public Text tx;
	[HideInInspector]
	public Item.ItemType type;
	
	public void OnPointerClick (PointerEventData eventData)
	{
		Debug.Log(type);
	}
}
