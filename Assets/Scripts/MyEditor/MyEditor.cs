using UnityEngine;
using System.Collections;
using System;

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

	void Start () 
	{
		instantiateEditorCells ();
		instantiateEditorItems ();
		instantiateEditorSeparators ();
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
				if(cellInfo.destroyType == Cell.Sprites.DestroyType.notDestroy)
					cellEditor.tx.text = "Not Destroy";
				else
					cellEditor.tx.text = "Lives: "+ (cellInfo.sprites.Length - j);
			}
		}
	}
}
