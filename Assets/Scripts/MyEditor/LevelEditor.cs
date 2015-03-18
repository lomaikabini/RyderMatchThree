using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class LevelEditor
{
	public int moves = 5;
	public int curentLvl = 1;
	public CellEditor[,] cells;
	public BubbleEditor[,] bubbles;
	public ItemEditor[,] items;
	public SeparatorEditor[,] separatorsHorizontal;
	public SeparatorEditor[,] separatorsVertical;
	public Dictionary<FieldItem.Type,int> bubblesDamages =  new Dictionary<FieldItem.Type, int>();
	public Dictionary<string,int> goals = new Dictionary<string, int>();
	public List<Bubble.Type> availableTypes =  new List<Bubble.Type>();
	public List<WizardEditor> wizards = new List<WizardEditor> ();

	public LevelEditor (int TableSize)
	{
		bubbles = new BubbleEditor[TableSize, TableSize];
		cells = new CellEditor[TableSize, TableSize];
		separatorsHorizontal = new SeparatorEditor[TableSize, TableSize];
		separatorsVertical = new SeparatorEditor[TableSize, TableSize];
		items = new ItemEditor[TableSize, TableSize];
	}

}
