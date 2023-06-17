using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public enum ScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}
public class ScoreManager : MonoBehaviour
{
    static private ScoreManager Self;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set Dynamically")] 
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;
    public int multiplier = 1;

    private void Awake()
    {
        if (Self == null)
        {
            Self = this;
        }

        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(ScoreEvent evt)
    {
        try
        {
            Self.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError($"ScoreManager:EVENT() called while Self = null. \n {nre}");
        }
    }

    void Event(ScoreEvent evt)
    {
        switch (evt)
        {
            case ScoreEvent.draw:
            case ScoreEvent.gameWin:
            case ScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun * multiplier;
                scoreRun = 0;
                multiplier = 1;
                break;
            
            case ScoreEvent.mine:
                chain++;
                scoreRun += chain;
                break;
            
            case ScoreEvent.mineGold:
                chain++;
                scoreRun += chain;
                multiplier++;
                break;
        }

        switch (evt)
        {
            case ScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;
            
            case ScoreEvent.gameLoss:
                if (HIGH_SCORE < score)
                {
                    print("You got the high score! High score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                }

                break;
            default:
                print($"score: {score} scoreRun:{scoreRun} chain: {chain}");
                break;
        }
    }
    
    static public int CHAIN
    {
        get { return Self.chain; }
    }

    static public int SCORE
    {
        get { return Self.score; }
    }

    public static int SCORE_RUN
    {
        get { return Self.scoreRun; }
    }
    
    
}
