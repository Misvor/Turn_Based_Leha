using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

public class Card : MonoBehaviour
{
	
	[Header("Set Dynamically")]
	public string suit;
	public int rank;
	public Color color;
	public string colS = "Black";

	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();
	public GameObject back;
	public CardDefinition def;

	public SpriteRenderer[] spriteRenderers;
	
	public bool IsGolden = false;

	private void Start()
	{
		SetSortOrder(0);
	}

	public void PopulateSpriteRenderers()
	{
		if (spriteRenderers == null || spriteRenderers.Length == 0)
		{
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}
	}

	public void SetSortingLayerName(string tempSortingLayerName)
	{
		PopulateSpriteRenderers();

		foreach (var renderer in spriteRenderers)
		{
			renderer.sortingLayerName = tempSortingLayerName;
		}
	}

	public void SetSortOrder(int sortOrder)
	{
		PopulateSpriteRenderers();

		foreach (var renderer in spriteRenderers)
		{
			if (renderer.gameObject == this.gameObject)
			{
				renderer.sortingOrder = sortOrder;
				continue;
			}

			switch (renderer.gameObject.name)
			{
				case "back":
					renderer.sortingOrder = sortOrder + 2;
					break;
				case "face":
					default:
					renderer.sortingOrder = sortOrder + 1;
					break;
			}
		}
	}
	public bool faceUp
	{
		get
		{
			return !back.activeSelf;
		}
		set
		{
			back.SetActive(!value);
		}
	}

	public virtual void OnMouseUpAsButton()
	{
		print(name);
	}
}

[System.Serializable] // to change in Inspector
public class Decorator
{
	[FormerlySerializedAs("type")] public string Type;
	[FormerlySerializedAs("location")] public Vector3 Location;
	[FormerlySerializedAs("flip")] public bool Flip = false;
	[FormerlySerializedAs("scale")] public float Scale = 1f;
}

[Serializable]
public class CardDefinition
{
	[FormerlySerializedAs("face")] public string Face; // sprite name
	[FormerlySerializedAs("rank")] public int Rank; // 1-13
	[FormerlySerializedAs("pips")] public List<Decorator> Pips = new List<Decorator>(); // icons
}