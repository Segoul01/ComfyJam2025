using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    private List<LetterData> letters = new List<LetterData>();

    public List<LetterData> GetLetters() => letters;

    public LetterData GetLetter(int letterIndex) => letters[letterIndex];

    public LetterData GetLetterFromHouseID(int houseID)
    {
        foreach (LetterData letter in letters)
        {
            if (letter.houseID == houseID)
                return letter;
        }
        return null;
    }

    public void AddLetter(LetterData newLetter) => letters.Add(newLetter);

    public void RemoveLetter(LetterData letter) => letters.Remove(letter);
}
