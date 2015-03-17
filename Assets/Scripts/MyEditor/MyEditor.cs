using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

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

	private float bubbleSize;
	private float bubblesOffset;
	private int TableSize = 7;
	private float BubblePadding = 5;

	private CellEditor[,] cells;
	private BubbleEditor[,] bubbles;
	private SeparatorEditor[,] separatorsHorizontal;
	private SeparatorEditor[,] separatorsVertical;

	List<Bubble.Type> availableTypes =  new List<Bubble.Type>();
	public static MyEditor instance;

	EditorState editorState = EditorState.free;
	CellEditor insertCell;
	SeparatorEditor insertSeparator;
	BubbleEditor insertBubble;
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
		bubbles = new BubbleEditor[TableSize, TableSize];
		cells = new CellEditor[TableSize, TableSize];
		separatorsHorizontal = new SeparatorEditor[TableSize, TableSize];
		separatorsVertical = new SeparatorEditor[TableSize, TableSize];
		instantiateEditorCells ();
		instantiateEditorItems ();
		instantiateEditorSeparators ();
		instantiateEditorBubbles ();
		instantiateEditorBubblesInGame ();
		calculateBubblesValues ();
		fillTableCells ();
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
			cell.SetType(Cell.Type.empty,bubbleSize);
		}
	}
	void insertBubbleInTable (BubbleEditor bubble)
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
			b.tx.text = "Not available";
			availableTypes.Remove(b.type);
		}
		else
		{
			b.tx.text = "Available";
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

	public void OnBubbleClick (BubbleEditor c)
	{
		inputHeandler (c.posX, c.posY);
	}

	public void OnCellClick(CellEditor c)
	{
		inputHeandler (c.posX, c.posY);
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
			cells[posX,posY].SetType(insertCell.type);
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
				cells[posX,posY].SetType(Cell.Type.empty);
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
		
		if(editorState == EditorState.clear)
		{
			cells[posX,posY].SetType(Cell.Type.empty);
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
			bubEditor.tx.text = "Available";
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
				cellEditor.lives = (cellInfo.sprites.Length - j);
				cellEditor.isMenu = true;
				CellEditor.SpritesKit = c.SpritesKit;
				if(cellInfo.destroyType == Cell.Sprites.DestroyType.notDestroy)
					cellEditor.tx.text = "Not Destroy";
				else
					cellEditor.tx.text = "Lives: "+ (cellInfo.sprites.Length - j);
			}
		}
	}
}
