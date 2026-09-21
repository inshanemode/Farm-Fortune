using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player milestone goals, provides HUD descriptions, and awards milestone bonuses.
/// </summary>
public class GoalManager : MonoBehaviour
{
    private static GoalManager instance;
    public static GoalManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GoalManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GoalManager");
                    instance = go.AddComponent<GoalManager>();
                }
            }
            return instance;
        }
    }

    [Header("Main Money Goal")]
    public int mainMoneyGoal = Global.MONEY_GOAL;

    public int currentMilestoneIndex { get; private set; } = 0;
    public int harvestedCropsCount { get; private set; } = 0;
    public int soldProductsCount { get; private set; } = 0;
    public int purchasedLandsCount { get; private set; } = 0;
    public int hiredWorkersCount { get; private set; } = 0;

    [System.Serializable]
    public class Milestone
    {
        public string title;
        public string targetText;
        public int targetCount;
        public int rewardCoins;

        public Milestone(string title, string targetText, int targetCount, int rewardCoins)
        {
            this.title = title;
            this.targetText = targetText;
            this.targetCount = targetCount;
            this.rewardCoins = rewardCoins;
        }
    }

    private readonly List<Milestone> milestones = new List<Milestone>
    {
        new Milestone("Thu hoạch 3 cây trồng", "Thu hoạch cây", 3, 1000),
        new Milestone("Bán 100 sản phẩm", "Bán nông sản", 100, 5000),
        new Milestone("Mua thêm 1 mảnh đất", "Mở rộng đất", 1, 10000),
        new Milestone("Thuê worker đầu tiên", "Thuê nhân công", 1, 15000),
        new Milestone("Hoàn thành đơn hàng đầu tiên", "Giao đơn hàng", 1, 20000),
        new Milestone("Trở thành Đại Phú Gia", "Tích lũy tài sản", Global.MONEY_GOAL, 100000)
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        EventManager.StartListening<OnHarvestFarmEntity>(OnHarvestEvent);
        EventManager.StartListening<OnPlayerSellItem>(OnSellEvent);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnHarvestFarmEntity>(OnHarvestEvent);
        EventManager.StopListening<OnPlayerSellItem>(OnSellEvent);
    }

    private void OnHarvestEvent(EventParam param)
    {
        harvestedCropsCount++;
        CheckMilestoneProgress();
    }

    private void OnSellEvent(EventParam param)
    {
        OnPlayerSellItem sellEvent = param as OnPlayerSellItem;
        if (sellEvent != null)
        {
            soldProductsCount += sellEvent.amount;
            CheckMilestoneProgress();
        }
    }

    public void OnLandPurchased()
    {
        purchasedLandsCount++;
        CheckMilestoneProgress();
    }

    public void OnWorkerHired()
    {
        hiredWorkersCount++;
        CheckMilestoneProgress();
    }

    public void OnOrderCompleted()
    {
        CheckMilestoneProgress();
    }

    public void CheckMilestoneProgress()
    {
        if (currentMilestoneIndex >= milestones.Count) return;

        Player player = Player.main;
        if (player == null) return;

        int current = GetCurrentProgressAmount(player);
        Milestone ms = milestones[currentMilestoneIndex];

        if (current >= ms.targetCount)
        {
            // Milestone achieved!
            player.AddMoney(ms.rewardCoins);
            FeedbackManager.Instance?.PlayCoinSound();
            FeedbackManager.Instance?.SpawnCoinText(ms.rewardCoins);

            NotificationUI.Show(
                "Mục Tiêu Hoàn Thành! 🎉",
                $"{ms.title}\nThưởng: +{ms.rewardCoins:N0} xu!",
                4.0f
            );

            currentMilestoneIndex++;
            Debug.Log($"[GoalManager] Milestone {currentMilestoneIndex - 1} completed! Advanced to milestone {currentMilestoneIndex}.");

            // Check if subsequent milestone is already satisfied
            if (currentMilestoneIndex < milestones.Count)
            {
                CheckMilestoneProgress();
            }
        }
    }

    public int GetCurrentProgressAmount(Player player)
    {
        switch (currentMilestoneIndex)
        {
            case 0: return harvestedCropsCount;
            case 1: return soldProductsCount;
            case 2: return purchasedLandsCount;
            case 3: return hiredWorkersCount;
            case 4: return OrderManager.Instance != null ? OrderManager.Instance.completedOrdersCount : 0;
            case 5: return player != null ? player.totalMoney : 0;
            default: return 0;
        }
    }

    public float GetGoalProgress01(Player player)
    {
        if (currentMilestoneIndex >= milestones.Count) return 1.0f;
        Milestone ms = milestones[currentMilestoneIndex];
        if (ms.targetCount <= 0) return 1.0f;

        int cur = GetCurrentProgressAmount(player);
        return Mathf.Clamp01((float)cur / ms.targetCount);
    }

    public string GetGoalDescription(Player player)
    {
        if (currentMilestoneIndex >= milestones.Count)
        {
            return "<color=#F4B942>🏆 Đã Đạt Đỉnh Cao Tycoon Master!</color>";
        }

        Milestone ms = milestones[currentMilestoneIndex];
        int cur = GetCurrentProgressAmount(player);
        return $"{ms.title}: {Math.Min(cur, ms.targetCount):N0}/{ms.targetCount:N0}";
    }

    public GoalSaveData GetSaveData()
    {
        return new GoalSaveData
        {
            currentMilestoneIndex = currentMilestoneIndex,
            harvestedCropsCount = harvestedCropsCount,
            soldProductsCount = soldProductsCount,
            purchasedLandsCount = purchasedLandsCount,
            hiredWorkersCount = hiredWorkersCount
        };
    }

    public void LoadSaveData(GoalSaveData data)
    {
        if (data == null) return;
        currentMilestoneIndex = data.currentMilestoneIndex;
        harvestedCropsCount = data.harvestedCropsCount;
        soldProductsCount = data.soldProductsCount;
        purchasedLandsCount = data.purchasedLandsCount;
        hiredWorkersCount = data.hiredWorkersCount;
    }
}
