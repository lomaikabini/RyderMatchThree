using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {

	public RectTransform map;
	public RectTransform frameView;
	public static Map instance;
	private int startCount = 5;
	GameData data;

	void Awake()
	{
		data = GameData.Get ();
		instance = this;
		GameObject[] objs = GameObject.FindGameObjectsWithTag("lvl");
		int count = 0;
		for(int i = 0; i < objs.Length; i++)
		{
			int lvl = objs[i].GetComponent<MapItem>().lvl;
			if(lvl > data.unlockLvls)
				objs[i].SetActive(false);
			else
				count++;
		}
//		int dif = Mathf.Max (0, count - objs.Length);
//		Rect r = frameView.rect;
//		r.height = r.height + dif * 500f;
//		frameView.rect = r;
	}

	void Update () 
	{
		map.position = frameView.position;	
	}

	public void OnMapItemClick(int lvl)
	{
		data.currentLvl = lvl;
		data.save ();
		GoTo.LoadGame ();
	}
}
