using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Item : MonoBehaviour {
	
	[HideInInspector]
	public Type type;
	[HideInInspector]
	public int lives;
	[HideInInspector]
	public int posX;
	[HideInInspector]
	public int posY;
	
	public Sprites[] sprites;
	
	public enum Type
	{
		horizontal,
		vertical
	}
	
	[Serializable]
	public struct Sprites
	{
		public Type type;
		public Sprite[] sprites;
	}
	
	[HideInInspector]
	public RectTransform rectTransform;
	Image img;
	Sprites kit;

	void Start () {
	
	}
}
