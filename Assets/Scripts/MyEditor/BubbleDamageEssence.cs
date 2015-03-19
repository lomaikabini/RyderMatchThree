using UnityEngine;
using System.Collections;

public class BubbleDamageEssence {
	public FieldItem.Type type;
	public int damage;

	public BubbleDamageEssence(FieldItem.Type t,int d)
	{
		type = t;
		damage = d;
	}
	public BubbleDamageEssence (){}
}
