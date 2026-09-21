using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Centralized save and load management with versioning, legacy migration,
/// and file backup.
/// </summary>
public static class SaveSystem
{
    public const int CURRENT_SAVE_VERSION = 2;
    public const string SAVE_KEY = "FarmFortune_SaveData";
    public const string LEGACY_INIT_KEY = "Init";
    public const string NEW_GAME_FLAG_KEY = "FarmFortune_NewGameFlag";
    private const string SAVE_FILE_NAME = "farmfortune_save.json";

    private static string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    /// <summary>
    /// Check whether any valid save data exists (either versioned JSON or legacy PlayerPrefs).
    /// </summary>
    public static bool HasSave()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY)) return true;
        if (File.Exists(GetSaveFilePath())) return true;
        if (PlayerPrefs.GetInt(LEGACY_INIT_KEY, 0) == 1 && PlayerPrefs.HasKey("money")) return true;
        return false;
    }

    /// <summary>
    /// Flags that the player requested a brand new game (e.g. from main menu 'Start').
    /// </summary>
    public static void RequestNewGame()
    {
        PlayerPrefs.SetInt(NEW_GAME_FLAG_KEY, 1);
        PlayerPrefs.SetInt(LEGACY_INIT_KEY, 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Check if a new game was requested.
    /// </summary>
    public static bool IsNewGameRequested()
    {
        return PlayerPrefs.GetInt(NEW_GAME_FLAG_KEY, 0) == 1;
    }

    /// <summary>
    /// Clears the new game flag.
    /// </summary>
    public static void ClearNewGameRequest()
    {
        PlayerPrefs.DeleteKey(NEW_GAME_FLAG_KEY);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Serializes and persists SaveData.
    /// </summary>
    public static void Save(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveSystem] Cannot save null SaveData.");
            return;
        }

        data.saveVersion = CURRENT_SAVE_VERSION;
        data.timestamp = DateTime.UtcNow.Ticks;

        string json = JsonUtility.ToJson(data, true);

        // Save to PlayerPrefs
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.SetInt(LEGACY_INIT_KEY, 1);
        PlayerPrefs.DeleteKey(NEW_GAME_FLAG_KEY);
        PlayerPrefs.Save();

        // Backup to persistent file
        try
        {
            string path = GetSaveFilePath();
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveSystem] Failed to write backup save file: {ex.Message}");
        }

        Debug.Log($"[SaveSystem] Game saved successfully (v{data.saveVersion}, {data.playerData.money} coins, {data.playerData.inventory.Count} item types).");
    }

    /// <summary>
    /// Loads SaveData. Migrates legacy saves if necessary.
    /// Returns null if no save exists or if a new game was explicitly requested.
    /// </summary>
    public static SaveData Load()
    {
        if (IsNewGameRequested())
        {
            Debug.Log("[SaveSystem] New game was requested; skipping save load.");
            ClearNewGameRequest();
            return null;
        }

        string json = null;

        // Try PlayerPrefs first
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            json = PlayerPrefs.GetString(SAVE_KEY);
        }
        else
        {
            // Try backup file
            string path = GetSaveFilePath();
            if (File.Exists(path))
            {
                try
                {
                    json = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveSystem] Failed to read backup save file: {ex.Message}");
                }
            }
        }

        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data != null)
                {
                    if (data.saveVersion < CURRENT_SAVE_VERSION)
                    {
                        data = MigrateVersion(data);
                    }
                    Debug.Log($"[SaveSystem] Loaded save data v{data.saveVersion} (Money: {data.playerData.money}).");
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Error parsing save data JSON: {ex.Message}");
            }
        }

        // Check for legacy PlayerPrefs save
        if (PlayerPrefs.GetInt(LEGACY_INIT_KEY, 0) == 1 && PlayerPrefs.HasKey("money"))
        {
            Debug.Log("[SaveSystem] Legacy save detected. Migrating to SaveData v1...");
            SaveData migrated = MigrateLegacySave();
            Save(migrated);
            return migrated;
        }

        return null;
    }

    /// <summary>
    /// Version schema upgrade migration hook.
    /// </summary>
    private static SaveData MigrateVersion(SaveData oldData)
    {
        Debug.Log($"[SaveSystem] Migrating save data from v{oldData.saveVersion} to v{CURRENT_SAVE_VERSION}...");
        if (oldData.goalData == null) oldData.goalData = new GoalSaveData();
        if (oldData.tutorialData == null) oldData.tutorialData = new TutorialSaveData();
        if (oldData.orderData == null) oldData.orderData = new OrderSaveData();
        oldData.saveVersion = CURRENT_SAVE_VERSION;
        return oldData;
    }

    /// <summary>
    /// Migrates unversioned loose PlayerPrefs keys into structured SaveData.
    /// Preserves existing money, land, workers, tools, crops, and inventory.
    /// </summary>
    private static SaveData MigrateLegacySave()
    {
        SaveData data = new SaveData();
        data.saveVersion = CURRENT_SAVE_VERSION;
        data.gameState = PlayerPrefs.GetInt("state", 1);

        string timeStr = PlayerPrefs.GetString("time", "");
        if (!long.TryParse(timeStr, out data.timestamp))
        {
            data.timestamp = DateTime.UtcNow.Ticks;
        }

        // Player Data: strictly preserve existing saved money!
        data.playerData.money = PlayerPrefs.GetInt("money", 0);
        data.playerData.totalLand = PlayerPrefs.GetInt("land", 3);
        data.playerData.totalWorker = PlayerPrefs.GetInt("worker", 1);
        data.playerData.toolLevel = PlayerPrefs.GetInt("tool", 1);

        // Lands
        for (int i = 0; i < data.playerData.totalLand; i++)
        {
            FarmLandSaveData landSave = new FarmLandSaveData();
            landSave.landIndex = i;
            int flag = PlayerPrefs.GetInt($"land_{i}_flag", 0);
            if (flag > 0)
            {
                landSave.isOccupied = true;
                landSave.seedId = PlayerPrefs.GetInt($"land_{i}_seed", -1);
                landSave.liveTime = PlayerPrefs.GetFloat($"land_{i}_liveTime", 0f);
                landSave.harvestTime = PlayerPrefs.GetFloat($"land_{i}_harvestTime", 0f);
                landSave.stateName = "LandGrowState";
            }
            else
            {
                landSave.isOccupied = false;
                landSave.stateName = "LandFreeState";
            }
            data.farmLandData.Add(landSave);
        }

        // Inventory
        int itemCount = PlayerPrefs.GetInt("item_count", 0);
        for (int i = 0; i < itemCount; i++)
        {
            int id = PlayerPrefs.GetInt($"item_{i}_id", -1);
            int amount = PlayerPrefs.GetInt($"item_{i}_amount", 0);
            if (id >= 0 && amount > 0)
            {
                data.playerData.inventory.Add(new InventoryItemSaveData(id, amount));
            }
        }

        // Clean up loose legacy keys
        ClearLegacyKeys();

        return data;
    }

    /// <summary>
    /// Cleans up legacy loose keys from PlayerPrefs.
    /// </summary>
    private static void ClearLegacyKeys()
    {
        PlayerPrefs.DeleteKey("money");
        PlayerPrefs.DeleteKey("land");
        PlayerPrefs.DeleteKey("worker");
        PlayerPrefs.DeleteKey("tool");
        PlayerPrefs.DeleteKey("time");
        PlayerPrefs.DeleteKey("state");

        int itemCount = PlayerPrefs.GetInt("item_count", 0);
        for (int i = 0; i < itemCount; i++)
        {
            PlayerPrefs.DeleteKey($"item_{i}_id");
            PlayerPrefs.DeleteKey($"item_{i}_amount");
        }
        PlayerPrefs.DeleteKey("item_count");

        for (int i = 0; i < 100; i++)
        {
            if (!PlayerPrefs.HasKey($"land_{i}_flag")) break;
            PlayerPrefs.DeleteKey($"land_{i}_flag");
            PlayerPrefs.DeleteKey($"land_{i}_seed");
            PlayerPrefs.DeleteKey($"land_{i}_liveTime");
            PlayerPrefs.DeleteKey($"land_{i}_harvestTime");
        }
    }

    /// <summary>
    /// Resets all saved data completely.
    /// </summary>
    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.DeleteKey(NEW_GAME_FLAG_KEY);
        PlayerPrefs.SetInt(LEGACY_INIT_KEY, 0);
        ClearLegacyKeys();
        PlayerPrefs.Save();

        try
        {
            string path = GetSaveFilePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveSystem] Failed to delete save file: {ex.Message}");
        }

        Debug.Log("[SaveSystem] All save data has been successfully cleared.");
    }
}
