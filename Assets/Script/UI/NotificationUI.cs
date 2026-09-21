using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Standardized cozy notification popup system for Farm Fortune.
/// </summary>
public class NotificationUI : MonoBehaviour
{
    private static NotificationUI instance;
    public static NotificationUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NotificationUI>();
                if (instance == null)
                {
                    Canvas canvas = FindObjectOfType<Canvas>();
                    GameObject go = new GameObject("NotificationUI");
                    if (canvas != null) go.transform.SetParent(canvas.transform, false);
                    instance = go.AddComponent<NotificationUI>();
                }
            }
            return instance;
        }
    }

    private GameObject popupPanel;
    private TMP_Text titleText;
    private TMP_Text messageText;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    /// <summary>
    /// Shows a cozy notification banner.
    /// </summary>
    public static void Show(string title, string message, float duration = 3.5f)
    {
        try
        {
            // Lazily create the UI so the first gameplay notification is visible
            // even when no NotificationUI object was placed in the scene.
            Instance.Display(title, message, duration);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Notification] {title}: {message} ({ex.Message})");
        }
    }

    public void Display(string title, string message, float duration = 3.5f)
    {
        if (popupPanel == null) CreatePopupUI();

        titleText.text = title;
        messageText.text = message;
        popupPanel.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    private void CreatePopupUI()
    {
        popupPanel = new GameObject("NotificationPopup", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        popupPanel.transform.SetParent(transform, false);

        RectTransform rect = popupPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.85f);
        rect.anchorMax = new Vector2(0.5f, 0.85f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(380f, 85f);

        Image img = popupPanel.GetComponent<Image>();
        FarmUITheme.StylePanel(img);

        VerticalLayoutGroup layout = popupPanel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 10, 10);
        layout.spacing = 4;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        TMP_FontAsset pixelFont = FindObjectOfType<TMP_Text>()?.font ?? TMP_Settings.defaultFontAsset;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(popupPanel.transform, false);
        titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.font = pixelFont;
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = FarmUITheme.InkBrown;
        titleText.alignment = TextAlignmentOptions.Center;

        GameObject msgObj = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        msgObj.transform.SetParent(popupPanel.transform, false);
        messageText = msgObj.GetComponent<TextMeshProUGUI>();
        messageText.font = pixelFont;
        messageText.fontSize = 18;
        messageText.color = FarmUITheme.WoodBrown;
        messageText.alignment = TextAlignmentOptions.Center;

        popupPanel.SetActive(false);
    }
}
