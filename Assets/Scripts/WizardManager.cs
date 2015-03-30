using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WizardManager : MonoBehaviour {

	public Transform wizardContainer;
	public GameObject wizardPrefab;
	public GameObject bubblePrefab;
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
	public void HideCurrentWizardDamage()
	{
		for(int i = 0; i < wizards.Count; i++)
		{
			wizards[i].HideHealth();
		}
	}
}
