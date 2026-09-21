using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles game progression milestones, crop unlocks, and feature unlock checks.
/// </summary>
public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }

    [SerializeField]
    private List<int> unlockedItemIds = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool IsItemUnlocked(int itemId)
    {
        if (Player.main != null && Player.main.unlockedCrops != null && Player.main.unlockedCrops.Contains(itemId))
        {
            return true;
        }
        return unlockedItemIds.Contains(itemId);
    }

    public void UnlockItem(int itemId, string itemName, Sprite icon = null)
    {
        if (!IsItemUnlocked(itemId))
        {
            if (Player.main != null && !Player.main.unlockedCrops.Contains(itemId))
            {
                Player.main.unlockedCrops.Add(itemId);
            }
            unlockedItemIds.Add(itemId);
            UnlockUI.ShowUnlock("NEW UNLOCK!", $"You unlocked {itemName}!", icon);
            Debug.Log($"[UnlockManager] Unlocked item: {itemName} (ID: {itemId})");
        }
    }
}
