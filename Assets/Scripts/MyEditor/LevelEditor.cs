using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using LitJson;
using System.IO;


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
		public List<CellEssence> cells = new List<CellEssence>();
		public List<SeparatorEssence> separators = new List<SeparatorEssence> ();
		public List<BubbleEssence> bubbles = new List<BubbleEssence> ();
		public List<ItemEssence> items = new List<ItemEssence> ();
		public List<BubbleDamageEssence> bubblesDamages = new List<BubbleDamageEssence>();
		public Dictionary<string,int> goals = new Dictionary<string, int>();
		public List<Bubble.Type> availableTypes =  new List<Bubble.Type>();
		public List<WizardEssence> wizards = new List<WizardEssence> ();
		public LevelEditorSerializable(){}
		public LevelEditorSerializable(LevelEditor conf)
		{
			moves = conf.moves;
			curentLvl = conf.curentLvl;
			for(int i = 0; i < LevelEditor.tableSize; i++)
				for(int j = 0; j < LevelEditor.tableSize; j++)
			{
					cells.Add(conf.cells[i,j].cellInfo);
				if(conf.separatorsHorizontal[i,j]!= null)
					separators.Add(conf.separatorsHorizontal[i,j].separatorConfig);
				if(conf.separatorsVertical[i,j]!= null)
					separators.Add(conf.separatorsVertical[i,j].separatorConfig);
				if(conf.bubbles[i,j] != null)
					bubbles.Add(conf.bubbles[i,j].bubbleConfig);
				if(conf.items[i,j] != null)
					items.Add(conf.items[i,j].itemConfig);
			}
			for(int i =0; i < conf.wizards.Count; i++)
			{
				wizards.Add(conf.wizards[i].wizardConfig);
			}
			for(int i =0; i < conf.availableTypes.Count; i++)
			{
				availableTypes.Add(conf.availableTypes[i]);
			}
			foreach(KeyValuePair<string,int> k in conf.goals)
			{
				goals.Add(k.Key,k.Value);
			}
			foreach(KeyValuePair<FieldItem.Type,int> obj in conf.bubblesDamages)
			{
				bubblesDamages.Add(new BubbleDamageEssence(obj.Key,obj.Value));
			}
		}
	}

	public static LevelEditorSerializable LoadLevel(int id)
	{
		string fileName =String.Concat(Directory.GetCurrentDirectory(), "/Assets/Levels/" ,"Level " ,id.ToString(),".txt");
		if (File.Exists (fileName))
		{
			string s = File.ReadAllText(fileName);
			LevelEditorSerializable data;
			try
			{
				data = JsonMapper.ToObject<LevelEditorSerializable>(s);
			}
			catch (System.Exception e)
			{
				Debug.Log(e);
				return null;
			}
			return data;
		}
		else
		{
			Debug.LogError("Level didn't find on path: "+fileName);
			return null;
		}
	}

	public void Save(bool rewrite = false)
	{
		string fileName =String.Concat(Directory.GetCurrentDirectory(), "/Assets/Levels/" ,"Level " ,curentLvl.ToString(),".txt");
		if (!CheckFile (fileName) || rewrite)
			WriteToFile (JsonMapper.ToJson (new LevelEditorSerializable (this)), fileName);
		else
		{
			MyEditor.instance.OnTrySaveExistLvl();
		}
	}

	public bool CheckFile(string fileName)
	{
		if (File.Exists (fileName))
			return true;
		else
			return false;
	}
	public void WriteToFile(string text,string fileName)
	{
			StreamWriter fileWriter;
			fileWriter = File.CreateText (fileName);
			fileWriter.WriteLine (text);
			fileWriter.Close ();
			Debug.Log ("Save on path: "+fileName);
	}
}
