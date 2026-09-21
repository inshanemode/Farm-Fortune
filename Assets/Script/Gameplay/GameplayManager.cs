using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    [Header("Game goal")]
    public int moneyGoal = 1000000;
    [Header("Level settings")]
    public List<ItemInfo> starterItems;
    public GameObject workerRestPlace;
    public int startWorker = 1;
    public int startLand = 3;
    public int startToolLevel = 1;
    public int startMoney = 200000;

    private float saveTime = 0;

    public GameState currentState;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure Order and Production managers are attached
        if (GetComponent<OrderManager>() == null) gameObject.AddComponent<OrderManager>();
        if (GetComponent<ProductionManager>() == null) gameObject.AddComponent<ProductionManager>();
    }

    private void Start()
    {
        LoadGame();
    }

    private void Update()
    {
        saveTime += Time.deltaTime;
        if (saveTime >= Global.DEFAULT_SAVE_TIME)
        {
            Debug.Log("[GameplayManager] Auto-saving game...");
            SaveGame();
            saveTime = 0;
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnEnable()
    {
        EventManager.StartListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
        SaveGame();
    }

    private void OnPlayerStatUpdate(EventParam param)
    {
        OnPlayerStatUpdate eventParam = param as OnPlayerStatUpdate;
        if (eventParam == null || !eventParam.player.Equals(Player.main)) return;
        if (eventParam.player.totalMoney >= moneyGoal && currentState != GameState.GAME_WIN_END)
        {
            SetState(GameState.GAME_WIN_END);
            SaveGame();
            return;
        }
    }

    public Vector2 GetRandomSpawn(bool random = true)
    {
        if (workerRestPlace == null) return Vector2.zero;
        Vector2 direction = UnityEngine.Random.insideUnitSphere.normalized;
        return (Vector2)workerRestPlace.transform.position + (random ? direction * 3.5f + direction * UnityEngine.Random.Range(0f, 1f) : Vector2.zero);
    }

    public void SaveGame()
    {
        if (Player.main == null) return;

        SaveData data = new SaveData();
        data.saveVersion = SaveSystem.CURRENT_SAVE_VERSION;
        data.gameState = (int)currentState;

        // 1. Player Data
        data.playerData.money = Player.main.totalMoney;
        data.playerData.totalLand = Player.main.landManager != null ? Player.main.landManager.availableLands.Count : Player.main.totalLand;
        data.playerData.totalWorker = Player.main.totalWorker;
        data.playerData.toolLevel = Player.main.toolLevel;
        data.playerData.workerUpgradeLevel = Player.main.workerUpgradeLevel;
        data.playerData.workerSpeedMultiplier = Player.main.workerSpeedMultiplier;
        data.playerData.workerEfficiencyMultiplier = Player.main.workerEfficiencyMultiplier;
        data.playerData.unlockedCrops = new List<int>(Player.main.unlockedCrops);

        // Inventory
        if (Player.main.inventory != null && Player.main.inventory.items != null)
        {
            foreach (KeyValuePair<Item, int> pair in Player.main.inventory.items)
            {
                if (pair.Key != null && pair.Value > 0)
                {
                    data.playerData.inventory.Add(new InventoryItemSaveData(pair.Key.id, pair.Value));
                }
            }
        }

        // 2. Farmlands
        if (Player.main.landManager != null)
        {
            for (int i = 0; i < Player.main.landManager.availableLands.Count; i++)
            {
                Land land = Player.main.landManager.availableLands[i];
                FarmLandSaveData landSave = new FarmLandSaveData();
                landSave.landIndex = i;
                landSave.stateName = land.CurrentStateName;

                Item seedItem = ItemManager.GetSeedFromPlant(land.entityData);
                if (!land.IsLandEmpty() && seedItem != null)
                {
                    landSave.isOccupied = true;
                    landSave.seedId = seedItem.id;
                    landSave.liveTime = land.liveTime;
                    landSave.harvestTime = land.harvestTime;
                }
                else
                {
                    landSave.isOccupied = false;
                    landSave.seedId = -1;
                }
                data.farmLandData.Add(landSave);
            }
        }

        // 3. Workers
        if (Player.main.workerManager != null)
        {
            IReadOnlyList<WorkerEntity> workers = Player.main.workerManager.Workers;
            for (int i = 0; i < workers.Count; i++)
            {
                WorkerEntity worker = workers[i];
                if (worker == null) continue;

                WorkerSaveData wData = new WorkerSaveData();
                wData.workerIndex = i;
                wData.currentTask = (int)worker.CurrentTask;
                wData.selectedSeedId = worker.SelectedSeed != null ? worker.SelectedSeed.id : -1;
                wData.posX = worker.transform.position.x;
                wData.posY = worker.transform.position.y;
                wData.assignedLandIndex = (worker.currentLand != null && Player.main.landManager != null)
                    ? Player.main.landManager.availableLands.IndexOf(worker.currentLand)
                    : -1;
                wData.speed = worker.speed;
                wData.workTime = worker.workTime;
                wData.stateName = worker.CurrentStateName;
                wData.upgradeLevel = Player.main.workerUpgradeLevel;

                data.workerData.Add(wData);
            }
        }

        // 4. Orders
        if (OrderManager.Instance != null)
        {
            data.orderData = OrderManager.Instance.GetSaveData();
        }

        // 5. Production
        if (ProductionManager.Instance != null)
        {
            data.productionData = ProductionManager.Instance.GetSaveData();
        }

        // 6. Goals
        if (GoalManager.Instance != null)
        {
            data.goalData = GoalManager.Instance.GetSaveData();
        }

        // 7. Tutorial
        if (TutorialManager.Instance != null)
        {
            data.tutorialData = TutorialManager.Instance.GetSaveData();
        }

        SaveSystem.Save(data);
    }

    public void LoadGame()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null)
        {
            Debug.Log("[GameplayManager] No save file found or new game requested. Starting NEW GAME...");
            SetState(GameState.NEW_GAME);
            return;
        }

        Debug.Log($"[GameplayManager] Loading save game (v{saveData.saveVersion})...");
        currentState = GameState.LOAD_GAME;

        // 1. Player Data - PRESERVE SAVED MONEY!
        Player.main.totalMoney = saveData.playerData.money;
        Player.main.totalLand = saveData.playerData.totalLand;
        Player.main.totalWorker = saveData.playerData.totalWorker;
        Player.main.toolLevel = saveData.playerData.toolLevel;
        Player.main.workerUpgradeLevel = saveData.playerData.workerUpgradeLevel;
        Player.main.workerSpeedMultiplier = saveData.playerData.workerSpeedMultiplier > 0 ? saveData.playerData.workerSpeedMultiplier : 1.0f;
        Player.main.workerEfficiencyMultiplier = saveData.playerData.workerEfficiencyMultiplier > 0 ? saveData.playerData.workerEfficiencyMultiplier : 1.0f;
        Player.main.unlockedCrops = saveData.playerData.unlockedCrops != null ? new List<int>(saveData.playerData.unlockedCrops) : new List<int>();
        Player.main.UpdateStat();

        // 2. Inventory - CLEAR FIRST TO PREVENT DUPLICATION!
        Player.main.inventory.ClearItems();
        if (saveData.playerData.inventory != null)
        {
            foreach (InventoryItemSaveData itemData in saveData.playerData.inventory)
            {
                if (itemData.itemId >= 0 && itemData.amount > 0)
                {
                    Player.main.inventory.AddItem(itemData.itemId, itemData.amount);
                }
            }
        }

        // 3. Time elapsed
        long timeDiff = DateTime.UtcNow.Ticks - saveData.timestamp;
        int secondsPassed = (int)Mathf.Abs(timeDiff / TimeSpan.TicksPerSecond > int.MaxValue ? int.MaxValue : timeDiff / TimeSpan.TicksPerSecond);

        // 4. Farmlands
        if (saveData.farmLandData != null && Player.main.landManager != null)
        {
            for (int i = 0; i < saveData.farmLandData.Count; i++)
            {
                FarmLandSaveData landSave = saveData.farmLandData[i];
                if (landSave.landIndex < 0 || landSave.landIndex >= Player.main.landManager.availableLands.Count) continue;
                Land targetLand = Player.main.landManager.availableLands[landSave.landIndex];

                if (!landSave.isOccupied || landSave.seedId < 0) continue;

                SeedItem seedItem = ItemManager.GetItem(landSave.seedId) as SeedItem;
                if (seedItem == null || seedItem.seedData == null) continue;

                targetLand.GrowPlant(seedItem.seedData);
                float totalLiveTime = landSave.liveTime + secondsPassed;
                targetLand.liveTime = totalLiveTime;
                targetLand.harvestTime = landSave.harvestTime;

                if (totalLiveTime - seedItem.seedData.timeToHarvest >= seedItem.seedData.timeToDecompose)
                {
                    targetLand.ChangeState("LandDecomposeState");
                }
                else if (landSave.stateName == "LandDecomposeState")
                {
                    targetLand.ChangeState("LandDecomposeState");
                }
                else if (totalLiveTime >= seedItem.seedData.timeToHarvest)
                {
                    targetLand.farmEntity?.UpdateStage(1f);
                }
                else
                {
                    targetLand.farmEntity?.UpdateStage(targetLand.GetProgress());
                }
            }
        }

        // 5. Workers
        if (saveData.workerData != null && Player.main.workerManager != null)
        {
            IReadOnlyList<WorkerEntity> workers = Player.main.workerManager.Workers;
            for (int i = 0; i < saveData.workerData.Count && i < workers.Count; i++)
            {
                WorkerSaveData wData = saveData.workerData[i];
                WorkerEntity worker = workers[i];
                if (worker == null) continue;

                Land assignedLand = null;
                if (wData.assignedLandIndex >= 0 && wData.assignedLandIndex < Player.main.landManager.availableLands.Count)
                {
                    assignedLand = Player.main.landManager.availableLands[wData.assignedLandIndex];
                }

                worker.RestoreFromSave(wData, assignedLand);
            }
            Player.main.ApplyWorkerUpgrades();
        }

        // 6. Orders
        if (OrderManager.Instance != null && saveData.orderData != null)
        {
            OrderManager.Instance.LoadOrderData(saveData.orderData);
        }

        // 7. Production
        if (ProductionManager.Instance != null && saveData.productionData != null)
        {
            ProductionManager.Instance.LoadProductionData(saveData.productionData);
        }

        // 8. Goals
        if (GoalManager.Instance != null && saveData.goalData != null)
        {
            GoalManager.Instance.LoadSaveData(saveData.goalData);
        }

        // 9. Tutorial
        if (TutorialManager.Instance != null && saveData.tutorialData != null)
        {
            TutorialManager.Instance.LoadSaveData(saveData.tutorialData);
        }
    }

    private void SetState(GameState state)
    {
        currentState = state;
        OnGameStateChange stateEvent = new OnGameStateChange(this, state);
        EventManager.TriggerEvent(stateEvent);
    }

#if UNITY_EDITOR
    [ContextMenu("Clear Data")]
    public void ClearData()
    {
        SaveSystem.ClearSave();
    }
#endif
}
