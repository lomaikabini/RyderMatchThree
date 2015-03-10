using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEditor;

public class Game : MonoBehaviour {

	public int TableSize;
	public int ScoresPerBubble;
	public float BubblePadding;

	public GameObject cellPrefab;
	public GameObject separatorPrefab;

	public RectTransform BubbleContainer;
	public RectTransform CellsContainer;
	public RectTransform SeparatorContainer;

	public Text scoresView;
	public Animator curtainAnimator;

	[HideInInspector]
	public GameState gameState;
	public enum GameState
	{
		free,
		bubblePressed,
		InAction
	}

	private float speed = 5f;
	private float bubbleSize;
	private float bubblesOffset;

	private int bubblesInAction = 0;
	private int scores = 0;

	private Cell[,] cells;
	private Bubble[,] bubbles;
	private Separator[,] separators;
	private Bubble firstBubble;
	private Bubble secondBubble;
	private List<Bubble> matchBubbles =  new List<Bubble>();

	private List<ParallaxJoint> joints = new List<ParallaxJoint>();

	private static Game instance;

	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.LogError("Game duplicate!");
			DestroyObject(gameObject);
		}
	}

	void Start () 
	{
		gameState = GameState.free;
		cells = new Cell[TableSize, TableSize];
		bubbles = new Bubble[TableSize,TableSize];
		separators = new Separator[TableSize, TableSize];
		BubblePool.Get ().Initialize (TableSize);
		JointsPool.Get ().Initialize (TableSize);
		calculateBubblesValues ();
		fillTableCells ();
		//fillTableSeparators ();
		fillTableBubbles ();
		showScores ();
	}

	public static Game Get()
	{
		return instance;
	}

	public void BubblePress(Bubble bubble)
	{
		if (gameState != GameState.free)
						return;
		matchBubbles.Add (bubble);
		bubble.SetChosed ();
		gameState = GameState.bubblePressed;
		hideDifferentBubbles (bubble);
	}

	public void BubblePointerEnter(Bubble bubble)
	{
		if (gameState != GameState.bubblePressed)
			return;
		if (matchBubbles.Count == 0 || bubble.type != matchBubbles [0].type)
			return;
		if(matchBubbles[matchBubbles.Count-1].posY == bubble.posY)
		{
			int lowX = Mathf.Min(matchBubbles[matchBubbles.Count-1].posX, bubble.posX);
			if(separators[lowX,bubble.posY] != null && separators[lowX,bubble.posY].type == Separator.Type.vertical)
				return;
		}
		if(matchBubbles[matchBubbles.Count-1].posX == bubble.posX)
		{
			int maxY = Mathf.Max(matchBubbles[matchBubbles.Count-1].posY, bubble.posY);
			if(separators[bubble.posX, maxY] != null && separators[bubble.posX, maxY].type == Separator.Type.horizontal)
				return;
		}


		bool exist = matchBubbles.Exists (e => e == bubble);
		if(exist && matchBubbles.IndexOf(bubble) == matchBubbles.Count-2)
		{
			int id = matchBubbles.Count-1;
			matchBubbles[id].SetNotChosed();
			matchBubbles.Remove(matchBubbles[id]);
			if(joints.Count > 0)
			{
				JointsPool.Get().Push(joints[joints.Count - 1]);
				joints.RemoveAt(joints.Count -1);
			}
			return;
		}

		int indx = matchBubbles.Count - 1;
		if (!exist && Mathf.Abs (matchBubbles[indx].posX - bubble.posX) <= 1 && Mathf.Abs (matchBubbles[indx].posY - bubble.posY) <= 1)
			{
				matchBubbles.Add(bubble);
				bubble.SetChosed();
				if(matchBubbles.Count == 3)
				{
					for(int i =0; i < 2; i++)
					{
						ParallaxJoint j = JointsPool.Get().Pull();
						j.transform.SetParent(BubbleContainer);
						j.transform.SetSiblingIndex(0);
						j.RotateJoint(matchBubbles[i],matchBubbles[i+1]);
						j.RunJoint();
						joints.Add(j);
					}
				}
				else if(matchBubbles.Count >3)
				{
					ParallaxJoint j = JointsPool.Get().Pull();
					j.transform.SetParent(BubbleContainer);
					j.transform.SetSiblingIndex(0);
					j.RotateJoint(matchBubbles[matchBubbles.Count-2],matchBubbles[matchBubbles.Count-1]);
					j.RunJoint();
					joints.Add(j);
				}
			}
	}

	public void BubblePointerUp (Bubble bubble)
	{
		if (matchBubbles.Count >= 3) {
			destroyFoundBubbles (matchBubbles);
		} else
			gameState = GameState.free;
		showAllBubbles();
		removeAllJoints ();
		matchBubbles.RemoveRange(0, matchBubbles.Count);
	}

	void removeAllJoints ()
	{
		for(int i = 0; i < joints.Count; i++)
		{
			joints[i].StopJoint();
			JointsPool.Get().Push(joints[i]);
		}
		joints.RemoveRange (0, joints.Count);
	}

	public void OnReloadClick()
	{
		if(gameState != GameState.free)
			return;
		curtainAnimator.Play ("curtain_close", 0, 0f);
	}
	public void Restart ()
	{
		scores = 0;
		showScores ();
		gameState = GameState.free;
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize; j++)
			{
				if(bubbles[i,j] == null) continue;
				BubblePool.Get().Push(bubbles[i,j].gameObject);
			}

		bubbles = new Bubble[TableSize,TableSize];
		fillTableBubbles ();
		curtainAnimator.Play ("curtain_open", 0, 0f);
	}

	void hideDifferentBubbles (Bubble bubble)
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize; j++)
		{
			if(bubbles[i,j] != null && bubbles[i,j].type != bubble.type)
				bubbles[i,j].HideBubble();
		}
	}

	void showAllBubbles ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize; j++)
		{
			if(bubbles[i,j] != null)
				bubbles[i,j].RealeaseBubble();
		}
	}

	void showScores()
	{
		scoresView.text = "Scores: " + scores.ToString ();
	}

	void destroyFoundBubbles (List<Bubble> list)
	{
		removeDublicate (ref list);
		explositionNearBlocks (list);
		for(int i = 0;i < list.Count; i++)
		{
			Bubble bubble = bubbles[list[i].posX,list[i].posY];
			bubbles[list[i].posX,list[i].posY] = null;
			Vector3 pos  = bubble.transform.localPosition;
			BubblePool.Get().Push(bubble.gameObject);
		}
		scores += list.Count * ScoresPerBubble;
		showScores ();
		dropBalls ();
	}
	void explositionNearBlocks(List<Bubble> list)
	{
		List<Cell> usedCells = new List<Cell> ();
		for(int i = 0; i < list.Count; i++)
		{
			Bubble b = list[i];
			if(b.posY + 1 < TableSize)
				GiveDamageForCell(ref usedCells,cells[b.posX,b.posY + 1]);
			if(b.posY - 1 >= 0)
				GiveDamageForCell(ref usedCells,cells[b.posX,b.posY - 1]);
			if(b.posX + 1 < TableSize)
				GiveDamageForCell(ref usedCells,cells[b.posX + 1,b.posY]);
			if(b.posX - 1 >= 0)
				GiveDamageForCell(ref usedCells,cells[b.posX - 1,b.posY]);
		}
	}

	void GiveDamageForCell(ref List<Cell> usedCells, Cell c)
	{
		if(!usedCells.Exists (e => e == c))
		{
			usedCells.Add(c);
			c.GiveDamage();
		}
	}

	void dropBalls (bool withSlip = false)
	{
		bool isMoved = false;
		for(int i = 0; i < TableSize; i++)
			for(int j = 1; j < TableSize; j++)
		{
			int indx = j;
			while(indx >= 1 && bubbles[i,indx-1] == null && cells[i,indx-1].cellType == Cell.Type.empty && (separators[i,indx] == null || separators[i,indx].type == Separator.Type.vertical))
			{
				indx--;
			}
			indx =(int) Mathf.Max(0f,(float)indx);
			if((indx != j || withSlip) && bubbles[i,j] != null)
			{
				Bubble tmp  = bubbles[i,j];
				bubbles[i,j] = null;

				int tmpX = i;
				int tmpY = indx;
				List<KeyValuePair<int,Vector2>> positions = new List<KeyValuePair<int,Vector2>>();
				if(indx!= j)
					positions.Add(new KeyValuePair<int, Vector2>(j - indx, new Vector2((float) i,(float) indx)));
				if(withSlip)
				{
					while(tmpX-1 >= 0 && tmpY-1 >=0 && bubbles[tmpX-1,tmpY-1] == null && !collumIsFree(tmpX-1,tmpY-1) && cells[tmpX-1,tmpY-1].cellType == Cell.Type.empty)
					{
						tmpX = tmpX-1;
						tmpY = tmpY-1;
						positions.Add(new KeyValuePair<int, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
					}
					if(positions.Count < 2)
						while(tmpX+1 < TableSize && tmpY-1 >=0 && bubbles[tmpX+1,tmpY-1] == null && !collumIsFree(tmpX+1,tmpY-1) && cells[tmpX+1,tmpY-1].cellType == Cell.Type.empty)
						{
							tmpX = tmpX+1;
							tmpY = tmpY-1;
							positions.Add(new KeyValuePair<int, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
						}
				}
				bubbles[tmpX,tmpY] = tmp;
				tmp.posX = tmpX;
				tmp.posY = tmpY;
				if(positions.Count >0)
				{
					bubblesInAction++;
					StartCoroutine(moveBubble(tmp,positions,!withSlip));
				}
				if(positions.Count>0) isMoved = true;
			}
		}
		if (isMoved && withSlip)
			dropBalls (true);
		else
		if(bubblesInAction == 0 )
		{
			if(withSlip)
				dropNewBalls();
			else
				dropBalls(true);
		}
	}



	IEnumerator moveBubble (Bubble bubble,List<KeyValuePair<int,Vector2>> positions,bool repeatBubbleDrop)
	{
		yield return new WaitForEndOfFrame ();
		for(int i = 0; i < positions.Count; i++)
		{
			int steps = positions [i].Key;
			Vector3 startPos = bubble.transform.localPosition;
			Vector3 endPos = new Vector3((float)positions[i].Value.x * bubbleSize + ((float)(bubble.posX) * BubblePadding)-bubblesOffset,(float)positions[i].Value.y * bubbleSize + ((float)(bubble.posY) * BubblePadding)-bubblesOffset, 0f);
			float cof = 0;
			while(cof < 1f)
			{
				cof += Time.deltaTime*speed/(float)steps;
				cof = Mathf.Min(cof,1f);
				bubble.transform.localPosition = Vector3.Lerp(startPos,endPos,cof);
				yield return new WaitForEndOfFrame();
			}
			yield return new WaitForEndOfFrame();
		}
		bubblesInAction --;
		if(bubblesInAction == 0)
		{
			if(!repeatBubbleDrop)
				checkAllTable();
			else
				dropBalls(true);
		}
			
		yield return null;
	}

	void SetClearPositions ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize;j++)
		{
			Bubble bubble = bubbles[i,j];
			if(bubble != null)
			{
				Vector3 endPos = new Vector3((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding)-bubblesOffset,(float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding)-bubblesOffset, 0f);
				bubble.transform.localPosition = endPos;
			}
		}
	}

	IEnumerator moveBubble (Bubble bubble,int steps)
	{
		yield return new WaitForEndOfFrame ();
		Vector3 startPos = bubble.transform.localPosition;
		Vector3 endPos = new Vector3((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding)-bubblesOffset,(float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding)-bubblesOffset, 0f);
		float cof = 0;
		while(cof < 1f)
		{
			cof += Time.deltaTime*speed/(float)steps;
			cof = Mathf.Min(cof,1f);
			bubble.transform.localPosition = Vector3.Lerp(startPos,endPos,cof);
			yield return new WaitForEndOfFrame();
		}
		bubblesInAction --;
		if(bubblesInAction == 0)
			checkAllTable();
		yield return null;
	}

	void checkAllTable ()
	{
		List<Bubble> list = new List<Bubble> ();
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize;j++)
		{
			checkMatch(i,j,0,1,ref list);
			checkMatch(i,j,1,0,ref list);
		}
		//TODO: ydalit' elsi ann4ytsya gorbywki
