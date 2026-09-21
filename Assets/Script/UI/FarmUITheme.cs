using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Centralized UI style and theme management for Farm Fortune.
/// Adheres strictly to the pixel-art cozy farm tycoon palette.
/// </summary>
public static class FarmUITheme
{
    // ==========================================
    // Core Color Palette
    // ==========================================
    public static readonly Color InkBrown = new Color(0.231f, 0.110f, 0.071f, 1.0f);     // #3B1C12
    public static readonly Color WoodBrown = new Color(0.400f, 0.200f, 0.122f, 1.0f);    // #66331F
    public static readonly Color PanelCream = new Color(0.961f, 0.824f, 0.604f, 1.0f);   // #F5D29A
    public static readonly Color CardCream = new Color(0.957f, 0.820f, 0.561f, 1.0f);    // #F4D18F
    public static readonly Color GreenActive = new Color(0.435f, 0.686f, 0.271f, 1.0f);  // #6FAF45
    public static readonly Color RedWarning = new Color(0.725f, 0.290f, 0.208f, 1.0f);   // #B94A35
    public static readonly Color GoldReward = new Color(0.957f, 0.725f, 0.259f, 1.0f);   // #F4B942
    public static readonly Color GrayLocked = new Color(0.545f, 0.506f, 0.471f, 1.0f);   // #8B8178

    // Supporting UI Colors
    public static readonly Color TextDark = InkBrown;
    public static readonly Color TextLight = new Color(1.0f, 0.96f, 0.88f, 1.0f);
    public static readonly Color TextSubtle = WoodBrown;
    public static readonly Color OrangeWithering = new Color(0.902f, 0.494f, 0.133f, 1.0f); // #E67E22

    // ==========================================
    // Button Style States
    // ==========================================
    public enum ButtonState
    {
        Purchasable,   // Warm brown/orange, ready to buy
        NotEnoughMoney,// Darker/dimmed with red accent
        ActiveSelected,// Vibrant green
        Locked,        // Grayed out, locked
        Maxed,         // Neutral disabled with MAX label
        Warning,       // Red action (close, cancel, remove)
        Reward         // Gold/Yellow action
    }

    /// <summary>
    /// Applies standardized colors and transitions to any button.
    /// </summary>
    public static void ApplyButtonStyle(Button button, ButtonState state, TMP_Text labelText = null)
    {
        if (button == null) return;

        Image bgImage = button.GetComponent<Image>();
        Color baseColor;
        Color highlightColor;
        Color pressedColor;
        bool interactable = true;

        switch (state)
        {
            case ButtonState.Purchasable:
                baseColor = new Color(0.78f, 0.45f, 0.22f, 1.0f);
                highlightColor = new Color(0.88f, 0.55f, 0.30f, 1.0f);
                pressedColor = WoodBrown;
                interactable = true;
                break;

            case ButtonState.NotEnoughMoney:
                baseColor = new Color(0.48f, 0.32f, 0.28f, 0.85f);
                highlightColor = baseColor;
                pressedColor = baseColor;
                interactable = false;
                break;

            case ButtonState.ActiveSelected:
                baseColor = GreenActive;
                highlightColor = new Color(0.52f, 0.78f, 0.33f, 1.0f);
                pressedColor = new Color(0.35f, 0.58f, 0.20f, 1.0f);
                interactable = true;
                break;

            case ButtonState.Locked:
            case ButtonState.Maxed:
                baseColor = GrayLocked;
                highlightColor = GrayLocked;
                pressedColor = GrayLocked;
                interactable = false;
                break;

            case ButtonState.Warning:
                baseColor = RedWarning;
                highlightColor = new Color(0.85f, 0.35f, 0.25f, 1.0f);
                pressedColor = new Color(0.55f, 0.20f, 0.15f, 1.0f);
                interactable = true;
                break;

            case ButtonState.Reward:
                baseColor = GoldReward;
                highlightColor = new Color(1.0f, 0.80f, 0.35f, 1.0f);
                pressedColor = new Color(0.80f, 0.60f, 0.20f, 1.0f);
                interactable = true;
                break;

            default:
                baseColor = WoodBrown;
                highlightColor = WoodBrown;
                pressedColor = InkBrown;
                break;
        }

        if (bgImage != null)
        {
            bgImage.color = baseColor;
            // Runtime-created buttons do not always auto-assign their Image as targetGraphic.
            // Without this, the button may receive clicks but never show the intended state.
            if (button.targetGraphic == null) button.targetGraphic = bgImage;
        }

        button.interactable = interactable;
        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = highlightColor;
        colors.pressedColor = pressedColor;
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        button.colors = colors;

        if (labelText != null)
        {
            labelText.color = (state == ButtonState.Locked || state == ButtonState.Maxed)
                ? new Color(0.85f, 0.85f, 0.85f, 0.7f)
                : TextLight;
        }
    }

    /// <summary>
    /// Adds or updates an Outline component on a Graphic.
    /// </summary>
    public static Outline AddOutline(Graphic graphic, Color color, Vector2 distance)
    {
        if (graphic == null) return null;
        Outline outline = graphic.GetComponent<Outline>();
        if (outline == null)
        {
            outline = graphic.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = color;
        outline.effectDistance = distance;
        return outline;
    }

    /// <summary>
    /// Looks up a cozy panel sprite from resources or falls back gracefully.
    /// </summary>
    public static Sprite FindPanelSprite()
    {
        foreach (Image img in Resources.FindObjectsOfTypeAll<Image>())
        {
            if (img.sprite != null && img.sprite.name.ToLowerInvariant().StartsWith("paper"))
            {
                return img.sprite;
            }
        }
        return null;
    }

    /// <summary>
    /// Formats money string with color indicator: normal or red if player lacks funds.
    /// </summary>
    public static string FormatPriceTag(int price, int playerMoney)
    {
        string formatted = price.ToString("N0");
        if (playerMoney < price)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(RedWarning)}>{formatted} coins</color>";
        }
        return $"{formatted} coins";
    }

    /// <summary>
    /// Applies standard styling to a panel image.
    /// </summary>
    public static void StylePanel(Image panelImage)
    {
        if (panelImage == null) return;
        Sprite sprite = FindPanelSprite();
        if (sprite != null)
        {
            panelImage.sprite = sprite;
            panelImage.type = Image.Type.Sliced;
        }
        panelImage.color = PanelCream;
        AddOutline(panelImage, WoodBrown, new Vector2(3f, -3f));
    }

    /// <summary>
    /// Applies standard styling to a card image.
    /// </summary>
    public static void StyleCard(Image cardImage)
    {
        if (cardImage == null) return;
        cardImage.color = CardCream;
        AddOutline(cardImage, WoodBrown, new Vector2(2f, -2f));
    }
}
