using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerListRenderer : MonoBehaviour
{
    public static WorkerListRenderer Instance { get; private set; }

    private WorkerManager workerManager;
    private GameObject panel;
    private Transform scrollContent;
    private TMP_FontAsset pixelFont;
    private float refreshTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public static WorkerListRenderer Create(Transform canvasTransform, WorkerManager manager)
    {
        GameObject objectRenderer = new GameObject("WorkerListRenderer");
        objectRenderer.transform.SetParent(canvasTransform, false);
        WorkerListRenderer renderer = objectRenderer.AddComponent<WorkerListRenderer>();
        renderer.workerManager = manager;
        renderer.CreatePanel(canvasTransform);
        Instance = renderer;
        return renderer;
    }

    public void Toggle()
    {
        bool willShow = !panel.activeSelf;
        panel.SetActive(willShow);
        if (willShow) Refresh();
    }

    private void Update()
    {
        if (panel != null && panel.activeSelf)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= 0.5f)
            {
                refreshTimer = 0f;
                RefreshProgressAndStatus();
            }
        }
    }

    private void CreatePanel(Transform canvasTransform)
    {
        pixelFont = FindObjectOfType<TMP_Text>()?.font ?? TMP_Settings.defaultFontAsset;

        // Main Responsive Panel
        panel = new GameObject("WorkerListPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(canvasTransform, false);
        panel.layer = canvasTransform.gameObject.layer;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.08f);
        rect.anchorMax = new Vector2(0.9f, 0.92f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image panelImage = panel.GetComponent<Image>();
        FarmUITheme.StylePanel(panelImage);

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 20);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // Header Title
        CreateLabel(panel.transform, "WORKER MANAGEMENT", 30, FontStyles.Bold, FarmUITheme.InkBrown, 46);
        CreateLabel(panel.transform, "Assign tasks and seeds to automate planting & harvesting", 19, FontStyles.Normal, FarmUITheme.WoodBrown, 30);

        // Scroll Area for Workers
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(panel.transform, false);
        scrollObj.layer = panel.layer;

        RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.sizeDelta = new Vector2(0, 470);
        Image scrollBg = scrollObj.GetComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0.05f);

        ScrollRect scroll = scrollObj.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        SmoothScrollRect smoothScroll = scrollObj.AddComponent<SmoothScrollRect>();
        smoothScroll.Initialize();
        smoothScroll.AttachStyledScrollbar();

        // Viewport
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        viewport.layer = panel.layer;
        RectTransform vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        content.layer = panel.layer;
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(8, 8, 8, 8);
        contentLayout.spacing = 12;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;
        scroll.viewport = vpRect;
        scrollContent = content.transform;

        // Close button at bottom
        Button closeBtn = CreateButton(panel.transform, "CLOSE", () => panel.SetActive(false), FarmUITheme.ButtonState.Warning, 50);

        panel.SetActive(false);
    }

    private void Refresh()
    {
        if (scrollContent == null || workerManager == null) return;

        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // Clean up invalid workers
        IReadOnlyList<WorkerEntity> workers = workerManager.Workers;
        for (int i = 0; i < workers.Count; i++)
        {
            WorkerEntity worker = workers[i];
            if (worker != null)
            {
                CreateWorkerCard(i + 1, worker);
            }
        }
    }

    private void RefreshProgressAndStatus()
    {
        if (scrollContent == null || workerManager == null) return;
        IReadOnlyList<WorkerEntity> workers = workerManager.Workers;

        for (int i = 0; i < workers.Count && i < scrollContent.childCount; i++)
        {
            WorkerEntity worker = workers[i];
            Transform card = scrollContent.GetChild(i);
            if (worker == null || card == null) continue;

            TMP_Text headerText = card.Find("HeaderRow/HeaderText")?.GetComponent<TMP_Text>();
            if (headerText != null)
            {
                string status = worker.GetStatusText();
                string statusColorHex = ColorUtility.ToHtmlStringRGB(worker.GetStatusColor());
                float progress = worker.GetCurrentProgress();
                string progStr = progress > 0 ? $" ({progress:0}%)" : "";

                headerText.text = $"WORKER #{i + 1}  •  <color=#{statusColorHex}>[{status}{progStr}]</color>";
            }
        }
    }

    private void CreateWorkerCard(int index, WorkerEntity worker)
    {
        GameObject card = new GameObject($"WorkerCard_{index}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        card.transform.SetParent(scrollContent, false);
        card.layer = panel.layer;

        Image cardImage = card.GetComponent<Image>();
        FarmUITheme.StyleCard(cardImage);

        VerticalLayoutGroup cardLayout = card.GetComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(14, 14, 10, 10);
        cardLayout.spacing = 8;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = false;

        // Row 1: Header (Worker name, Status, Progress)
        GameObject headerRow = new GameObject("HeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerRow.transform.SetParent(card.transform, false);
        headerRow.layer = panel.layer;
        HorizontalLayoutGroup hLayout = headerRow.GetComponent<HorizontalLayoutGroup>();
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;

        string status = worker.GetStatusText();
        string statusColorHex = ColorUtility.ToHtmlStringRGB(worker.GetStatusColor());
        float progress = worker.GetCurrentProgress();
        string progStr = progress > 0 ? $" ({progress:0}%)" : "";

        TMP_Text headerLabel = CreateLabel(headerRow.transform, $"WORKER #{index}  -  <color=#{statusColorHex}>[{status}{progStr}]</color>", 20, FontStyles.Bold, FarmUITheme.InkBrown, 34);
        headerLabel.gameObject.name = "HeaderText";

        // Row 2: Tasks (Nothing, Plant, Harvest, All, or Replace Task if busy)
        GameObject taskRow = new GameObject("TaskRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        taskRow.transform.SetParent(card.transform, false);
        taskRow.layer = panel.layer;
        HorizontalLayoutGroup tLayout = taskRow.GetComponent<HorizontalLayoutGroup>();
        tLayout.spacing = 8;
        tLayout.childControlWidth = false;
        tLayout.childForceExpandWidth = false;

        CreateLabel(taskRow.transform, $"Task: {GetTaskName(worker.CurrentTask)}", 18, FontStyles.Bold, FarmUITheme.InkBrown, 40, 150);

        bool isWorking = worker.CurrentStateName == "WorkerWorkState";

        // Check if player has seed
        bool playerHasSeeds = HasAnySeedInInventory();

        CreateTaskBtn(taskRow.transform, "NOTHING", WorkerManager.WorkerTask.Manual, worker);

        // Plant button: disabled if no seed
        Button plantBtn = CreateTaskBtn(taskRow.transform, "PLANT", WorkerManager.WorkerTask.PlantOnly, worker, !playerHasSeeds);

        CreateTaskBtn(taskRow.transform, "HARVEST", WorkerManager.WorkerTask.HarvestOnly, worker);
        CreateTaskBtn(taskRow.transform, "ALL", WorkerManager.WorkerTask.PlantAndHarvest, worker, !playerHasSeeds);

        if (isWorking)
        {
            CreateButton(taskRow.transform, "REPLACE TASK", () =>
            {
                worker.ResetState();
                Refresh();
            }, FarmUITheme.ButtonState.Warning, 30, 120);
        }

        // Row 3: Seed Selection
        GameObject seedRow = new GameObject("SeedRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        seedRow.transform.SetParent(card.transform, false);
        seedRow.layer = panel.layer;
        HorizontalLayoutGroup sLayout = seedRow.GetComponent<HorizontalLayoutGroup>();
        sLayout.spacing = 6;
        sLayout.childControlWidth = false;
        sLayout.childForceExpandWidth = false;

        string seedName = worker.SelectedSeed != null ? worker.SelectedSeed.itemName : "NONE";
        CreateLabel(seedRow.transform, $"Seed: {seedName}", 17, FontStyles.Bold, FarmUITheme.WoodBrown, 38, 170);

        List<SeedItem> availableSeeds = GetAvailableSeeds();
        if (availableSeeds.Count == 0)
        {
            CreateLabel(seedRow.transform, "(No seeds in inventory. Buy in Shop!)", 16, FontStyles.Italic, FarmUITheme.RedWarning, 38, 300);
        }
        else
        {
            foreach (SeedItem seed in availableSeeds)
            {
                int count = Player.main != null ? Player.main.inventory.GetAmount(seed) : 0;
                bool isSelected = worker.SelectedSeed == seed;
                FarmUITheme.ButtonState bState = isSelected ? FarmUITheme.ButtonState.ActiveSelected : FarmUITheme.ButtonState.Purchasable;

                string label = $"{seed.itemName.Replace(" Seeds", "")} ({count})";
                CreateButton(seedRow.transform, label, () =>
                {
                    workerManager.SetSeed(worker, seed);
                    Refresh();
                }, bState, 28, 110);
            }
        }
    }

    private bool HasAnySeedInInventory()
    {
        if (Player.main == null || Player.main.inventory == null) return false;
        foreach (KeyValuePair<Item, int> kv in Player.main.inventory.items)
        {
            if (kv.Key is SeedItem && kv.Value > 0) return true;
        }
        return false;
    }

    private List<SeedItem> GetAvailableSeeds()
    {
        List<SeedItem> seeds = new List<SeedItem>();
        if (Player.main == null || Player.main.inventory == null) return seeds;

        foreach (KeyValuePair<Item, int> item in Player.main.inventory.items)
        {
            if (item.Key is SeedItem seed && item.Value > 0) seeds.Add(seed);
        }
        seeds.Sort((a, b) => string.Compare(a.itemName, b.itemName, StringComparison.Ordinal));
        return seeds;
    }

    private Button CreateTaskBtn(Transform parent, string label, WorkerManager.WorkerTask task, WorkerEntity worker, bool forceDisable = false)
    {
        bool isActive = worker.CurrentTask == task;
        FarmUITheme.ButtonState state;
        if (forceDisable) state = FarmUITheme.ButtonState.Locked;
        else if (isActive) state = FarmUITheme.ButtonState.ActiveSelected;
        else state = FarmUITheme.ButtonState.Purchasable;

        return CreateButton(parent, label, () =>
        {
            if (forceDisable) return;
            workerManager.SetTask(worker, task);
            Refresh();
        }, state, 30, 85);
    }

    private TMP_Text CreateLabel(Transform parent, string text, int fontSize, FontStyles fontStyle, Color color, float height, float width = 0)
    {
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        labelObj.transform.SetParent(parent, false);
        labelObj.layer = panel.layer;

        LayoutElement le = labelObj.GetComponent<LayoutElement>();
        le.preferredHeight = height;
        if (width > 0) le.preferredWidth = width;

        TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
        tmp.font = pixelFont;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        return tmp;
    }

    private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action, FarmUITheme.ButtonState state, float height, float width = 0)
    {
        GameObject btnObj = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        btnObj.transform.SetParent(parent, false);
        btnObj.layer = panel.layer;

        LayoutElement le = btnObj.GetComponent<LayoutElement>();
        le.preferredHeight = height;
        if (width > 0) le.preferredWidth = width;

        Button button = btnObj.GetComponent<Button>();
        button.onClick.AddListener(action);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        textObj.layer = panel.layer;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.font = pixelFont;
        text.text = label;
        text.fontSize = 17;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        FarmUITheme.ApplyButtonStyle(button, state, text);
        FarmUITheme.AddOutline(btnObj.GetComponent<Image>(), FarmUITheme.WoodBrown, new Vector2(1.5f, -1.5f));

        return button;
    }

    private string GetTaskName(WorkerManager.WorkerTask task)
    {
        switch (task)
        {
            case WorkerManager.WorkerTask.PlantOnly: return "PLANT";
            case WorkerManager.WorkerTask.HarvestOnly: return "HARVEST";
            case WorkerManager.WorkerTask.PlantAndHarvest: return "ALL";
            default: return "MANUAL";
        }
    }
}
