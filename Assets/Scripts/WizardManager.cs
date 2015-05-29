using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WizardManager : MonoBehaviour {

	public Transform wizardContainer;
	public GameObject wizardPrefab;
	public GameObject bubblePrefab;
	public GameObject dropItemPrefab;
	public GameObject cellPrefab;
	public Transform canvas;
	public Transform spawnPoints;
	public Transform dropPoint;
	private List<Wizard> wizards;
	private int localMoves;
	public static WizardManager instance;

	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.LogError("WizardManager duplicate!");
			DestroyObject(gameObject);
		}
	}

	public void Initialize(List<WizardEssence> list)
	{
		wizards = new List<Wizard> ();
		foreach(Transform child in wizardContainer) {
			Destroy(child.gameObject);
		}
		Bubble bubble = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].health <= 0) continue;
			GameObject obj = Instantiate(wizardPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			obj.transform.SetParent(wizardContainer);
			obj.transform.position = spawnPoints.GetChild(wizards.Count).position; //Vector3.zero;
			Wizard w = obj.GetComponent<Wizard>();
			w.config = list[i];
			w.currentHealth = list[i].health;
			Sprite sp = null;
			if(list[i].resistId>=0)
				sp = bubble.GetBubbleImageByType((FieldItem.Type)list[i].resistId);
			w.SetResistView(sp);
			wizards.Add(w);
		}
	}
	public FieldItem.Type GetCurrentWizardResist()
	{
		for(int i = 0; i < wizards.Count; i++)
		{
			if(wizards[i].currentHealth > 0)
				return (FieldItem.Type)wizards[i].config.resistId;
		}
		return FieldItem.Type.item;
	}
	public void ShowCurrentWizardDamage(int damage)
	{
		for(int i = 0; i < wizards.Count; i++)
		{
			if(wizards[i].currentHealth > 0)
			{
				wizards[i].ShowHealth(damage);
				return;
			}
		}
	}
	public void DropItem()
	{
		if(Game.instance.moves ==  localMoves)
		{
			Game.instance.ReleaseGame ();
			return;
		}
		//Game.instance.checkPossibleMatch();
		Wizard w = null;
		for(int i = 0; i < wizards.Count; i++)
		{
			if(wizards[i].currentHealth > 0)
			{
				w = wizards[i];
				break;
			}
		}
		int step = Game.instance.moves;
		localMoves = step;
		if (w != null) {
			bool drop = false;
			if (w.config.slimePeriod != 0 && step % w.config.slimePeriod == 0) {
				drop = true;
				StartCoroutine (dropSlime (Game.instance.FindConvertBubble (), dropPoint.position/*w.transform.position*/));
			}
			else if(w.config.toothPeriod !=0 && step % w.config.toothPeriod == 0)
			{
				drop = true;
				StartCoroutine (dropItem (Game.instance.FindConvertBubble (), dropPoint.position, Item.ItemType.tooth));
			}
			else if(w.config.jumpPeriond !=0 && step % w.config.jumpPeriond == 0)
			{
				drop = true;
				FieldItem itm1 = Game.instance.FindConvertBubble ();
				FieldItem itm2 = Game.instance.FindConvertBubble (new FieldItem[]{itm1});
				StartCoroutine (dropJumpers(new FieldItem[]{itm1,itm2}, new Vector3[]{dropPoint.position,dropPoint.position}));
			}
			if(!drop)
				Game.instance.PlayJumpers();
		}
		else
			Game.instance.PlayJumpers();

	}

	IEnumerator dropJumpers(FieldItem[] b, Vector3[] startPos)
	{
		yield return new WaitForEndOfFrame ();
		Item.ItemType t = Item.ItemType.jumper;
		GameObject obj = Instantiate (dropItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		obj.transform.SetParent (canvas);
		obj.transform.localScale = new Vector3 (1f, 1f, 1f);
		obj.transform.position = startPos[0];
		obj.GetComponent<Image> ().sprite = bubblePrefab.GetComponent<Item> ().FindSpriteByType (t);
		obj.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Game.instance.bubbleSize, Game.instance.bubbleSize);

		GameObject obj1 = Instantiate (dropItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		obj1.transform.SetParent (canvas);
		obj1.transform.localScale = new Vector3 (1f, 1f, 1f);
		obj1.transform.position = startPos[1];
		obj1.GetComponent<Image> ().sprite = bubblePrefab.GetComponent<Item> ().FindSpriteByType (t);
		obj1.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Game.instance.bubbleSize, Game.instance.bubbleSize);

		Vector3 startScale = new Vector3 (1f, 1f, 1f);
		Vector3 endScale = new Vector3 (1.5f, 1.5f, 1.5f);
		Vector3 endPos = startPos[0] + new Vector3 (0.5f, 0.5f, 0f);
		float cof = 0;
		while(cof < 1f)
		{
			cof +=Time.deltaTime*5f;
			cof = Mathf.Min(1f,cof);
			obj.transform.position = Vector3.Lerp(startPos[0],endPos,cof);
			obj.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			obj1.transform.position = Vector3.Lerp(startPos[1],endPos,cof);
			obj1.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			yield return new WaitForEndOfFrame();
		}
		startScale = new Vector3 (1.5f, 1.5f, 1.5f);
		endScale = new Vector3 (1f, 1f, 1f);
		startPos[0] = obj.transform.position;
		endPos = b[0].transform.position;
		startPos[1] = obj1.transform.position;
		Vector3 endPos1 = b[1].transform.position;
		cof = 0f;
		while(cof < 1f)
		{
			cof +=Time.deltaTime*5f;
			cof = Mathf.Min(1f,cof);
			obj.transform.position = Vector3.Lerp(startPos[0],endPos,cof);
			obj.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			obj1.transform.position = Vector3.Lerp(startPos[1],endPos1,cof);
			obj1.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			yield return new WaitForEndOfFrame();
		}
		Game.instance.SetDropedItem (b[0].posX, b[0].posY, t);
		Game.instance.SetDropedItem (b[1].posX, b[1].posY, t);
		Destroy (obj);
		Destroy (obj1);
		Game.instance.PlayJumpers(new List<FieldItem>(){b[0],b[1]});
		//Game.instance.jumpers.Add (b [0]);
		//Game.instance.jumpers.Add (b [1]);
		yield return null;
	}


	IEnumerator dropItem(FieldItem b,Vector3 startPos, Item.ItemType t)
	{
		yield return new WaitForEndOfFrame ();
		GameObject obj = Instantiate (dropItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		obj.transform.SetParent (canvas);
		obj.transform.localScale = new Vector3 (1f, 1f, 1f);
		obj.transform.position = startPos;
		obj.GetComponent<Image> ().sprite = bubblePrefab.GetComponent<Item> ().FindSpriteByType (t);
		obj.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Game.instance.bubbleSize, Game.instance.bubbleSize);
		Vector3 startScale = new Vector3 (1f, 1f, 1f);
		Vector3 endScale = new Vector3 (1.5f, 1.5f, 1.5f);
		Vector3 endPos = startPos + new Vector3 (0.5f, 0.5f, 0f);
		float cof = 0;
		while(cof < 1f)
		{
			cof +=Time.deltaTime*5f;
			cof = Mathf.Min(1f,cof);
			obj.transform.position = Vector3.Lerp(startPos,endPos,cof);
			obj.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			yield return new WaitForEndOfFrame();
		}
		startScale = new Vector3 (1.5f, 1.5f, 1.5f);
		endScale = new Vector3 (1f, 1f, 1f);
		startPos = obj.transform.position;
		endPos = b.transform.position;
		cof = 0f;
		while(cof < 1f)
		{
			cof +=Time.deltaTime*5f;
			cof = Mathf.Min(1f,cof);
			obj.transform.position = Vector3.Lerp(startPos,endPos,cof);
			obj.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			yield return new WaitForEndOfFrame();
		}
		Game.instance.SetDropedItem (b.posX, b.posY, t);
		Destroy (obj);
		Game.instance.PlayJumpers();
		yield return null;
		
	}

	IEnumerator dropSlime(FieldItem b,Vector3 startPos)
	{
		yield return new WaitForEndOfFrame ();
		GameObject obj = Instantiate (dropItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		obj.transform.SetParent (canvas);
		obj.transform.localScale = new Vector3 (1f, 1f, 1f);
		obj.transform.position = startPos;
		obj.GetComponent<Image> ().sprite = cellPrefab.GetComponent<Cell> ().getKitByType (Cell.Type.spot).sprites [0];
		obj.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Game.instance.bubbleSize, Game.instance.bubbleSize);
		Vector3 startScale = new Vector3 (1f, 1f, 1f);
		Vector3 endScale = new Vector3 (1.5f, 1.5f, 1.5f);
		Vector3 endPos = startPos + new Vector3 (0.5f, 0.5f, 0f);
		float cof = 0;
		while(cof < 1f)
		{
			cof +=Time.deltaTime*5f;
			cof = Mathf.Min(1f,cof);
			obj.transform.position = Vector3.Lerp(startPos,endPos,cof);
			obj.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			yield return new WaitForEndOfFrame();
		}
		startScale = new Vector3 (1.5f, 1.5f, 1.5f);
		endScale = new Vector3 (1f, 1f, 1f);
		startPos = obj.transform.position;
		endPos = b.transform.position;
		cof = 0f;
		while(cof < 1f)
		{
			cof +=Time.deltaTime*5f;
			cof = Mathf.Min(1f,cof);
			obj.transform.position = Vector3.Lerp(startPos,endPos,cof);
			obj.transform.localScale = Vector3.Lerp(startScale,endScale,cof);
			yield return new WaitForEndOfFrame();
		}
		Game.instance.SetSlime (b.posX, b.posY);
		Destroy (obj);
		Game.instance.PlayJumpers ();
		yield return null;

	}
	public void CauseDamage(int damage,float time)
	{
		int count = 0;
		for(int i = 0; i < wizards.Count; i++)
		{
			if(wizards[i].currentHealth > 0)
			{
				if(wizards[i].currentHealth < damage)
				{
					int leftDamage = damage - wizards[i].currentHealth;
					wizards[i].CauseDamage(damage,time);
					int j = i;
					count++;
					while(j+1 < wizards.Count && leftDamage > 0f)
					{
						j++;
						int d;
						if(wizards[j].currentHealth < leftDamage)
						{
							count++;
							d = leftDamage - wizards[j].currentHealth;
						}
						else
							d = 0;
						wizards[j].CauseDamage(leftDamage,time);
						leftDamage = d;
					}
				}
				else
					wizards[i].CauseDamage(damage,time);
				WorldCameraManager.instance.Run(count);
				return;
			}
		}
	}
	public void HideCurrentWizardDamage()
	{
		for(int i = 0; i < wizards.Count; i++)
		{
			wizards[i].HideHealth();
		}
	}
}