//		if(list.Count>0)
//		{
//			destroyFoundBubbles(list);
//		}
//		else
		dropNewBalls();
	}

	void dropNewBalls ()
	{
		int steps = 0;
		List<KeyValuePair<Bubble,int>> newBubbles = new List<KeyValuePair<Bubble,int>> (); 
		for(int i = 0; i < TableSize; i++)
		{
			steps = 0;
			for(int j = 0; j < TableSize;j++)
			{
				if(bubbles[i,j] == null && collumIsFree(i,j))
				{
					steps ++;
					Bubble bubble = BubblePool.Get().Pull().GetComponent<Bubble>();
					bubbles[i,j] = bubble;
					bubble.posX = i;
					bubble.posY = j;
					bubblesInAction++;
					bubble.transform.SetParent(BubbleContainer.transform);
					bubble.SetType ((Bubble.Type)Mathf.RoundToInt(UnityEngine.Random.Range(0,Enum.GetNames(typeof(Bubble.Type)).Length)), bubbleSize);
					bubble.transform.localPosition = new Vector3 ((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding)-bubblesOffset, 
					                                              (float)(TableSize+steps) * bubbleSize + ((float)(TableSize+steps) * BubblePadding)-bubblesOffset, 0f);
					StartCoroutine(moveBubble(bubble,TableSize+steps-j));
				}
			}
		}
		if(bubblesInAction== 0)
		{
			gameState = GameState.free;
		}
	}

	bool collumIsFree (int coll , int row)
	{
		for(int i = row; i < TableSize; i++)
		{
			if(cells[coll,i].cellType != Cell.Type.empty) return false;
			if(separators[coll,i] != null && separators[coll,i].type == Separator.Type.horizontal) return false;
		}
		return true;
	}

	void removeDublicate (ref List<Bubble> list)
	{
		for(int i = 0; i < list.Count; i++)
			for(int j = 0; j < list.Count; j++)
		{
			if(i != j && list[i] == list[j])
			{
				list.RemoveAt(j);
				removeDublicate(ref list);
			}
		}
	}

	void fillTableSeparators ()
	{
		for(int i = 0; i < 3;i++)
		{
			GameObject obj = Instantiate(separatorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Separator separ = obj.GetComponent<Separator>(); 
			separ.posX = i+2;
			separ.posY = 4;
			separators[separ.posX,separ.posY] = separ;
			if(i == 1)
				separ.SetType(Separator.Type.vertical,Separator.DestroyType.destroy,bubbleSize,i+1);
			else
				separ.SetType(Separator.Type.horizontal,Separator.DestroyType.destroy,bubbleSize,i+1);
			insertSeparatorTable(separ);
		}

		for(int j = 0; j < 3;j++)
		{
			GameObject obj = Instantiate(separatorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Separator separ = obj.GetComponent<Separator>(); 
			separ.posX = j+4;
			separ.posY = 5;
			separators[separ.posX,separ.posY] = separ;
			if(j==1)
				separ.SetType(Separator.Type.vertical,Separator.DestroyType.notDestroy,bubbleSize,1);
			else
				separ.SetType(Separator.Type.horizontal,Separator.DestroyType.notDestroy,bubbleSize,1);
			insertSeparatorTable(separ);
		}

	}

	void fillTableCells ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize;j++)
		{
			GameObject obj = Instantiate(cellPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Cell cell = obj.GetComponent<Cell>(); 
			cell.posX = i;
			cell.posY = j;
			cells[i,j] = cell;
			insertCellTable(cell);
			if((i == 2 || i ==1 || i ==3) && j == 4)
				cell.SetType(Cell.Type.groundBlock,bubbleSize);
			else
				cell.SetType(Cell.Type.empty,bubbleSize);
		}
	}
		
	void fillTableBubbles ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize;j++)
			{
				if(cells[i,j].cellType != Cell.Type.empty) continue;
				GameObject obj = BubblePool.Get().Pull();
				Bubble bubble = obj.GetComponent<Bubble>(); 
				bubble.posX = i;
				bubble.posY = j;
				bubbles[i,j] = bubble;
				insertBubbleInTable(bubble);
			}
	}

	bool checkPossibleMatch()
	{
		bool res = false;
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize;j++)
		{
			if(bubbles[i,j] == null) continue;

			swapBubblesIndexes(i,j,0,1);
			if(checkAllDirections(i,j))
			{
				swapBubblesIndexes(i,j,0,-1);
				res = true;
				goto exit;
			}

			swapBubblesIndexes(i,j,0,-1);
			if(checkAllDirections(i,j))
			{
				swapBubblesIndexes(i,j,0,1);
				res = true;
				goto exit;
			}

			swapBubblesIndexes(i,j,1,0);
			if(checkAllDirections(i,j))
			{
				swapBubblesIndexes(i,j,-1,0);
				res = true;
				goto exit;
			}

			swapBubblesIndexes(i,j,-1,0);
			if(checkAllDirections(i,j))
			{
				swapBubblesIndexes(i,j,1,0);
				res = true;
				goto exit;
			}
		}
		exit:
		return res;
	}

	bool checkAllDirections(int x, int y)
	{
		if(checkLineMatch(x,0,0,1)) return true;
		if(checkLineMatch(0,y,1,0)) return true;
		return false;
	}

	bool checkAllDirections(int x, int y, ref List<Bubble> list)
	{
		bool res = false;
		if(checkLineMatch(x,0,0,1,ref list)) res = true;
		if(checkLineMatch(0,y,1,0,ref list)) res = true;
		return res;
	}

	void swapBubblesIndexes(int x, int y, int dirX,int dirY)
	{
		int newX = x + dirX;
		int newY = y + dirY;

		Bubble tmp = bubbles [x, y];
		Bubble tmp2;
		if(newX >= TableSize || newX < 0 || newY >=TableSize || newY < 0)
			tmp2 = null;
		else 
			tmp2 = bubbles [newX, newY];
		if(tmp2 != null)
		{
			bubbles [x, y] = tmp2;
			bubbles [newX, newY] = tmp;
			bubbles [newX, newY].posX = newX;
			bubbles [newX, newY].posY = newY;

			bubbles [x,y].posX = x;
			bubbles [x,y].posY = y;
		}
	}

	bool checkMatch (int x, int y, int dirX, int dirY, ref List<Bubble> list)
	{
		int tmpX = 0;
		int tmpY = 0;
		int tmpL = checkDirection (x, y, dirX, dirY,ref tmpX, out tmpY);
			if(tmpL >=3)
			{
				while(tmpL>0)
				{
					tmpL --;
					tmpX -= dirX;
					tmpY -= dirY;
					if(list != null)
						list.Add(bubbles[tmpX,tmpY]);
				}
				return true;
			}
			return false;
	}
	bool checkMatch (int x, int y, int dirX, int dirY)
	{
		int tmpX = 0;
		int tmpY = 0;
		int tmpL = checkDirection (x, y, dirX, dirY,ref tmpX, out tmpY);
		if(tmpL >=3)
		{
			return true;
		}
		return false;
	}
	int checkDirection (int x, int y, int dirX, int dirY, ref int resX , out int resY)
	{
		int tmpX = x + dirX;
		int tmpY = y + dirY;
		int tmpL = 1;
		if(bubbles[x,y] != null)
		{
			while(tmpX <= TableSize-1 && tmpY <= TableSize-1 && bubbles[tmpX,tmpY] != null && bubbles[x,y] != null && bubbles[x,y].type == bubbles[tmpX,tmpY].type)
			{
				tmpX += dirX;
				tmpY += dirY;
				tmpL ++;
			}
		}
		resX = tmpX;
		resY = tmpY;
		return tmpL;
	}

	bool checkLineMatch(int x, int y, int dirX,int dirY)
	{
		for(int i = 0; i < TableSize; i++)
		{
			if(checkMatch (x,y,dirX,dirY)) return true;
			x += dirX;
			y +=dirY;
		}
		return false;
	}

	bool checkLineMatch(int x, int y, int dirX,int dirY, ref List<Bubble> list)
	{
		bool res = false;
		for(int i = 0; i < TableSize; i++)
		{
			if(checkMatch (x,y,dirX,dirY,ref list)) res = true;
			x += dirX;
			y +=dirY;
		}
		return res;
	}

	void insertSeparatorTable(Separator separ)
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

	void insertCellTable(Cell cell)
	{
		cell.transform.SetParent(CellsContainer.transform);
		cell.rectTransform.localScale = new Vector3 (1f, 1f, 1f);
		cell.transform.localPosition = new Vector3 ((float)cell.posX * bubbleSize + ((float)(cell.posX) * BubblePadding)-bubblesOffset, 
		                                              (float)cell.posY * bubbleSize + ((float)(cell.posY) * BubblePadding)-bubblesOffset, 0f);
	}

	void insertBubbleInTable (Bubble bubble)
	{
		bubble.transform.SetParent(BubbleContainer.transform);
		bubble.SetType ((Bubble.Type)Mathf.RoundToInt(UnityEngine.Random.Range(0,Enum.GetNames(typeof(Bubble.Type)).Length)), bubbleSize);
		bubble.transform.localPosition = new Vector3 ((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding)-bubblesOffset, 
		                                              (float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding)-bubblesOffset, 0f);
		int id = -1;
		while(checkLineMatch(bubble.posX,0,0,1) || checkLineMatch(0, bubble.posY,1,0))
		{
			if(id < Enum.GetNames(typeof(Bubble.Type)).Length - 1)
				id++;
			else
				id = 0;
			bubble.SetType ((Bubble.Type)id, bubbleSize);
		}
	}

	void calculateBubblesValues ()
	{
		bubbleSize = (BubbleContainer.rect.height - ((TableSize + 1) * BubblePadding)) / (float)TableSize;
		bubblesOffset = (BubbleContainer.rect.height/2f) - bubbleSize/2f - BubblePadding;
	}
}
