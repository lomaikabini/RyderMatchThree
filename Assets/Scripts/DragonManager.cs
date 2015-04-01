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
	private List<FieldItem.Type> matchBubbles;
	private List<KeyValuePair<Vector2,Vector2>> blockedCells = new List<KeyValuePair<Vector2, Vector2>> ();
	private List<Separator> usedSeparators = new List<Separator> ();
	private List<Cell> usedCells = new List<Cell> ();
	List<FieldItem> boosterList;
	Vector2 boosterPos;
	List<Vector2> allBoosterPos;
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
	public void GetDragonItems(List<FieldItem> list,List<FieldItem> boosterList,Vector2 boosterPos,List<Vector2> allBoosterPos)
	{
		//TODO: vozmojno tyt naod bydet ydalyat' dyblikati s boosterlist
		matchBubbles = new List<FieldItem.Type>();
		for (int i=0; i < list.Count; i++)
			matchBubbles.Add (list [i].type);

		movingCount = list.Count;
		this.boosterList = boosterList;
		this.boosterPos = boosterPos;
		this.allBoosterPos =new List<Vector2>(allBoosterPos);
		usedSeparators.RemoveRange (0, usedSeparators.Count);
		usedCells.RemoveRange (0, usedCells.Count);
		blockedCells.RemoveRange (0, blockedCells.Count);

		StartCoroutine (moveObjectsToDragons (list));
	}
	IEnumerator moveBubblesBooster()
	{
		movingCount = boosterList.Count;
		int startX =(int) boosterPos.x;
		int startY =(int) boosterPos.y;
		int size = Game.instance.TableSize;
		for(int i = 0; i < size; i++)
		{
			int count = i;//(i*2)-1;
			int m = 0;
			for(int q = startX-count; q <= startX+count; q++)
			{
				for(int w = startY-count; w <= startY+count; w++)
				{
					if( (q == startX-count || q == startX+count || w == startY-count || w == startY+count) /*&& boosterList.Exists(o=> o.posX == startX+q && o.posY == startY+w)*/)
					{
						FieldItem itm = boosterList.Find(o=> o.posX == q && o.posY == w);
						if(allBoosterPos.Exists(o=> o.x == (float)q && o.y == w))
						{
							//vozmojno dvoinoi zvriv
							Game.instance.explositionFromBooster(new Vector2((float)q,(float)w));
						}
						if(itm != null)
						{
							StartCoroutine(ScaleUpObj(itm));
							boosterList.Remove(itm);
							matchBubbles.Add(itm.type);
							m++;
						}
					}
				}
			}
			if(m!=0)
				yield return new WaitForSeconds(0.12f);
			else
				yield return null;
		}
		yield return null;
	}

	IEnumerator moveObjectsToDragons(List<FieldItem> list)
	{
		yield return new WaitForEndOfFrame ();
		for(int i =0;i < list.Count;i++)
		{
			StartCoroutine(ScaleUpObj(list[i]));
			Game.instance.explositionNearSeparators(list[i],ref blockedCells,ref usedSeparators);
			Game.instance.explositionNearBlocks(list[i],blockedCells,ref usedCells);
			yield return new WaitForSeconds(0.12f);
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
			if(movingCount == 0)
			{
				if(boosterList.Count == 0 )
					StartCoroutine(attackWizard());
				else
					StartCoroutine(moveBubblesBooster());
			}
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
			yield return null;
		}
		IncreaseIndicatorFact (t.type);
		movingCount--;
		BubblePool.Get ().Push (t.gameObject);
		if(movingCount == 0)
		{
			if(boosterList.Count == 0 )
				StartCoroutine(attackWizard());
			else
				StartCoroutine(moveBubblesBooster());
		}
		yield return null;
	}
	IEnumerator attackWizard()
	{
		yield return new WaitForSeconds (0.1f);
		HideShowedBoosters();
		float waitTime = 0.5f;
		WizardManager.instance.CauseDamage (Game.instance.damage, waitTime);
		yield return new WaitForSeconds (waitTime);
		Game.instance.ContinueGame();
		BoosterManager.instance.AddCollectItems (matchBubbles);
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
			WizardManager.instance.DropItem();
			//Game.instance.checkPossibleMatch();
			//Game.instance.ReleaseGame ();
		}
		yield return null;
	}
	public void DropBoosters()
	{
		if (dropBoosters.Count == 0)
			WizardManager.instance.DropItem ();//Game.instance.ReleaseGame ();
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
		SetIndicatorCount (t, 1);
		//IncreaseIndicatorCurrent (t);
		dragon.boosterView.SetActive(true);
	}
	public void HideBooster(FieldItem.Type t)
	{
		Dragon dragon = dragons.Find(o => o.type == t);
		if(dragon.boosterView.activeSelf)
			dragon .boosterView.SetActive(false);
	}
	public void SetIndicatorCount(FieldItem.Type t,int count = 1)
	{
		int maxCount = BoosterManager.instance.itemsRequire[(int)t];
		float value = ((float) count) / maxCount;
		Dragon dragon = dragons.Find(o => o.type == t);
		float newValue = Mathf.Max (0f, dragon.overlayFact.fillAmount - value);
		dragon.overlayCurrent.fillAmount = newValue; 
	}
	public void Clear()
	{
		foreach(Dragon d in dragons)
		{
			d.overlayFact.fillAmount = 1f;
			d.overlayCurrent.fillAmount = 1f;
		}
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
