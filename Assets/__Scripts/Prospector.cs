using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.WSA;

public class Prospector : MonoBehaviour
{
	public static Prospector Self;

	[Header("Set in Inspector")]
	public TextAsset deckXML;
	public TextAsset layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;
	public Vector2 fsPosMid = new Vector2(0.5f, 0.9f);
	public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
	public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
	public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
	public float reloadDelay = 2f;
	public TextMeshProUGUI gameOverText, roundResultText, highScoreText;

	[Header("Sets dynamically")]
	public Deck deck;
	public Layout layout;
	public List<CardProspector> drawPile;
	public Transform layoutAnchor;
	public CardProspector target;
	public List<CardProspector> table;
	public List<CardProspector> discardPile;
	public FloatingScore fsRun;

	void Awake()
	{
		Self = this;
		SetUITexts();
	}

	void SetUITexts()
	{
		GameObject go = GameObject.Find("HighScore");
		if (go != null)
		{
			highScoreText = go.GetComponent<TextMeshProUGUI>();
		}

		int highScore = ScoreManager.HIGH_SCORE;
		string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
		go.GetComponent<TextMeshProUGUI>().text = hScore;

		go = GameObject.Find("GameOver");
		if (go != null)
		{
			gameOverText = go.GetComponent<TextMeshProUGUI>();
		}
		go = GameObject.Find("RoundResult");
		if (go != null)
		{
			roundResultText = go.GetComponent<TextMeshProUGUI>();
		}

		ShowResultsUI(false);
	}

	void ShowResultsUI(bool show)
	{
		gameOverText.gameObject.SetActive(show);
		roundResultText.gameObject.SetActive(show);
	}
	void Start()
	{
		Scoreboard.Self.Score = ScoreManager.SCORE;
		deck = GetComponent<Deck>();
		deck.InitDeck(deckXML.text);
		Deck.Shuffle(ref deck.cards);

		// Card c;
		// for (var i = 0; i < deck.cards.Count; i++)
		// {
		// 	c = deck.cards[i];
		// 	c.transform.localPosition = new Vector3((i % 13) * 3, (i / 13) * 4, 0);
		// }

		layout = GetComponent<Layout>();
		layout.ReadLayout(layoutXML.text);
		drawPile = deck.cards.Select(x => x as CardProspector).ToList();
		LayoutGame();
	}

	CardProspector Draw()
	{
		CardProspector cd = drawPile[0];
		drawPile.RemoveAt(0);
		return cd;
	}

	void LayoutGame()
	{
		if (layoutAnchor == null)
		{
			var tempGO = new GameObject("_LayoutAnchor");
			layoutAnchor = tempGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}

		CardProspector cp;
		foreach (var tempSlotDef in layout.SlotDefs)
		{
			cp = Draw();
			cp.faceUp = tempSlotDef.faceUp;
			var transform1 = cp.transform;
			transform1.parent = layoutAnchor;

			transform1.localPosition = new Vector3(layout.multiplier.x * tempSlotDef.x,
				layout.multiplier.y * tempSlotDef.y, -tempSlotDef.layerID);

			cp.layoutId = tempSlotDef.id;
			cp.SlotDef = tempSlotDef;
			cp.state = eCardState.tableau;
			cp.SetSortingLayerName(tempSlotDef.layerName);
			table.Add(cp);
		}

		foreach (var card in table)
		{
			foreach (var hider in card.SlotDef.hiddenBy)
			{
				cp = FindCardByLayoutId(hider);
				card.hiddenBy.Add(cp);
			}
		}
		
		MoveToTarget(Draw());
		
		UpdateDrawPile();
	}

	CardProspector FindCardByLayoutId(int layoutId)
	{
		foreach (var card in table)
		{
			if (card.layoutId == layoutId)
			{
				return card;
			}
		}

		return null;
	}

	void ShowUnhidden()
	{
		foreach (var card in table)
		{
			card.faceUp = !card.hiddenBy.Any(x => x.state == eCardState.tableau);
		}
	}

