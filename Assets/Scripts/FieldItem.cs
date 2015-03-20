using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class FieldItem : MonoBehaviour {

	public 	Animator animator;
	public 	Image img;
	[HideInInspector]
	public int posX;
	[HideInInspector]
	public int posY;
	[HideInInspector]
	public Bubble bubbleScript;
	[HideInInspector]
	public Item itemScript;
	[HideInInspector]
	public RectTransform rectTransform;

	[HideInInspector]
	public Type type;
	public enum Type
	{
		blue,
		green,
		purple,
		red,
		yellow,
		item
	};
	public List<KeyValuePair<float,Vector2>> whereMove =  new List<KeyValuePair<float,Vector2>>();
	void Awake () 
	{
		rectTransform = GetComponent<RectTransform> ();
		SetBubbleScript ();
		SetItemScript ();
	}

	public void addMovePoints(List<KeyValuePair<float,Vector2>> list)
	{
		int count = list.Count;
		for(int i = 0; i < count; i++)
		{
			whereMove.Add(list[i]);
		}
	}

	public void playMovedAnim(bool an = false)
	{
		if (! an) {
			int num = (int)Random.Range (0, 3);
			switch (num) {
			case 0:
				animator.Play ("Bounce", 0, 0f);
				break;
			case 1:
				animator.Play ("Scale", 0, 0f);
				break;
			default:
				break;
			}
		} else
			animator.Play ("Bounce", 0, 0f);
	}
	
	public void playChosedAnim()
	{
		animator.Play ("Scale", 0, 0f);
	}
	public abstract void SetChosed ();
	public abstract void SetNotChosed();
	public abstract void HideItem ();
	public abstract void RealeaseItem();
	public virtual void SetType(Type tp,float size,Bubble.BoosterType bT){}
	//public virtual void SetType(Type tp,float size){}
	public virtual void SetBubbleScript(){}
	public virtual void SetItemScript(){}
}
