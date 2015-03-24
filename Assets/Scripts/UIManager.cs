using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

	public Text movesView;
	public Transform goalsContainer;
	public GameObject goalPrefab;
	public static UIManager instance;
	private Dictionary<string,Text> goals = new Dictionary<string, Text>();
	void Awake()
	{
		instance = this;
	}

	public void SetMovesView(int count)
	{
		movesView.text = count.ToString ();
	}

	public void InstantiateGoal(string type,Sprite sp,int count)
	{
		if (!goalsContainer.gameObject.activeSelf)
			goalsContainer.gameObject.SetActive (true);
		GameObject obj = Instantiate (goalPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		obj.transform.SetParent (goalsContainer);
		obj.transform.localScale = new Vector3 (1f, 1f, 1f);
		Image img = obj.transform.FindChild ("img").GetComponent<Image> ();
		img.sprite = sp;
		Text tx = img.transform.FindChild ("lbl").GetComponent<Text> ();
		goals.Add (type, tx);
		SetDictionaryView (type, count);
	}

	public void SetDictionaryView(string type,int count)
	{
		goals [type].text = count.ToString ();
	}

}
