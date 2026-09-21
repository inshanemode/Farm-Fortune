#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FarmFortuneDebugWindow : EditorWindow
{
    private Vector2 scrollPos;

    [MenuItem("FarmFortune/Debug Panel", false, 0)]
    public static void ShowWindow()
    {
        FarmFortuneDebugWindow window = GetWindow<FarmFortuneDebugWindow>("Farm Debug");
        window.minSize = new Vector2(320, 480);
        window.Show();
    }

    [MenuItem("FarmFortune/Reset Save Data", false, 1)]
    public static void MenuResetSave()
    {
        SaveSystem.ClearSave();
        EditorUtility.DisplayDialog("Farm Fortune", "Save data has been reset completely.", "OK");
    }

    [MenuItem("FarmFortune/Debug/Add Money (+100,000)", false, 20)]
    public static void MenuAddMoney()
    {
        ExecuteAddMoney();
    }

    [MenuItem("FarmFortune/Debug/Unlock All Crops", false, 21)]
    public static void MenuUnlockAllCrops()
    {
        ExecuteUnlockAllCrops();
    }

    [MenuItem("FarmFortune/Debug/Add Worker", false, 22)]
    public static void MenuAddWorker()
    {
        ExecuteAddWorker();
    }

    [MenuItem("FarmFortune/Debug/Fast Mature Crops", false, 23)]
    public static void MenuFastMatureCrops()
    {
        ExecuteFastMatureCrops();
    }

    [MenuItem("FarmFortune/Debug/Create Sample Order", false, 24)]
    public static void MenuCreateSampleOrder()
    {
        ExecuteCreateSampleOrder();
    }

    [MenuItem("FarmFortune/Debug/Clear All Inventory", false, 25)]
    public static void MenuClearInventory()
    {
        ExecuteClearInventory();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space(10);
        GUILayout.Label("FARM FORTUNE — SPRINT 0 DEBUG", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Status
        EditorGUILayout.LabelField("Status:", EditorStyles.boldLabel);
        bool hasSave = SaveSystem.HasSave();
        EditorGUILayout.LabelField("Has Save Data:", hasSave ? "YES" : "NO");
        EditorGUILayout.LabelField("Game Running:", Application.isPlaying ? "YES" : "NO");

        if (Application.isPlaying && Player.main != null)
        {
            EditorGUILayout.LabelField("Money:", $"{Player.main.totalMoney:N0} coins");
            EditorGUILayout.LabelField("Total Land:", $"{Player.main.totalLand}");
            EditorGUILayout.LabelField("Total Workers:", $"{Player.main.totalWorker}");
            EditorGUILayout.LabelField("Tool Level:", $"{Player.main.toolLevel}");
            if (Player.main.inventory != null)
            {
                EditorGUILayout.LabelField("Inventory Types:", $"{Player.main.inventory.items.Count}");
            }
            if (OrderManager.Instance != null)
            {
                EditorGUILayout.LabelField("Active Orders:", $"{OrderManager.Instance.ActiveOrders.Count}");
            }
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play Mode gives full access to live gameplay debugging. Off-play mode actions modify saved files directly.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Tools:", EditorStyles.boldLabel);

        // 1. Reset Save
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("1. Reset Save Data", GUILayout.Height(30)))
        {
            ExecuteResetSave();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(4);

        // 2. Add Money
        if (GUILayout.Button("2. Thêm tiền (+100,000 coins)", GUILayout.Height(28)))
        {
            ExecuteAddMoney();
        }

        EditorGUILayout.Space(4);

        // 3. Unlock All Crops
        if (GUILayout.Button("3. Mở khóa toàn bộ cây & hạt", GUILayout.Height(28)))
        {
            ExecuteUnlockAllCrops();
        }

        EditorGUILayout.Space(4);

        // 4. Add Worker
        if (GUILayout.Button("4. Thêm Worker (+1)", GUILayout.Height(28)))
        {
            ExecuteAddWorker();
        }

        EditorGUILayout.Space(4);

        // 5. Fast Mature Crops
        if (GUILayout.Button("5. Hoàn thành nhanh cây trồng", GUILayout.Height(28)))
        {
            ExecuteFastMatureCrops();
        }

        EditorGUILayout.Space(4);

        // 6. Create Sample Order
        if (GUILayout.Button("6. Tạo đơn hàng mẫu", GUILayout.Height(28)))
        {
            ExecuteCreateSampleOrder();
        }

        EditorGUILayout.Space(4);

        // 7. Clear All Inventory
        GUI.backgroundColor = new Color(1f, 0.7f, 0.5f);
        if (GUILayout.Button("7. Xóa toàn bộ inventory", GUILayout.Height(28)))
        {
            ExecuteClearInventory();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(15);
        if (GUILayout.Button("Refresh / Repaint", GUILayout.Height(24)))
        {
            Repaint();
        }

        EditorGUILayout.EndScrollView();
    }

    public static void ExecuteResetSave()
    {
        SaveSystem.ClearSave();
        if (Application.isPlaying && GameplayManager.Instance != null)
        {
            GameplayManager.Instance.LoadGame();
        }
        Debug.Log("[DebugTool] Save data reset completed.");
    }

    public static void ExecuteAddMoney(int amount = 100000)
    {
        if (Application.isPlaying && Player.main != null)
        {
            Player.main.AddMoney(amount);
            Debug.Log($"[DebugTool] Added {amount} coins. New total: {Player.main.totalMoney}.");
        }
        else
        {
            SaveData data = SaveSystem.Load() ?? new SaveData();
            data.playerData.money += amount;
            SaveSystem.Save(data);
            Debug.Log($"[DebugTool] Added {amount} coins to save file. New total: {data.playerData.money}.");
        }
    }

    public static void ExecuteUnlockAllCrops()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[DebugTool] Please run in Play Mode to add crops to live inventory.");
            return;
        }

        if (ItemManager.Instance == null || Player.main == null || Player.main.inventory == null)
        {
            Debug.LogWarning("[DebugTool] Gameplay or ItemManager not ready.");
            return;
        }

        int count = 0;
        foreach (Item item in ItemManager.Instance.itemList)
        {
            if (item != null)
            {
                if (!Player.main.unlockedCrops.Contains(item.id))
                {
                    Player.main.unlockedCrops.Add(item.id);
                }

                if (item is SeedItem)
                {
                    Player.main.inventory.AddItem(item, 50);
                    count++;
                }
            }
        }
        Debug.Log($"[DebugTool] Unlocked all crops and granted 50 units for {count} seed types.");
    }

    public static void ExecuteAddWorker()
    {
        if (Application.isPlaying && Player.main != null)
        {
            Player.main.AddWorker(1);
            Debug.Log($"[DebugTool] Added 1 worker. Total workers: {Player.main.totalWorker}.");
        }
        else
        {
            SaveData data = SaveSystem.Load() ?? new SaveData();
            data.playerData.totalWorker += 1;
            SaveSystem.Save(data);
            Debug.Log($"[DebugTool] Added 1 worker to save file. Total workers: {data.playerData.totalWorker}.");
        }
    }

    public static void ExecuteFastMatureCrops()
    {
        if (!Application.isPlaying || LandManager.Instance == null)
        {
            Debug.LogWarning("[DebugTool] Fast mature requires Play Mode with LandManager.");
            return;
        }

        int maturedCount = 0;
        foreach (Land land in LandManager.Instance.availableLands)
        {
            if (land != null && !land.IsLandEmpty() && land.entityData != null)
            {
                land.liveTime = land.entityData.timeToHarvest;
                land.farmEntity?.UpdateStage(1f);
                maturedCount++;
            }
        }
        Debug.Log($"[DebugTool] Fast matured {maturedCount} planted crops to ready-to-harvest stage.");
    }

    public static void ExecuteCreateSampleOrder()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[DebugTool] Sample orders require Play Mode.");
            return;
        }

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.CreateSampleOrder();
        }
        else
        {
            Debug.LogWarning("[DebugTool] OrderManager instance not found.");
        }
    }

    public static void ExecuteClearInventory()
    {
        if (Application.isPlaying && Player.main != null && Player.main.inventory != null)
        {
            Player.main.inventory.ClearItems();
            Debug.Log("[DebugTool] Inventory cleared.");
        }
        else
        {
            SaveData data = SaveSystem.Load();
            if (data != null)
            {
                data.playerData.inventory.Clear();
                SaveSystem.Save(data);
                Debug.Log("[DebugTool] Inventory cleared in save file.");
            }
        }
    }
}
#endif
