using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FloatScoreState
{
    idle,
    pre,
    active,
    post
}
public class FloatingScore : MonoBehaviour
{

    [Header("Sets Dynamically")] public FloatScoreState state = FloatScoreState.idle;

    [SerializeField] protected int _score = 0;
    public string scoreString;

    public int score
    {
        get { return _score; }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");
            GetComponent<TMPro.TextMeshProUGUI>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts;
    public List<float> fontSizes;
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut;

    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;
    private TextMeshProUGUI txt;


    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<TextMeshProUGUI>();

        bezierPts = new List<Vector2>(ePts);

        if (ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }

        if (eTimeS == 0)
        {
            eTimeS = Time.time;
        }

        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = FloatScoreState.pre;
    }

    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    private void Update()
    {
        if (state == FloatScoreState.idle)
        {
            return;
        }

        var u = (Time.time - timeStart) / timeDuration;
        var uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        {
            state = FloatScoreState.pre;
            txt.enabled = false;
        }
        else
        {
            if (u >= 1)
            {
                uC = 1;
                state = FloatScoreState.post;

                if (reportFinishTo != null)
                {
                    reportFinishTo.SendMessage("FSCallback", this);
                    Destroy(gameObject);
                }
                else
                {
                    state = FloatScoreState.idle;
                }
            }
            else
            {
                state = FloatScoreState.active;
                txt.enabled = true;
            }

            Vector2 pos = Utils.Bezier(uC, bezierPts);

            rectTrans.anchorMin = rectTrans.anchorMax = pos;

            if (fontSizes != null && fontSizes.Count > 0)
            {
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<TextMeshProUGUI>().fontSize = size;
            }
        }
    }
}
