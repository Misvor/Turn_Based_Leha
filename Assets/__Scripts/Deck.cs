using System;
using System.Collections.Generic;
using System.Linq;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Deck : MonoBehaviour
{
	[Header("Set in Inspector")] 
	
	public bool startFaceUp = false;
	
	// suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;
	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;

	public GameObject prefabCard;
	public GameObject prefabSprite;
	
	[FormerlySerializedAs("XmlReader")] [Header("Set Dynamically")] 
	public PT_XMLReader xmlReader;

	public List<Card> cards;
	public List<string> cardNames;
	public List<Decorator> decorators;
	public List<CardDefinition> cardDefs;
	public Transform deckAnchor;
	public Dictionary<string, Sprite> Suits;

	public void InitDeck(string deckXMLText)
	{
		if (GameObject.Find("_Deck") == null)
		{
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}

		Suits = new Dictionary<string, Sprite>()
		{
			{ "C", suitClub },
			{ "D", suitDiamond },
			{ "H", suitHeart },
			{ "S", suitSpade },
		};
		
		ReadDeck(deckXMLText);
		
		MakeCards();
	}

	public void ReadDeck(string deckXMLText)
	{
		xmlReader = new PT_XMLReader();
		xmlReader.Parse(deckXMLText);

		string s = "xml[0] decorator[0] ";

		//example
		s += "type= " + xmlReader.xml["xml"][0]["decorator"][0].att("type");
		s += "x= " + xmlReader.xml["xml"][0]["decorator"][0].att("x");
		s += "y= " + xmlReader.xml["xml"][0]["decorator"][0].att("y");
		s += "scale= " + xmlReader.xml["xml"][0]["decorator"][0].att("scale");
		// print(s);
		decorators = new List<Decorator>();

		PT_XMLHashList xDecos = xmlReader.xml["xml"][0]["decorator"];
		Decorator deco;

		for (var i = 0; i < xDecos.Count; i++)
		{
			deco = new Decorator();
			deco.Type = xDecos[i].att("type");
			deco.Scale = float.Parse(xDecos[i].att("scale"));
			deco.Flip = String.Equals(xDecos[i].att("flip"), "1");
			
			deco.Location.x = float.Parse(xDecos[i].att("x"));
			deco.Location.y = float.Parse(xDecos[i].att("y"));
			deco.Location.z = float.Parse(xDecos[i].att("z"));
			decorators.Add(deco);
		}

		cardDefs = new List<CardDefinition>();
		PT_XMLHashList xCardDefs = xmlReader.xml["xml"][0]["card"];
		for (var i = 0; i < xCardDefs.length; i++)
		{
			CardDefinition cDef = new CardDefinition();
			
			cDef.Rank = Int32.Parse(xCardDefs[i].att("rank"));

			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null)
			{
				for( var j = 0; j < xPips.Count; j++)
				{
					deco = new Decorator();
					deco.Type = "pip";
					deco.Flip = String.Equals(xPips[j].att("flip"), "1");
					deco.Location.x = float.Parse(xPips[j].att("x"));
					deco.Location.y = float.Parse(xPips[j].att("y"));
					deco.Location.z = float.Parse(xPips[j].att("z"));
					if (xPips[j].HasAtt("scale"))
					{
						deco.Scale = float.Parse(xPips[j].att("scale"));
					}
					
					cDef.Pips.Add(deco);
				}
			}

			if (xCardDefs[i].HasAtt("face"))
			{
				cDef.Face = xCardDefs[i].att("face");
			}
			
			cardDefs.Add(cDef);
		}



	}

	public CardDefinition GetCardDefinitionByRank(int rank)
	{
		foreach (var cardDef in cardDefs)
		{
			if (cardDef.Rank == rank)
			{
				return cardDef;
			}
		}
		
		return null;
	}

	public void MakeCards()
	{
		cardNames = new List<string>();
		string[] letters = new[] { "C", "H", "D", "S" };
		foreach (var letter in letters)
		{
			for (var i = 0; i < 13; i++)
			{
				cardNames.Add(letter+(i+1));
			}
		}

		cards = new List<Card>();

		for (var i = 0; i < cardNames.Count; i++)
		{
			var rand = Random.Range(1, 11);
			if (rand == 10)
			{
				cards.Add(MakeCard(i));
			}
			else
			{
				cards.Add(MakeCard(i));
			}
		}
	}

	public static void Shuffle(ref List<Card> cards)
	{
		List<Card> tempCards = new List<Card>();

		int ndx;
		while (cards.Count > 0)
		{
			ndx = Random.Range(0, cards.Count);
			tempCards.Add(cards[ndx]);
			cards.RemoveAt(ndx);
		}

		cards = tempCards;
	}

	private Card MakeCard(int cardsInHand)
	{
		GameObject cgo = Instantiate(prefabCard, deckAnchor, true) as GameObject;
		Card card = cgo.GetComponent<Card>();

		bool isGolden = Random.value < 0.1f;
		cgo.transform.localPosition = new Vector3((cardsInHand % 13) * 3, cardsInHand / 13 * 4, 0);
		cgo.GetComponent<SpriteRenderer>().sprite = isGolden ? cardFrontGold : cardFront;

		card.name = cardNames[cardsInHand];
		card.suit = card.name[0].ToString();
		card.rank = int.Parse(card.name.AsSpan()[1..]);
		switch (card.suit)
		{
			case "D":
			case "H":
				card.color = Color.red;
				card.colS = "Red";
				break;
			case "S":
			case "C":
				card.color = Color.black;
				card.colS = "Black";
				break;
		}

		card.def = GetCardDefinitionByRank(card.rank);

		card.IsGolden = isGolden;
		AddDecorators(card);
		AddFace(card);
		AddPips(card);
		AddBack(card);
		return card;
	}


	private Sprite _tSp = null;
	private GameObject _tGO = null;
	private SpriteRenderer _tSR = null;

	private void AddDecorators(Card card)
	{
		foreach (var dec in decorators)
		{
			_tGO = Instantiate(prefabSprite) as GameObject;

			_tSR = _tGO.GetComponent<SpriteRenderer>();
			
			_tGO.transform.SetParent(card.transform);

			if (dec.Type == "suit")
			{
				_tSR.sprite = Suits[card.suit];
			}
			else
			{
				_tSp = rankSprites[card.rank];
				_tSR.sprite = _tSp;
				_tSR.color = card.color;
			}

			_tSR.sortingOrder = 1; // sprites over card
			_tGO.transform.localPosition = dec.Location;

			if (dec.Flip)
			{
				_tGO.transform.rotation = Quaternion.Euler(0,0, 180);
			}

			if (dec.Scale != 1)
			{
				_tGO.transform.localScale = Vector3.one * dec.Scale;
			}

			_tGO.name = dec.Type;
			
			card.decoGOs.Add(_tGO);
		}
	}

	private void AddPips(Card card)
	{
		foreach (var pip in card.def.Pips)
		{
			_tGO = Instantiate(prefabSprite) as GameObject;
			_tGO.transform.SetParent(card.transform);
			_tGO.transform.localPosition = pip.Location;

			if (pip.Flip)
			{
				_tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
			}

			if (pip.Scale != 1)
			{
				_tGO.transform.localScale = pip.Scale * Vector3.one;
			}

			_tGO.name = "pip";

			_tSR = _tGO.GetComponent<SpriteRenderer>();

			_tSR.sprite = Suits[card.suit];
			_tSR.sortingOrder = 1;
			
			card.pipGOs.Add(_tGO);
		}
	}

	private void AddFace(Card card)
	{
		if (card.def.Face == String.Empty)
		{
			return;
		}

		_tGO = Instantiate(prefabSprite);
		_tSp = GetFace($"{card.def.Face}{card.suit}");
		_tSR = _tGO.GetComponent<SpriteRenderer>();
		_tSR.sprite = _tSp;
		_tSR.sortingOrder = 1;
		_tGO.transform.SetParent(card.transform);
		_tGO.transform.localPosition = Vector3.zero; // default position;
		if (card.IsGolden)
		{
			_tSR.sprite = cardFrontGold;
		}
		_tGO.name = "face";
	}
	private Sprite GetFace(string face) => faceSprites.FirstOrDefault(x => x.name == face);

	private void AddBack(Card card)
	{
		
		_tGO = Instantiate(prefabSprite);
		_tSR = _tGO.GetComponent<SpriteRenderer>();
		_tSR.sprite = cardBack;
		_tGO.transform.SetParent(card.transform);
		_tGO.transform.localPosition = Vector3.zero;
		if (card.IsGolden)
		{
			_tSR.sprite = cardBackGold;
		}
		_tSR.sortingOrder = 2;
		_tGO.name = "back";
		card.back = _tGO;
		card.faceUp = startFaceUp;
	}
	
}
