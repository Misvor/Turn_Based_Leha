using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef
{
    
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}
public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlReader;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;

    public List<SlotDef> SlotDefs;
    public SlotDef drawPile;
    public SlotDef discardPile;

    public string[] sortingLayerNames = new[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    public void ReadLayout(string xmlText)
    {
        xmlReader = new PT_XMLReader();
        xmlReader.Parse(xmlText);
        xml = xmlReader.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        SlotDef tempSD;

        PT_XMLHashList slotsX = xml["slot"];

        for (var i = 0; i < slotsX.length; i++)
        {
            tempSD = new SlotDef();

            if (slotsX[i].HasAtt("type"))
            {
                tempSD.type = slotsX[i].att("type");
            }
            else
            {
                tempSD.type = "slot";
            }

            tempSD.x = float.Parse(slotsX[i].att("x"));
            tempSD.y = float.Parse(slotsX[i].att("y"));
            tempSD.layerID = int.Parse(slotsX[i].att("layer"));

            tempSD.layerName = sortingLayerNames[tempSD.layerID]; // 0

            switch (tempSD.type)
            {
                case "slot":
                    tempSD.faceUp = slotsX[i].att("faceup") == "1";
                    tempSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (var hider in hiding)
                        {
                            tempSD.hiddenBy.Add(int.Parse(hider));
                        }
                    }
                    SlotDefs.Add(tempSD);
                    break;
                case "drawpile":
                    tempSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tempSD;
                    break;
                case "discardpile":
                    discardPile = tempSD;
                    break;
            }

        }
    }

}
