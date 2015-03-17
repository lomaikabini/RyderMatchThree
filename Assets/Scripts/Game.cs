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
	public Animator tableAnimator;

	[HideInInspector]
	public GameState gameState;
	public enum GameState
	{
		free,
		bubblePressed,
		InAction
	}

	private float speedStart = 9f;
	private float speedMax = 12f;
	private float speedBoost = 3.5f;
	private float bubbleSize;
	private float bubblesOffset;
	private float slipStep;
	private float dir = -1;

	private int bubblesInAction = 0;
	private int lastBubblePosY;

	private Cell[,] cells;
	private FieldItem[,] bubbles;
	private Separator[,] separatorsHorizontal;
	private Separator[,] separatorsVertical;
	private Bubble firstBubble;
	private Bubble secondBubble;
	private List<Bubble> matchBubbles =  new List<Bubble>();
	private List<FieldItem> moveBubbles = new List<FieldItem> ();

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
		separatorsHorizontal = new Separator[TableSize, TableSize];
		separatorsVertical = new Separator[TableSize, TableSize];
		BubblePool.Get ().Initialize (TableSize);
		JointsPool.Get ().Initialize (TableSize);
		calculateBubblesValues ();
		fillEnvironment ();
		fillTableCells ();
		fillTableSeparators ();
		gameState = GameState.InAction;
		dropNewBalls ();
		moveAllBubbles ();
	}

	public static Game Get()
	{
		return instance;
	}

	public void BubblePress(Bubble bubble)
	{
		if (gameState != GameState.free)
						return;
		bubble.playChosedAnim ();
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
		int xMin = Mathf.Min(matchBubbles[matchBubbles.Count-1].posX, bubble.posX);
		int xMax = Mathf.Max(matchBubbles[matchBubbles.Count-1].posX, bubble.posX);
		int yMax = Mathf.Max(matchBubbles[matchBubbles.Count-1].posY, bubble.posY);
		int yMin = Mathf.Min(matchBubbles[matchBubbles.Count-1].posY, bubble.posY);
		if(matchBubbles[matchBubbles.Count-1].posY == bubble.posY)
		{
			if(separatorsVertical[xMin,bubble.posY] != null)
				return;
		}
		if(matchBubbles[matchBubbles.Count-1].posX == bubble.posX)
		{
			if(separatorsHorizontal[bubble.posX, yMax] != null)
				return;
		}
		//Didn't test this exception block
		if(matchBubbles[matchBubbles.Count-1].posY != bubble.posY && matchBubbles[matchBubbles.Count-1].posX != bubble.posX)
		{
			//for  horizontals blocks
			if((separatorsHorizontal[matchBubbles[matchBubbles.Count-1].posX,yMax] != null || cells[matchBubbles[matchBubbles.Count-1].posX,yMax].cellType != Cell.Type.empty || cells[matchBubbles[matchBubbles.Count-1].posX,yMin].cellType != Cell.Type.empty)
			   && (separatorsHorizontal[bubble.posX,yMax] != null || cells[bubble.posX,yMax].cellType != Cell.Type.empty || cells[bubble.posX,yMin].cellType != Cell.Type.empty)) return;
			//for vertical blocks
			if((separatorsVertical[xMin,matchBubbles[matchBubbles.Count-1].posY] != null || cells[xMin,matchBubbles[matchBubbles.Count-1].posY].cellType != Cell.Type.empty || cells[xMax,matchBubbles[matchBubbles.Count-1].posY].cellType != Cell.Type.empty)
			   && (separatorsVertical[xMin,bubble.posY] != null || cells[xMin,bubble.posY].cellType != Cell.Type.empty || cells[xMax,bubble.posY].cellType != Cell.Type.empty)) return;

			//dlya iglovix pregrad isklu4itelno iz separatorov
			if(separatorsHorizontal[xMin,yMax] != null && separatorsVertical[xMin,yMax] != null &&
			   ((matchBubbles[matchBubbles.Count-1].posY > bubble.posY && matchBubbles[matchBubbles.Count-1].posX < bubble.posX)||(bubble.posY > matchBubbles[matchBubbles.Count-1].posY && bubble.posX < matchBubbles[matchBubbles.Count-1].posX))) return;
			if(separatorsVertical[xMin,yMax] != null && separatorsHorizontal[xMax, yMax] != null && 
			   ((matchBubbles[matchBubbles.Count-1].posY > bubble.posY && matchBubbles[matchBubbles.Count-1].posX > bubble.posX)||(bubble.posY > matchBubbles[matchBubbles.Count-1].posY && bubble.posX > matchBubbles[matchBubbles.Count-1].posX))) return;
			if(separatorsHorizontal[xMin,yMax] != null && separatorsVertical[xMin,yMin] != null &&
			   ((matchBubbles[matchBubbles.Count-1].posY < bubble.posY && matchBubbles[matchBubbles.Count-1].posX < bubble.posX)||(bubble.posY < matchBubbles[matchBubbles.Count-1].posY && bubble.posX < matchBubbles[matchBubbles.Count-1].posX))) return;
			if(separatorsHorizontal[xMax,yMax] != null && separatorsVertical[xMax, yMin] != null &&
			   ((matchBubbles[matchBubbles.Count-1].posY < bubble.posY && matchBubbles[matchBubbles.Count-1].posX > bubble.posX)||(bubble.posY < matchBubbles[matchBubbles.Count-1].posY && bubble.posX > matchBubbles[matchBubbles.Count-1].posX))) return;
		}

		bool exist = matchBubbles.Exists (e => e == bubble);
		if(exist && matchBubbles.IndexOf(bubble) == matchBubbles.Count-2)
		{
			int id = matchBubbles.Count-1;
			matchBubbles[id].SetNotChosed();
			matchBubbles[id].playChosedAnim();
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
				bubble.playChosedAnim ();
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
		{
			gameState = GameState.free;
			for(int i = 0; i < matchBubbles.Count; i++)
				matchBubbles[i].playChosedAnim();
		}
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
		gameState = GameState.free;
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize; j++)
			{
				if(bubbles[i,j] != null)
				BubblePool.Get().Push(bubbles[i,j].gameObject);
				Destroy(cells[i,j].gameObject);
				if(separatorsHorizontal[i,j]!=null)
					Destroy(separatorsHorizontal[i,j].gameObject);
				if(separatorsVertical[i,j]!= null)
					Destroy(separatorsVertical[i,j].gameObject);
		}
		
		cells = new Cell[TableSize, TableSize];
		bubbles = new Bubble[TableSize,TableSize];
		separatorsHorizontal = new Separator[TableSize, TableSize];
		separatorsVertical = new Separator[TableSize, TableSize];
		fillTableCells ();
		fillTableSeparators ();
		gameState = GameState.InAction;
		dropNewBalls ();
		moveAllBubbles ();
		curtainAnimator.Play ("curtain_open", 0, 0f);
	}

	void hideDifferentBubbles (Bubble bubble)
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize; j++)
		{
			if(bubbles[i,j] != null && bubbles[i,j].type != bubble.type)
				bubbles[i,j].HideItem();
		}
	}

	void showAllBubbles ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize; j++)
		{
			if(bubbles[i,j] != null)
				bubbles[i,j].RealeaseItem();
		}
	}

	void destroyFoundBubbles (List<Bubble> list)
	{
		removeDublicate (ref list);
		explositionNearBlocks (list);
		explositionNearSeparators (list);
		for(int i = 0;i < list.Count; i++)
		{
			FieldItem bubble = bubbles[list[i].posX,list[i].posY];
			bubbles[list[i].posX,list[i].posY] = null;
			BubblePool.Get().Push(bubble.gameObject);
		}
		StartCoroutine (dropBallsWithDelay (1f));
	}

	IEnumerator dropBallsWithDelay(float delay)
	{
		tableAnimator.Play ("DarkenTheScreen",0,0f);
		yield return new WaitForSeconds (delay);
		tableAnimator.Play ("LightenTheScreen",0,0f);
		dropBalls ();
		yield return null;
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
		Separator separ;
		if (type == Separator.Type.vertical)
			separ = separatorsVertical [x, y];
		else
			separ = separatorsHorizontal [x, y];
		if(!usedSeparators.Exists (e => e == separ) && separ !=null)
		{
			usedSeparators.Add(separ);
			if(separ.GiveDamage())
			{
				if(type == Separator.Type.vertical)
				{
					Destroy(separatorsVertical[x,y].gameObject);
					separatorsVertical[x,y] = null;
				}
				else
				{
					Destroy(separatorsHorizontal[x,y].gameObject);
					separatorsHorizontal[x,y] = null;
				}
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
		for(int j = 1; j < TableSize; j++)
			for(int i = 0; i < TableSize; i++)
		{
			if(separatorsHorizontal[i,j] != null) continue;
			int indx = j;
			while(indx >= 1 && bubbles[i,indx-1] == null && cells[i,indx-1].cellType == Cell.Type.empty && separatorsHorizontal[i,indx] == null)
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
					positions = findNewPositionForBubble(positions,ref tmpX,out tmpY,tmpY);
				}
				if(positions.Count >0)
				{
					FieldItem tmp  = bubbles[i,j];
					bubbles[i,j] = null;
					bubbles[tmpX,tmpY] = tmp;
					tmp.posX = tmpX;
					tmp.posY = tmpY;
					tmp.addMovePoints(positions);
					moveBubbles.Add(tmp);
				}
			}
		}
		if (!withSlip)
			dropBalls (true);
		else
		{
			moveAllBubbles();
		}
	}

	List<KeyValuePair<float,Vector2>> findNewPositionForBubble(List<KeyValuePair<float,Vector2>> positions,ref int tmpX,out int tmpY,int y)
	{
		tmpY = y;
		if(dir==1)
		{
			dir = -1;
		slipStart1:
				while(tmpX-1 >= 0 && tmpY-1 >=0 && bubbles[tmpX-1,tmpY-1] == null && !collumIsFree(tmpX-1,tmpY-1) && cells[tmpX-1,tmpY-1].cellType == Cell.Type.empty 
				      && separatorsHorizontal[tmpX,tmpY] == null &&((cells[tmpX-1,tmpY].cellType == Cell.Type.empty || separatorsVertical[tmpX-1,tmpY] == null) && separatorsVertical[tmpX-1,tmpY-1] == null))
			{
				//dir = -1;
				tmpX = tmpX-1;
				tmpY = tmpY-1;
				positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
				while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && separatorsHorizontal[tmpX,tmpY] == null)
				{
					tmpY--;
					positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
				}
			}
			
			while(tmpX+1 < TableSize && tmpY-1 >=0 && bubbles[tmpX+1,tmpY-1] == null && !collumIsFree(tmpX+1,tmpY-1) && cells[tmpX+1,tmpY-1].cellType == Cell.Type.empty 
			      && separatorsHorizontal[tmpX,tmpY] == null &&((cells[tmpX+1,tmpY].cellType == Cell.Type.empty  || separatorsVertical[tmpX,tmpY] == null) && separatorsVertical[tmpX,tmpY-1] == null))
			{
				//dir = 1;
				tmpX = tmpX+1;
				tmpY = tmpY-1;
				positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
				while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && separatorsHorizontal[tmpX,tmpY] == null)
				{
					tmpY--;
					positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
				}
				goto slipStart1;
			}
		}
		else
		{
			dir = 1;
		slipStart2:
				
				while(tmpX+1 < TableSize && tmpY-1 >=0 && bubbles[tmpX+1,tmpY-1] == null && !collumIsFree(tmpX+1,tmpY-1) && cells[tmpX+1,tmpY-1].cellType == Cell.Type.empty 
				      && separatorsHorizontal[tmpX,tmpY] == null &&((cells[tmpX+1,tmpY].cellType == Cell.Type.empty  || separatorsVertical[tmpX,tmpY] == null) && separatorsVertical[tmpX,tmpY-1] == null))
			{
				//dir = 1;
				tmpX = tmpX+1;
				tmpY = tmpY-1;
				positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
				while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && separatorsHorizontal[tmpX,tmpY] == null)
				{
					tmpY--;
					positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
				}
				
			}
			while(tmpX-1 >= 0 && tmpY-1 >=0 && bubbles[tmpX-1,tmpY-1] == null && !collumIsFree(tmpX-1,tmpY-1) && cells[tmpX-1,tmpY-1].cellType == Cell.Type.empty 
			      && separatorsHorizontal[tmpX,tmpY] == null &&((cells[tmpX-1,tmpY].cellType == Cell.Type.empty || separatorsVertical[tmpX-1,tmpY] == null) && separatorsVertical[tmpX-1,tmpY-1] == null))
			{
				//dir = -1;
				tmpX = tmpX-1;
				tmpY = tmpY-1;
				positions.Add(new KeyValuePair<float, Vector2>(slipStep, new Vector2((float) (tmpX),(float) (tmpY))));
				while(tmpY >= 1 && bubbles[tmpX,tmpY-1] == null && cells[tmpX,tmpY-1].cellType == Cell.Type.empty && separatorsHorizontal[tmpX,tmpY] == null)
				{
					tmpY--;
					positions.Add(new KeyValuePair<float, Vector2>(1, new Vector2((float) (tmpX),(float) (tmpY))));
				}
				goto slipStart2;
			}
		}

		return positions;
	}

	void moveAllBubbles ()
	{
		dropNewBalls ();
		int count = moveBubbles.Count;
		lastBubblePosY = 999;
		for(int i = 0; i < count; i++)
		{
			if(moveBubbles[i].posY < lastBubblePosY)
				lastBubblePosY = moveBubbles[i].posY; 
		}
		for(int i = 0; i < count; i++)
		{
			bubblesInAction++;
			StartCoroutine(moveBubble(moveBubbles[i],moveBubbles[i].whereMove,false));
		}
		if (bubblesInAction == 0)
			checkPossibleMatch ();
	}

	void moveBubblesMix()
	{
		float size = ((float)(TableSize - 1)) / 2f;
		for(int i = 1; i <= moveBubbles.Count; i++)
		{
			float posX;
			float posY;
			if(i==1)
			{
				posX = size - 0.5f;
				posY = size + 0.5f;
			}
			else if(i == 2)
			{
				posX = size;
				posY = size+0.5f;
			}
			else if(i == 3)
			{
				posX = size +0.5f;
				posY = size +0.5f;
			}
			else if(i == 4)
			{
				posX = size - 1f;
				posY = size;
			}
			else if(i == 5)
			{
				posX = size;
				posY = size;
			}
			else if(i == 6)
			{
				posX = size + 1f;
				posY = size;
			}
			else if(i == 7)
			{
				posX = size  - 0.5f;
				posY = size - 0.5f;
			}
			else if(i == 8)
			{
				posX = size;
				posY = size -1f;
			}
			else if(i==9)
			{
				posX = size + 1f;
				posY = size - 1f;
			}
			else
			{
				posX = UnityEngine.Random.Range((size-0.5f)*100f,(size+0.5f)*100f)/100f;
				posY = UnityEngine.Random.Range((size-0.5f)*100f,(size+0.5f)*100f)/100f;
			}
			moveBubbles[i-1].whereMove.Add(new KeyValuePair<float, Vector2>(2f,new Vector2(posX,posY)));
			moveBubbles[i-1].whereMove.Add(new KeyValuePair<float, Vector2>(2f,new Vector2((float)moveBubbles[i-1].posX,(float)moveBubbles[i-1].posY)));
		}

		for(int i = 0; i < moveBubbles.Count; i++)
		{
			bubblesInAction++;
			StartCoroutine(moveBubbleMix(moveBubbles[i],moveBubbles[i].whereMove));
		}
	}

	IEnumerator moveBubble (FieldItem bubble,List<KeyValuePair<float,Vector2>> positions,bool repeatBubbleDrop)
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
		bool an = bubble.posY <= lastBubblePosY;
		bubble.playMovedAnim (an);

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
				checkPossibleMatch();
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
			FieldItem bubble = bubbles[i,j];
			if(bubble != null)
			{
				Vector3 endPos = new Vector3((float)bubble.posX * bubbleSize + ((float)(bubble.posX) * BubblePadding)-bubblesOffset,(float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding)-bubblesOffset, 0f);
				bubble.transform.localPosition = endPos;
			}
		}
	}

	IEnumerator moveBubbleMix (FieldItem bubble,List<KeyValuePair<float,Vector2>> positions)
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
			float size = ((float)(TableSize-1))/2f;
			if(i==0)
			for(int j = 0; j < 5; j++)
			{
				float c = 0f;
				Vector3 p1 = bubble.transform.localPosition;
				Vector3 p2 = new Vector3(UnityEngine.Random.Range((size-100f)*100f,(size+100f)*100f)/100f,UnityEngine.Random.Range((size-100f)*100f,(size+100f)*100f)/100f,0f);
				while(c < 1f)
				{
					c += Time.deltaTime*6f;
					c = Mathf.Min(c,1f);
					bubble.transform.localPosition = Vector3.Lerp(p1,p2,c);
					yield return new WaitForEndOfFrame();
				}
			}
			yield return new WaitForEndOfFrame();
		}
		bubblesInAction --;
		bubble.playMovedAnim ();
		if(bubblesInAction == 0)
		{
			int count = moveBubbles.Count;
			for(int i = 0; i < count; i++)
			{
				moveBubbles[i].whereMove.RemoveRange(0,moveBubbles[i].whereMove.Count);
			}
			moveBubbles.RemoveRange (0, moveBubbles.Count);
			checkPossibleMatch();
		}
		
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

					positions = findNewPositionForBubble(positions,ref tmpX,out tmpY,tmpY);
					if(positions.Count > 0)
					{
						Bubble bubble = BubblePool.Get().Pull().GetComponent<Bubble>();
						bubble.transform.SetParent(BubbleContainer.transform);
						bubble.SetType ((Bubble.Type)Mathf.RoundToInt(UnityEngine.Random.Range(0,Enum.GetNames(typeof(Bubble.Type)).Length-1)), bubbleSize);
						bubble.transform.localPosition = new Vector3 ((float)(i) * bubbleSize + ((float)(i) * BubblePadding)-bubblesOffset, 
						                                              (float)(TableSize+repeats[i]) * bubbleSize + ((float)(TableSize+repeats[i]) * BubblePadding)-bubblesOffset, 0f);
						bubbles[tmpX,tmpY] = bubble;
						bubble.posX = tmpX;
						bubble.posY = tmpY;
						//bubblesInAction++;
						repeats[i]++;
						bubble.addMovePoints(positions);
						moveBubbles.Add(bubble);
						//StartCoroutine(moveBubble(bubble,positions,false));
					}

					if(positions.Count>1)
					{
						goto startView;
					}
				}
			}
		}
