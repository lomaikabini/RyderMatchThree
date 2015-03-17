using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class BubbleInGameEditor : MonoBehaviour,IPointerClickHandler {
	
	public Image img;
	public Text tx;
	[HideInInspector]
	public FieldItem.Type type;

	public void OnPointerClick (PointerEventData eventData)
	{
		MyEditor.instance.BubbleInGameClick (this);
	}
}
