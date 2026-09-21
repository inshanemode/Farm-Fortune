using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý giao diện cài đặt trong game:
/// - Nút Bánh răng ⚙️ ở góc trên bên phải để mở/đóng popup Cài đặt.
/// - Popup Cài đặt (mặc định ẩn): gồm tùy chọn màn hình, về menu chính và nút Đóng.
/// - Không che chắn hoặc chặn tương tác nông trại khi chưa mở.
/// </summary>
public class SettingRenderer : MonoBehaviour
{
    public Button mainMenuButton; // Nút bánh răng ⚙️

    private GameObject modalRoot;
    private TMP_Text fullscreenLabel;
    private TMP_Text statusLabel;
    private TMP_FontAsset pixelFont;

    private void Start()
    {
        FindFontAsset();
        ConfigureGearButton();
        BuildSettingsModal();
    }

    private void FindFontAsset()
    {
        TMP_Text existing = GetComponentInChildren<TMP_Text>();
        if (existing != null)
        {
            pixelFont = existing.font;
        }
    }

    private void ConfigureGearButton()
    {
        if (mainMenuButton == null) return;

        // Đặt nút bánh răng gọn gàng bên cạnh nút DEBUG
        RectTransform rt = mainMenuButton.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            // Keep the gear beside the DEBUG control while making it easy to click.
            rt.anchoredPosition = new Vector2(-105f, -10f);
            rt.sizeDelta = new Vector2(60f, 60f);
        }

        FarmUITheme.ApplyButtonStyle(mainMenuButton, FarmUITheme.ButtonState.Purchasable);

