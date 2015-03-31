using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using LitJson;
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

	public GameObject wizardEditorPrefab;
	public Transform wizardsWrapper;

	public GameObject rewriteLvlMenu;	

	public List<Sprite> goalSprites;

	private float bubbleSize;
	private float bubblesOffset;
	private int TableSize = 7;
	private float BubblePadding = 5;
	
	private LevelEditor levelEditor;
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
		levelEditor = new LevelEditor (TableSize);
		instance = this;
		InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
		submitEvent.AddListener(SubmitMoves);
		movesField.onEndEdit = submitEvent;

		InputField.SubmitEvent submitEvent2 = new InputField.SubmitEvent();
		submitEvent2.AddListener(OnLvlChanged);
		lvlField.onEndEdit = submitEvent2;

		instantiateEditorCells ();
		instantiateEditorItems ();
		instantiateEditorSeparators ();
		instantiateEditorBubbles ();
		instantiateEditorBubblesInGame ();
		instantiateEditorBubblesDamage ();
		instantiateEditorBoostersDamage ();
		instantiateEditorGoals ();
		instantiateEditorWizards ();
		calculateBubblesValues ();
		fillTableCells ();
	}
	private void SubmitMoves(string count)
	{
		levelEditor.moves = int.Parse(count);
	}
	public void OnLvlChanged(string lvl)
	{
		if(!lvl.Equals(""))
		{
			levelEditor.curentLvl = int.Parse(lvl);
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
			cell.cellInfo.posX = i;
			cell.cellInfo.posY = j;
			levelEditor.cells[i,j] = cell;
			insertCellTable(cell);
			cell.SetType(Cell.Type.empty,bubbleSize,1);
		}
	}
	void insertBubbleInTable (BubbleEditor bubble)
	{
		bubble.transform.SetParent (BubbleContainer.transform);
		bubble.transform.localScale = new Vector3 (1f, 1f, 1f);
		bubble.transform.localPosition = new Vector3 ((float)bubble.bubbleConfig.posX * bubbleSize + ((float)(bubble.bubbleConfig.posX) * BubblePadding) - bubblesOffset, 
		                                              (float)bubble.bubbleConfig.posY * bubbleSize + ((float)(bubble.bubbleConfig.posY) * BubblePadding) - bubblesOffset, 0f);
	}
	void insertItemInTable (ItemEditor bubble)
	{
		bubble.transform.SetParent (BubbleContainer.transform);
		bubble.transform.localScale = new Vector3 (1f, 1f, 1f);
		bubble.transform.localPosition = new Vector3 ((float)bubble.itemConfig.posX * bubbleSize + ((float)(bubble.itemConfig.posX) * BubblePadding) - bubblesOffset, 
		                                              (float)bubble.itemConfig.posY * bubbleSize + ((float)(bubble.itemConfig.posY) * BubblePadding) - bubblesOffset, 0f);
	}
	void insertCellTable(CellEditor cell)
	{
		cell.transform.SetParent(CellsContainer.transform);
		cell.rectTransform.localScale = new Vector3 (1f, 1f, 1f);
		cell.transform.localPosition = new Vector3 ((float)cell.cellInfo.posX * bubbleSize + ((float)(cell.cellInfo.posX) * BubblePadding)-bubblesOffset, 
		                                            (float)cell.cellInfo.posY * bubbleSize + ((float)(cell.cellInfo.posY) * BubblePadding)-bubblesOffset, 0f);
	}
	void insertSeparatorTable(SeparatorEditor separ)
	{
		separ.transform.SetParent(SeparatorContainer.transform);
		separ.rectTransform.localScale = new Vector3 (1f, 1f, 1f);
		if(separ.separatorConfig.type == Separator.Type.vertical)
		{
			separ.transform.localPosition = new Vector3 ((float)separ.separatorConfig.posX * bubbleSize+bubbleSize/2f+BubblePadding/2f + ((float)(separ.separatorConfig.posX) * BubblePadding)-bubblesOffset, 
			                                             (float)separ.separatorConfig.posY * bubbleSize + ((float)(separ.separatorConfig.posY) * BubblePadding)-bubblesOffset, 0f);
		}
		else
		{
			separ.rectTransform.localRotation =Quaternion.Euler(new Vector3(0f,0f,90f));
			separ.transform.localPosition = new Vector3 ((float)separ.separatorConfig.posX * bubbleSize+ ((float)(separ.separatorConfig.posX) * BubblePadding)-bubblesOffset, 
			                                             (float)separ.separatorConfig.posY * bubbleSize - bubbleSize/2f - BubblePadding/2f + ((float)(separ.separatorConfig.posY) * BubblePadding)-bubblesOffset, 0f);
		}
	}
	void calculateBubblesValues ()
	{
		bubbleSize = (BubbleContainer.rect.height - ((TableSize + 1) * BubblePadding)) / (float)TableSize;
		bubblesOffset = (BubbleContainer.rect.height/2f) - bubbleSize/2f - BubblePadding;
	}
	public void BubbleInGameClick(BubbleInGameEditor b)
	{
		if(levelEditor.availableTypes.Exists(a=> a== b.type))
		{
			b.tx.text = "--";
			levelEditor.availableTypes.Remove(b.type);
		}
		else
		{
			b.tx.text = "++";
			levelEditor.availableTypes.Add(b.type);
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
		inputHeandler (c.bubbleConfig.posX, c.bubbleConfig.posY);
	}

	public void OnCellClick(CellEditor c)
	{
		inputHeandler (c.cellInfo.posX, c.cellInfo.posY);
	}

	public void OnItemClick (ItemEditor itemEditor)
	{
		inputHeandler (itemEditor.itemConfig.posX, itemEditor.itemConfig.posY);
	}

	public void OnDamageChanged(BubbleEditorDamage b, int damage)
	{
		levelEditor.bubblesDamages [b.type] = damage;
	}
	public void OnDamageChangedBooster(BubbleEditorDamage b, int damage)
	{
		levelEditor.boostersDamages [b.boosterType] = damage;
	}
	public void OnPlayClick()
	{
		GameData.Get ().currentLvl = levelEditor.curentLvl;
		GameData.Get ().save ();
		GoTo.LoadGame ();
	}

	public void OnGoalChanged(BubbleEditorDamage b, int count)
	{
		if(b.isWizard)
		{
			if(levelEditor.goals.ContainsKey("wizard"))
				levelEditor.goals["wizard"] = count;
			else
				levelEditor.goals.Add("wizard",count);
		}
		else if(b.isCell)
		{
			if(levelEditor.goals.ContainsKey(b.cellType.ToString()))
			{
				levelEditor.goals[b.cellType.ToString()] = count;
			}
			else
			{
				levelEditor.goals.Add(b.cellType.ToString(),count);
			}
		}
		else if(b.type == FieldItem.Type.item)
		{
			if(levelEditor.goals.ContainsKey(b.itemType.ToString()))
			{
				levelEditor.goals[b.itemType.ToString()] = count;
			}
			else
			{
				levelEditor.goals.Add(b.itemType.ToString(),count);
			}
		}
		else
		{
			if(levelEditor.goals.ContainsKey(b.type.ToString()))
			{
				levelEditor.goals[b.type.ToString()] =  count;
			}
			else
			{
				levelEditor.goals.Add(b.type.ToString(),count);
			}
		}
	}

	void inputHeandler(int posX, int posY)
	{
		if(editorState == EditorState.insertCells)
		{
			if(levelEditor.bubbles[posX,posY] != null)
			{
				Destroy(levelEditor.bubbles[posX,posY].gameObject);
				levelEditor.bubbles[posX,posY] = null;
			}
			if(levelEditor.items[posX,posY] != null)
			{
				Destroy(levelEditor.items[posX,posY].gameObject);
				levelEditor.items[posX,posY] = null;
			}
			levelEditor.cells[posX,posY].SetType(insertCell.cellInfo.type,-1,insertCell.cellInfo.lives);
		}
		if(editorState == EditorState.insertSeparators)
		{
			GameObject obj = Instantiate(separatorEditorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			SeparatorEditor separ = obj.GetComponent<SeparatorEditor>(); 
			separ.tx.enabled = false;
			separ.separatorConfig.posX = posX;
			separ.separatorConfig.posY = posY;
			if(insertSeparator.separatorConfig.type == Separator.Type.vertical)
			{
				if(levelEditor.separatorsVertical[separ.separatorConfig.posX,separ.separatorConfig.posY] != null)
				{
					Destroy(levelEditor.separatorsVertical[separ.separatorConfig.posX,separ.separatorConfig.posY].gameObject);
				}
				levelEditor.separatorsVertical[separ.separatorConfig.posX,separ.separatorConfig.posY] = separ;
			}
			else
			{
				if(levelEditor.separatorsHorizontal[separ.separatorConfig.posX,separ.separatorConfig.posY] != null)
				{
					Destroy(levelEditor.separatorsHorizontal[separ.separatorConfig.posX,separ.separatorConfig.posY].gameObject);
				}
				levelEditor.separatorsHorizontal[separ.separatorConfig.posX,separ.separatorConfig.posY] = separ;
			}
			separ.SetType(insertSeparator.separatorConfig.type,insertSeparator.separatorConfig.destroyType,bubbleSize,insertSeparator.separatorConfig.lives);
			insertSeparatorTable(separ);
		}
		
		if(editorState == EditorState.insertBubbles)
		{
			if(levelEditor.bubbles[posX,posY] != null)
			{
				Destroy(levelEditor.bubbles[posX,posY].gameObject);
				levelEditor.bubbles[posX,posY] = null;
			}
			if(levelEditor.cells[posX,posY].cellInfo.type != Cell.Type.empty)
			{
				levelEditor.cells[posX,posY].SetType(Cell.Type.empty,-1,1);
			}
			if(levelEditor.items[posX,posY] != null)
			{
				Destroy(levelEditor.items[posX,posY].gameObject);
				levelEditor.items[posX,posY] = null;
			}
			GameObject obj = Instantiate(bubbleEditorPrefab,Vector3.zero,Quaternion.identity) as GameObject;
			BubbleEditor bubble = obj.GetComponent<BubbleEditor>(); 
			bubble.bubbleConfig.posX = posX;
			bubble.bubbleConfig.posY = posY;
			levelEditor.bubbles[posX,posY] = bubble;
			bubble.SetType (insertBubble.bubbleConfig.type, bubbleSize);
			bubble.tx.enabled = false;
			insertBubbleInTable(bubble);
		}
		if(editorState == EditorState.insertItems)
		{
			if(levelEditor.bubbles[posX,posY] != null)
			{
				Destroy(levelEditor.bubbles[posX,posY].gameObject);
				levelEditor.bubbles[posX,posY] = null;
			}
			if(levelEditor.cells[posX,posY].cellInfo.type != Cell.Type.empty)
			{
				levelEditor.cells[posX,posY].SetType(Cell.Type.empty,-1,1);
			}
			if(levelEditor.items[posX,posY] != null)
			{
				Destroy(levelEditor.items[posX,posY].gameObject);
				levelEditor.items[posX,posY] = null;
			}
			GameObject obj = Instantiate(itemEditorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			ItemEditor it = obj.GetComponent<ItemEditor>();
			it.itemConfig.posX = posX;
			it.itemConfig.posY = posY;
			levelEditor.items[posX,posY] =  it;
			it.SetType(insertItem.itemConfig.type,bubbleSize);
			insertItemInTable(it);
		}
		if(editorState == EditorState.clear)
		{
			levelEditor.cells[posX,posY].SetType(Cell.Type.empty,-1,1);
			if(levelEditor.separatorsHorizontal[posX,posY] != null)
			{
				Destroy(levelEditor.separatorsHorizontal[posX,posY].gameObject);
				levelEditor.separatorsHorizontal[posX,posY] = null;
			}
			if(levelEditor.separatorsVertical[posX,posY] != null)
			{
				Destroy(levelEditor.separatorsVertical[posX,posY].gameObject);
				levelEditor.separatorsVertical[posX,posY] = null;
			}
			if(levelEditor.bubbles[posX,posY] != null)
			{
				Destroy(levelEditor.bubbles[posX,posY].gameObject);
				levelEditor.bubbles[posX,posY] = null;
			}
			if(levelEditor.items[posX,posY] != null)
			{
				Destroy(levelEditor.items[posX,posY].gameObject);
				levelEditor.items[posX,posY] = null;
			}
		}
	}
	public void OnBtnClearAllClick()
	{
		for(int i = 0; i < TableSize;i++)
			for(int j = 0; j < TableSize;j++)
		{
			levelEditor.cells[i,j].SetType(Cell.Type.empty,-1,1);
			if(levelEditor.separatorsHorizontal[i,j] != null)
			{
				Destroy(levelEditor.separatorsHorizontal[i,j].gameObject);
				levelEditor.separatorsHorizontal[i,j] = null;
			}
			if(levelEditor.separatorsVertical[i,j] != null)
			{
				Destroy(levelEditor.separatorsVertical[i,j].gameObject);
				levelEditor.separatorsVertical[i,j] = null;
			}
			if(levelEditor.bubbles[i,j] != null)
			{
				Destroy(levelEditor.bubbles[i,j].gameObject);
				levelEditor.bubbles[i,j] = null;
			}
			if(levelEditor.items[i,j] != null)
			{
				Destroy(levelEditor.items[i,j].gameObject);
				levelEditor.items[i,j] = null;
			}
		}
	}

	public void OnBtnSaveClick()
	{
		levelEditor.Save ();
	}

	public void OnBtnClearClick()
	{
		editorState = EditorState.clear;
	}
	public void OnTrySaveExistLvl()
	{
		rewriteLvlMenu.SetActive (true);
	}
	public void OnBtnRewriteClick()
	{
		levelEditor.Save (true);
	}
	void instantiateEditorWizards ()
	{
		for(int i = 0; i < 8;i++)
		{
			GameObject obj = Instantiate(wizardEditorPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			WizardEditor we = obj.GetComponent<WizardEditor>();
			we.SetName(i);
			obj.transform.SetParent(wizardsWrapper);
			obj.transform.localScale = new Vector3(1f,1f,1f);
			levelEditor.wizards.Add(we);
		}
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
			levelEditor.availableTypes.Add(type);
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
			bubEditor.bubbleConfig.type = type;
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


		GameObject w = Instantiate(bubbleDamagePrefab,Vector3.zero,Quaternion.identity) as GameObject;
		BubbleEditorDamage wEditor = w.GetComponent<BubbleEditorDamage>();
		w.transform.SetParent(goalContainer);
		w.transform.localScale = new Vector3(1f,1f,1f);
		wEditor.isGoal = true;
		wEditor.isWizard = true;
		wEditor.img.sprite = goalSprites.Find(c => c.name.Equals("wizard"));
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
			levelEditor.bubblesDamages.Add(type,0);
		}
	}
	void instantiateEditorBoostersDamage ()
	{
		Bubble bubble = bubblePrefab.GetComponent<Bubble> ();
		for(int i = 0;i < Enum.GetNames(typeof(Bubble.BoosterType)).Length;i++)
		{
			Bubble.BoosterType type = (Bubble.BoosterType)i;
			if(type == Bubble.BoosterType.none) continue;
			GameObject obj = Instantiate(bubbleDamagePrefab,Vector3.zero,Quaternion.identity) as GameObject;
			BubbleEditorDamage bubEditor = obj.GetComponent<BubbleEditorDamage>();
			BubbleEditor.boosterImages = bubble.boosterImages;
			obj.transform.SetParent(bubbleDamageContainer);
			obj.transform.localScale = new Vector3(1f,1f,1f);
			bubEditor.boosterType = type;
			bubEditor.isBooster = true;
			bubEditor.img.sprite = bubble.boosterImages.Find(a => {return a.name.Contains(type.ToString())? a : null;});
			levelEditor.boostersDamages.Add(type,0);
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
					sepEditor.separatorConfig.destroyType = destroyType;
					sepEditor.separatorConfig.type = type;
					sepEditor.separatorConfig.lives = separInfo.sprites.Length-j;
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
			itemEditor.itemConfig.type = type;
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
				cellEditor.cellInfo.type = (Cell.Type)i;
				cellEditor.cellInfo.lives = (j+1);
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
