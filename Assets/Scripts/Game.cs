using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Game : MonoBehaviour {

	public int TableSize;
	public int ScoresPerBubble;
	public float BubblePadding;
	public RectTransform Table;
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

	private Bubble[,] bubbles;
	private Bubble firstBubble;
	private Bubble secondBubble;
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
		bubbles = new Bubble[TableSize,TableSize];
		BubblePool.Get ().Initialize (TableSize);
		ParticlesPool.Get ().Initialize (20);
		calculateBubblesValues ();
		fillTable ();
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
		firstBubble = bubble;
		gameState = GameState.bubblePressed;
	}

	public void BubblePointerEnter(Bubble bubble)
	{
		if (gameState != GameState.bubblePressed)
						return;
		if((Mathf.Abs(firstBubble.posX - bubble.posX) == 1 && firstBubble.posY - bubble.posY == 0) ||
		   (Mathf.Abs(firstBubble.posY - bubble.posY) == 1 && firstBubble.posX - bubble.posX == 0))
		{

			gameState = GameState.InAction;
			secondBubble = bubble;
			swapBubbles();
		}
		else
			gameState =  GameState.free;
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
				BubblePool.Get().Push(bubbles[i,j].gameObject);

		bubbles = new Bubble[TableSize,TableSize];
		fillTable ();
		curtainAnimator.Play ("curtain_open", 0, 0f);
	}

	void showScores()
	{
		scoresView.text = "Scores: " + scores.ToString ();
	}
	void swapBubbles ()
	{
		List<Bubble> list = new List<Bubble> ();
		bubbles [firstBubble.posX, firstBubble.posY] = secondBubble;
		bubbles [secondBubble.posX, secondBubble.posY] = firstBubble;
		bool a = checkAllDirections (firstBubble.posX, firstBubble.posY,ref list);
		bool b = checkAllDirections (secondBubble.posX, secondBubble.posY,ref list);
		if(a || b)
		{
			int tmpX = firstBubble.posX;
			int tmpY = firstBubble.posY;
			firstBubble.posX = secondBubble.posX;
			firstBubble.posY = secondBubble.posY;
			secondBubble.posX = tmpX;
			secondBubble.posY = tmpY;
			StartCoroutine(swapBubblesAnim(false,list));
		}
		else
		{
			bubbles [firstBubble.posX, firstBubble.posY] = firstBubble;
			bubbles [secondBubble.posX, secondBubble.posY] = secondBubble;
			StartCoroutine(swapBubblesAnim(true));
		}
	}

	IEnumerator swapBubblesAnim(bool reverse, List<Bubble> list = null)
	{
		yield return new WaitForEndOfFrame();
		float cof = 0f;
		Vector3 firstPos = firstBubble.transform.position;
		Vector3 secondPos = secondBubble.transform.position;
		while(cof < 1f)
		{
			cof += Time.deltaTime*speed;
			cof = Mathf.Min(cof,1f);
			firstBubble.transform.position = Vector3.Lerp(firstPos,secondPos,cof);
			secondBubble.transform.position = Vector3.Lerp(secondPos,firstPos,cof);
			yield return new WaitForEndOfFrame();
		}
		if(reverse)
		{
			while(cof > 0f)
			{
				cof -= Time.deltaTime*speed;
				cof = Mathf.Max(cof,0f);
				firstBubble.transform.position = Vector3.Lerp(firstPos,secondPos,cof);
				secondBubble.transform.position = Vector3.Lerp(secondPos,firstPos,cof);
				yield return new WaitForEndOfFrame();
			}
			gameState = GameState.free;
		}
		else
		{
			destroyFoundBubbles(list);
		}
		yield return null;
	}

	void destroyFoundBubbles (List<Bubble> list)
	{
		removeDublicate (ref list);
		for(int i = 0;i < list.Count; i++)
		{
			Bubble bubble = bubbles[list[i].posX,list[i].posY];
			bubbles[list[i].posX,list[i].posY] = null;
			Vector3 pos  = bubble.transform.localPosition;
			BubblePool.Get().Push(bubble.gameObject);
			ParticleEmitter particle = ParticlesPool.Get().Pull();
			particle.transform.SetParent(Table.transform);
			particle.transform.localPosition = pos;
			StartCoroutine(removeParticle(particle));
		}
		scores += list.Count * ScoresPerBubble;
		showScores ();
		dropBalls ();
	}
	IEnumerator removeParticle(ParticleEmitter particle)
	{
		yield return new WaitForEndOfFrame ();
		particle.emit = true;
		yield return new WaitForSeconds (0.3f);
		ParticlesPool.Get ().Push (particle);
		yield return null;
	}
	void dropBalls ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 1; j < TableSize; j++)
		{
			int indx = j;
			while(indx >= 1 &&bubbles[i,indx-1] == null)
			{
				indx--;
			}
			indx =(int) Mathf.Max(0f,(float)indx);
			if(indx != j && bubbles[i,j] != null)
			{
				//tyt vozmojno nado zdelat' rekyrsiu
				Bubble tmp  = bubbles[i,j];
				bubbles[i,j] = null;
				bubbles[i,indx] = tmp;
				tmp.posY = indx;
				bubblesInAction++;
				StartCoroutine(moveBubble(tmp,j-indx));
			}
		}
		if(bubblesInAction == 0 )
			dropNewBalls();
	}

	IEnumerator moveBubble (Bubble bubble,int steps)
	{
		yield return new WaitForEndOfFrame ();
		Vector3 startPos = bubble.transform.localPosition;
		Vector3 endPos = new Vector3(startPos.x,(float)bubble.posY * bubbleSize + ((float)(bubble.posY) * BubblePadding)-bubblesOffset, 0f);
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
		if(list.Count>0)
		{
			destroyFoundBubbles(list);
		}
		else
			dropNewBalls();
	}

	void dropNewBalls ()
	{
		int preValue;
		int steps = 0;
		for(int i = 0; i < TableSize; i++)
		{
			preValue = i;
			for(int j = 0; j < TableSize;j++)
			{
				if(preValue != i)
				{
					preValue = i;
					steps = 0;
				}
				if(bubbles[i,j] == null)
				{
					steps ++;
					Bubble bubble = BubblePool.Get().Pull().GetComponent<Bubble>();
					bubbles[i,j] = bubble;
					bubble.posX = i;
					bubble.posY = j;
					bubblesInAction++;
					bubble.transform.parent = Table.transform;
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
		
	void fillTable ()
	{
		for(int i = 0; i < TableSize; i++)
			for(int j = 0; j < TableSize;j++)
			{
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

	void insertBubbleInTable (Bubble bubble)
	{
		bubble.transform.parent = Table.transform;
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
		bubbleSize = (Table.rect.height - ((TableSize + 1) * BubblePadding)) / (float)TableSize;
		bubblesOffset = (Table.rect.height/2f) - bubbleSize/2f - BubblePadding;
	}
}
