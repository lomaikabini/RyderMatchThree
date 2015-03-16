using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeparatorEditor : MonoBehaviour,IPointerClickHandler {
	
	public Image img;
	public Text tx;
	[HideInInspector]
	public Separator.DestroyType destroyType;
	[HideInInspector]
	public Separator.Type type;
	[HideInInspector]
	public int lives;
	
	public void OnPointerClick (PointerEventData eventData)
	{
		Debug.Log(destroyType+"   " + type + "  "+lives);
	}
}
