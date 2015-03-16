using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Separator : MonoBehaviour {
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

	public enum DestroyType
	{
		notDestroy,
		destroy
	}

	[Serializable]
	public struct Sprites
	{
		public DestroyType destroyType;
		public Sprite[] sprites;
	}

	[HideInInspector]
	public RectTransform rectTransform;
	Image img;
	Sprites kit;

	void Awake () 
	{
		rectTransform = GetComponent<RectTransform> ();
		img = GetComponent<Image> ();
	}

	public void SetType(Type t,DestroyType dT, float size = -1, int health = 1)
	{
		type = t;
		kit = GetKitByType (dT);
		lives = health;
		img.sprite = kit.sprites [kit.sprites.Length - lives];
		if(size != -1)
			rectTransform.sizeDelta = new Vector2 (rectTransform.sizeDelta.x, size);
	}

	public bool GiveDamage ()
	{
		if(kit.destroyType == DestroyType.destroy)
		{
			lives --;
			if(lives == 0)
			{
				return true;
			}
			img.sprite = kit.sprites [kit.sprites.Length - lives];
		}
		return false;
	}

	public Sprites GetKitByType (DestroyType dt)
	{
		for(int i = 0; i < sprites.Length; i++)
		{
			if(sprites[i].destroyType == dt) return sprites[i];
		}
		return default(Sprites);
	}

}
