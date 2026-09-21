using System;
using UnityEngine;

public enum TutorialStep
{
    PlantSeed = 0,
    HarvestCrop = 1,
    SellCrop = 2,
    BuySeed = 3,
    BuyLand = 4,
    HireWorker = 5,
    Completed = 6
}

/// <summary>
/// Manages introductory player onboarding step-by-step.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance;
    public static TutorialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TutorialManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TutorialManager");
                    instance = go.AddComponent<TutorialManager>();
                }
            }
            return instance;
        }
    }

    public TutorialStep currentStep { get; private set; } = TutorialStep.PlantSeed;
    public bool isCompleted { get; private set; } = false;

    public event Action<TutorialStep> OnStepChanged;

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
        EventManager.StartListening<OnHarvestFarmEntity>(OnHarvestCrop);
        EventManager.StartListening<OnPlayerSellItem>(OnSellCrop);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnHarvestFarmEntity>(OnHarvestCrop);
        EventManager.StopListening<OnPlayerSellItem>(OnSellCrop);
    }

    public void AdvanceStep(TutorialStep completedStep)
    {
        if (isCompleted || currentStep != completedStep) return;

        FeedbackManager.Instance?.PlayPurchaseSound();

        int next = (int)currentStep + 1;
        if (next >= (int)TutorialStep.Completed)
        {
            currentStep = TutorialStep.Completed;
            isCompleted = true;
            NotificationUI.Show("🎉 Hướng Dẫn Hoàn Thành!", "Bạn đã sẵn sàng phát triển nông trại Farm Fortune thịnh vượng!", 5f);
        }
        else
        {
            currentStep = (TutorialStep)next;
            NotificationUI.Show("Bước Hoàn Thành!", $"Chuyển sang: {GetStepTitle(currentStep)}", 3f);
        }

        OnStepChanged?.Invoke(currentStep);
        Debug.Log($"[TutorialManager] Advanced to step: {currentStep}");
    }

    public void SkipTutorial()
    {
        currentStep = TutorialStep.Completed;
        isCompleted = true;
        OnStepChanged?.Invoke(currentStep);
        NotificationUI.Show("Đã Bỏ Qua Hướng Dẫn", "Bạn có thể tự do quản lý nông trại của mình!", 3f);
    }

    // Callbacks from gameplay events
    public void OnCropPlanted()
    {
        AdvanceStep(TutorialStep.PlantSeed);
    }

    private void OnHarvestCrop(EventParam param)
    {
        AdvanceStep(TutorialStep.HarvestCrop);
    }

    private void OnSellCrop(EventParam param)
    {
        AdvanceStep(TutorialStep.SellCrop);
    }

    public void OnSeedPurchased()
    {
        AdvanceStep(TutorialStep.BuySeed);
    }

    public void OnLandPurchased()
    {
        AdvanceStep(TutorialStep.BuyLand);
    }

    public void OnWorkerHired()
    {
        AdvanceStep(TutorialStep.HireWorker);
    }

    public string GetStepTitle(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.PlantSeed: return "Bước 1: Trồng cây";
            case TutorialStep.HarvestCrop: return "Bước 2: Thu hoạch";
            case TutorialStep.SellCrop: return "Bước 3: Bán hàng";
            case TutorialStep.BuySeed: return "Bước 4: Mua hạt giống";
            case TutorialStep.BuyLand: return "Bước 5: Mở rộng đất";
            case TutorialStep.HireWorker: return "Bước 6: Thuê công nhân";
            default: return "Hoàn thành!";
        }
    }

    public string GetStepDescription(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.PlantSeed:
                return "Kéo hạt giống từ túi đồ vào một ô đất trống để bắt đầu gieo trồng.";
            case TutorialStep.HarvestCrop:
                return "Chờ cây lớn chín vàng (hoặc có viền vàng) rồi nhấp vào để thu hoạch nông sản.";
            case TutorialStep.SellCrop:
                return "Mở túi đồ và nhấp vào nông sản vừa thu hoạch để bán kiếm tiền.";
            case TutorialStep.BuySeed:
                return "Vào Cửa hàng (Shop) và mua thêm hạt giống mới để tiếp tục canh tác.";
            case TutorialStep.BuyLand:
                return "Vào Cửa hàng mua thêm 1 ô đất canh tác để mở rộng diện tích nông trại.";
            case TutorialStep.HireWorker:
                return "Thuê một công nhân đầu tiên trong Cửa hàng để tự động hóa việc trồng và thu hoạch!";
            default:
                return "Tuyệt vời! Bạn đã nắm vững các bước cơ bản của nông trại!";
        }
    }

    public TutorialSaveData GetSaveData()
    {
        return new TutorialSaveData
        {
            currentStep = (int)currentStep,
            isCompleted = isCompleted
        };
    }

    public void LoadSaveData(TutorialSaveData data)
    {
        if (data == null) return;
        currentStep = (TutorialStep)data.currentStep;
        isCompleted = data.isCompleted;
        OnStepChanged?.Invoke(currentStep);
    }
}
