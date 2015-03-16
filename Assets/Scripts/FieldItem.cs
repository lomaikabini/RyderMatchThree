using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class FieldItem : MonoBehaviour {

	public 	Animator animator;
	public 	Image img;
	[HideInInspector]
	public int posX;
	[HideInInspector]
	public int posY;
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

	void Awake () 
	{
		rectTransform = GetComponent<RectTransform> ();
	}

	public void playMovedAnim()
	{
		int num =(int) Random.Range (0, 3);
		switch(num)
		{
		case 0:animator.Play ("Bounce", 0, 0f);break;
		case 1:animator.Play ("Scale", 0, 0f);break;
		default:break;
		}
		
	}
	
	public void playChosedAnim()
	{
		animator.Play ("Scale", 0, 0f);
	}
	public abstract void SetChosed ();
	public abstract void SetNotChosed();
}
