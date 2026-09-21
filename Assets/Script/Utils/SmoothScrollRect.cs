using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Cung cấp trải nghiệm cuộn danh sách mượt mà (smooth scrolling) cho Unity ScrollRect.
/// Hỗ trợ mượt cả trên chuột cuộn (mouse wheel) và trackpad macOS, có quán tính mềm mại và nảy nhẹ ở 2 đầu.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class SmoothScrollRect : MonoBehaviour, IScrollHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Scroll Dynamics")]
    [Tooltip("Hệ số gia tốc cuộn cho con lăn chuột rời")]
    public float mouseWheelSpeed = 950f;

    [Tooltip("Hệ số gia tốc cuộn cho trackpad macOS / touchpad")]
    public float trackpadSpeed = 380f;

    [Tooltip("Vận tốc cuộn tối đa")]
    public float maxVelocity = 4000f;

    [Tooltip("Tỷ lệ giảm tốc quán tính (càng nhỏ trượt càng lâu, mặc định 0.07 là mượt nhất)")]
    public float customDecelerationRate = 0.065f;

    [Tooltip("Độ co giãn đàn hồi khi chạm đỉnh hoặc đáy")]
    public float customElasticity = 0.12f;

    private ScrollRect scrollRect;
    private bool isDragging;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (scrollRect != null)
        {
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = customElasticity;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = customDecelerationRate;

            // Vô hiệu hóa bước nhảy giật mặc định của ScrollRect để SmoothScrollRect toàn quyền xử lý
            scrollRect.scrollSensitivity = 0f;
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (scrollRect == null || isDragging) return;

        float deltaY = eventData.scrollDelta.y;
        if (Mathf.Abs(deltaY) < 0.0001f) return;

        // Phân biệt chuột lăn bước lớn (thường là số nguyên +/-1, +/-2) và trackpad macOS (giá trị thực nhỏ)
        bool isDiscreteWheel = Mathf.Abs(deltaY) >= 0.85f;
        float speed = isDiscreteWheel ? mouseWheelSpeed : trackpadSpeed;

        // Khi cuộn xuống (deltaY < 0), content cần di chuyển lên trên (velocity.y > 0)
        float impulse = -deltaY * speed;

        Vector2 currentVel = scrollRect.velocity;
        currentVel.y += impulse;
        currentVel.y = Mathf.Clamp(currentVel.y, -maxVelocity, maxVelocity);

        // Áp dụng gia tốc vào ScrollRect để hệ thống physics quán tính tự trượt êm dịu
        scrollRect.velocity = currentVel;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void Update()
    {
        // Hỗ trợ phím mũi tên Lên/Xuống cuộn mượt khi hover
        if (!isDragging && scrollRect != null && scrollRect.vertical)
        {
            float arrowInput = 0f;
            if (Input.GetKey(KeyCode.DownArrow)) arrowInput -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) arrowInput += 1f;

            if (Mathf.Abs(arrowInput) > 0.01f)
            {
                Vector2 currentVel = scrollRect.velocity;
                currentVel.y += -arrowInput * (mouseWheelSpeed * 0.4f);
                currentVel.y = Mathf.Clamp(currentVel.y, -maxVelocity, maxVelocity);
                scrollRect.velocity = currentVel;
            }
        }
    }

    /// <summary>
    /// Tạo thanh cuộn thẩm mỹ (Cozy Scrollbar) theo chuẩn màu FarmUITheme nếu chưa có.
    /// </summary>
    public Scrollbar AttachStyledScrollbar()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null || scrollRect.verticalScrollbar != null) return scrollRect?.verticalScrollbar;

        RectTransform viewRect = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

        // Container cho Scrollbar
        GameObject barObj = new GameObject("VerticalScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        barObj.transform.SetParent(transform, false);
        barObj.layer = gameObject.layer;

        RectTransform barRt = barObj.GetComponent<RectTransform>();
        barRt.anchorMin = new Vector2(1f, 0f);
        barRt.anchorMax = new Vector2(1f, 1f);
        barRt.pivot = new Vector2(1f, 0.5f);
        barRt.sizeDelta = new Vector2(12f, -40f);
        barRt.anchoredPosition = new Vector2(-4f, 0f);

        // Nền thanh cuộn (Track)
        Image trackImg = barObj.GetComponent<Image>();
        trackImg.color = new Color(FarmUITheme.InkBrown.r, FarmUITheme.InkBrown.g, FarmUITheme.InkBrown.b, 0.45f);
        FarmUITheme.AddOutline(trackImg, FarmUITheme.WoodBrown, new Vector2(1f, -1f));

        // Vùng trượt (Sliding Area)
        GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
        slidingArea.transform.SetParent(barObj.transform, false);
        slidingArea.layer = gameObject.layer;
        RectTransform slidingRt = slidingArea.GetComponent<RectTransform>();
        slidingRt.anchorMin = Vector2.zero;
        slidingRt.anchorMax = Vector2.one;
        slidingRt.offsetMin = new Vector2(2f, 4f);
        slidingRt.offsetMax = new Vector2(-2f, -4f);

        // Con trượt (Handle)
        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(slidingArea.transform, false);
        handleObj.layer = gameObject.layer;
        RectTransform handleRt = handleObj.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(0f, 0f);

        Image handleImg = handleObj.GetComponent<Image>();
        handleImg.color = FarmUITheme.PanelCream;
        FarmUITheme.AddOutline(handleImg, FarmUITheme.WoodBrown, new Vector2(1f, -1f));

        // Cấu hình Scrollbar component
        Scrollbar scrollbar = barObj.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRt;
        scrollbar.targetGraphic = handleImg;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        ColorBlock cb = scrollbar.colors;
        cb.normalColor = FarmUITheme.PanelCream;
        cb.highlightedColor = FarmUITheme.GoldReward;
        cb.pressedColor = FarmUITheme.GreenActive;
        scrollbar.colors = cb;

        // Gắn vào ScrollRect
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = 6f;

        return scrollbar;
    }
}