//		if(bubblesInAction== 0)
//		{
//			checkPossibleMatch();
//		}
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
			if(separatorsHorizontal[coll,i] != null) return false;
		}
		
		return true;
	}

	bool collumIsFree (int coll , int row)
	{
	
		for(int i = row; i < TableSize; i++)
		{
			if(cells[coll,i].cellType != Cell.Type.empty) return false;
			if(separatorsHorizontal[coll,i] != null) return false;
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
		for(int i = 0; i < TableSize;i++)
		{
			for(int j = 0; j < TableSize;j++)
			{
				if((i == 1 || i ==4) && (j==3 /*|| j==4*/  ||j==5))
				{
					GameObject obj = Instantiate(separatorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
					Separator separ = obj.GetComponent<Separator>(); 
					separ.posX = i;
					separ.posY = j;
					separatorsVertical[separ.posX,separ.posY] = separ;
					separ.SetType(Separator.Type.vertical,Separator.DestroyType.destroy,bubbleSize,UnityEngine.Random.Range(1,4));
					insertSeparatorTable(separ);
				}
			}
		}
//		for(int i = 0; i < TableSize;i++)
//		{
//			GameObject obj = Instantiate(separatorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
//			Separator separ = obj.GetComponent<Separator>(); 
//			separ.posX = i;
//			separ.posY = 3;
//			separators[separ.posX,separ.posY] = separ;
////			if(i == 0)
////				separ.SetType(Separator.Type.vertical,Separator.DestroyType.destroy,bubbleSize,UnityEngine.Random.Range(1,4));
////			else
//			separ.SetType(Separator.Type.horizontal,Separator.DestroyType.destroy,bubbleSize,1);
//			insertSeparatorTable(separ);
//		}
		for(int j = 0; j < TableSize;j++)
		{
			if(j==3)continue;
			GameObject obj = Instantiate(separatorPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Separator separ = obj.GetComponent<Separator>(); 
			separ.posX = j;
			separ.posY = 3;
			separatorsHorizontal[separ.posX,separ.posY] = separ;
			separ.SetType(Separator.Type.horizontal,Separator.DestroyType.destroy,bubbleSize,UnityEngine.Random.Range(1,4));
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
			cell.SetType(Cell.Type.groundBlock,bubbleSize+2f);
		}

		for(int i = TableSize; i < TableSize+3; i++)
			for(int j = 0; j < TableSize;j++)
		{
			GameObject obj = Instantiate(cellPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Cell cell = obj.GetComponent<Cell>(); 
			cell.posX = i;
			cell.posY = j;
			insertCellTable(cell);
			cell.SetType(Cell.Type.groundBlock,bubbleSize+2f);
		}
		for(int i = -3; i < TableSize+3; i++)
			for(int j = -2; j < 0;j++)
		{
			GameObject obj = Instantiate(cellPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			Cell cell = obj.GetComponent<Cell>(); 
			cell.posX = i;
			cell.posY = j;
			insertCellTable(cell);
			cell.SetType(Cell.Type.groundBlock,bubbleSize+2f);
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
			if( j == TableSize - 1 && (i !=2 && i !=3 && i !=4))
				cell.SetType(Cell.Type.groundBlock,bubbleSize);
			else
				cell.SetType(Cell.Type.empty,bubbleSize);
		}
	}
		
	void fillTableBubbles ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 3; j < TableSize;j++)
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
			FieldItem bubble = bubbles[i,j];
			if(bubble == null) continue;
			//vertikalnie i gorizontalnie matchi
			if(i-1 >= 0 && bubbles[i-1,j] != null &&bubbles[i-1,j].type == bubble.type && separatorsVertical[i-1,j] == null)
				equals++;
			if(j-1 >= 0 && bubbles[i,j-1] != null && bubbles[i,j-1].type == bubble.type && separatorsHorizontal[i,j] == null)
				equals++;
			if(j+1 < TableSize && bubbles[i,j+1] != null && bubbles[i,j+1].type == bubble.type && separatorsHorizontal[i,j+1] == null)
				equals++;
			if(i+1 < TableSize && bubbles[i+1,j] != null && bubbles[i+1,j].type == bubble.type && separatorsVertical[i,j] == null)
				equals++;

			//diagonalnie
			if(i-1 >= 0 && j+1 < TableSize && bubbles[i-1,j+1] != null && bubbles[i-1,j+1].type == bubble.type &&
			   !((separatorsHorizontal[i-1,j+1] != null || cells[i-1,j].cellType != Cell.Type.empty || separatorsVertical[i-1,j] != null) && (separatorsVertical[i-1,j+1]!=null || cells[i,j+1].cellType != Cell.Type.empty || separatorsHorizontal[i,j+1] != null)))
				equals++;
			if(i-1 >= 0 && j-1 >=0 && bubbles[i-1,j-1] != null && bubbles[i-1,j-1].type == bubble.type &&
			   !((separatorsHorizontal[i-1,j] != null || cells[i-1,j].cellType != Cell.Type.empty || separatorsVertical[i-1,j] != null) && (separatorsVertical[i-1,j-1] != null || cells[i,j-1].cellType != Cell.Type.empty || separatorsHorizontal[i,j-1] != null)))
				equals++;
			if(i+1 < TableSize && j-1 >= 0 && bubbles[i+1,j-1] != null && bubbles[i+1,j-1].type == bubble.type &&
			   !((separatorsHorizontal[i+1,j] != null || cells[i+1,j].cellType != Cell.Type.empty || separatorsVertical[i,j] != null) && (separatorsVertical[i,j-1]!= null || cells[i,j-1].cellType != Cell.Type.empty || separatorsHorizontal[i,j] != null)))
				equals++;
			if(i+1 < TableSize && j+1 < TableSize && bubbles[i+1,j+1] != null && bubbles[i+1,j+1].type == bubble.type &&
			   !((separatorsHorizontal[i+1,j+1] != null || cells[i+1,j].cellType != Cell.Type.empty || separatorsVertical[i,j] != null) && (separatorsVertical[i,j+1] != null || cells[i,j+1].cellType != Cell.Type.empty || separatorsHorizontal[i,j+1] != null)))
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
		List<Vector2> positions = new List<Vector2> ();
		List<FieldItem> tmpBubbles = new List<FieldItem> ();
		for(int i =0; i < TableSize;i++)
			for(int j =0; j < TableSize; j++)
		{
			if(bubbles[i,j] == null)
				newPositions[i,j] = -1;
			else
			{
				tmpBubbles.Add(bubbles[i,j]);
				newPositions[i,j] = 0;
			}
		}

		for(int i =0; i < TableSize;i++)
			for(int j =0; j < TableSize; j++)
		{
			if(newPositions[i,j] == -1) continue;
			positions.Add(new Vector2(i,j));
			//vertikalnie i gorizontalnie matchi
			if(i-1 >= 0 && newPositions[i-1,j] != -1 && separatorsVertical[i-1,j] == null)
			{
				positions.Add(new Vector2((float)(i-1),(float)j));
			}
			if(j-1 >= 0 && newPositions[i,j-1] != -1 && separatorsHorizontal[i,j] == null)
			{
				positions.Add(new Vector2((float)(i),(float)(j-1)));
			}
			if(j+1 < TableSize && newPositions[i,j+1] != -1 && separatorsHorizontal[i,j+1] == null)
			{
				positions.Add(new Vector2((float)i,(float)(j+1)));
			}
			if(i+1 < TableSize && newPositions[i+1,j] != -1 && separatorsVertical[i,j] == null)
			{
				positions.Add(new Vector2((float)(i+1),(float)j));
			}
			//diagonalnie
			if(i-1 >= 0 && j+1 < TableSize && newPositions[i-1,j+1] != -1 &&
			   !((separatorsHorizontal[i-1,j+1] != null || cells[i-1,j].cellType != Cell.Type.empty || separatorsVertical[i-1,j] != null) && (separatorsVertical[i-1,j+1]!=null || cells[i,j+1].cellType != Cell.Type.empty || separatorsHorizontal[i,j+1] != null)))
			{
				positions.Add(new Vector2((float)(i-1),(float)(j+1)));
			}
			if(i-1 >= 0 && j-1 >=0 && newPositions[i-1,j-1] != -1 &&
			   !((separatorsHorizontal[i-1,j] != null || cells[i-1,j].cellType != Cell.Type.empty || separatorsVertical[i-1,j] != null) && (separatorsVertical[i-1,j-1] != null || cells[i,j-1].cellType != Cell.Type.empty || separatorsHorizontal[i,j-1] != null)))
			{
				positions.Add(new Vector2((float)(i-1),(float)(j-1)));
			}
			if(i+1 < TableSize && j-1 >= 0 && newPositions[i+1,j-1] != -1 &&
			   !((separatorsHorizontal[i+1,j] != null || cells[i+1,j].cellType != Cell.Type.empty || separatorsVertical[i,j] != null) && (separatorsVertical[i,j-1]!= null || cells[i,j-1].cellType != Cell.Type.empty || separatorsHorizontal[i,j] != null)))
			{
				positions.Add(new Vector2((float)(i+1),(float)(j-1)));
			}
			if(i+1 < TableSize && j+1 < TableSize && newPositions[i+1,j+1] != -1 &&
			   !((separatorsHorizontal[i+1,j+1] != null || cells[i+1,j].cellType != Cell.Type.empty || separatorsVertical[i,j] != null) && (separatorsVertical[i,j+1] != null || cells[i,j+1].cellType != Cell.Type.empty || separatorsHorizontal[i,j+1] != null)))
			{
				positions.Add(new Vector2((float)(i+1),(float)(j+1)));
			}
			if(positions.Count >=3)
			{
				goto next;
			}
			else
				positions.RemoveRange(0,positions.Count);
		}


		next:
		List<FieldItem> bubblesForMatch;
		for(int i = 0; i < Enum.GetNames(typeof(Bubble.Type)).Length;i++)
		{
			bubblesForMatch = tmpBubbles.FindAll(e => e.type == (Bubble.Type)i);
			if(bubblesForMatch.Count >= 3) break;
		}

		int count = Mathf.Min(positions.Count, bubblesForMatch.Count);

		for (int i = 0; i < count; i++) 
		{
			int x = (int) positions[i].x;
			int y = (int) positions[i].y;
			tmpBubbles.Remove(bubblesForMatch[i]);
			bubblesForMatch[i].posX = x;
			bubblesForMatch[i].posY = y;
			newPositions[x,y] = 1;
			moveBubbles.Add(bubblesForMatch[i]);
			bubbles[x,y] = bubblesForMatch[i];
		}

		for(int i = 0; i < tmpBubbles.Count; i++)
		{
			int x = UnityEngine.Random.Range(0,TableSize);
			int y = UnityEngine.Random.Range(0,TableSize);

			while(newPositions[x,y]!=0)
			{
				if(x+1 < TableSize)
					x++;
				else
				{
					x = 0;
					if(y+1 < TableSize)
						y++;
					else
						y = 0;
				}
			}

			tmpBubbles[i].posX = x;
			tmpBubbles[i].posY = y;
			newPositions[x,y] = 1;
			moveBubbles.Add(tmpBubbles[i]);
			bubbles[x,y] = tmpBubbles[i];
		}
		moveBubblesMix();
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
