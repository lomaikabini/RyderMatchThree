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
		float val = ((float)damage / (float)config.health);
		val = Mathf.Max(0f,healthBarFact.fillAmount-val);
		healthBarCurrent.fillAmount = val;
	}
	public void CauseDamage(int damage,float time)
	{
		currentHealth -= damage;
		if (currentHealth <= 0)
			Game.instance.KelledWizard ();
		float val = ((float)damage / (float)config.health);
		val = Mathf.Max(0f,healthBarFact.fillAmount-val);
		StartCoroutine (moveHealthBar (time, val));
	}
	IEnumerator moveHealthBar(float time,float val)
	{
		yield return new WaitForEndOfFrame ();
		float startValue = healthBarFact.fillAmount;
		float cof = 0f;
		while(cof < 1f)
		{
			float delta = Time.deltaTime/time;
			cof = Mathf.Min(1f,cof+delta);
			healthBarFact.fillAmount = Mathf.Lerp(startValue,val,cof);
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds (1f);
		HideHealth ();
		yield return null;
	}
	public void HideHealth()
	{
		healthView.text = "";
		healthBarCurrent.fillAmount = healthBarFact.fillAmount;
	}

	public void SetResistView(Sprite sp)
	{
		resistView.sprite = sp;
	}
}
