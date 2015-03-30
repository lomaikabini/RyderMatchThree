using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Wizard : MonoBehaviour {

	[HideInInspector]
	public WizardEssence config;
	[HideInInspector]
	public int currentHealth;
	public Text healthView;
	public Image resistView;
	public Image healthBarFact;
	public Image healthBarCurrent;

	public void ShowHealth(int damage)
	{
		healthView.text = damage.ToString () + "/" + currentHealth.ToString ();
	}

	public void HideHealth()
	{
		healthView.text = "";
	}

	public void SetResistView(Sprite sp)
	{
		resistView.sprite = sp;
	}
}