	void MoveToDiscard(CardProspector cd)
	{
		cd.state = eCardState.discard;
		
		discardPile.Add(cd);
		cd.transform.parent = layoutAnchor;

		cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID + 0.5f);
		cd.faceUp = true;
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100+discardPile.Count);
	}

	void MoveToTarget(CardProspector cd)
	{
		if (target != null)
		{
			MoveToDiscard(target);
		}

		target = cd;
		cd.state = eCardState.target;
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID);
		cd.faceUp = true;
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(0);

	}

	void UpdateDrawPile()
	{
		CardProspector cd;

		for (var i = 0; i < drawPile.Count; i++)
		{
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;

			Vector2 dpStagger = layout.drawPile.stagger;

			cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + dpStagger.x * i),
				layout.multiplier.y * (layout.drawPile.y + dpStagger.y * i),
				-layout.drawPile.layerID + 0.1f*i);
			cd.faceUp = false;
			cd.state = eCardState.drawpile;
			
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10*i);
		}
	}

	public void CardClicked(CardProspector cd)
	{
		switch (cd.state)
		{
			case eCardState.target:
				break;
			case eCardState.drawpile:
				MoveToDiscard(target);
				MoveToTarget(Draw());
				UpdateDrawPile();
				ScoreManager.EVENT(ScoreEvent.draw);
				FloatingScoreHandler(ScoreEvent.draw);
				break;
			case eCardState.tableau:
				
				if (!cd.faceUp || !AdjacentRank(cd, target))
				{
					return;
				}

				table.Remove(cd);
				MoveToTarget(cd);
				ShowUnhidden();
				
				//if(cd.IsGolden)
				//{
				// ScoreManager.EVENT(ScoreEvent.mineGold);
				//}

				ScoreManager.EVENT(ScoreEvent.mine);
				FloatingScoreHandler(ScoreEvent.mine);
				break;
				
		}

		CheckForGameOver();
	}

	void CheckForGameOver()
	{
		if (table.Count == 0)
		{
			GameOver(true);
			return;
		}

		if (drawPile.Count > 0)
		{
			return;
		}

		foreach (var card in table)
		{
			if (AdjacentRank(card, target))
			{
				return;
			}
		}
		GameOver(false);
	}

	void GameOver(bool win)
	{
		int score = ScoreManager.SCORE;
		if (fsRun != null)
		{
			score += fsRun.score;
		}
		
		if (win)
		{
			gameOverText.text = "Round Over";
			roundResultText.text = "You won this round! \nRound Score: " + score;
			ShowResultsUI(true);
			//print("Game Over. You won! :D");
			ScoreManager.EVENT(ScoreEvent.gameWin);
			FloatingScoreHandler(ScoreEvent.gameWin);
		}
		else
		{
			gameOverText.text = "Game Over";
			if (ScoreManager.HIGH_SCORE <= score)
			{
				roundResultText.text = "You got the High Score! \nhigh score: " + score;
			}
			else
			{
				roundResultText.text = "Your final score was: " + score;
			}
			
			ShowResultsUI(true);
			//print("Game Over. You lost D:");
			ScoreManager.EVENT(ScoreEvent.gameLoss);
			FloatingScoreHandler(ScoreEvent.gameLoss);
		}

		//SceneManager.LoadScene("__Prospector_Scene_0");
		
		Invoke("ReloadLevel", reloadDelay);
	}

	void ReloadLevel()
	{
		SceneManager.LoadScene("__Prospector_Scene_0");
	}
	
	private bool AdjacentRank(CardProspector first, CardProspector second)
	{
		if (!first.faceUp || !second.faceUp)
		{
			return false;
		}

		if (Mathf.Abs(first.rank - second.rank) == 1)
		{
			return true;
		}

		if (first.rank == 1 && second.rank == 13)
		{
			return true;
		}

		if (first.rank == 13 && second.rank == 1)
		{
			return true;
		}

		return false;
	}

	private void FloatingScoreHandler(ScoreEvent evt)
	{
		var fsPts = new List<Vector2>();

		switch (evt)
		{
			case ScoreEvent.draw:
			case ScoreEvent.gameWin:
			case ScoreEvent.gameLoss:
			if (fsRun != null)
			{
				fsPts.Add(fsPosRun);
				fsPts.Add(fsPosMid2);
				fsPts.Add(fsPosEnd);
				fsRun.Init(fsPts, 0, 1);

				fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
				fsRun = null;
			}

			break;
			case ScoreEvent.mine:
				FloatingScore fs;
				Vector2 p0 = Input.mousePosition;
				p0.x /= Screen.width;
				p0.y /= Screen.height;
				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosMid);
				fsPts.Add(fsPosRun);
				fs = Scoreboard.Self.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
				if (fsRun == null)
				{
					fsRun = fs;
					fsRun.reportFinishTo = null;
				}
				else
				{
					fs.reportFinishTo = fsRun.gameObject;
				}

				break;
		}
	}
}
