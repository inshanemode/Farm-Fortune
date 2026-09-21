using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopRenderer : MonoBehaviour
{
    public GameObject itemPrefab;
    public GameObject shopContent;
    public TMP_Text shopName;
    public Button closeButton;

    public CursorMessage currentMessage;

    private Shop currentShop;
    private Player currentPlayer;
    private Transform tabContainer;
    private ShopStock.ShopCategory activeCategory = ShopStock.ShopCategory.Seeds;
    private readonly Dictionary<ShopStock.ShopCategory, Image> tabImages = new Dictionary<ShopStock.ShopCategory, Image>();

    private void Start()
    {
        if (closeButton != null)
        {
            FarmUITheme.ApplyButtonStyle(closeButton, FarmUITheme.ButtonState.Warning);
            closeButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                CursorRenderer.Instance.SetMessageIndex(0);
            });
        }

        Image panelImage = GetComponent<Image>();
        if (panelImage != null)
        {
            FarmUITheme.StylePanel(panelImage);
        }

        SetupSmoothScrolling();
    }

    private void SetupSmoothScrolling()
    {
        if (shopContent == null) return;
        ScrollRect scroll = shopContent.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            SmoothScrollRect smooth = scroll.gameObject.GetComponent<SmoothScrollRect>();
            if (smooth == null)
            {
                smooth = scroll.gameObject.AddComponent<SmoothScrollRect>();
            }
            smooth.Initialize();
            smooth.AttachStyledScrollbar();
        }
    }

    private void ClearShop()
    {
        foreach (Transform child in shopContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void RenderShop(Shop shop, Player player)
    {
        gameObject.SetActive(true);
        CursorRenderer.Instance.SetMessageIndex(1);

        currentShop = shop;
        currentPlayer = player;
        if (shopName != null)
        {
            shopName.text = shop.shopName;
            shopName.color = FarmUITheme.InkBrown;
        }

        CreateTabs();
        if (!HasItemsInCategory(activeCategory)) activeCategory = GetFirstAvailableCategory();
        RenderActiveCategory();
    }

    private void CreateTabs()
    {
        if (tabContainer != null) return;

        GameObject tabs = new GameObject("ShopTabs", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        tabs.transform.SetParent(transform, false);
        tabs.layer = gameObject.layer;
        tabContainer = tabs.transform;

        RectTransform rect = tabs.GetComponent<RectTransform>();
        // Responsive anchors at top of panel
        rect.anchorMin = new Vector2(0.05f, 0.82f);
        rect.anchorMax = new Vector2(0.95f, 0.94f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image tabBg = tabs.GetComponent<Image>();
        tabBg.color = new Color(FarmUITheme.InkBrown.r, FarmUITheme.InkBrown.g, FarmUITheme.InkBrown.b, 0.9f);
        FarmUITheme.AddOutline(tabBg, FarmUITheme.WoodBrown, new Vector2(2f, -2f));

        HorizontalLayoutGroup layout = tabs.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(6, 6, 6, 6);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;

        CreateTab(ShopStock.ShopCategory.Seeds, "SEEDS");
        CreateTab(ShopStock.ShopCategory.Animals, "ANIMALS");
        CreateTab(ShopStock.ShopCategory.Tools, "TOOLS");
        CreateTab(ShopStock.ShopCategory.Upgrades, "UPGRADES");
    }

    private void CreateTab(ShopStock.ShopCategory category, string label)
    {
        GameObject tab = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        tab.transform.SetParent(tabContainer, false);
        tab.layer = gameObject.layer;

        Image image = tab.GetComponent<Image>();
        tabImages[category] = image;
        tab.GetComponent<LayoutElement>().minWidth = 100f;

        Button button = tab.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            activeCategory = category;
            RenderActiveCategory();
        });

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(tab.transform, false);
        textObject.layer = gameObject.layer;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (shopName != null) text.font = shopName.font;
        text.text = label;
        text.fontSize = 18;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = FarmUITheme.TextLight;
        text.raycastTarget = false;
    }

    private void RenderActiveCategory()
    {
        if (currentShop == null || currentPlayer == null) return;
        ClearShop();
        UpdateTabColors();
        SetupSmoothScrolling();

        ScrollRect scroll = shopContent != null ? shopContent.GetComponentInParent<ScrollRect>() : null;
        if (scroll != null)
        {
            scroll.verticalNormalizedPosition = 1f;
            scroll.velocity = Vector2.zero;
        }

        foreach (ShopStock stock in currentShop.buyableItems)
        {
            if (stock.GetCategory() != activeCategory) continue;

            GameObject newItemPrefab = Instantiate(itemPrefab, shopContent.transform);
            ItemRenderer itemRenderer = newItemPrefab.GetComponent<ItemRenderer>();

            // 4 States evaluation
            bool isMaxed = stock.IsMaxed(currentPlayer);
            bool isUnlocked = stock.IsUnlocked(currentPlayer);
            int currentPrice = stock.GetBuyPrice(currentPlayer);
            bool hasMoney = currentPlayer.HasMoney(currentPrice);

            FarmUITheme.ButtonState buttonState;
            string priceLabel;
            Color priceColor;
            string tooltipTitle = stock.GetStockName();
            string tooltipDesc;

            if (isMaxed)
            {
                buttonState = FarmUITheme.ButtonState.Maxed;
                priceLabel = "MAX";
                priceColor = FarmUITheme.GrayLocked;
                tooltipDesc = $"{stock.GetStockName()}\n<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.GoldReward)}>Already Maxed Out!</color>";
            }
            else if (!isUnlocked)
            {
                buttonState = FarmUITheme.ButtonState.Locked;
                priceLabel = "LOCKED";
                priceColor = FarmUITheme.GrayLocked;
                tooltipDesc = $"{stock.GetStockName()}\n<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.RedWarning)}>{stock.GetUnlockRequirementText(currentPlayer)}</color>";
            }
            else if (!hasMoney)
            {
                buttonState = FarmUITheme.ButtonState.NotEnoughMoney;
                priceLabel = currentPrice.ToString("N0");
                priceColor = FarmUITheme.RedWarning;
                tooltipDesc = $"{stock.GetStockName()}\nPrice: {currentPrice:N0} coins (<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.RedWarning)}>NOT ENOUGH COINS</color>)\n\n{stock.GetDescription(currentPlayer)}";
            }
            else
            {
                buttonState = FarmUITheme.ButtonState.Purchasable;
                priceLabel = currentPrice.ToString("N0");
                priceColor = FarmUITheme.TextLight;
                tooltipDesc = $"{stock.GetStockName()}\nPrice: {currentPrice:N0} coins\n\n{stock.GetDescription(currentPlayer)}";
            }

            UnityAction OnPointerClick = () =>
            {
                if (isMaxed)
                {
                    Debug.Log($"[Shop] Item '{stock.GetStockName()}' is already at maximum level.");
                    return;
                }
                if (!isUnlocked)
                {
                    Debug.LogWarning($"[Shop] Item '{stock.GetStockName()}' is locked: {stock.GetUnlockRequirementText(currentPlayer)}");
                    return;
                }
                if (!currentPlayer.HasMoney(currentPrice))
                {
                    Debug.LogWarning($"[Shop] Not enough coins to purchase '{stock.GetStockName()}'. Required: {currentPrice}, Have: {currentPlayer.totalMoney}");
                    return;
                }

                // Deduct money and add stock
                currentPlayer.AddMoney(-1 * currentPrice);
                stock.AddStock(currentPlayer);
                FeedbackManager.Instance?.PlayPurchaseSound();
                if (stock is ItemStock itemStock && itemStock.item is SeedItem)
                {
                    TutorialManager.Instance?.OnSeedPurchased();
                }
                Debug.Log($"[Shop] Successfully purchased '{stock.GetStockName()}' for {currentPrice} coins.");

                // Immediately re-render shop with updated prices and states
                RenderActiveCategory();
            };

            UnityAction OnPointerEnter = () =>
            {
                currentMessage = new CursorMessage(tooltipTitle, tooltipDesc, 1);
                EventManager.TriggerEvent(new OnCursorMessageRequest(currentMessage, true));
            };

            UnityAction OnPointerExit = () =>
            {
                ClearMessage();
            };

            itemRenderer.RenderShopItem(stock.GetStockIcon(), priceLabel, buttonState, priceColor, OnPointerClick, OnPointerEnter, OnPointerExit);
        }
    }

    private bool HasItemsInCategory(ShopStock.ShopCategory category)
    {
        if (currentShop == null) return false;
        foreach (ShopStock stock in currentShop.buyableItems)
        {
            if (stock.GetCategory() == category) return true;
        }
        return false;
    }

    private ShopStock.ShopCategory GetFirstAvailableCategory()
    {
        foreach (ShopStock.ShopCategory category in System.Enum.GetValues(typeof(ShopStock.ShopCategory)))
        {
            if (HasItemsInCategory(category)) return category;
        }
        return ShopStock.ShopCategory.Seeds;
    }

    private void UpdateTabColors()
    {
        foreach (KeyValuePair<ShopStock.ShopCategory, Image> tab in tabImages)
        {
            bool isActive = tab.Key == activeCategory;
            tab.Value.color = isActive ? FarmUITheme.GreenActive : FarmUITheme.WoodBrown;
        }
    }

    private void ClearMessage()
    {
        if (currentMessage == null) return;
        EventManager.TriggerEvent(new OnCursorMessageRequest(currentMessage, false));
        currentMessage = null;
    }
}
