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
	private List<Wizard> wizards;
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
		Bubble bubble = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].health <= 0) continue;
			GameObject obj = Instantiate(wizardPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			obj.transform.SetParent(wizardContainer);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = new Vector3(1f,1f,1f);
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
		Wizard w = null;
		for(int i = 0; i < wizards.Count; i++)
		{
			if(wizards[i].currentHealth > 0)
			{
				w = wizards[i];
				break;
			}
		}
		if (w != null)
		{
			int step = Game.instance.moves;
			if(true || step % w.config.slimePeriod == 0)
			{
				StartCoroutine(dropSlime(Game.instance.FindSlimeBubble(),w.transform.position));
			}
		}

	
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
		yield return null;

	}
	public void CauseDamage(int damage,float time)
	{
		for(int i = 0; i < wizards.Count; i++)
		{
			if(wizards[i].currentHealth > 0)
			{
				if(wizards[i].currentHealth < damage)
				{
					int leftDamage = damage - wizards[i].currentHealth;
					wizards[i].CauseDamage(damage,time);
					int j = i;
					while(j+1 < wizards.Count && leftDamage > 0f)
					{
						j++;
						int d;
						if(wizards[j].currentHealth < leftDamage)
							d = leftDamage - wizards[j].currentHealth;
						else
							d = 0;
						wizards[j].CauseDamage(leftDamage,time);
						leftDamage = d;
					}
				}
				else
					wizards[i].CauseDamage(damage,time);
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
