using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup screen displayed when player unlocks new seeds, tools, or farm features.
/// </summary>
public class UnlockUI : MonoBehaviour
{
    public static UnlockUI Instance { get; private set; }

    [SerializeField] private GameObject window;
    [SerializeField] private TMP_Text unlockTitle;
    [SerializeField] private TMP_Text unlockDescription;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button confirmButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (confirmButton != null)
        {
            FarmUITheme.ApplyButtonStyle(confirmButton, FarmUITheme.ButtonState.Purchasable);
            confirmButton.onClick.AddListener(() => Hide());
        }
    }

    public static void ShowUnlock(string title, string description, Sprite icon = null)
    {
        if (Instance != null)
        {
            Instance.Display(title, description, icon);
        }
        else
        {
            NotificationUI.Show(title, description);
        }
    }

    public void Display(string title, string description, Sprite icon = null)
    {
        if (window != null) window.SetActive(true);
        if (unlockTitle != null) unlockTitle.text = title;
        if (unlockDescription != null) unlockDescription.text = description;
        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(icon != null);
            if (icon != null) itemIcon.sprite = icon;
        }
    }

    public void Hide()
    {
        if (window != null) window.SetActive(false);
    }
}
