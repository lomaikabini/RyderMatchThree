using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using LitJson;
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
	public static int tableSize;
	public LevelEditor (int TableSize)
	{
		tableSize = TableSize;
		bubbles = new BubbleEditor[TableSize, TableSize];
		cells = new CellEditor[TableSize, TableSize];
		separatorsHorizontal = new SeparatorEditor[TableSize, TableSize];
		separatorsVertical = new SeparatorEditor[TableSize, TableSize];
		items = new ItemEditor[TableSize, TableSize];
	}

	public class LevelEditorSerializable
	{
		public int moves = 5;
		public int curentLvl = 1;
		public CellEssence[,] cells;
		public LevelEditorSerializable(int m,int curl,CellEditor[,] c)
		{
			moves = m;
			curentLvl = curl;
			cells = new CellEssence[LevelEditor.tableSize,LevelEditor.tableSize];
			for(int i = 0; i < LevelEditor.tableSize; i++)
				for(int j = 0; j < LevelEditor.tableSize; j++)
					cells[i,j] = c[i,j].cellInfo;
		}
	}

	public void Save()
	{
		Debug.Log (JsonMapper.ToJson(new LevelEditorSerializable (moves,curentLvl,cells)));
	}
}
