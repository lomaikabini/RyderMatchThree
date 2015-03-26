using UnityEngine;
using System.Collections;

public class BubbleDamageEssence {
	public FieldItem.Type type;
	public Bubble.BoosterType boosterType;
	public int damage;

	public BubbleDamageEssence(FieldItem.Type t,int d)
	{
		type = t;
		damage = d;
	}
	public BubbleDamageEssence(Bubble.BoosterType t,int d)
	{
		boosterType = t;
		damage = d;
	}
	public BubbleDamageEssence (){}
}
