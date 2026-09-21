using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dedicated modal UI for Farm Goals, Town Orders, Crop Timers, Worker Status, and Progression.
/// Replaces the cramped top HUD bar with a cozy, well-proportioned pixel-art notice board.
/// </summary>
public class QuestBoardUI : MonoBehaviour
{
    private static QuestBoardUI instance;
    public static QuestBoardUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<QuestBoardUI>();
                if (instance == null)
                {
                    Canvas canvas = FindObjectOfType<Canvas>();
                    GameObject go = new GameObject("QuestBoardUI");
                    if (canvas != null) go.transform.SetParent(canvas.transform, false);
                    instance = go.AddComponent<QuestBoardUI>();
                }
            }
            return instance;
        }
    }

    private GameObject modalRoot;
    private TMP_FontAsset pixelFont;

    // Card 1: Milestone Goal
    private TMP_Text goalTitleText;
    private TMP_Text goalProgressText;
    private RectTransform goalProgressBarFill;
    private TMP_Text goalRewardText;

    // Card 2: Town Orders
    private TMP_Text orderStatusText;
    private Button openOrdersButton;

    // Card 3: Crop Timers
    private TMP_Text cropStatusText;

    // Card 4: Workers
    private TMP_Text workerStatusText;
    private Button openWorkersButton;

    // Card 5: Next Unlock
    private TMP_Text nextUnlockText;

    private Shop cachedShop;
    private Land[] cachedLands;
    private float refreshTimer = 0f;
    private float cacheTimer = 0f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        BuildUI();
    }

    private void Start()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(false);
        }
    }

    private void Update()
    {
        // Toggle hotkeys: Q opens/closes; Escape closes
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Toggle();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (modalRoot != null && modalRoot.activeSelf)
            {
                Hide();
            }
        }

        if (modalRoot != null && modalRoot.activeSelf)
        {
            float dt = Time.unscaledDeltaTime;
            refreshTimer += dt;
            cacheTimer += dt;

            if (cacheTimer >= 2.0f)
            {
                cacheTimer = 0f;
                cachedLands = FindObjectsOfType<Land>();
                if (cachedShop == null) cachedShop = FindObjectOfType<Shop>();
            }

            if (refreshTimer >= 0.25f)
            {
                refreshTimer = 0f;
                RefreshData();
            }
        }
    }

    public void Toggle()
    {
        if (modalRoot == null) BuildUI();

        bool willShow = !modalRoot.activeSelf;
        modalRoot.SetActive(willShow);

        if (willShow)
        {
            cachedLands = FindObjectsOfType<Land>();
            if (cachedShop == null) cachedShop = FindObjectOfType<Shop>();
            RefreshData();
            FeedbackManager.Instance?.PlayClickSound();
        }
    }

    public void Show()
    {
        if (modalRoot == null) BuildUI();
        modalRoot.SetActive(true);
        cachedLands = FindObjectsOfType<Land>();
        if (cachedShop == null) cachedShop = FindObjectOfType<Shop>();
        RefreshData();
        FeedbackManager.Instance?.PlayClickSound();
    }

    public void Hide()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(false);
            FeedbackManager.Instance?.PlayClickSound();
        }
    }

    public bool IsOpen()
    {
        return modalRoot != null && modalRoot.activeSelf;
    }

    private void BuildUI()
    {
        if (modalRoot != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();

        pixelFont = FindObjectOfType<TMP_Text>()?.font ?? TMP_Settings.defaultFontAsset;

        // 1. Overlay Dimmer
        modalRoot = new GameObject("QuestBoardModal", typeof(RectTransform), typeof(Image));
        modalRoot.transform.SetParent(canvas != null ? canvas.transform : transform, false);
        modalRoot.layer = canvas != null ? canvas.gameObject.layer : 5;

        RectTransform overlayRect = modalRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        modalRoot.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.06f, 0.65f);

        // Click on dimmer background closes modal
        Button dimmerBtn = modalRoot.AddComponent<Button>();
        dimmerBtn.transition = Selectable.Transition.None;
        dimmerBtn.onClick.AddListener(Hide);

        // 2. Centered Board Panel
        GameObject boardPanel = new GameObject("BoardPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        boardPanel.transform.SetParent(modalRoot.transform, false);

        RectTransform panelRect = boardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(720f, 680f);

        Image panelImg = boardPanel.GetComponent<Image>();
        FarmUITheme.StylePanel(panelImg);

        VerticalLayoutGroup layout = boardPanel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 20);
        layout.spacing = 12;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Prevent click passing through board panel to dimmer
        Button boardBlocker = boardPanel.AddComponent<Button>();
        boardBlocker.transition = Selectable.Transition.None;

        // 3. Header Row (Title + Close X)
        GameObject headerRow = new GameObject("HeaderRow", typeof(RectTransform), typeof(LayoutElement));
        headerRow.transform.SetParent(boardPanel.transform, false);
        headerRow.GetComponent<LayoutElement>().preferredHeight = 52f;

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(headerRow.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0f);
        titleRect.anchorMax = new Vector2(0.88f, 1f);
        titleRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) titleTxt.font = pixelFont;
        titleTxt.text = "BẢNG NHIỆM VỤ NÔNG TRẠI";
        titleTxt.fontSize = 28;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.MidlineLeft;
        titleTxt.color = FarmUITheme.InkBrown;
        titleTxt.raycastTarget = false;

        // Close X Button
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(headerRow.transform, false);
        RectTransform closeXRect = closeXObj.GetComponent<RectTransform>();
        closeXRect.anchorMin = new Vector2(0.9f, 0f);
        closeXRect.anchorMax = new Vector2(1f, 1f);
        closeXRect.sizeDelta = Vector2.zero;

        Button closeXBtn = closeXObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(closeXBtn, FarmUITheme.ButtonState.Warning);
        closeXBtn.onClick.AddListener(Hide);

        GameObject closeXTxtObj = new GameObject("XText", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeXTxtObj.transform.SetParent(closeXObj.transform, false);
        RectTransform xRect = closeXTxtObj.GetComponent<RectTransform>();
        xRect.anchorMin = Vector2.zero;
        xRect.anchorMax = Vector2.one;
        xRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI xTxt = closeXTxtObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) xTxt.font = pixelFont;
        xTxt.text = "X";
        xTxt.fontSize = 22;
        xTxt.fontStyle = FontStyles.Bold;
        xTxt.alignment = TextAlignmentOptions.Center;
        xTxt.color = FarmUITheme.TextLight;
        xTxt.raycastTarget = false;

        // 4. Card 1: Milestone Goal
        CreateMilestoneCard(boardPanel.transform);

        // 5. Card 2: Town Orders
        CreateOrdersCard(boardPanel.transform);

        // 6. Card 3: Crop Timers & Harvest
        CreateCropsCard(boardPanel.transform);

        // 7. Card 4: Worker Management
        CreateWorkersCard(boardPanel.transform);

        // 8. Card 5: Next Unlock
        CreateUnlockCard(boardPanel.transform);

        // 9. Footer Close Button
        GameObject footerRow = new GameObject("FooterRow", typeof(RectTransform), typeof(LayoutElement));
        footerRow.transform.SetParent(boardPanel.transform, false);
        footerRow.GetComponent<LayoutElement>().preferredHeight = 48f;

        GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(footerRow.transform, false);
        RectTransform cRect = closeBtnObj.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.25f, 0f);
        cRect.anchorMax = new Vector2(0.75f, 1f);
        cRect.sizeDelta = Vector2.zero;

        Button closeBtn = closeBtnObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(closeBtn, FarmUITheme.ButtonState.Warning);
        closeBtn.onClick.AddListener(Hide);

        GameObject cbTxtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        cbTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform cbRect = cbTxtObj.GetComponent<RectTransform>();
        cbRect.anchorMin = Vector2.zero;
        cbRect.anchorMax = Vector2.one;
        cbRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI cbTxt = cbTxtObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) cbTxt.font = pixelFont;
        cbTxt.text = "ĐÓNG (Q / ESC)";
        cbTxt.fontSize = 18;
        cbTxt.fontStyle = FontStyles.Bold;
        cbTxt.alignment = TextAlignmentOptions.Center;
        cbTxt.color = FarmUITheme.TextLight;
        cbTxt.raycastTarget = false;
    }

    private void CreateMilestoneCard(Transform parent)
    {
        GameObject card = CreateCardObject(parent, "MilestoneCard", 132f);

        // Card Header
        CreateCardHeader(card.transform, "MỤC TIÊU NÔNG TRẠI (CHÍNH)");

        // Goal Description
        goalTitleText = CreateCardText(card.transform, "GoalTitle", "Đang tải mục tiêu...", 17, FontStyles.Bold, FarmUITheme.InkBrown);

        // Progress Bar Container
        GameObject barBg = new GameObject("ProgressBarBg", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(card.transform, false);
        RectTransform barBgRect = barBg.GetComponent<RectTransform>();
        barBgRect.sizeDelta = new Vector2(0, 26f);
        barBg.GetComponent<Image>().color = new Color(0.26f, 0.16f, 0.11f, 0.9f);
        FarmUITheme.AddOutline(barBg.GetComponent<Image>(), FarmUITheme.WoodBrown, new Vector2(1f, -1f));

        // Fill bar
        GameObject barFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        goalProgressBarFill = barFill.GetComponent<RectTransform>();
        goalProgressBarFill.anchorMin = Vector2.zero;
        goalProgressBarFill.anchorMax = new Vector2(0.5f, 1f);
        goalProgressBarFill.sizeDelta = Vector2.zero;
        barFill.GetComponent<Image>().color = FarmUITheme.GoldReward;

        // Progress Text
        GameObject pTxtObj = new GameObject("ProgressText", typeof(RectTransform), typeof(TextMeshProUGUI));
        pTxtObj.transform.SetParent(barBg.transform, false);
        RectTransform ptRect = pTxtObj.GetComponent<RectTransform>();
        ptRect.anchorMin = Vector2.zero;
        ptRect.anchorMax = Vector2.one;
        ptRect.sizeDelta = Vector2.zero;

        goalProgressText = pTxtObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) goalProgressText.font = pixelFont;
        goalProgressText.text = "0/0 (0%)";
        goalProgressText.fontSize = 15;
        goalProgressText.fontStyle = FontStyles.Bold;
        goalProgressText.alignment = TextAlignmentOptions.Center;
        goalProgressText.color = FarmUITheme.InkBrown;

        // Reward line
        goalRewardText = CreateCardText(card.transform, "GoalReward", "Thuong: Dang tinh...", 15, FontStyles.Normal, FarmUITheme.WoodBrown);
    }

    private void CreateOrdersCard(Transform parent)
    {
        GameObject card = CreateCardObject(parent, "OrdersCard", 102f);

        CreateCardHeader(card.transform, "ĐƠN HÀNG THỊ TRẤN");

        GameObject row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(card.transform, false);
        HorizontalLayoutGroup rLayout = row.GetComponent<HorizontalLayoutGroup>();
        rLayout.spacing = 10;
        rLayout.childControlWidth = true;
        rLayout.childControlHeight = true;
        rLayout.childForceExpandHeight = true;

        orderStatusText = CreateCardText(row.transform, "OrderStatus", "Đang tải đơn hàng...", 16, FontStyles.Normal, FarmUITheme.InkBrown);
        orderStatusText.alignment = TextAlignmentOptions.MidlineLeft;

        // Button to open OrderUI
        GameObject btnObj = new GameObject("OpenOrdersBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        btnObj.transform.SetParent(row.transform, false);
        btnObj.GetComponent<LayoutElement>().preferredWidth = 210f;
        btnObj.GetComponent<LayoutElement>().preferredHeight = 44f;

        openOrdersButton = btnObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(openOrdersButton, FarmUITheme.ButtonState.Purchasable);
        openOrdersButton.onClick.AddListener(() =>
        {
            Hide();
            OrderUI.Instance?.Show();
        });

        GameObject btnTxtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        RectTransform btRect = btnTxtObj.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI bTxt = btnTxtObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) bTxt.font = pixelFont;
        bTxt.text = "MỞ BẢNG ĐƠN";
        bTxt.fontSize = 16;
        bTxt.fontStyle = FontStyles.Bold;
        bTxt.alignment = TextAlignmentOptions.Center;
        bTxt.color = FarmUITheme.TextLight;
        bTxt.raycastTarget = false;
    }

    private void CreateCropsCard(Transform parent)
    {
        GameObject card = CreateCardObject(parent, "CropsCard", 82f);

        CreateCardHeader(card.transform, "TÌNH TRẠNG MÙA VỤ & CÂY TRỒNG");
        cropStatusText = CreateCardText(card.transform, "CropStatus", "Đang kiểm tra cây trồng...", 16, FontStyles.Normal, FarmUITheme.InkBrown);
    }

    private void CreateWorkersCard(Transform parent)
    {
        GameObject card = CreateCardObject(parent, "WorkersCard", 102f);

        CreateCardHeader(card.transform, "NHÂN LỰC NÔNG TRẠI (WORKERS)");

        GameObject row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(card.transform, false);
        HorizontalLayoutGroup rLayout = row.GetComponent<HorizontalLayoutGroup>();
        rLayout.spacing = 10;
        rLayout.childControlWidth = true;
        rLayout.childControlHeight = true;
        rLayout.childForceExpandHeight = true;

        workerStatusText = CreateCardText(row.transform, "WorkerStatus", "Worker: Đang tải...", 16, FontStyles.Normal, FarmUITheme.InkBrown);
        workerStatusText.alignment = TextAlignmentOptions.MidlineLeft;

        // Button to open Worker list
        GameObject btnObj = new GameObject("OpenWorkersBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        btnObj.transform.SetParent(row.transform, false);
        btnObj.GetComponent<LayoutElement>().preferredWidth = 210f;
        btnObj.GetComponent<LayoutElement>().preferredHeight = 44f;

        openWorkersButton = btnObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(openWorkersButton, FarmUITheme.ButtonState.Purchasable);
        openWorkersButton.onClick.AddListener(() =>
        {
            Hide();
            if (WorkerListRenderer.Instance != null)
            {
                WorkerListRenderer.Instance.Toggle();
            }
            else
            {
                WorkerListRenderer workerUI = WorkerListRenderer.Create(
                    FindObjectOfType<Canvas>()?.transform,
                    Player.main != null ? Player.main.workerManager : null);
                workerUI?.Toggle();
            }
        });

        GameObject btnTxtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        RectTransform btRect = btnTxtObj.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI bTxt = btnTxtObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) bTxt.font = pixelFont;
        bTxt.text = "MỞ QUẢN LÝ WORKER";
        bTxt.fontSize = 16;
        bTxt.fontStyle = FontStyles.Bold;
        bTxt.alignment = TextAlignmentOptions.Center;
        bTxt.color = FarmUITheme.TextLight;
        bTxt.raycastTarget = false;
    }

    private void CreateUnlockCard(Transform parent)
    {
        GameObject card = CreateCardObject(parent, "UnlockCard", 82f);

        CreateCardHeader(card.transform, "MỞ KHÓA TIẾP THEO TẠI CỬA HÀNG");
        nextUnlockText = CreateCardText(card.transform, "NextUnlock", "Đang kiểm tra cửa hàng...", 16, FontStyles.Normal, FarmUITheme.InkBrown);
    }

    private GameObject CreateCardObject(Transform parent, string name, float height)
    {
        GameObject card = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        card.transform.SetParent(parent, false);

        card.GetComponent<LayoutElement>().preferredHeight = height;

        Image cardImg = card.GetComponent<Image>();
        FarmUITheme.StyleCard(cardImg);

        VerticalLayoutGroup vlg = card.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 6, 6);
        vlg.spacing = 3;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        return card;
    }

    private void CreateCardHeader(Transform parent, string headerText)
    {
        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(parent, false);
        TextMeshProUGUI txt = headerObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) txt.font = pixelFont;
        txt.text = headerText;
        txt.fontSize = 16;
        txt.fontStyle = FontStyles.Bold;
        txt.color = FarmUITheme.WoodBrown;
    }

    private TextMeshProUGUI CreateCardText(Transform parent, string name, string defaultText, float fontSize, FontStyles style, Color color)
    {
        GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(parent, false);
        TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
        if (pixelFont != null) txt.font = pixelFont;
        txt.text = defaultText;
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.color = color;
        txt.raycastTarget = false;
        return txt;
    }

    private void RefreshData()
    {
        Player player = Player.main;
        if (player == null) return;

        // 1. Milestone Goal
        if (GoalManager.Instance != null)
        {
            string desc = GoalManager.Instance.GetGoalDescription(player);
            if (goalTitleText != null) goalTitleText.text = desc;

            float prog = GoalManager.Instance.GetGoalProgress01(player);
            if (goalProgressBarFill != null)
            {
                goalProgressBarFill.anchorMax = new Vector2(prog, 1f);
            }

            int cur = GoalManager.Instance.GetCurrentProgressAmount(player);
            int target = 1;
            if (GoalManager.Instance.currentMilestoneIndex < 6)
            {
                // Access target amount
                target = cur > 0 ? (int)(cur / Mathf.Max(0.001f, prog)) : 1;
            }
            if (goalProgressText != null)
            {
                goalProgressText.text = $"{cur:N0} ({(prog * 100f):F0}%)";
            }

            if (goalRewardText != null)
            {
                goalRewardText.text = prog >= 1f
                    ? "<color=#6FAF45>[OK] Đã hoàn thành mục tiêu này!</color>"
                    : "Tiếp tục thực hiện để nhận thưởng xu & nâng cấp trang trại";
            }
        }

        // 2. Town Orders
        if (OrderManager.Instance != null && orderStatusText != null)
        {
            int active = OrderManager.Instance.ActiveOrders.Count;
            int avail = OrderManager.Instance.AvailableOrders.Count;
            orderStatusText.text = $"- Đang thực hiện: <b>{active}</b> đơn\n- Đơn hàng mới có sẵn: <b>{avail}</b> đơn";
        }

        // 3. Crops Status
        if (cropStatusText != null && cachedLands != null)
        {
            bool hasReady = false;
            float minRemain = float.MaxValue;
            int plantedCount = 0;

            foreach (Land land in cachedLands)
            {
                if (land == null) continue;
                if (land.ReadyToHarvest())
                {
                    hasReady = true;
                    break;
                }
                if (!land.IsLandEmpty() && land.entityData != null && !land.IsLandDecompose())
                {
                    plantedCount++;
                    float remain = land.entityData.timeToHarvest - land.liveTime;
                    if (remain > 0 && remain < minRemain)
                    {
                        minRemain = remain;
                    }
                }
            }

            if (hasReady)
            {
                cropStatusText.text = "<color=#F4B942>CÓ CÂY ĐÃ CHÍN! Hãy tiến hành thu hoạch ngay.</color>";
            }
            else if (plantedCount > 0 && minRemain < float.MaxValue)
            {
                int m = Mathf.FloorToInt(minRemain / 60f);
                int s = Mathf.FloorToInt(minRemain % 60f);
                cropStatusText.text = $"THỜI GIAN THU HOẠCH SỚM NHẤT: <b>{m:00}:{s:00}</b> ({plantedCount} luống đang lớn)";
            }
            else
            {
                cropStatusText.text = "<color=#8B8178>Đất đang trống. Hãy mua hạt giống và gieo trồng!</color>";
            }
        }

        // 4. Workers Status
        if (workerStatusText != null)
        {
            int idle = Mathf.Max(0, player.totalWorker - player.totalWorkingWorker);
            workerStatusText.text = $"Đang làm: <b>{player.totalWorkingWorker}</b> | Đang rảnh: <b>{idle}</b> (Tổng: {player.totalWorker})";
        }

        // 5. Next Unlock
        if (nextUnlockText != null && cachedShop != null && cachedShop.buyableItems != null)
        {
            ShopStock cheapestLocked = null;
            int lowestCost = int.MaxValue;

            foreach (var stock in cachedShop.buyableItems)
            {
                if (stock != null && stock.unlockCost > 0 && !stock.IsUnlocked(player) && stock.unlockCost < lowestCost)
                {
                    lowestCost = stock.unlockCost;
                    cheapestLocked = stock;
                }
            }

            if (cheapestLocked != null)
            {
                int diff = lowestCost - player.totalMoney;
                if (diff <= 0)
                {
                nextUnlockText.text = $"<color=#6FAF45>[OK] Đủ tiền mở khóa <b>{cheapestLocked.GetStockName()}</b> ({lowestCost:N0} xu) tại Cửa Hàng!</color>";
                }
                else
                {
                    nextUnlockText.text = $"- Kế tiếp: <b>{cheapestLocked.GetStockName()}</b> (Cần {lowestCost:N0} xu, còn thiếu {diff:N0} xu)";
                }
            }
            else
            {
                nextUnlockText.text = "<color=#6FAF45>Bạn đã mở khóa tất cả vật phẩm hiện có tại Cửa Hàng!</color>";
            }
        }
    }
}
