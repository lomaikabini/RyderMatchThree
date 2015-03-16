using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BubbleEditor : MonoBehaviour,IPointerClickHandler {
	
	public Image img;
	public Text tx;
	[HideInInspector]
	public FieldItem.Type type;
	
	public void OnPointerClick (PointerEventData eventData)
	{
		Debug.Log(type);
	}
}
