using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    private List<LetterData> letters = new List<LetterData>();

    [Header("Auto-fill from Resources (if startingLetters empty)")]
    public string resourcePath = "Letters";

    public bool instantiateRuntimeCopies = true;

    [Header("Debug / Safety")]
    public int expectedStartingCount = 6;

    public List<LetterData> GetLetters() => letters;

    public LetterData GetLetter(int letterIndex)
    {
        if (letterIndex < 0 || letterIndex >= letters.Count) return null;
        return letters[letterIndex];
    }

    public LetterData GetLetterFromHouseID(int houseID)
    {
        foreach (LetterData letter in letters)
        {
            if (letter != null && letter.houseID == houseID)
                return letter;
        }
        return null;
    }

    public void AddLetter(LetterData newLetter)
    {
        if (newLetter == null) return;
        letters.Add(newLetter);
    }

    public void RemoveLetter(LetterData letter)
    {
        if (letter == null) return;
        letters.Remove(letter);
    }

    private void Awake()
    {
        if (letters == null) letters = new List<LetterData>();
        if (letters.Count == 0)
        {
            LoadLettersFromResources(resourcePath);
        }

        if (letters.Count < expectedStartingCount)
        {
            Debug.LogWarning($"PlayerInventoryManager: Loaded {letters.Count} letters, expected {expectedStartingCount}. " +
                             "Eðer 6 adet mektup istiyorsan LetterData asset'lerini Assets/Resources/" + resourcePath + " içine koyduðundan emin ol.");
        }
        else
        {
            Debug.Log($"PlayerInventoryManager: Loaded {letters.Count} starting letters from Resources/{resourcePath}");
        }
    }

    private void LoadLettersFromResources(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("PlayerInventoryManager: resourcePath boþ. Loading skipped.");
            return;
        }

        LetterData[] loaded = Resources.LoadAll<LetterData>(path);
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning($"PlayerInventoryManager: No LetterData assets found in Resources/{path}.");
            return;
        }

        foreach (var asset in loaded)
        {
            if (asset == null) continue;

            if (instantiateRuntimeCopies)
            {
                var runtimeCopy = ScriptableObject.Instantiate(asset);
                runtimeCopy.name = asset.name + "_runtime";
                letters.Add(runtimeCopy);
            }
            else
            {
                letters.Add(asset);
            }
        }
    }
}
