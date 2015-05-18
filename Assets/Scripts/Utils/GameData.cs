using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using LitJson;

public class GameData{
	
	private static GameData instance;
	public int currentLvl;
	public int unlockLvls;
	private string versionId = "save_002";
	public static GameData Get()
	{
		if (instance == null)
		{
			instance = new GameData();
			instance = instance.Load();
		}
		return instance;
	}
	
	
	GameData Load ()
	{
		string data = PlayerPrefs.GetString(versionId, null);
		Debug.Log("Load game data:" + data);
		if (data == null || data.Trim() == "")
		{
			reset();
			return this;
		}
		GameData gdata;
		try
		{
			gdata = JsonMapper.ToObject<GameData>(data);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			reset();
			return this;
		}
		return gdata;
	}
	
	void reset ()
	{
		currentLvl = 1;
		unlockLvls = 10;
	}
	
	public void save ()
	{
		string data = JsonMapper.ToJson(this);
		Debug.Log("Save gamedata as:" + data);
		PlayerPrefs.SetString(versionId, data);
	}
	
}