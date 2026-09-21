using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Order Board Modal UI with dual tabs (Available & Active orders), FarmUITheme styling,
/// reputation display, and dynamic OrderCards.
/// </summary>
public class OrderUI : MonoBehaviour
{
    private static OrderUI instance;
    public static OrderUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<OrderUI>();
                if (instance == null)
                {
                    Canvas canvas = FindObjectOfType<Canvas>();
                    GameObject go = new GameObject("OrderUI");
                    if (canvas != null) go.transform.SetParent(canvas.transform, false);
                    instance = go.AddComponent<OrderUI>();
                }
            }
            return instance;
        }
    }

    [SerializeField] private GameObject orderPanel;
    [SerializeField] private Transform orderContainer;
    [SerializeField] private Button closeButton;

    private enum Tab { Available, Active }
    private Tab currentTab = Tab.Available;

    private TMP_Text reputationText;
    private Button availableTabBtn;
    private Button activeTabBtn;
    private TMP_Text availableTabTxt;
    private TMP_Text activeTabTxt;
    private TMP_Text emptyLabel;
    private TMP_FontAsset pixelFont;

    private float refreshTimer = 0f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (orderPanel == null)
        {
            BuildOrderUIModal();
        }
        else
        {
            // Panel assigned via inspector, apply styling and wire close button
            Image bg = orderPanel.GetComponent<Image>();
            if (bg != null) FarmUITheme.StylePanel(bg);

            if (closeButton != null)
            {
                FarmUITheme.ApplyButtonStyle(closeButton, FarmUITheme.ButtonState.Warning);
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }
    }

    private void Update()
    {
        if (orderPanel != null && orderPanel.activeSelf)
        {
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= 1.0f)
            {
                refreshTimer = 0f;
                // Periodic refresh to update button states if inventory changed
                UpdateCardsOnly();
            }
        }
    }

    public void Toggle()
    {
        if (orderPanel == null) return;
        bool willShow = !orderPanel.activeSelf;
        orderPanel.SetActive(willShow);
        if (willShow) Refresh();
    }

    public void Show()
    {
        if (orderPanel != null)
        {
            orderPanel.SetActive(true);
            Refresh();
        }
    }

    public void Hide()
    {
        if (orderPanel != null) orderPanel.SetActive(false);
    }

    public void SwitchTab(int tabIndex)
    {
        currentTab = (Tab)tabIndex;
        Refresh();
    }

    public void Refresh()
    {
        if (OrderManager.Instance == null) return;

        UpdateReputationHeader();
        UpdateTabButtons();

        if (orderContainer == null) return;

        // Clear existing cards
        foreach (Transform child in orderContainer)
        {
            Destroy(child.gameObject);
        }

        IReadOnlyList<OrderInstance> list = currentTab == Tab.Available 
            ? OrderManager.Instance.AvailableOrders 
            : OrderManager.Instance.ActiveOrders;

        if (emptyLabel != null)
        {
            emptyLabel.gameObject.SetActive(list.Count == 0);
            emptyLabel.text = currentTab == Tab.Available
                ? "Hiện chưa có thêm đơn hàng mới nào."
                : "Bạn chưa nhận đơn hàng nào. Hãy chọn đơn từ mục Đơn Có Sẵn!";
        }

        foreach (OrderInstance order in list)
        {
            if (order == null) continue;
            GameObject cardObj = new GameObject($"Card_{order.orderId}", typeof(RectTransform), typeof(Image), typeof(OrderCard));
            cardObj.transform.SetParent(orderContainer, false);
            OrderCard card = cardObj.GetComponent<OrderCard>();
            card.Setup(order, Refresh);
        }
    }

    private void UpdateCardsOnly()
    {
        if (orderContainer == null) return;
        foreach (Transform child in orderContainer)
        {
            OrderCard card = child.GetComponent<OrderCard>();
            if (card != null) card.RefreshDisplay();
        }
    }

    private void UpdateReputationHeader()
    {
        if (reputationText != null && OrderManager.Instance != null)
        {
            reputationText.text = $"DANH TIẾNG: Cấp {OrderManager.Instance.ReputationLevel} ({OrderManager.Instance.farmReputation} điểm)";
            reputationText.color = FarmUITheme.InkBrown;
        }
    }

    private void UpdateTabButtons()
    {
        int availCount = OrderManager.Instance != null ? OrderManager.Instance.AvailableOrders.Count : 0;
        int activeCount = OrderManager.Instance != null ? OrderManager.Instance.ActiveOrders.Count : 0;

        if (availableTabTxt != null) availableTabTxt.text = $"Đơn Có Sẵn ({availCount})";
        if (activeTabTxt != null) activeTabTxt.text = $"Đang Thực Hiện ({activeCount}/{OrderManager.MAX_ACTIVE_ORDERS})";

        if (availableTabBtn != null)
        {
            FarmUITheme.ApplyButtonStyle(availableTabBtn, currentTab == Tab.Available ? FarmUITheme.ButtonState.ActiveSelected : FarmUITheme.ButtonState.Purchasable);
        }

        if (activeTabBtn != null)
        {
            FarmUITheme.ApplyButtonStyle(activeTabBtn, currentTab == Tab.Active ? FarmUITheme.ButtonState.ActiveSelected : FarmUITheme.ButtonState.Purchasable);
        }
    }

    private void BuildOrderUIModal()
    {
        pixelFont = FindObjectOfType<TMP_Text>()?.font ?? TMP_Settings.defaultFontAsset;

        orderPanel = new GameObject("OrderModalPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        orderPanel.transform.SetParent(transform, false);

        RectTransform rect = orderPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(720f, 620f);

        Image panelImg = orderPanel.GetComponent<Image>();
        FarmUITheme.StylePanel(panelImg);

        VerticalLayoutGroup vLayout = orderPanel.GetComponent<VerticalLayoutGroup>();
        vLayout.padding = new RectOffset(24, 24, 20, 20);
        vLayout.spacing = 12;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;

        // Top Bar: Title, Reputation, Close
        GameObject topBar = new GameObject("TopBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        topBar.transform.SetParent(orderPanel.transform, false);
        topBar.AddComponent<LayoutElement>().preferredHeight = 48f;
        HorizontalLayoutGroup topLayout = topBar.GetComponent<HorizontalLayoutGroup>();
        topLayout.childControlWidth = true;
        topLayout.childControlHeight = true;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(topBar.transform, false);
        TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
        title.text = "BẢNG ĐƠN HÀNG";
        title.font = pixelFont;
        title.fontSize = 28;
        title.fontStyle = FontStyles.Bold;
        title.color = FarmUITheme.InkBrown;
        title.raycastTarget = false;

        GameObject repObj = new GameObject("Reputation", typeof(RectTransform), typeof(TextMeshProUGUI));
        repObj.transform.SetParent(topBar.transform, false);
        reputationText = repObj.GetComponent<TextMeshProUGUI>();
        reputationText.font = pixelFont;
        reputationText.fontSize = 18;
        reputationText.fontStyle = FontStyles.Bold;
        reputationText.alignment = TextAlignmentOptions.Center;
        reputationText.raycastTarget = false;

        GameObject closeObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObj.transform.SetParent(topBar.transform, false);
        closeButton = closeObj.GetComponent<Button>();
        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.sizeDelta = new Vector2(48f, 44f);
        FarmUITheme.ApplyButtonStyle(closeButton, FarmUITheme.ButtonState.Warning);
        closeButton.onClick.AddListener(Hide);

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeObj.transform, false);
        TextMeshProUGUI closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "X";
        closeTxt.font = pixelFont;
        closeTxt.fontSize = 22;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;
        closeTxt.raycastTarget = false;
        RectTransform cTxtRect = closeTxtObj.GetComponent<RectTransform>();
        cTxtRect.anchorMin = Vector2.zero;
        cTxtRect.anchorMax = Vector2.one;
        cTxtRect.sizeDelta = Vector2.zero;

        // Tabs row
        GameObject tabsRow = new GameObject("TabsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabsRow.transform.SetParent(orderPanel.transform, false);
        HorizontalLayoutGroup tabLayout = tabsRow.GetComponent<HorizontalLayoutGroup>();
        tabLayout.childControlWidth = true;
        tabLayout.childControlHeight = true;
        tabLayout.spacing = 10;

        // Available Tab
        GameObject availObj = new GameObject("AvailableTab", typeof(RectTransform), typeof(Image), typeof(Button));
        availObj.transform.SetParent(tabsRow.transform, false);
        availObj.AddComponent<LayoutElement>().preferredHeight = 48f;
        availableTabBtn = availObj.GetComponent<Button>();
        availableTabBtn.onClick.AddListener(() => SwitchTab(0));

        GameObject aTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        aTxtObj.transform.SetParent(availObj.transform, false);
        availableTabTxt = aTxtObj.GetComponent<TextMeshProUGUI>();
        availableTabTxt.font = pixelFont;
        availableTabTxt.fontSize = 18;
        availableTabTxt.fontStyle = FontStyles.Bold;
        availableTabTxt.alignment = TextAlignmentOptions.Center;
        availableTabTxt.color = Color.white;
        availableTabTxt.raycastTarget = false;
        RectTransform atRect = aTxtObj.GetComponent<RectTransform>();
        atRect.anchorMin = Vector2.zero;
        atRect.anchorMax = Vector2.one;
        atRect.sizeDelta = Vector2.zero;

        // Active Tab
        GameObject actObj = new GameObject("ActiveTab", typeof(RectTransform), typeof(Image), typeof(Button));
        actObj.transform.SetParent(tabsRow.transform, false);
        actObj.AddComponent<LayoutElement>().preferredHeight = 48f;
        activeTabBtn = actObj.GetComponent<Button>();
        activeTabBtn.onClick.AddListener(() => SwitchTab(1));

        GameObject acTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        acTxtObj.transform.SetParent(actObj.transform, false);
        activeTabTxt = acTxtObj.GetComponent<TextMeshProUGUI>();
        activeTabTxt.font = pixelFont;
        activeTabTxt.fontSize = 18;
        activeTabTxt.fontStyle = FontStyles.Bold;
        activeTabTxt.alignment = TextAlignmentOptions.Center;
        activeTabTxt.color = Color.white;
        activeTabTxt.raycastTarget = false;
        RectTransform actRect = acTxtObj.GetComponent<RectTransform>();
        actRect.anchorMin = Vector2.zero;
        actRect.anchorMax = Vector2.one;
        actRect.sizeDelta = Vector2.zero;

        // Scrollable card area: orders remain usable when the board contains more than two cards.
        GameObject scrollObj = new GameObject("OrderScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
        scrollObj.transform.SetParent(orderPanel.transform, false);
        Image scrollImage = scrollObj.GetComponent<Image>();
        scrollImage.color = new Color(FarmUITheme.WoodBrown.r, FarmUITheme.WoodBrown.g, FarmUITheme.WoodBrown.b, 0.16f);
        LayoutElement scrollLayout = scrollObj.GetComponent<LayoutElement>();
        scrollLayout.minHeight = 300f;
        scrollLayout.preferredHeight = 410f;
        scrollLayout.flexibleHeight = 1f;

        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        Image viewportImage = viewportObj.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.001f);
        viewportObj.GetComponent<Mask>().showMaskGraphic = false;

        GameObject containerObj = new GameObject("OrderContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        containerObj.transform.SetParent(viewportObj.transform, false);
        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0f, 1f);
        containerRect.anchorMax = new Vector2(1f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = Vector2.zero;
        orderContainer = containerObj.transform;
        VerticalLayoutGroup cLayout = containerObj.GetComponent<VerticalLayoutGroup>();
        cLayout.padding = new RectOffset(4, 4, 4, 4);
        cLayout.spacing = 8;
        cLayout.childControlWidth = true;
        cLayout.childControlHeight = false;
        cLayout.childForceExpandWidth = true;
        ContentSizeFitter contentFitter = containerObj.GetComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = containerRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 35f;

        // Empty label
        GameObject emptyObj = new GameObject("EmptyLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        emptyObj.transform.SetParent(orderPanel.transform, false);
        emptyLabel = emptyObj.GetComponent<TextMeshProUGUI>();
        emptyLabel.font = pixelFont;
        emptyLabel.fontSize = 18;
        emptyLabel.fontStyle = FontStyles.Italic;
        emptyLabel.alignment = TextAlignmentOptions.Center;
        emptyLabel.color = FarmUITheme.WoodBrown;
        emptyLabel.raycastTarget = false;
        emptyLabel.gameObject.SetActive(false);

        orderPanel.SetActive(false);
    }
}
