using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
   public static Scoreboard Self;

   [Header("Set in Inspector")] public GameObject prefabFloatingScore;

   [Header("Set Dynamically")] [SerializeField]
   private int _score = 0;

   [SerializeField] private string _scoreString;

   private Transform canvasTrans;

   public int Score
   {
      get
      {
         return _score;
      }
      set
      {
         _score = value;
         scoreString = _score.ToString("N0");
      }
   }

   public string scoreString
   {
      get
      {
         return (_scoreString);
      }
      set
      {
         _scoreString = value;
         GetComponent<TextMeshProUGUI>().text = _scoreString;
      }
   }

   private void Awake()
   {
      if (Self == null)
      {
         Self = this;
      }
      else
      {
         Debug.LogError("ERROR: Scoreboard.Awake(): Self is already set!");
      }

      canvasTrans = transform.parent;
   }

   public void FSCallback(FloatingScore fs)
   {
      Score += fs.score;
   }

   public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
   {
      GameObject go = Instantiate<GameObject>(prefabFloatingScore);
      go.transform.SetParent(canvasTrans);
      FloatingScore fs = go.GetComponent<FloatingScore>();

      fs.score = amt;
      fs.reportFinishTo = this.gameObject;
      fs.Init(pts);
      return fs;
   }
   
}
