using System.Collections.Generic;
using UnityEngine;

public class LetterDeliverySystem : MonoBehaviour
{
    public static LetterDeliverySystem Instance { get; private set; }

    private HashSet<int> deliveredHouseIDs = new HashSet<int>();

    private PlayerMovementManager playerMovement;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerMovement = FindFirstObjectByType<PlayerMovementManager>();

    }

    /// <summary>
    /// Attempts to deliver letter to recipientHouseID.
    /// Returns true if correct and not previously delivered; false otherwise.
    /// On success it marks house as delivered.
    /// </summary>

    public bool AttemptDeliver(LetterData letter, int recipientHouseID)
    {

        if (letter == null) return false;
        if (deliveredHouseIDs.Contains(recipientHouseID)) return false;
        bool ok = letter.houseID == recipientHouseID;
        if (ok)
        {
            deliveredHouseIDs.Add(recipientHouseID);

          //  if (TaskManager.Instance != null)
          //      TaskManager.Instance.NotifyLetterDelivered(letter, recipientHouseID); */
        }
        return ok;
    }

    public bool IsDelivered(int houseID)
    {
        return deliveredHouseIDs.Contains(houseID);
    }
}
