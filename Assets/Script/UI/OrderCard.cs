using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual UI card representing an order with customer profile, multi-item requirements,
/// countdown timers, and interactive action buttons.
/// </summary>
public class OrderCard : MonoBehaviour
{
    private OrderInstance order;
    private Action onActionCallback;

    private TMP_Text customerText;
    private TMP_Text noteText;
    private TMP_Text reqsText;
    private TMP_Text timerText;
    private TMP_Text rewardText;
    private Button actionBtn;
    private TMP_Text actionBtnText;
    private Button cancelBtn;
    private TMP_FontAsset pixelFont;

    public void Setup(OrderInstance order, Action onActionCallback)
    {
        this.order = order;
        this.onActionCallback = onActionCallback;

        BuildCardUI();
        RefreshDisplay();
    }

    private void Update()
    {
        if (order != null && order.state == OrderState.Active && timerText != null)
        {
            timerText.text = $"TIME {order.GetFormattedTimeRemaining()}";
            if (order.timeRemaining < 60f)
            {
                timerText.color = FarmUITheme.RedWarning;
            }
            else
            {
                timerText.color = FarmUITheme.WoodBrown;
            }
        }
    }

    public void RefreshDisplay()
    {
        if (order == null) return;

        Player player = Player.main;

        if (customerText != null)
        {
            customerText.text = $"KHACH: {order.customerName}";
            customerText.color = FarmUITheme.InkBrown;
        }

        if (noteText != null)
        {
            noteText.text = string.IsNullOrEmpty(order.customerNote) ? order.orderTitle : $"\"{order.customerNote}\"";
            noteText.color = FarmUITheme.WoodBrown;
        }

        if (reqsText != null)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var req in order.requirements)
            {
                Item item = req.GetItem();
                string itemName = item != null ? item.itemName : $"Món #{req.itemId}";
                int owned = req.GetOwnedAmount(player);
                int needed = req.requiredAmount;

                if (owned >= needed)
                {
                    sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.GreenActive)}>[OK] {itemName}: {owned}/{needed}</color>   ");
                }
                else
                {
                    sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.RedWarning)}>[THIEU] {itemName}: {owned}/{needed} (thieu {needed - owned})</color>   ");
                }
            }
            reqsText.text = sb.ToString();
        }

        if (rewardText != null)
        {
            string coinHex = ColorUtility.ToHtmlStringRGB(FarmUITheme.GoldReward);
            string repHex = ColorUtility.ToHtmlStringRGB(FarmUITheme.GreenActive);
            rewardText.text = $"Thưởng: <color=#{coinHex}>+{order.rewardCoins:N0} xu</color>  |  <color=#{repHex}>+{order.rewardReputation} Uy tín</color>";
        }

        if (order.state == OrderState.Available)
        {
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (cancelBtn != null) cancelBtn.gameObject.SetActive(false);

            if (actionBtn != null)
            {
                FarmUITheme.ApplyButtonStyle(actionBtn, FarmUITheme.ButtonState.ActiveSelected);
                if (actionBtnText != null) actionBtnText.text = "Nhận Đơn";

                actionBtn.onClick.RemoveAllListeners();
                actionBtn.onClick.AddListener(() =>
                {
                    OrderManager.Instance?.AcceptOrder(order);
                    onActionCallback?.Invoke();
                });
            }
        }
        else if (order.state == OrderState.Active)
        {
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
                timerText.text = $"TIME {order.GetFormattedTimeRemaining()}";
            }

            if (cancelBtn != null)
            {
                cancelBtn.gameObject.SetActive(true);
                FarmUITheme.ApplyButtonStyle(cancelBtn, FarmUITheme.ButtonState.Warning);
                cancelBtn.onClick.RemoveAllListeners();
                cancelBtn.onClick.AddListener(() =>
                {
                    OrderManager.Instance?.CancelOrder(order.orderId);
                    onActionCallback?.Invoke();
                });
            }

            bool canDeliver = order.CanDeliver(player);
            if (actionBtn != null)
            {
                FarmUITheme.ApplyButtonStyle(actionBtn, canDeliver ? FarmUITheme.ButtonState.ActiveSelected : FarmUITheme.ButtonState.Locked);
                if (actionBtnText != null) actionBtnText.text = "Giao Hàng";

                actionBtn.onClick.RemoveAllListeners();
                actionBtn.onClick.AddListener(() =>
                {
                    if (player != null)
                    {
                        OrderManager.Instance?.TryCompleteOrder(order.orderId, player);
                        onActionCallback?.Invoke();
                    }
                });
            }
        }
    }

    private void BuildCardUI()
    {
        pixelFont = FindObjectOfType<TMP_Text>()?.font ?? TMP_Settings.defaultFontAsset;

        // Style root card
        Image cardBg = GetComponent<Image>();
        if (cardBg == null) cardBg = gameObject.AddComponent<Image>();
        FarmUITheme.StyleCard(cardBg);

        VerticalLayoutGroup rootLayout = GetComponent<VerticalLayoutGroup>();
        if (rootLayout == null) rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(14, 14, 10, 10);
        rootLayout.spacing = 6;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        LayoutElement cardSize = GetComponent<LayoutElement>();
        if (cardSize == null) cardSize = gameObject.AddComponent<LayoutElement>();
        cardSize.minHeight = 180f;
        cardSize.preferredHeight = 190f;

        // Header Row: Customer + Timer
        GameObject header = new GameObject("HeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(transform, false);
        HorizontalLayoutGroup hLayout = header.GetComponent<HorizontalLayoutGroup>();
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;

        GameObject custObj = new GameObject("Customer", typeof(RectTransform), typeof(TextMeshProUGUI));
        custObj.transform.SetParent(header.transform, false);
        customerText = custObj.GetComponent<TextMeshProUGUI>();
        customerText.font = pixelFont;
        customerText.fontSize = 20;
        customerText.fontStyle = FontStyles.Bold;
        customerText.raycastTarget = false;

        GameObject timerObj = new GameObject("Timer", typeof(RectTransform), typeof(TextMeshProUGUI));
        timerObj.transform.SetParent(header.transform, false);
        timerText = timerObj.GetComponent<TextMeshProUGUI>();
        timerText.font = pixelFont;
        timerText.fontSize = 18;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Right;
        timerText.raycastTarget = false;

        // Note
        GameObject noteObj = new GameObject("Note", typeof(RectTransform), typeof(TextMeshProUGUI));
        noteObj.transform.SetParent(transform, false);
        noteText = noteObj.GetComponent<TextMeshProUGUI>();
        noteText.font = pixelFont;
        noteText.fontSize = 15;
        noteText.fontStyle = FontStyles.Italic;
        noteText.raycastTarget = false;

        // Requirements Row
        GameObject reqsObj = new GameObject("Reqs", typeof(RectTransform), typeof(TextMeshProUGUI));
        reqsObj.transform.SetParent(transform, false);
        reqsText = reqsObj.GetComponent<TextMeshProUGUI>();
        reqsText.font = pixelFont;
        reqsText.fontSize = 16;
        reqsText.fontStyle = FontStyles.Bold;
        reqsText.raycastTarget = false;

        // Bottom Row: Rewards + Action Buttons
        GameObject footer = new GameObject("FooterRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footer.transform.SetParent(transform, false);
        HorizontalLayoutGroup fLayout = footer.GetComponent<HorizontalLayoutGroup>();
        fLayout.childControlWidth = true;
        fLayout.childControlHeight = true;
        fLayout.spacing = 10;

        GameObject rewardObj = new GameObject("Reward", typeof(RectTransform), typeof(TextMeshProUGUI));
        rewardObj.transform.SetParent(footer.transform, false);
        rewardText = rewardObj.GetComponent<TextMeshProUGUI>();
        rewardText.font = pixelFont;
        rewardText.fontSize = 16;
        rewardText.fontStyle = FontStyles.Bold;
        rewardText.raycastTarget = false;

        // Button container
        GameObject btnBox = new GameObject("BtnBox", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnBox.transform.SetParent(footer.transform, false);
        HorizontalLayoutGroup bLayout = btnBox.GetComponent<HorizontalLayoutGroup>();
        bLayout.childControlWidth = false;
        bLayout.childControlHeight = true;
        bLayout.spacing = 6;
        bLayout.childAlignment = TextAnchor.MiddleRight;

        // Cancel Button
        GameObject cBtnObj = new GameObject("CancelBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        cBtnObj.transform.SetParent(btnBox.transform, false);
        cancelBtn = cBtnObj.GetComponent<Button>();
        RectTransform cRect = cBtnObj.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(88f, 42f);

        GameObject cTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        cTxtObj.transform.SetParent(cBtnObj.transform, false);
        TextMeshProUGUI cTxt = cTxtObj.GetComponent<TextMeshProUGUI>();
        cTxt.text = "Hủy";
        cTxt.font = pixelFont;
        cTxt.fontSize = 16;
        cTxt.alignment = TextAlignmentOptions.Center;
        cTxt.color = Color.white;
        cTxt.raycastTarget = false;
        RectTransform cTxtRect = cTxtObj.GetComponent<RectTransform>();
        cTxtRect.anchorMin = Vector2.zero;
        cTxtRect.anchorMax = Vector2.one;
        cTxtRect.sizeDelta = Vector2.zero;

        // Action Button
        GameObject aBtnObj = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        aBtnObj.transform.SetParent(btnBox.transform, false);
        actionBtn = aBtnObj.GetComponent<Button>();
        RectTransform aRect = aBtnObj.GetComponent<RectTransform>();
        aRect.sizeDelta = new Vector2(132f, 42f);

        GameObject aTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        aTxtObj.transform.SetParent(aBtnObj.transform, false);
        actionBtnText = aTxtObj.GetComponent<TextMeshProUGUI>();
        actionBtnText.font = pixelFont;
        actionBtnText.fontSize = 16;
        actionBtnText.fontStyle = FontStyles.Bold;
        actionBtnText.alignment = TextAlignmentOptions.Center;
        actionBtnText.color = Color.white;
        actionBtnText.raycastTarget = false;
        RectTransform aTxtRect = aTxtObj.GetComponent<RectTransform>();
        aTxtRect.anchorMin = Vector2.zero;
        aTxtRect.anchorMax = Vector2.one;
        aTxtRect.sizeDelta = Vector2.zero;
    }
}
