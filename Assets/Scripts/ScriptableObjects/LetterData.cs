using UnityEngine;

[CreateAssetMenu(fileName = "LetterData", menuName = "Scriptable Objects/LetterData")]
public class LetterData : ScriptableObject
{
    public int houseID;
    public string title;
    public string sender;
    public string receiver;
    
    [TextArea(15,20)]
    public string content;

    [Header("Optional")]
    public Sprite icon;
}
