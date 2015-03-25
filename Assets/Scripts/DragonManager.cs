using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DragonManager : MonoBehaviour {

	public List<Dragon> dragons;
	public GameObject bubblePrefab;
	public GameObject boosterPrefab;
	public static DragonManager instance;
	private int movingCount = 0;
	private int movingBoosters = 0;
	private List<FieldItem> matchBubbles;
	public List<KeyValuePair<Dragon,FieldItem>> dropBoosters = new List<KeyValuePair<Dragon,FieldItem>>();

	void Awake()
	{
		instance = this;
		setBoostersImg ();
	}

	void setBoostersImg ()
	{
		Bubble b = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0; i < dragons.Count; i++)
		{
			dragons[i].img.sprite = b.GetBoosterSpriteByType(dragons[i].type,dragons[i].boosterType);
		}
	}

	public void GiveMeBooster(FieldItem.Type bubbleType)
	{
		Dragon dragon = dragons.Find (o => o.type == bubbleType);
		if(dragon == null)
			Debug.LogError("Dragon didn't find by type!");
		FieldItem itm = Game.instance.CreateBooster (dragon);
		dropBoosters.Add (new KeyValuePair<Dragon, FieldItem> (dragon, itm));
	}
	public void GetDragonItems(List<FieldItem> list)
	{
		matchBubbles = list;
		StartCoroutine (moveObjectsToDragons (list));
	}
	IEnumerator moveObjectsToDragons(List<FieldItem> list)
	{
		yield return new WaitForEndOfFrame ();
		movingCount = list.Count;
		for(int i =0;i < list.Count;i++)
		{
			StartCoroutine(ScaleUpObj(list[i]));
			yield return new WaitForSeconds(0.2f);
		}
		yield return null;
	}
	IEnumerator ScaleUpObj(FieldItem t)
	{
		yield return new WaitForEndOfFrame ();
		float cof = 0f;
		Vector3 startScale = new Vector3 (1f, 1f, 1f);
		Vector3 targetScale = new Vector3 (1.5f, 1.5f, 1.5f);
		Vector3 startPos = t.transform.localPosition;
		Vector3 endPos = t.transform.localPosition + new Vector3 (0f, -50f, 0f);
		t.transform.SetSiblingIndex (999);
		while(cof < 1f)
		{
			cof +=Time.deltaTime*6f;
			cof = Mathf.Min(cof,1f);
			t.transform.localScale = Vector3.Lerp(startScale,targetScale,cof);
			t.transform.localPosition = Vector3.Lerp(startPos,endPos,cof);
			yield return new WaitForEndOfFrame();
		}
		if (t.type != FieldItem.Type.item)
			StartCoroutine (GoToDragon (t));
		else {
			movingCount--;
			BubblePool.Get ().Push (t.gameObject);
		}
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
		IncreaseIndicatorFact (t.type);
		movingCount--;
		BubblePool.Get ().Push (t.gameObject);
		if(movingCount == 0)
		{
			StartCoroutine(attackWizard());
			BoosterManager.instance.AddCollectItems(matchBubbles);
			//Game.instance.ContinueGame();
		}
		yield return null;
	}
	IEnumerator attackWizard()
	{
		yield return new WaitForSeconds (0.1f);
		HideShowedBoosters();
		Game.instance.ContinueGame();
		yield return null;
	}
	IEnumerator DropBooster(KeyValuePair<Dragon,FieldItem> itm)
	{
		yield return new WaitForSeconds (0.2f);
		GameObject boosterView = Instantiate (boosterPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		Image img = boosterView.GetComponent<Image> ();
		img.sprite = bubblePrefab.GetComponent<Bubble> ().GetBoosterSpriteByType (itm.Key.type, itm.Key.boosterType);
		img.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Game.instance.bubbleSize, Game.instance.bubbleSize);
		boosterView.transform.SetParent (itm.Key.transform);
		boosterView.transform.position = itm.Key.transform.position;
		boosterView.transform.localScale = new Vector3 (1f, 1f, 1f);
		Vector3 startPos = boosterView.transform.position;
		Vector3 targetPos = itm.Value.transform.position;
		Vector3 startScale = boosterView.transform.localScale;
		Vector3 targetScale = new Vector3 (1.4f, 1.4f, 1.4f);
		float cof = 0f;
		while(cof < 1f)
		{
			cof += Time.deltaTime * 6f;
			cof = Mathf.Min(cof,1f);
			boosterView.transform.position = Vector3.Lerp(startPos,startPos + new Vector3(0.5f,0.5f),cof);
			boosterView.transform.localScale = Vector3.Lerp(startScale,targetScale,cof);
			yield return new WaitForEndOfFrame();
		}
		cof = 0f;
		startPos = boosterView.transform.position;
		targetScale = new Vector3 (1f, 1f, 1f);
		startScale = boosterView.transform.localScale;
		while(cof < 1f)
		{
			cof += Time.deltaTime * 6f;
			cof = Mathf.Min(cof,1f);
			boosterView.transform.position = Vector3.Lerp(startPos,targetPos,cof);
			boosterView.transform.localScale = Vector3.Lerp(startScale,targetScale,cof);
			yield return new WaitForEndOfFrame();
		}
		itm.Value.SetType (itm.Key.type, Game.instance.bubbleSize, itm.Key.boosterType);
		Destroy (boosterView);
		movingBoosters--;
		if(movingBoosters==0)
		{
			dropBoosters.RemoveRange(0,dropBoosters.Count);
			Game.instance.checkPossibleMatch();
			//Game.instance.ReleaseGame ();
		}
		yield return null;
	}
	public void DropBoosters()
	{
		if(dropBoosters.Count == 0)
			Game.instance.ReleaseGame ();
		else
		{
			for(int i = 0; i < dropBoosters.Count;i++)
			{
				dropBoosters[i].Key.overlayFact.fillAmount = 1f;
				movingBoosters++;
				StartCoroutine(DropBooster(dropBoosters[i]));
			}
		}
	}
	public void ShowBooster(FieldItem.Type t)
	{
		int maxCount = BoosterManager.instance.itemsRequire[(int)t];
		Dragon dragon = dragons.Find(o => o.type == t);
		dragon.overlayCurrent.fillAmount = dragon.overlayFact.fillAmount;
		IncreaseIndicatorCurrent (t);
		dragon.boosterView.SetActive(true);
	}
	public void IncreaseIndicatorCurrent(FieldItem.Type t)
	{
		int maxCount = BoosterManager.instance.itemsRequire[(int)t];
		float value = 1f / maxCount;
		Dragon dragon = dragons.Find(o => o.type == t);
		float newValue = Mathf.Max (0f, dragon.overlayCurrent.fillAmount - value);
		dragon.overlayCurrent.fillAmount = newValue; 
	}
	public void DecreaseIndicatorCurrent(FieldItem.Type t)
	{
		int maxCount = BoosterManager.instance.itemsRequire[(int)t];
		float value = 1f / maxCount;
		Dragon dragon = dragons.Find(o => o.type == t);
		float newValue = Mathf.Min (1f, dragon.overlayCurrent.fillAmount + value);
		dragon.overlayCurrent.fillAmount = newValue; 
	}
	public void IncreaseIndicatorFact(FieldItem.Type t)
	{
		int maxCount = BoosterManager.instance.itemsRequire[(int)t];
		float value = 1f / maxCount;
		Dragon dragon = dragons.Find(o => o.type == t);
		float newValue = Mathf.Max (0f, dragon.overlayFact.fillAmount - value);
		dragon.overlayFact.fillAmount = newValue; 
	}
	public void HideShowedBoosters()
	{
		foreach(Dragon d in dragons)
		{
			if(d.boosterView.activeSelf /*&& !dropBoosters.Exists(o=>o.Value.type == d.type)*/)
				d.boosterView.SetActive(false);
		}
	}
}
