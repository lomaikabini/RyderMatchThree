using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class MyEditor : MonoBehaviour {

	public GameObject cellPrefab;
	public GameObject cellEditorPrefab;
	public Transform cellList;

	public GameObject bubblePrefab;
	public GameObject itemEditorPrefab;
	public Transform itemList;

	public GameObject separatorPrefab;
	public GameObject separatorEditorPrefab;
	public Transform separatorList;

	public GameObject bubbleEditorPrefab;
	public Transform bubbleList;
	public Transform bubbleInGameList;

	public RectTransform BubbleContainer;
	public RectTransform CellsContainer;
	public RectTransform SeparatorContainer;

	public InputField movesField;
	public InputField lvlField;

	public GameObject bubbleDamagePrefab;
	public RectTransform bubbleDamageContainer;

	public RectTransform goalContainer;

	public GameObject wizardsWrapper;

	private float bubbleSize;
	private float bubblesOffset;
	private int TableSize = 7;
	private float BubblePadding = 5;
	private int moves = 5;
	private int curentLvl = 1;

	private CellEditor[,] cells;
	private BubbleEditor[,] bubbles;
	private ItemEditor[,] items;
	private SeparatorEditor[,] separatorsHorizontal;
	private SeparatorEditor[,] separatorsVertical;
	private Dictionary<FieldItem.Type,int> bubblesDamages =  new Dictionary<FieldItem.Type, int>();
	private Dictionary<string,int> goals = new Dictionary<string, int>();
	private List<Bubble.Type> availableTypes =  new List<Bubble.Type>();

	public static MyEditor instance;

	EditorState editorState = EditorState.free;
	CellEditor insertCell;
	SeparatorEditor insertSeparator;
	BubbleEditor insertBubble;
	ItemEditor insertItem;
	public enum EditorState
	{
		insertItems,
		insertBubbles,
		insertSeparators,
		insertCells,
		clear,
		free
	}

	void Start () 
	{
		instance = this;

		InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
		submitEvent.AddListener(SubmitMoves);
		movesField.onEndEdit = submitEvent;

		InputField.SubmitEvent submitEvent2 = new InputField.SubmitEvent();
		submitEvent2.AddListener(OnLvlChanged);
		lvlField.onEndEdit = submitEvent2;


		bubbles = new BubbleEditor[TableSize, TableSize];
		cells = new CellEditor[TableSize, TableSize];
		separatorsHorizontal = new SeparatorEditor[TableSize, TableSize];
		separatorsVertical = new SeparatorEditor[TableSize, TableSize];
		items = new ItemEditor[TableSize, TableSize];
		instantiateEditorCells ();
		instantiateEditorItems ();
		instantiateEditorSeparators ();
		instantiateEditorBubbles ();
		instantiateEditorBubblesInGame ();
		instantiateEditorBubblesDamage ();
		instantiateEditorGoals ();
		calculateBubblesValues ();
		fillTableCells ();
	}
	private void SubmitMoves(string count)
	{
		moves = int.Parse(count);
	}
	public void OnLvlChanged(string lvl)
	{
		if(!lvl.Equals(""))
		{
			curentLvl = int.Parse(lvl);
			Debug.Log(curentLvl);
		}
	}
	void fillTableCells ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize;j++)
		{
			GameObject obj = Instantiate(cellEditorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			CellEditor cell = obj.GetComponent<CellEditor>(); 
			cell.tx.enabled = false;
			cell.posX = i;
			cell.posY = j;
			cells[i,j] = cell;
			insertCellTable(cell);
			cell.SetType(Cell.Type.empty,bubbleSize,1);
		}
	}
	void insertBubbleInTable (BubbleEditor bubble)
	{
		bubble.transform.SetParent (BubbleContainer.transform);
		bubble.transform.localScale = new Vector3 (1f, 1f, 1f);
		bubble.transform.localPosition = new Vector3 ((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding) - bubblesOffset, 
		                                              (float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding) - bubblesOffset, 0f);
	}
	void insertItemInTable (ItemEditor bubble)
	{
		bubble.transform.SetParent (BubbleContainer.transform);
		bubble.transform.localScale = new Vector3 (1f, 1f, 1f);
		bubble.transform.localPosition = new Vector3 ((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding) - bubblesOffset, 
		                                              (float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding) - bubblesOffset, 0f);
	}
	void insertCellTable(CellEditor cell)
	{
		cell.transform.SetParent(CellsContainer.transform);
		cell.rectTransform.localScale = new Vector3 (1f, 1f, 1f);
		cell.transform.localPosition = new Vector3 ((float)cell.posX * bubbleSize + ((float)(cell.posX) * BubblePadding)-bubblesOffset, 
		                                            (float)cell.posY * bubbleSize + ((float)(cell.posY) * BubblePadding)-bubblesOffset, 0f);
	}
	void insertSeparatorTable(SeparatorEditor separ)
	{
		separ.transform.SetParent(SeparatorContainer.transform);
		separ.rectTransform.localScale = new Vector3 (1f, 1f, 1f);
		if(separ.type == Separator.Type.vertical)
		{
			separ.transform.localPosition = new Vector3 ((float)separ.posX * bubbleSize+bubbleSize/2f+BubblePadding/2f + ((float)(separ.posX) * BubblePadding)-bubblesOffset, 
			                                             (float)separ.posY * bubbleSize + ((float)(separ.posY) * BubblePadding)-bubblesOffset, 0f);
		}
		else
		{
			separ.rectTransform.localRotation =Quaternion.Euler(new Vector3(0f,0f,90f));
			separ.transform.localPosition = new Vector3 ((float)separ.posX * bubbleSize+ ((float)(separ.posX) * BubblePadding)-bubblesOffset, 
			                                             (float)separ.posY * bubbleSize - bubbleSize/2f - BubblePadding/2f + ((float)(separ.posY) * BubblePadding)-bubblesOffset, 0f);
		}
	}
	void calculateBubblesValues ()
	{
		bubbleSize = (BubbleContainer.rect.height - ((TableSize + 1) * BubblePadding)) / (float)TableSize;
		bubblesOffset = (BubbleContainer.rect.height/2f) - bubbleSize/2f - BubblePadding;
	}
	public void BubbleInGameClick(BubbleInGameEditor b)
	{
		if(availableTypes.Exists(a=> a== b.type))
		{
			b.tx.text = "--";
			availableTypes.Remove(b.type);
		}
		else
		{
			b.tx.text = "++";
			availableTypes.Add(b.type);
		}
	}
	public void OnMenuCellClick(CellEditor c)
	{
		editorState = EditorState.insertCells;
		insertCell = c;
	}
	public void OnMenuSeparatorClick(SeparatorEditor s)
	{
		editorState = EditorState.insertSeparators;
		insertSeparator = s;
	}

	public void OnMenuBubbleClick(BubbleEditor b)
	{
		editorState = EditorState.insertBubbles;
		insertBubble = b;
	}

	public void OnMenuItemClick(ItemEditor it)
	{
		editorState = EditorState.insertItems;
		insertItem = it;
	}

	public void OnBubbleClick (BubbleEditor c)
	{
		inputHeandler (c.posX, c.posY);
	}

	public void OnCellClick(CellEditor c)
	{
		inputHeandler (c.posX, c.posY);
	}

	public void OnItemClick (ItemEditor itemEditor)
	{
		inputHeandler (itemEditor.posX, itemEditor.posY);
	}

	public void OnDamageChanged(BubbleEditorDamage b, int damage)
	{
		bubblesDamages [b.type] = damage;
	}

	public void OnGoalChanged(BubbleEditorDamage b, int count)
	{
		if(b.isCell)
		{
			if(goals.ContainsKey(b.cellType.ToString()))
			{
				goals[b.cellType.ToString()] = count;
			}
			else
			{
				goals.Add(b.cellType.ToString(),count);
			}
		}
		else if(b.type == FieldItem.Type.item)
		{
			if(goals.ContainsKey(b.itemType.ToString()))
			{
				goals[b.itemType.ToString()] = count;
			}
			else
			{
				goals.Add(b.itemType.ToString(),count);
			}
		}
		else
		{
			if(goals.ContainsKey(b.type.ToString()))
			{
				goals[b.type.ToString()] =  count;
			}
			else
			{
				goals.Add(b.type.ToString(),count);
			}
		}
	}

	void inputHeandler(int posX, int posY)
	{
		if(editorState == EditorState.insertCells)
		{
			if(bubbles[posX,posY] != null)
			{
				Destroy(bubbles[posX,posY].gameObject);
				bubbles[posX,posY] = null;
			}
			if(items[posX,posY] != null)
			{
				Destroy(items[posX,posY].gameObject);
				items[posX,posY] = null;
			}
			cells[posX,posY].SetType(insertCell.type,-1,insertCell.lives);
		}
		if(editorState == EditorState.insertSeparators)
		{
			GameObject obj = Instantiate(separatorEditorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			SeparatorEditor separ = obj.GetComponent<SeparatorEditor>(); 
			separ.tx.enabled = false;
			separ.posX = posX;
			separ.posY = posY;
			if(insertSeparator.type == Separator.Type.vertical)
			{
				if(separatorsVertical[separ.posX,separ.posY] != null)
				{
					Destroy(separatorsVertical[separ.posX,separ.posY].gameObject);
				}
				separatorsVertical[separ.posX,separ.posY] = separ;
			}
			else
			{
				if(separatorsHorizontal[separ.posX,separ.posY] != null)
				{
					Destroy(separatorsHorizontal[separ.posX,separ.posY].gameObject);
				}
				separatorsHorizontal[separ.posX,separ.posY] = separ;
			}
			separ.SetType(insertSeparator.type,insertSeparator.destroyType,bubbleSize,insertSeparator.lives);
			insertSeparatorTable(separ);
		}
		
		if(editorState == EditorState.insertBubbles)
		{
			if(bubbles[posX,posY] != null)
			{
				Destroy(bubbles[posX,posY].gameObject);
				bubbles[posX,posY] = null;
			}
			if(cells[posX,posY].type != Cell.Type.empty)
			{
				cells[posX,posY].SetType(Cell.Type.empty,-1,1);
			}
			if(items[posX,posY] != null)
			{
				Destroy(items[posX,posY].gameObject);
				items[posX,posY] = null;
			}
			GameObject obj = Instantiate(bubbleEditorPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			BubbleEditor bubble = obj.GetComponent<BubbleEditor>(); 
			bubble.posX = posX;
			bubble.posY = posY;
			bubbles[posX,posY] = bubble;
			bubble.SetType (insertBubble.type, bubbleSize);
			bubble.tx.enabled = false;
			insertBubbleInTable(bubble);
		}
		if(editorState == EditorState.insertItems)
		{
			if(bubbles[posX,posY] != null)
			{
				Destroy(bubbles[posX,posY].gameObject);
				bubbles[posX,posY] = null;
			}
			if(cells[posX,posY].type != Cell.Type.empty)
			{
				cells[posX,posY].SetType(Cell.Type.empty,-1,1);
			}
			if(items[posX,posY] != null)
			{
				Destroy(items[posX,posY].gameObject);
				items[posX,posY] = null;
			}
			GameObject obj = Instantiate(itemEditorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			ItemEditor it = obj.GetComponent<ItemEditor>();
			it.posX = posX;
			it.posY = posY;
			items[posX,posY] =  it;
			it.SetType(insertItem.type,bubbleSize);
			insertItemInTable(it);
		}
		if(editorState == EditorState.clear)
		{
			cells[posX,posY].SetType(Cell.Type.empty,-1,1);
			if(separatorsHorizontal[posX,posY] != null)
			{
				Destroy(separatorsHorizontal[posX,posY].gameObject);
				separatorsHorizontal[posX,posY] = null;
			}
			if(separatorsVertical[posX,posY] != null)
			{
				Destroy(separatorsVertical[posX,posY].gameObject);
				separatorsVertical[posX,posY] = null;
			}
			if(bubbles[posX,posY] != null)
			{
				Destroy(bubbles[posX,posY].gameObject);
				bubbles[posX,posY] = null;
			}
		}
	}
	public void OnBtnClearAllClick()
	{
		for(int i = 0; i < TableSize;i++)
			for(int j = 0; j < TableSize;j++)
		{
			cells[i,j].SetType(Cell.Type.empty,-1,1);
			if(separatorsHorizontal[i,j] != null)
			{
				Destroy(separatorsHorizontal[i,j].gameObject);
				separatorsHorizontal[i,j] = null;
			}
			if(separatorsVertical[i,j] != null)
			{
				Destroy(separatorsVertical[i,j].gameObject);
				separatorsVertical[i,j] = null;
			}
			if(bubbles[i,j] != null)
			{
				Destroy(bubbles[i,j].gameObject);
				bubbles[i,j] = null;
			}
		}
	}
	public void OnBtnClearClick()
	{
		editorState = EditorState.clear;
	}
	void instantiateEditorBubblesInGame ()
	{
		Bubble bubble = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0;i < Enum.GetNames(typeof(Bubble.Type)).Length-1;i++)
		{
			Bubble.Type type = (Bubble.Type)i;
			GameObject obj = Instantiate(bubbleEditorPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			BubbleInGameEditor bubEditor = obj.GetComponent<BubbleInGameEditor>();
			obj.GetComponent<BubbleEditor>().enabled = false;
			obj.transform.SetParent(bubbleInGameList);
			obj.transform.localScale = new Vector3(1f,1f,1f);
			bubEditor.type = type;
			bubEditor.img.sprite = bubble.bubbleImages.Find(a => {return a.name == "bubble_"+type.ToString()? a : null;});
			bubEditor.tx.text = "++";
			availableTypes.Add(type);
		}
	}

	void instantiateEditorBubbles ()
	{
		Bubble bubble = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0;i < Enum.GetNames(typeof(Bubble.Type)).Length-1;i++)
		{
			Bubble.Type type = (Bubble.Type)i;
			GameObject obj = Instantiate(bubbleEditorPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			BubbleEditor bubEditor = obj.GetComponent<BubbleEditor>();
			BubbleEditor.bubbleImages = bubble.bubbleImages;
			obj.GetComponent<BubbleInGameEditor>().enabled = false;
			obj.transform.SetParent(bubbleList);
			obj.transform.localScale = new Vector3(1f,1f,1f);
			bubEditor.type = type;
			bubEditor.isMenu = true;
			bubEditor.img.sprite = bubble.bubbleImages.Find(a => {return a.name == "bubble_"+type.ToString()? a : null;});
		}
	}

	void instantiateEditorGoals ()
	{
		Bubble bubble = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0;i < Enum.GetNames(typeof(Bubble.Type)).Length-1;i++)
		{
			Bubble.Type type = (Bubble.Type)i;
			GameObject obj = Instantiate(bubbleDamagePrefab,Vector3.zero,Quaternion.identity) as GameObject;
			BubbleEditorDamage bubEditor = obj.GetComponent<BubbleEditorDamage>();
			BubbleEditor.bubbleImages = bubble.bubbleImages;
			obj.transform.SetParent(goalContainer);
			obj.transform.localScale = new Vector3(1f,1f,1f);
			bubEditor.type = type;
			bubEditor.isGoal = true;
			bubEditor.img.sprite = bubble.bubbleImages.Find(a => {return a.name == "bubble_"+type.ToString()? a : null;});
		}
		Item itm = bubblePrefab.GetComponent<Item> ();
		GameObject o = Instantiate(bubbleDamagePrefab,Vector3.zero,Quaternion.identity) as GameObject;
		BubbleEditorDamage bEditor = o.GetComponent<BubbleEditorDamage>();
		o.transform.SetParent(goalContainer);
		o.transform.localScale = new Vector3(1f,1f,1f);
		bEditor.type = FieldItem.Type.item;
		bEditor.itemType = Item.ItemType.gold;
		bEditor.isGoal = true;
		bEditor.img.sprite = itm.spritesIdle.Find(i => {return i.name == "item_"+Item.ItemType.gold.ToString()? i : null;});

		Cell cell = cellPrefab.GetComponent<Cell> ();
		GameObject ob = Instantiate(bubbleDamagePrefab,Vector3.zero,Quaternion.identity) as GameObject;
		BubbleEditorDamage baEditor = ob.GetComponent<BubbleEditorDamage>();
		ob.transform.SetParent(goalContainer);
		ob.transform.localScale = new Vector3(1f,1f,1f);
		baEditor.isGoal = true;
		baEditor.isCell = true;
		baEditor.cellType = Cell.Type.box;
		baEditor.img.sprite = cell.getKitByType (Cell.Type.box).sprites [0];

	}

	void instantiateEditorBubblesDamage ()
	{
		Bubble bubble = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0;i < Enum.GetNames(typeof(Bubble.Type)).Length-1;i++)
		{
			Bubble.Type type = (Bubble.Type)i;
			GameObject obj = Instantiate(bubbleDamagePrefab,Vector3.zero,Quaternion.identity) as GameObject;
			BubbleEditorDamage bubEditor = obj.GetComponent<BubbleEditorDamage>();
			BubbleEditor.bubbleImages = bubble.bubbleImages;
			obj.transform.SetParent(bubbleDamageContainer);
			obj.transform.localScale = new Vector3(1f,1f,1f);
			bubEditor.type = type;
			bubEditor.img.sprite = bubble.bubbleImages.Find(a => {return a.name == "bubble_"+type.ToString()? a : null;});
			bubblesDamages.Add(type,0);
		}
	}

	void instantiateEditorSeparators ()
	{
		Separator sep = separatorPrefab.GetComponent<Separator> ();
		for(int i = 0 ; i < Enum.GetNames(typeof(Separator.DestroyType)).Length;i++)
		{
			Separator.DestroyType destroyType = (Separator.DestroyType)i;
			Separator.Sprites separInfo = sep.GetKitByType(destroyType);
			for(int j = 0;j < separInfo.sprites.Length;j++)
			{
				for(int k =0; k <Enum.GetNames(typeof(Separator.Type)).Length;k++)
				{
					Separator.Type type = (Separator.Type)k;
					GameObject obj = Instantiate(separatorEditorPrefab,Vector3.zero,Quaternion.identity) as GameObject;
					SeparatorEditor sepEditor = obj.GetComponent<SeparatorEditor>();
					obj.transform.SetParent(separatorList);
					obj.transform.localScale = new Vector3(1f,1f,1f);
					sepEditor.destroyType = destroyType;
					sepEditor.type = type;
					sepEditor.lives = separInfo.sprites.Length-j;
					sepEditor.isMenu = true;
					SeparatorEditor.sprites = sep.sprites;
					sepEditor.img.sprite = separInfo.sprites[j];
					if(destroyType == Separator.DestroyType.notDestroy)
						sepEditor.tx.text = "Not Destroy";
					else
						sepEditor.tx.text = "Lives: "+ (separInfo.sprites.Length-j);
					if(type == Separator.Type.horizontal)
						sepEditor.img.transform.localRotation = Quaternion.Euler(new Vector3(0f,0f,90f));
				}
			}
		}
	}

	void instantiateEditorItems ()
	{
		Item itm = bubblePrefab.GetComponent<Item> ();
		for(int i = 0 ; i < Enum.GetNames(typeof(Item.ItemType)).Length;i++)
		{
			Item.ItemType type = (Item.ItemType)i;
			GameObject obj = Instantiate(itemEditorPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			ItemEditor itemEditor = obj.GetComponent<ItemEditor>();
			obj.transform.SetParent(itemList);
			obj.transform.localScale = new Vector3(1f,1f,1f);
			itemEditor.type = type;
			itemEditor.isMenu = true;
			ItemEditor.spritesIdle = itm.spritesIdle;
			itemEditor.img.sprite = itm.spritesIdle.Find(a => {return a.name == "item_"+type.ToString()? a : null;});
			itemEditor.tx.text = type.ToString();
		}
	}

	void instantiateEditorCells ()
	{
		Cell c = cellPrefab.GetComponent<Cell> ();
		for(int i = 0 ; i < Enum.GetNames(typeof(Cell.Type)).Length;i++)
		{
			Cell.Sprites cellInfo = c.getKitByType((Cell.Type)i);
			for(int j = 0;j < cellInfo.sprites.Length;j++)
			{
				GameObject obj = Instantiate(cellEditorPrefab,Vector3.zero,Quaternion.identity) as GameObject;
				CellEditor cellEditor = obj.GetComponent<CellEditor>();
				obj.transform.SetParent(cellList);
				obj.transform.localScale = new Vector3(1f,1f,1f);
				cellEditor.img.sprite = cellInfo.sprites[cellInfo.sprites.Length -1 - j];
				cellEditor.type = (Cell.Type)i;
				cellEditor.lives = (j+1);
				cellEditor.isMenu = true;
				CellEditor.SpritesKit = c.SpritesKit;
				if(cellInfo.destroyType == Cell.Sprites.DestroyType.notDestroy)
					cellEditor.tx.text = "Not Destroy";
				else
					cellEditor.tx.text = "Lives: "+ (j+1);
			}
		}
	}
}
