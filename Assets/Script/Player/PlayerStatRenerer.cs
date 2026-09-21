using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact, clean HUD renderer displaying player resources (Money, Worker, Land, Tool)
/// and a quick-access shortcut button for the Quest Board [Q].
/// </summary>
public class PlayerStatRenerer : MonoBehaviour
{
    [Header("Core Stats")]
    public TMP_Text moneyText;
    public TMP_Text workerText;
    public TMP_Text landText;
    public TMP_Text toolText;

    [Header("Unused Legacy Text References")]
    [HideInInspector] public TMP_Text goalText;
    [HideInInspector] public TMP_Text orderText;
    [HideInInspector] public TMP_Text nextUnlockText;
    [HideInInspector] public TMP_Text idleWorkerText;
    [HideInInspector] public TMP_Text harvestTimerText;
    [HideInInspector] public TMP_Text recentProfitText;

    private WorkerListRenderer workerListRenderer;
    private Image workerIconImage;
    private WorkerManager workerManager;
    private float periodicTimer = 0f;

    private void Start()
    {
        workerManager = Player.main != null ? Player.main.GetComponent<WorkerManager>() : FindObjectOfType<WorkerManager>();

        if (workerManager != null)
        {
            workerListRenderer = WorkerListRenderer.Create(transform.parent, workerManager);

            if (workerText != null && workerText.transform.parent != null)
            {
                Button workerButton = workerText.transform.parent.GetComponent<Button>();
                if (workerButton == null) workerButton = workerText.transform.parent.gameObject.AddComponent<Button>();
                workerButton.targetGraphic = workerText.transform.parent.GetComponent<Graphic>();
                workerButton.onClick.AddListener(workerListRenderer.Toggle);

                // Find worker icon to color code
                workerIconImage = workerText.transform.parent.GetComponentInChildren<Image>();
            }
        }

        // Apply FarmUITheme card background if image present
        Image bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            FarmUITheme.StyleCard(bgImage);
        }

        CreateQuestBoardHUDButton();

        if (Player.main != null)
        {
            UpdateDisplay(Player.main);
        }
    }

    private void CreateQuestBoardHUDButton()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject btnObj = new GameObject("QuestBoardHUDButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(16f, -12f);
        rt.sizeDelta = new Vector2(240f, 56f);

        Button btn = btnObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(btn, FarmUITheme.ButtonState.Purchasable);
        btn.onClick.AddListener(() => QuestBoardUI.Instance?.Toggle());

        GameObject txtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
        TMP_Text existing = GetComponentInChildren<TMP_Text>();
        if (existing != null) txt.font = existing.font;
        txt.text = "NHIỆM VỤ [Q]";
        txt.fontSize = 18;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = FarmUITheme.TextLight;
        txt.raycastTarget = false;
    }

    private void OnEnable()
    {
        EventManager.StartListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
    }

    private void OnPlayerStatUpdate(EventParam param)
    {
        OnPlayerStatUpdate eventParam = param as OnPlayerStatUpdate;
        if (eventParam != null && eventParam.player != null)
        {
            UpdateDisplay(eventParam.player);
        }
    }

    private void Update()
    {
        periodicTimer += Time.deltaTime;
        if (periodicTimer >= 0.5f)
        {
            periodicTimer = 0f;
            if (Player.main != null)
            {
                UpdateWorkerColor(Player.main);
            }
        }
    }

    private void UpdateDisplay(Player player)
    {
        if (player == null) return;

        if (moneyText != null)
        {
            moneyText.text = $"{player.totalMoney:N0} xu";
            moneyText.color = player.totalMoney < 0 ? FarmUITheme.RedWarning : FarmUITheme.InkBrown;
        }

        if (workerText != null)
        {
            workerText.text = $"{player.totalWorkingWorker}/{player.totalWorker}";
            UpdateWorkerColor(player);
        }

        if (landText != null)
        {
            landText.text = $"{player.totalLand}";
            landText.color = FarmUITheme.InkBrown;
        }

        if (toolText != null)
        {
            toolText.text = $"Lv.{player.toolLevel}";
            toolText.color = FarmUITheme.InkBrown;
        }
    }

    private void UpdateWorkerColor(Player player)
    {
        if (workerManager == null || workerManager.Workers == null) return;

        bool hasMissingSeedWorker = false;
        bool hasWorkingWorker = false;

        foreach (WorkerEntity worker in workerManager.Workers)
        {
            if (worker == null) continue;

            if (worker.CurrentTask == WorkerManager.WorkerTask.PlantOnly || worker.CurrentTask == WorkerManager.WorkerTask.PlantAndHarvest)
            {
                if (worker.SelectedSeed == null || player.inventory.GetAmount(worker.SelectedSeed) <= 0)
                {
                    hasMissingSeedWorker = true;
                    break;
                }
            }

            if (worker.CurrentStateName == "WorkerWorkState")
            {
                hasWorkingWorker = true;
            }
        }

        Color targetColor;
        if (hasMissingSeedWorker)
        {
            targetColor = FarmUITheme.RedWarning; // Stuck / Missing seed
        }
        else if (hasWorkingWorker || player.totalWorkingWorker > 0)
        {
            targetColor = FarmUITheme.GoldReward; // Working
        }
        else
        {
            targetColor = FarmUITheme.GreenActive; // Idle
        }

        if (workerIconImage != null)
        {
            workerIconImage.color = targetColor;
        }
        else
        {
            workerText.color = targetColor;
        }
    }
}