        // Đổi chức năng nút bánh răng thành Mở / Đóng popup cài đặt
        mainMenuButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.AddListener(ToggleModal);
    }

    private void BuildSettingsModal()
    {
        if (modalRoot != null) return;

        // Root Overlay che toàn màn hình khi mở để chặn click nhầm vào nông trại
        modalRoot = new GameObject("SettingsModalRoot");
        modalRoot.transform.SetParent(transform, false);

        RectTransform rootRt = modalRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        Image overlayImg = modalRoot.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.45f);

        // The dimmer is also a safe, obvious way to close the modal.
        Button overlayButton = modalRoot.AddComponent<Button>();
        overlayButton.transition = Selectable.Transition.None;
        overlayButton.onClick.AddListener(CloseModal);

        // Panel Hộp thoại Cài đặt ở giữa màn hình
        GameObject dialog = new GameObject("DialogBox");
        dialog.transform.SetParent(modalRoot.transform, false);

        RectTransform dialogRt = dialog.AddComponent<RectTransform>();
        dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRt.pivot = new Vector2(0.5f, 0.5f);
        dialogRt.sizeDelta = new Vector2(500f, 460f);
        dialogRt.anchoredPosition = Vector2.zero;

        Image dialogBg = dialog.AddComponent<Image>();
        FarmUITheme.StylePanel(dialogBg);

        // Nút X đóng nhanh ở góc trên phải hộp thoại
        GameObject closeXObj = CreateButton(dialog.transform, "btn_close_x", new Vector2(215f, 195f), new Vector2(42f, 42f), "X");
        Button closeXBtn = closeXObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(closeXBtn, FarmUITheme.ButtonState.Warning, closeXObj.GetComponentInChildren<TMP_Text>());
        closeXBtn.onClick.AddListener(CloseModal);

        // Tiêu đề CÀI ĐẶT
        CreateText(dialog.transform, "CÀI ĐẶT GAME", new Vector2(0f, 170f), 28f, true, FarmUITheme.InkBrown);

        // Nhóm Cài đặt màn hình
        CreateText(dialog.transform, "— CHẾ ĐỘ HIỂN THỊ —", new Vector2(0f, 115f), 18f, true, FarmUITheme.WoodBrown);

        // Nút Toàn màn hình / Cửa sổ
        GameObject fsBtnObj = CreateButton(dialog.transform, "btn_fs", new Vector2(0f, 70f), new Vector2(360f, 48f), "");
        Button fsBtn = fsBtnObj.GetComponent<Button>();
        fullscreenLabel = fsBtnObj.GetComponentInChildren<TMP_Text>();
        UpdateFullscreenButtonState();
        fsBtn.onClick.AddListener(() =>
        {
            WindowSettings.ToggleFullscreen();
            UpdateFullscreenButtonState();
            UpdateStatusText();
        });

        // 3 nút độ phân giải
        float btnW = 110f;
        float spacing = 125f;
        float startX = -spacing;
        float resY = 10f;

        CreateResButton(dialog.transform, "1280x720", 1280, 720, new Vector2(startX, resY), new Vector2(btnW, 40f));
        CreateResButton(dialog.transform, "1600x900", 1600, 900, new Vector2(startX + spacing, resY), new Vector2(btnW, 40f));
        CreateResButton(dialog.transform, "1920x1080", 1920, 1080, new Vector2(startX + spacing * 2, resY), new Vector2(btnW, 40f));

        // Dòng trạng thái hiện tại
        statusLabel = CreateText(dialog.transform, "", new Vector2(0f, -42f), 16f, false, FarmUITheme.WoodBrown);
        UpdateStatusText();

        // Chú thích phím tắt
        CreateText(dialog.transform, "Phím tắt: F11 hoặc Cmd+F (Toàn màn hình)\nCmd+1, Cmd+2, Cmd+3 (Đổi kích thước nhanh)", new Vector2(0f, -85f), 13f, false, new Color(0.35f, 0.20f, 0.12f, 0.85f));

        // Nút VỀ MENU CHÍNH
        GameObject menuBtnObj = CreateButton(dialog.transform, "btn_main_menu", new Vector2(0f, -145f), new Vector2(300f, 44f), "VỀ MENU CHÍNH");
        Button menuBtn = menuBtnObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(menuBtn, FarmUITheme.ButtonState.Warning, menuBtnObj.GetComponentInChildren<TMP_Text>());
        menuBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });

        // Nút ĐÓNG
        GameObject closeBtnObj = CreateButton(dialog.transform, "btn_close", new Vector2(0f, -200f), new Vector2(300f, 42f), "ĐÓNG");
        Button closeBtn = closeBtnObj.GetComponent<Button>();
        FarmUITheme.ApplyButtonStyle(closeBtn, FarmUITheme.ButtonState.ActiveSelected, closeBtnObj.GetComponentInChildren<TMP_Text>());
        closeBtn.onClick.AddListener(CloseModal);

        // QUAN TRỌNG: Mặc định khi vào game, bảng Cài đặt PHẢI ẨN để không chặn nông trại
        modalRoot.SetActive(false);
    }

    public void ToggleModal()
    {
        if (modalRoot == null) return;
        bool willBeActive = !modalRoot.activeSelf;
        modalRoot.SetActive(willBeActive);
        if (willBeActive)
        {
            UpdateFullscreenButtonState();
            UpdateStatusText();
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(false);
        }
    }

    private void Update()
    {
        // Nhấn Escape để đóng popup nếu đang mở
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (modalRoot != null && modalRoot.activeSelf)
            {
                CloseModal();
            }
        }
    }

    private void CreateResButton(Transform parent, string label, int width, int height, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = CreateButton(parent, $"res_{label}", pos, size, label);
        Button btn = btnObj.GetComponent<Button>();
        TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
        FarmUITheme.ApplyButtonStyle(btn, FarmUITheme.ButtonState.Purchasable, txt);

        btn.onClick.AddListener(() =>
        {
            WindowSettings.SetResolution(width, height, false);
            UpdateFullscreenButtonState();
            UpdateStatusText();
        });
    }

    private void UpdateFullscreenButtonState()
    {
        if (fullscreenLabel == null) return;
        bool isFs = Screen.fullScreen;
        fullscreenLabel.text = isFs ? "Chế độ: Toàn màn hình [BẬT]" : "Chế độ: Cửa sổ [BẬT TOÀN MÀN HÌNH]";
        Button btn = fullscreenLabel.GetComponentInParent<Button>();
        if (btn != null)
        {
            FarmUITheme.ApplyButtonStyle(btn, isFs ? FarmUITheme.ButtonState.ActiveSelected : FarmUITheme.ButtonState.Purchasable, fullscreenLabel);
        }
    }

    private void UpdateStatusText()
    {
        if (statusLabel == null) return;
        string mode = Screen.fullScreen ? "Toàn màn hình" : "Cửa sổ";
        statusLabel.text = $"Hiện tại: {Screen.width} x {Screen.height} ({mode})";
    }

    private GameObject CreateButton(Transform parent, string name, Vector2 pos, Vector2 size, string text)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.color = FarmUITheme.WoodBrown;
        FarmUITheme.AddOutline(img, FarmUITheme.InkBrown, new Vector2(1.5f, -1.5f));

        Button btn = btnObj.AddComponent<Button>();

        CreateText(btnObj.transform, text, Vector2.zero, 17f, true, FarmUITheme.TextLight);

        return btnObj;
    }

    private TMP_Text CreateText(Transform parent, string content, Vector2 pos, float fontSize, bool isBold, Color color)
    {
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(parent, false);

        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = pos;

        TMP_Text tmp = txtObj.AddComponent<TextMeshProUGUI>();
        if (pixelFont != null)
        {
            tmp.font = pixelFont;
        }
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        return tmp;
    }
}
