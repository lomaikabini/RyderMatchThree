using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
//using UnityEditor;

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

	private float speedStart = 4f;
	private float speedMax = 10f;
	private float speedBoost = 10f;
	private float bubbleSize;
	private float bubblesOffset;
	private float slipStep;

	private int bubblesInAction = 0;
	private int scores = 0;

	private Cell[,] cells;
	private Bubble[,] bubbles;
	private Separator[,] separators;
	private Bubble firstBubble;
	private Bubble secondBubble;
	private List<Bubble> matchBubbles =  new List<Bubble>();
	private List<Bubble> moveBubbles = new List<Bubble> ();

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
		fillEnvironment ();
		fillTableCells ();
		fillTableSeparators ();
		//fillTableBubbles ();
		gameState = GameState.InAction;
		dropNewBalls ();
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
		explositionNearSeparators (list);
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

	void explositionNearSeparators (List<Bubble> list)
	{
		List<Separator> usedSeparators = new List<Separator> ();
		for (int i = 0; i < list.Count; i++) 
		{
			Bubble b = list [i];
			giveDamageForSeparator(ref usedSeparators,b.posX,b.posY,Separator.Type.horizontal);
			giveDamageForSeparator(ref usedSeparators,b.posX,b.posY,Separator.Type.vertical);

			if(b.posY + 1 < TableSize)
				giveDamageForSeparator(ref usedSeparators,b.posX,b.posY + 1,Separator.Type.horizontal);
			if(b.posX - 1 >= 0)
				giveDamageForSeparator(ref usedSeparators,b.posX - 1,b.posY,Separator.Type.vertical);
		}
	}

	void giveDamageForSeparator(ref List<Separator> usedSeparators,int x,int y, Separator.Type type)
	{
		Separator separ = separators[x,y];
		if(!usedSeparators.Exists (e => e == separ) && separ !=null && separ.type == type)
		{
			usedSeparators.Add(separ);
			if(separ.GiveDamage())
			{
				Destroy(separators[x,y].gameObject);
				separators[x,y] = null;
			}
		}
	}

	void explositionNearBlocks(List<Bubble> list)
	{
		List<Cell> usedCells = new List<Cell> ();
		for(int i = 0; i < list.Count; i++)
		{
			Bubble b = list[i];
			if(b.posY + 1 < TableSize)
				giveDamageForCell(ref usedCells,cells[b.posX,b.posY + 1]);
			if(b.posY - 1 >= 0)
				giveDamageForCell(ref usedCells,cells[b.posX,b.posY - 1]);
			if(b.posX + 1 < TableSize)
				giveDamageForCell(ref usedCells,cells[b.posX + 1,b.posY]);
			if(b.posX - 1 >= 0)
				giveDamageForCell(ref usedCells,cells[b.posX - 1,b.posY]);
		}
	}

	void giveDamageForCell(ref List<Cell> usedCells, Cell c)
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
		for(int j = 1; j < TableSize; j++)
			for(int i = 0; i < TableSize; i++)
		{
			if(separators[i,j] != null && separators[i,j].type == Separator.Type.horizontal) continue;
			int indx = j;
			while(indx >= 1 && bubbles[i,indx-1] == null && cells[i,indx-1].cellType == Cell.Type.empty && (separators[i,indx] == null || separators[i,indx].type == Separator.Type.vertical))
			{
				indx--;
			}
			indx =(int) Mathf.Max(0f,(float)indx);
			if((indx != j || withSlip) && bubbles[i,j] != null)
			{
				int tmpX = i;
				int tmpY = indx;
				List<KeyValuePair<float,Vector2>> positions = new List<KeyValuePair<float,Vector2>>();
				if(indx!= j)
					positions.Add(new KeyValuePair<float, Vector2>(j - indx, new Vector2((float) tmpX,(float) tmpY)));
				if(withSlip)
				{
					slipStart:
					while(tmpX-1 >= 0 && tmpY-1 >=0 && bubbles[tmpX-1,tmpY-1] == null && !collumIsFree(tmpX-1,tmpY-1) && cells[tmpX-1,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
					{
						tmpX = tmpX-1;
						tmpY = tmpY-1;
						positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
						while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
						{
							tmpY--;
							positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
						}
					}

					while(tmpX+1 < TableSize && tmpY-1 >=0 && bubbles[tmpX+1,tmpY-1] == null && !collumIsFree(tmpX+1,tmpY-1) && cells[tmpX+1,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
					{
						tmpX = tmpX+1;
						tmpY = tmpY-1;
						positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
						while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
						{
							tmpY--;
							positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
						}
						goto slipStart;
					}
				}
				if(positions.Count >0)
				{
					Bubble tmp  = bubbles[i,j];
					bubbles[i,j] = null;
					bubbles[tmpX,tmpY] = tmp;
					tmp.posX = tmpX;
					tmp.posY = tmpY;
					tmp.addMovePoints(positions);
					moveBubbles.Add(tmp);
//					bubblesInAction++;
//					StartCoroutine(moveBubble(tmp,positions,!withSlip));
				}
				if(positions.Count>0) isMoved = true;
			}
		}
//		if (isMoved && withSlip)
//			dropBalls (true);
//		else
		if (!withSlip)
			dropBalls (true);
		else
		{
			moveAllBubbles();
		}
//		if(bubblesInAction == 0 )
//		{
//			if(withSlip)
//				dropNewBalls();
//			else
//				dropBalls(true);
//		}
	}

	void moveAllBubbles ()
	{
		int count = moveBubbles.Count;
		for(int i = 0; i < count; i++)
		{
			bubblesInAction++;
			StartCoroutine(moveBubble(moveBubbles[i],moveBubbles[i].whereMove,false));
		}
		dropNewBalls ();
//		if(bubblesInAction ==0 )
//			checkAllTable();
	}


	IEnumerator moveBubble (Bubble bubble,List<KeyValuePair<float,Vector2>> positions,bool repeatBubbleDrop)
	{
		yield return new WaitForEndOfFrame ();
		float speed = speedStart;
		for(int i = 0; i < positions.Count; i++)
		{
			float steps = positions [i].Key;
			Vector3 startPos = bubble.transform.localPosition;
			Vector3 endPos = new Vector3((float)positions[i].Value.x * bubbleSize + ((float)(positions[i].Value.x) * BubblePadding)-bubblesOffset,(float)positions[i].Value.y * bubbleSize + ((float)(positions[i].Value.y) * BubblePadding)-bubblesOffset, 0f);
			float cof = 0;
			while(cof < 1f)
			{
				speed +=Time.deltaTime * speedBoost;
				speed = Mathf.Min(speed, speedMax);
				cof += Time.deltaTime*speed/(float)steps;
				cof = Mathf.Min(cof,1f);
				bubble.transform.localPosition = Vector3.Lerp(startPos,endPos,cof);
				yield return new WaitForEndOfFrame();
			}
			yield return new WaitForEndOfFrame();
		}
		bubblesInAction --;
		bubble.playMovedAnim ();
		if(bubblesInAction == 0)
		{
			if(!repeatBubbleDrop)
			{
				int count = moveBubbles.Count;
				for(int i = 0; i < count; i++)
				{
					moveBubbles[i].whereMove.RemoveRange(0,moveBubbles[i].whereMove.Count);
				}
				moveBubbles.RemoveRange (0, moveBubbles.Count);
				checkAllTable();
			}
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

	IEnumerator moveBubbleMix (Bubble bubble,int steps,float posX,float posY)
	{
		yield return new WaitForEndOfFrame ();
		Vector3 startPos = bubble.transform.localPosition;
		Vector3 endPos = new Vector3((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding)-bubblesOffset,(float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding)-bubblesOffset, 0f);
		float cof = 0;
		while(cof < 1f)
		{
			cof += Time.deltaTime*speedStart/(float)steps;
			cof = Mathf.Min(cof,1f);
			bubble.transform.localPosition = Vector3.Lerp(startPos,endPos,cof);
			yield return new WaitForEndOfFrame();
		}
		bubblesInAction --;
		if (bubblesInAction == 0)
			dropBalls (true);//checkAllTable();
		yield return null;
	}

	void checkAllTable ()
	{
//		List<Bubble> list = new List<Bubble> ();
//		for(int i = 0; i < TableSize; i++)
//			for(int j = 0; j < TableSize;j++)
//		{
//			checkMatch(i,j,0,1,ref list);
//			checkMatch(i,j,1,0,ref list);
//		}
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
		int[] repeats = new int[TableSize];
		for(int k =0; k < repeats.Length;k++)
		{
			repeats[k] = 0;
		}
		List<KeyValuePair<Bubble,int>> newBubbles = new List<KeyValuePair<Bubble,int>> (); 
		for(int j = 0; j < TableSize; j++)
		{

			for(int i = 0; i < TableSize;i++)
			{
				steps = j;
				if((bubbles[i,j] == null) && collumIsFreeByCell(i,j) && collumIsFreeBySeparator(i,j+1)/*collumIsFree(i,j)*/)
				{
					startView:
					steps ++;
					int tmpX = i;
					int tmpY = j;
					List<KeyValuePair<float,Vector2>> positions = new List<KeyValuePair<float,Vector2>>();
					if(bubbles[i,j] == null)
					positions.Add(new KeyValuePair<float, Vector2>(TableSize+repeats[i]-j, new Vector2((float) tmpX,(float) tmpY)));
						slipStart:
						while(tmpX-1 >= 0 && tmpY-1 >=0 && bubbles[tmpX-1,tmpY-1] == null && !collumIsFree(tmpX-1,tmpY-1) && cells[tmpX-1,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
							{
								tmpX = tmpX-1;
								tmpY = tmpY-1;
								positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
								while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
								{
									tmpY--;
									positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
								}
							}
							
						while(tmpX+1 < TableSize && tmpY-1 >=0 && bubbles[tmpX+1,tmpY-1] == null && !collumIsFree(tmpX+1,tmpY-1) && cells[tmpX+1,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
							{
								tmpX = tmpX+1;
								tmpY = tmpY-1;
								positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
								while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && (separators[tmpX,tmpY] == null || separators[tmpX,tmpY].type == Separator.Type.vertical))
								{
									tmpY--;
									positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
								}
								goto slipStart;
							}

					if(positions.Count > 0)
					{
						Bubble bubble = BubblePool.Get().Pull().GetComponent<Bubble>();
						bubble.transform.SetParent(BubbleContainer.transform);
						bubble.SetType ((Bubble.Type)Mathf.RoundToInt(UnityEngine.Random.Range(0,Enum.GetNames(typeof(Bubble.Type)).Length)), bubbleSize);
						bubble.transform.localPosition = new Vector3 ((float)(i) * bubbleSize + ((float)(i) * BubblePadding)-bubblesOffset, 
						                                              (float)(TableSize+repeats[i]) * bubbleSize + ((float)(TableSize+repeats[i]) * BubblePadding)-bubblesOffset, 0f);
						bubbles[tmpX,tmpY] = bubble;
						bubble.posX = tmpX;
						bubble.posY = tmpY;
						bubblesInAction++;
						repeats[i]++;
						StartCoroutine(moveBubble(bubble,positions,false));
					}

					if(positions.Count>1)
					{
						goto startView;
					}
				}
			}
		}
		if(bubblesInAction== 0)
		{
			checkPossibleMatch();
		}
	}

	bool collumIsFreeByCell(int coll , int row)
	{
		
		for(int i = row; i < TableSize; i++)
		{
			if(cells[coll,i].cellType != Cell.Type.empty) return false;
		}
		
		return true;
	}

	bool collumIsFreeBySeparator (int coll , int row)
	{
		row = Mathf.Max (0, row);
		for(int i = row; i < TableSize; i++)
		{
			if(separators[coll,i] != null && separators[coll,i].type == Separator.Type.horizontal) return false;
		}
		
		return true;
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
//		for(int i = 0; i < TableSize;i++)
//		{
//			if(i == 2) continue;
//			GameObject obj = Instantiate(separatorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
//			Separator separ = obj.GetComponent<Separator>(); 
//			separ.posX = i;
//			separ.posY = 2;
//			separators[separ.posX,separ.posY] = separ;
////			if(i == 0)
////				separ.SetType(Separator.Type.vertical,Separator.DestroyType.destroy,bubbleSize,UnityEngine.Random.Range(1,4));
////			else
//			separ.SetType(Separator.Type.horizontal,Separator.DestroyType.destroy,bubbleSize,UnityEngine.Random.Range(1,4));
//			insertSeparatorTable(separ);
//		}

		for(int j = 0; j < TableSize;j++)
		{
			if(j==3)continue;
			GameObject obj = Instantiate(separatorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Separator separ = obj.GetComponent<Separator>(); 
			separ.posX = j;
			separ.posY = 3;
			separators[separ.posX,separ.posY] = separ;
//			if(j==1)
//				separ.SetType(Separator.Type.vertical,Separator.DestroyType.notDestroy,bubbleSize,1);
//			else
				separ.SetType(Separator.Type.horizontal,Separator.DestroyType.notDestroy,bubbleSize,1);
			insertSeparatorTable(separ);
		}

	}

	void fillEnvironment ()
	{
		for(int i = -3; i < 0; i++)
			for(int j = 0; j < TableSize;j++)
		{
			GameObject obj = Instantiate(cellPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Cell cell = obj.GetComponent<Cell>(); 
			cell.posX = i;
			cell.posY = j;
			insertCellTable(cell);
			cell.SetType(Cell.Type.groundBlock,bubbleSize);
		}

		for(int i = TableSize; i < TableSize+3; i++)
			for(int j = 0; j < TableSize;j++)
		{
			GameObject obj = Instantiate(cellPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Cell cell = obj.GetComponent<Cell>(); 
			cell.posX = i;
			cell.posY = j;
			insertCellTable(cell);
			cell.SetType(Cell.Type.groundBlock,bubbleSize);
		}
		for(int i = -3; i < TableSize+3; i++)
			for(int j = -2; j < 0;j++)
		{
			GameObject obj = Instantiate(cellPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Cell cell = obj.GetComponent<Cell>(); 
			cell.posX = i;
			cell.posY = j;
			insertCellTable(cell);
			cell.SetType(Cell.Type.groundBlock,bubbleSize);
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
//			if((i == 2 || i ==1 || i ==3) && j == 3)
//				cell.SetType(Cell.Type.groundBlock,bubbleSize);
//			else
				cell.SetType(Cell.Type.empty,bubbleSize);
		}
	}
		
	void fillTableBubbles ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 2; j < TableSize;j++)
			{
			if(i == 2 && j ==2) continue;
				if(cells[i,j].cellType != Cell.Type.empty) continue;
				GameObject obj = BubblePool.Get().Pull();
				Bubble bubble = obj.GetComponent<Bubble>(); 
				bubble.posX = i;
				bubble.posY = j;
				bubbles[i,j] = bubble;
				insertBubbleInTable(bubble);
			}
	}

	void checkPossibleMatch()
	{
		for(int i =0; i < TableSize;i++)
			for(int j =0; j < TableSize; j++)
		{
			int equals = 0;
			Bubble bubble = bubbles[i,j];
			if(bubble == null) continue;
			if(i-1 >= 0 && bubbles[i-1,j] != null &&bubbles[i-1,j].type == bubble.type && (separators[i-1,j] == null || separators[i-1,j].type != Separator.Type.vertical))
				equals++;
			if(i-1 >= 0 && j+1 < TableSize && bubbles[i-1,j+1] != null && bubbles[i-1,j+1].type == bubble.type)
				equals++;
			if(i-1 >= 0 && j-1 >=0 && bubbles[i-1,j-1] != null && bubbles[i-1,j-1].type == bubble.type)
				equals++;
			if(j-1 >= 0 && bubbles[i,j-1] != null && bubbles[i,j-1].type == bubble.type && (separators[i,j] == null || separators[i,j].type != Separator.Type.horizontal))
				equals++;
			if(j+1 < TableSize && bubbles[i,j+1] != null && bubbles[i,j+1].type == bubble.type && (separators[i,j+1] == null || separators[i,j+1].type != Separator.Type.horizontal))
				equals++;
			if(i+1 < TableSize && j+1 < TableSize && bubbles[i+1,j+1] != null && bubbles[i+1,j+1].type == bubble.type)
				equals++;
			if(i+1 < TableSize && j-1 >= 0 && bubbles[i+1,j-1] != null && bubbles[i+1,j-1].type == bubble.type)
				equals++;
			if(i+1 < TableSize && bubbles[i+1,j] != null && bubbles[i+1,j].type == bubble.type && (separators[i,j] == null || separators[i,j].type != Separator.Type.vertical))
				equals++;
			if(equals >=2)
			{
				gameState = GameState.free;
				return;
			}
		}
		mixBubbles ();
	}

	void mixBubbles ()
	{
		int[,] newPositions = new int[TableSize, TableSize];
		for(int i =0; i < TableSize;i++)
			for(int j =0; j < TableSize; j++)
		{
			newPositions[i,j] = 0;
		}
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
//		int id = -1;
//		while(checkLineMatch(bubble.posX,0,0,1) || checkLineMatch(0, bubble.posY,1,0))
//		{
//			if(id < Enum.GetNames(typeof(Bubble.Type)).Length - 1)
//				id++;
//			else
//				id = 0;
//			bubble.SetType ((Bubble.Type)id, bubbleSize);
//		}
	}

	void calculateBubblesValues ()
	{
		bubbleSize = (BubbleContainer.rect.height - ((TableSize + 1) * BubblePadding)) / (float)TableSize;
		bubblesOffset = (BubbleContainer.rect.height/2f) - bubbleSize/2f - BubblePadding;
		slipStep = Mathf.Sqrt ((bubbleSize * bubbleSize) * 2f) / bubbleSize;
	}
}
