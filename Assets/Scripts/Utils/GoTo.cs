using UnityEngine;
using System.Collections;

public class GoTo{

	public static void LoadGame()
	{
		Application.LoadLevel("Game");
	}

	public static void LoadEditor()
	{
		Application.LoadLevel("editor");
	}
}

