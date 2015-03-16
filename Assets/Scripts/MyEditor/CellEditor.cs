using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CellEditor : MonoBehaviour,IPointerClickHandler {

	public Image img;
	public Text tx;
	[HideInInspector]
	public Cell.Type type;
	[HideInInspector]
	public int lives;
	
	public void OnPointerClick (PointerEventData eventData)
	{
		Debug.Log(type + " " + lives);
	}
}
