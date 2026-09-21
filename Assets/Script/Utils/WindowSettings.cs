using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Quản lý kích thước cửa sổ, độ phân giải và chế độ toàn màn hình cho game.
/// Tự động khởi tạo khi game bắt đầu chạy, hỗ trợ phím tắt, lưu thiết lập người dùng,
/// và bật tính năng kéo viền cửa sổ (Drag-to-Resize) trên macOS.
/// </summary>
public static class WindowSettings
{
    private const string PREF_FULLSCREEN = "FarmFortune_IsFullscreen";
    private const string PREF_WIDTH = "FarmFortune_WindowWidth";
    private const string PREF_HEIGHT = "FarmFortune_WindowHeight";

    public const int DEFAULT_WIDTH = 1280;
    public const int DEFAULT_HEIGHT = 720;

    public static bool IsFullscreen => Screen.fullScreen;
    public static int CurrentWidth => Screen.width;
    public static int CurrentHeight => Screen.height;

#if UNITY_STANDALONE_OSX
    [DllImport("MacWindowPlugin")]
    private static extern void MakeWindowResizable();

    [DllImport("MacWindowPlugin")]
    private static extern void SetWindowDimensions(int width, int height);
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeWindow()
    {
#if !UNITY_EDITOR
        bool isFullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 0) == 1;
        int savedWidth = PlayerPrefs.GetInt(PREF_WIDTH, DEFAULT_WIDTH);
        int savedHeight = PlayerPrefs.GetInt(PREF_HEIGHT, DEFAULT_HEIGHT);

        if (isFullscreen)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(savedWidth, savedHeight, FullScreenMode.Windowed);
        }
#endif

        EnsureControllerExists();
    }

    private static void EnsureControllerExists()
    {
        if (Object.FindObjectOfType<WindowController>() == null)
        {
            GameObject go = new GameObject("[WindowController]");
            go.AddComponent<WindowController>();
            Object.DontDestroyOnLoad(go);
        }
    }

    /// <summary>
    /// Kích hoạt cờ NSWindowStyleMaskResizable trên macOS để cho phép kéo viền cửa sổ phóng to/thu nhỏ.
    /// </summary>
    public static void EnableNativeWindowResizing()
    {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        try
        {
            MakeWindowResizable();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[WindowSettings] Cannot set native resize mask: " + ex.Message);
        }
#endif
    }

    /// <summary>
    /// Chuyển đổi giữa chế độ Toàn màn hình (Fullscreen) và Cửa sổ (Windowed).
    /// </summary>
    public static void ToggleFullscreen()
    {
        SetFullscreen(!Screen.fullScreen);
    }

    /// <summary>
    /// Bật hoặc tắt chế độ toàn màn hình.
    /// </summary>
    public static void SetFullscreen(bool fullscreen)
    {
        if (fullscreen)
        {
            PlayerPrefs.SetInt(PREF_FULLSCREEN, 1);
            PlayerPrefs.Save();
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            int w = PlayerPrefs.GetInt(PREF_WIDTH, DEFAULT_WIDTH);
            int h = PlayerPrefs.GetInt(PREF_HEIGHT, DEFAULT_HEIGHT);
            PlayerPrefs.SetInt(PREF_FULLSCREEN, 0);
            PlayerPrefs.Save();
            Screen.SetResolution(w, h, FullScreenMode.Windowed);
            EnableNativeWindowResizing();
        }
    }

    /// <summary>
    /// Đặt kích thước độ phân giải cửa sổ.
    /// </summary>
    public static void SetResolution(int width, int height, bool fullscreen = false)
    {
        PlayerPrefs.SetInt(PREF_WIDTH, width);
        PlayerPrefs.SetInt(PREF_HEIGHT, height);
        PlayerPrefs.SetInt(PREF_FULLSCREEN, fullscreen ? 1 : 0);
        PlayerPrefs.Save();

        FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(width, height, mode);

        if (!fullscreen)
        {
            EnableNativeWindowResizing();
        }
    }

    /// <summary>
    /// Lưu kích thước cửa sổ hiện tại khi có sự thay đổi.
    /// </summary>
    public static void SaveCurrentSize()
    {
        if (!Screen.fullScreen && Screen.width > 300 && Screen.height > 200)
        {
            PlayerPrefs.SetInt(PREF_WIDTH, Screen.width);
            PlayerPrefs.SetInt(PREF_HEIGHT, Screen.height);
            PlayerPrefs.SetInt(PREF_FULLSCREEN, 0);
            PlayerPrefs.Save();
        }
    }
}

/// <summary>
/// MonoBehaviour chạy nền để lắng nghe phím tắt, kích hoạt kéo viền macOS và bắt sự kiện thay đổi kích thước.
/// </summary>
public class WindowController : MonoBehaviour
{
    private int lastWidth;
    private int lastHeight;
    private bool lastFullscreen;

    private void Start()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        lastFullscreen = Screen.fullScreen;

        StartCoroutine(EnableResizeDelayed());
    }

    private IEnumerator EnableResizeDelayed()
    {
        yield return new WaitForSeconds(0.2f);
        WindowSettings.EnableNativeWindowResizing();
        yield return new WaitForSeconds(0.8f);
        WindowSettings.EnableNativeWindowResizing();
    }

    private void Update()
    {
        // Phím tắt phóng to toàn màn hình: F11 hoặc Cmd+F / Ctrl+F
        bool cmdOrCtrl = Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
                         Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (Input.GetKeyDown(KeyCode.F11) || (cmdOrCtrl && Input.GetKeyDown(KeyCode.F)))
        {
            WindowSettings.ToggleFullscreen();
        }

        // Phím tắt Alt+Enter hoặc Cmd+Enter
        bool altOrCmd = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || cmdOrCtrl;
        if (altOrCmd && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            WindowSettings.ToggleFullscreen();
        }

        // Phím tắt chọn kích thước nhanh khi giữ Cmd/Ctrl:
        // 1: 1280x720 (Cửa sổ chuẩn)
        // 2: 1600x900 (Cửa sổ vừa)
        // 3: 1920x1080 (Cửa sổ lớn Full HD)
        // 0: Toàn màn hình
        if (cmdOrCtrl)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                WindowSettings.SetResolution(1280, 720, false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                WindowSettings.SetResolution(1600, 900, false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                WindowSettings.SetResolution(1920, 1080, false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                WindowSettings.SetFullscreen(true);
            }
        }

        // Theo dõi thay đổi kích thước cửa sổ (ví dụ khi kéo mép hoặc bấm nút xanh trên macOS)
        if (Screen.width != lastWidth || Screen.height != lastHeight || Screen.fullScreen != lastFullscreen)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            lastFullscreen = Screen.fullScreen;
            WindowSettings.SaveCurrentSize();

            if (!Screen.fullScreen)
            {
                WindowSettings.EnableNativeWindowResizing();
            }
        }
    }
}
