using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonManager : MonoBehaviour {

	public List<Dragon> dragons;
	public static DragonManager instance;
	private int movingCount = 0;
	private List<FieldItem> matchBubbles;
	void Awake()
	{
		instance = this;
	}

	public void GiveMeBooster(FieldItem.Type bubbleType)
	{
		Dragon dragon = dragons.Find (o => o.type == bubbleType);
		if(dragon == null)
			Debug.LogError("Dragon didn't find by type!");
		Game.instance.CreateBooster (dragon);
	}
	public void GetDragonItems(List<FieldItem> list)
	{
		matchBubbles = list;
		StartCoroutine (moveObjectsToDragons (list));
	}
	IEnumerator moveObjectsToDragons(List<FieldItem> list)
	{
		yield return new WaitForEndOfFrame ();
		for(int i =0;i < list.Count;i++)
		{
			movingCount++;
			StartCoroutine(ScaleUpObj(list[i]));
			yield return new WaitForSeconds(0.1f);
		}
		yield return null;
	}
	IEnumerator ScaleUpObj(FieldItem t)
	{
		yield return new WaitForEndOfFrame ();
		float cof = 0f;
		Vector3 startScale = new Vector3 (1f, 1f, 1f);
		Vector3 targetScale = new Vector3 (1.5f, 1.5f, 1.5f);
		while(cof < 1f)
		{
			cof +=Time.deltaTime*6f;
			cof = Mathf.Min(cof,1f);
			t.transform.localScale = Vector3.Lerp(startScale,targetScale,cof);
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine (GoToDragon (t));
		yield return null;
	}
	IEnumerator GoToDragon(FieldItem t)
	{
		yield return new WaitForEndOfFrame ();
		float cof = 0f;
		Vector3 startScale = new Vector3 (1.5f, 1.5f, 1.5f);
		Vector3 targetScale = new Vector3 (1f, 1f, 1f);
		Vector3 startPos = t.transform.position;
		Vector3 targetPos = dragons.Find (o => o.type == t.type).transform.position;
		float distance = Vector3.Distance (startPos, targetPos);
		while(cof < 1f)
		{
			cof +=Time.deltaTime*15f/distance;
			cof = Mathf.Min(cof,1f);
			t.transform.localScale = Vector3.Lerp(startScale,targetScale,cof);
			t.transform.position = Vector3.Lerp(startPos,targetPos,cof);
			yield return new WaitForEndOfFrame();
		}
		movingCount--;
		BubblePool.Get ().Push (t.gameObject);
		if(movingCount == 0)
		{
			BoosterManager.instance.AddCollectItems(matchBubbles);
			Game.instance.ContinueGame();
		}
		yield return null;
	}
}
