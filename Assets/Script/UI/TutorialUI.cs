using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Onscreen UI card for the onboarding tutorial with animated directional highlight.
/// </summary>
public class TutorialUI : MonoBehaviour
{
    private static TutorialUI instance;
    public static TutorialUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TutorialUI>();
                if (instance == null)
                {
                    Canvas canvas = FindObjectOfType<Canvas>();
                    GameObject go = new GameObject("TutorialUI");
                    if (canvas != null) go.transform.SetParent(canvas.transform, false);
                    instance = go.AddComponent<TutorialUI>();
                }
            }
            return instance;
        }
    }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text stepTitleText;
    [SerializeField] private TMP_Text stepDescText;
    [SerializeField] private Button skipButton;
    [SerializeField] private RectTransform arrowTransform;

    private float bobTimer = 0f;
    private Vector2 arrowBasePos;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (panel == null)
        {
            CreateTutorialPanel();
        }

        if (skipButton != null)
        {
            FarmUITheme.ApplyButtonStyle(skipButton, FarmUITheme.ButtonState.Warning);
            skipButton.onClick.AddListener(() =>
            {
                TutorialManager.Instance?.SkipTutorial();
            });
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnStepChanged += Refresh;
            Refresh(TutorialManager.Instance.currentStep);
        }
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnStepChanged -= Refresh;
        }
    }

    private void Update()
    {
        if (arrowTransform != null && panel != null && panel.activeSelf)
        {
            bobTimer += Time.unscaledDeltaTime * 6f;
            float offset = Mathf.Sin(bobTimer) * 8f;
            arrowTransform.anchoredPosition = arrowBasePos + new Vector2(0f, offset);
        }
    }

    public void Refresh(TutorialStep step)
    {
        if (TutorialManager.Instance == null || TutorialManager.Instance.isCompleted || step == TutorialStep.Completed)
        {
            if (panel != null) panel.SetActive(false);
            return;
        }

        if (panel != null) panel.SetActive(true);

        if (stepTitleText != null)
        {
            stepTitleText.text = TutorialManager.Instance.GetStepTitle(step);
            stepTitleText.color = FarmUITheme.InkBrown;
        }

        if (stepDescText != null)
        {
            stepDescText.text = TutorialManager.Instance.GetStepDescription(step);
            stepDescText.color = FarmUITheme.WoodBrown;
        }

        UpdateArrowTarget(step);
    }

    private void UpdateArrowTarget(TutorialStep step)
    {
        if (arrowTransform == null) return;

        // Position arrow appropriately depending on step
        switch (step)
        {
            case TutorialStep.PlantSeed:
                arrowBasePos = new Vector2(0, -35f);
                break;
            case TutorialStep.HarvestCrop:
                arrowBasePos = new Vector2(0, -35f);
                break;
            case TutorialStep.SellCrop:
                arrowBasePos = new Vector2(-120f, -35f);
                break;
            case TutorialStep.BuySeed:
            case TutorialStep.BuyLand:
            case TutorialStep.HireWorker:
                arrowBasePos = new Vector2(120f, -35f);
                break;
        }
    }

    private void CreateTutorialPanel()
    {
        panel = new GameObject("TutorialCard", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.95f);
        rect.anchorMax = new Vector2(0.5f, 0.95f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(620f, 150f);

        Image img = panel.GetComponent<Image>();
        FarmUITheme.StylePanel(img);

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 8, 8);
        layout.spacing = 3;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        // Title row with skip button
        GameObject headerRow = new GameObject("HeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerRow.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup hl = headerRow.GetComponent<HorizontalLayoutGroup>();
        hl.childControlWidth = true;
        hl.childControlHeight = true;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(headerRow.transform, false);
        stepTitleText = titleObj.GetComponent<TextMeshProUGUI>();
        stepTitleText.fontSize = 22;
        stepTitleText.fontStyle = FontStyles.Bold;
        stepTitleText.color = FarmUITheme.InkBrown;

        GameObject skipObj = new GameObject("SkipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        skipObj.transform.SetParent(headerRow.transform, false);
        skipButton = skipObj.GetComponent<Button>();
        RectTransform skipRect = skipObj.GetComponent<RectTransform>();
        skipRect.sizeDelta = new Vector2(110f, 38f);

        GameObject skipTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        skipTextObj.transform.SetParent(skipObj.transform, false);
        TextMeshProUGUI skipTxt = skipTextObj.GetComponent<TextMeshProUGUI>();
        skipTxt.text = "Bỏ qua";
        skipTxt.fontSize = 16;
        skipTxt.alignment = TextAlignmentOptions.Center;
        skipTxt.color = Color.white;
        RectTransform skipTxtRect = skipTextObj.GetComponent<RectTransform>();
        skipTxtRect.anchorMin = Vector2.zero;
        skipTxtRect.anchorMax = Vector2.one;
        skipTxtRect.sizeDelta = Vector2.zero;

        // Desc text
        GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        descObj.transform.SetParent(panel.transform, false);
        stepDescText = descObj.GetComponent<TextMeshProUGUI>();
        stepDescText.fontSize = 17;
        stepDescText.color = FarmUITheme.WoodBrown;

        // Animated Arrow
        GameObject arrowObj = new GameObject("ArrowIndicator", typeof(RectTransform), typeof(TextMeshProUGUI));
        arrowObj.transform.SetParent(panel.transform, false);
        arrowTransform = arrowObj.GetComponent<RectTransform>();
        arrowTransform.sizeDelta = new Vector2(40f, 30f);
        arrowBasePos = new Vector2(0, -35f);
        arrowTransform.anchoredPosition = arrowBasePos;

        TextMeshProUGUI arrowTxt = arrowObj.GetComponent<TextMeshProUGUI>();
        arrowTxt.text = "▼";
        arrowTxt.fontSize = 24;
        arrowTxt.alignment = TextAlignmentOptions.Center;
        arrowTxt.color = FarmUITheme.GoldReward;
    }
}
