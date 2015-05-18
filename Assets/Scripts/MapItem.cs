using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapItem : MonoBehaviour {

	public Text lvlView;
	public int lvl;

	void Start()
	{
		SetLvl ();
	}

	public void SetLvl()
	{
		lvlView.text = lvl.ToString ();
	}

	public void ItemClick()
	{
		Map.instance.OnMapItemClick (lvl);
	}
}
