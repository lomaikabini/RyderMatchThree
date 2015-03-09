using UnityEngine;
using System.Collections;

public class Curtain : MonoBehaviour {

	public void CurtainClosed()
	{
		Game.Get ().Restart ();
	}
}
