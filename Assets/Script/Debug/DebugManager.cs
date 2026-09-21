using UnityEngine;

/// <summary>
/// Runtime debug controller providing immediate in-game access to all test functions.
/// Toggle panel via F1 or the top-right DEBUG button.
/// </summary>
public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    [Header("Settings")]
    public bool showDebugPanel = false;
    public KeyCode toggleKey = KeyCode.F1;

    private Rect windowRect = new Rect(20, 20, 320, 420);
    private Vector2 scrollPosition;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject debugObj = new GameObject("[DebugManager]");
            debugObj.AddComponent<DebugManager>();
            DontDestroyOnLoad(debugObj);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            showDebugPanel = !showDebugPanel;
        }
    }

    private void OnGUI()
    {
        // Mini toggle button on top-right screen
        int buttonWidth = 80;
        int buttonHeight = 28;
        // Keep the developer toggle away from the player-facing settings button.
        // F1 / BackQuote still provide a fast keyboard shortcut for testing.
        Rect toggleBtnRect = new Rect(Screen.width - buttonWidth - 10, 60, buttonWidth, buttonHeight);

        GUI.color = showDebugPanel ? Color.yellow : Color.white;
        if (GUI.Button(toggleBtnRect, showDebugPanel ? "CLOSE" : "DEBUG"))
        {
            showDebugPanel = !showDebugPanel;
        }
        GUI.color = Color.white;

        if (!showDebugPanel) return;

        windowRect = GUI.Window(9999, windowRect, DrawDebugWindow, "Farm Fortune — Debug Tools");
    }

    private void DrawDebugWindow(int windowId)
    {
        GUI.DragWindow(new Rect(0, 0, windowRect.width - 25, 20));

        if (GUI.Button(new Rect(windowRect.width - 25, 2, 22, 18), "X"))
        {
            showDebugPanel = false;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(5);
        if (Player.main != null)
        {
            GUILayout.Label($"Money: {Player.main.totalMoney:N0} | Land: {Player.main.totalLand} | Workers: {Player.main.totalWorker}");
            GUILayout.Label($"Tool Lv: {Player.main.toolLevel} | Inv Types: {(Player.main.inventory != null ? Player.main.inventory.items.Count : 0)}");
        }
        else
        {
            GUILayout.Label("(Waiting for Player in Scene...)");
        }

        GUILayout.Space(10);

        // 1. Reset Save
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("1. Reset Save", GUILayout.Height(30)))
        {
            SaveSystem.ClearSave();
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.LoadGame();
            }
            Debug.Log("[DebugPanel] Save data has been reset.");
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        // 2. Add Money
        if (GUILayout.Button("2. Thêm tiền (+100,000 coins)", GUILayout.Height(30)))
        {
            if (Player.main != null)
            {
                Player.main.AddMoney(100000);
            }
        }

        GUILayout.Space(5);

        // 3. Unlock All Crops
        if (GUILayout.Button("3. Mở khóa toàn bộ cây & hạt", GUILayout.Height(30)))
        {
            UnlockAllCrops();
        }

        GUILayout.Space(5);

        // 4. Add Worker
        if (GUILayout.Button("4. Thêm worker (+1)", GUILayout.Height(30)))
        {
            if (Player.main != null)
            {
                Player.main.AddWorker(1);
            }
        }

        GUILayout.Space(5);

        // 5. Fast Mature Crops
        if (GUILayout.Button("5. Hoàn thành nhanh cây trồng", GUILayout.Height(30)))
        {
            FastMatureCrops();
        }

        GUILayout.Space(5);

        // 6. Create Sample Order
        if (GUILayout.Button("6. Tạo đơn hàng mẫu", GUILayout.Height(30)))
        {
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.CreateSampleOrder();
            }
        }

        GUILayout.Space(5);

        // 7. Clear All Inventory
        GUI.backgroundColor = new Color(1f, 0.7f, 0.5f);
        if (GUILayout.Button("7. Xóa toàn bộ inventory", GUILayout.Height(30)))
        {
            if (Player.main != null && Player.main.inventory != null)
            {
                Player.main.inventory.ClearItems();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        // 8. Order Board Toggle
        if (GUILayout.Button("8. Bật/Tắt Bảng Đơn Hàng", GUILayout.Height(30)))
        {
            OrderUI.Instance?.Toggle();
        }

        GUILayout.Space(5);

        // 9. Add Reputation
        if (GUILayout.Button("9. Thêm +50 Danh Tiếng", GUILayout.Height(30)))
        {
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.farmReputation += 50;
                NotificationUI.Show("Debug: Danh Tiếng +50", $"Danh tiếng hiện tại: {OrderManager.Instance.farmReputation} (Cấp {OrderManager.Instance.ReputationLevel})");
            }
        }

        GUILayout.Space(5);

        // 10. Advance Milestone Goal
        if (GUILayout.Button("10. Hoàn thành nhanh mục tiêu hiện tại", GUILayout.Height(30)))
        {
            if (GoalManager.Instance != null)
            {
                GoalManager.Instance.CheckMilestoneProgress();
            }
        }

        GUILayout.EndScrollView();
    }

    private void UnlockAllCrops()
    {
        if (ItemManager.Instance == null || Player.main == null || Player.main.inventory == null) return;

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
        Debug.Log($"[DebugPanel] Unlocked all crops and granted 50 seeds for {count} types.");
    }

    private void FastMatureCrops()
    {
        if (LandManager.Instance == null) return;
        int matured = 0;
        foreach (Land land in LandManager.Instance.availableLands)
        {
            if (land != null && !land.IsLandEmpty() && land.entityData != null)
            {
                land.liveTime = land.entityData.timeToHarvest;
                land.farmEntity?.UpdateStage(1f);
                matured++;
            }
        }
        Debug.Log($"[DebugPanel] Fast matured {matured} crops.");
    }
}
