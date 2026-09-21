using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Root Save Data structure with versioning support.
/// </summary>
[Serializable]
public class SaveData
{
    public int saveVersion = 2;
    public long timestamp;
    public int gameState;

    public PlayerSaveData playerData = new PlayerSaveData();
    public List<WorkerSaveData> workerData = new List<WorkerSaveData>();
    public List<FarmLandSaveData> farmLandData = new List<FarmLandSaveData>();
    public OrderSaveData orderData = new OrderSaveData();
    public ProductionSaveData productionData = new ProductionSaveData();
    public GoalSaveData goalData = new GoalSaveData();
    public TutorialSaveData tutorialData = new TutorialSaveData();
}

/// <summary>
/// Player-specific save data including currency, tools, land counts, and inventory.
/// </summary>
[Serializable]
public class PlayerSaveData
{
    public int money = 200000;
    public int totalLand = 3;
    public int totalWorker = 1;
    public int toolLevel = 1;
    public int workerUpgradeLevel = 1;
    public float workerSpeedMultiplier = 1.0f;
    public float workerEfficiencyMultiplier = 1.0f;
    public List<int> unlockedCrops = new List<int>();
    public List<InventoryItemSaveData> inventory = new List<InventoryItemSaveData>();
}

/// <summary>
/// Serializable representation of an inventory item and count.
/// </summary>
[Serializable]
public class InventoryItemSaveData
{
    public int itemId;
    public int amount;

    public InventoryItemSaveData() { }

    public InventoryItemSaveData(int itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }
}

/// <summary>
/// Worker-specific save data (task, selected seed, position, assigned land, upgrades, state).
/// </summary>
[Serializable]
public class WorkerSaveData
{
    public int workerIndex;
    public int currentTask; // Cast to WorkerManager.WorkerTask
    public int selectedSeedId = -1; // -1 means no seed selected
    public float posX;
    public float posY;
    public int assignedLandIndex = -1; // -1 means unassigned
    public float speed = 1f;
    public float workTime = 120f;
    public string stateName = "WorkerFreeState";
    public int upgradeLevel = 1;
}

/// <summary>
/// Farmland save data including plant status, growth progress, and lifecycle stage.
/// </summary>
[Serializable]
public class FarmLandSaveData
{
    public int landIndex;
    public bool isOccupied;
    public int seedId = -1;
    public float liveTime;
    public float harvestTime;
    public string stateName = "LandFreeState";
}

/// <summary>
/// Order/Quest save data for active, available, and completed orders.
/// </summary>
[Serializable]
public class OrderSaveData
{
    public int farmReputation = 0;
    public int completedOrdersCount = 0;
    public List<OrderEntrySaveData> activeOrders = new List<OrderEntrySaveData>();
    public List<OrderEntrySaveData> availableOrders = new List<OrderEntrySaveData>();
}

/// <summary>
/// Individual order entry supporting rich requirements and timer state.
/// </summary>
[Serializable]
public class OrderEntrySaveData
{
    public string orderId;
    public string customerName = "Khách Hàng";
    public string orderTitle;
    public string customerNote;
    public List<OrderRequirement> requirements = new List<OrderRequirement>();
    public int requiredItemId;
    public int requiredAmount;
    public int currentAmount;
    public int rewardMoney;
    public int rewardReputation = 10;
    public float totalDuration = 300f;
    public float timeRemaining = 300f;
    public int state; // Cast to OrderState
    public bool isCompleted;
}

/// <summary>
/// Milestone Goal save data.
/// </summary>
[Serializable]
public class GoalSaveData
{
    public int currentMilestoneIndex = 0;
    public int harvestedCropsCount = 0;
    public int soldProductsCount = 0;
    public int purchasedLandsCount = 0;
    public int hiredWorkersCount = 0;
}

/// <summary>
/// Tutorial progress save data.
/// </summary>
[Serializable]
public class TutorialSaveData
{
    public int currentStep = 0;
    public bool isCompleted = false;
}

/// <summary>
/// Factory and production buildings save data.
/// </summary>
[Serializable]
public class ProductionSaveData
{
    public List<FactorySaveData> factories = new List<FactorySaveData>();
}

/// <summary>
/// Individual factory state.
/// </summary>
[Serializable]
public class FactorySaveData
{
    public string factoryId;
    public string factoryName;
    public int level = 1;
    public bool isUnlocked;
    public bool isProducing;
    public int inputItemId = -1;
    public int outputItemId = -1;
    public float progress;
    public float cycleTime = 60f;
}
