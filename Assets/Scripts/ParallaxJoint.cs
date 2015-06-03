using UnityEngine;
using System.Collections;

public class ParallaxJoint : MonoBehaviour {

	public RectTransform rectA;
	public RectTransform rectB;

	[HideInInspector]
	public Bubble firstBubble;
	[HideInInspector]
	public Bubble secondBubble;
	
	Vector3 posA;
	Vector3 posB;

	float speed = 500f;
	float endPosX;

	RectTransform rectTransform;

	bool isRun = false;
	void Start()
	{
		posA = rectA.localPosition;
		posB = rectB.localPosition;
		endPosX = -(rectA.rect.width+rectA.localPosition.x);
	}

	void Update () 
	{
		if(isRun)
		{
			float value = -(Time.deltaTime * speed);
			Vector3 deltaPos = new Vector3 (value, 0f, 0f);
			if((rectA.localPosition + deltaPos).x > endPosX && (rectB.localPosition + deltaPos).x > endPosX )
			{
				rectA.localPosition += deltaPos;
				rectB.localPosition += deltaPos;
			}
			else
			{
				if(rectA.localPosition.x < rectB.localPosition.x)
				{
					rectA.localPosition = posB;
					rectB.localPosition = posA;
				}
				else
				{
					rectA.localPosition = posA;
					rectB.localPosition = posB;
				}
			}
		}

	}

	public void RotateJoint(FieldItem a, FieldItem b)
	{
		float angle = 0f;
		if (a.posX < b.posX && a.posY == b.posY)
			angle = 180f;
		else if (a.posX > b.posX && a.posY == b.posY)
			angle = 0f;
		else if (a.posX == b.posX && a.posY < b.posY)
			angle = -90f;
		else if (a.posX == b.posX && a.posY > b.posY)
			angle = 90f;
		else if (a.posX > b.posX && a.posY < b.posY)
			angle = -45f;
		else if (a.posX < b.posX && a.posY < b.posY)
			angle = -135f;
		else if (a.posX > b.posX && a.posY > b.posY)
			angle = 45f;
		else if (a.posX < b.posX && a.posY > b.posY)
			angle = 135f;
		if(rectTransform == null)
			rectTransform = GetComponent<RectTransform> ();
		rectTransform.localRotation = Quaternion.Euler (new Vector3 (0f, 0f, angle));
		rectTransform.localPosition =Vector3.Lerp(a.rectTransform.localPosition,b.rectTransform.localPosition, 0.5f);
	}
	public void RunJoint()
	{
		isRun = true;
	}
	public void StopJoint()
	{
		isRun = false;
	}
}
