using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Quản lý hiển thị ô vật phẩm trong túi đồ và cửa hàng.
/// Hỗ trợ mượt mà: nhấp để mua/chọn, rê chuột xem tooltip, và kéo để cuộn danh sách (Drag-to-Scroll)
/// mà không bị mua nhầm khi đang vuốt cuộn màn hình.
/// </summary>
public class ItemRenderer : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IScrollHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public TMP_Text amountCount;
    public Image imageIcon;
    public Button button;

    private UnityAction onClick, onEnter, onExit;
    private ScrollRect parentScrollRect;

    private Vector2 pointerDownPosition;
    private float pointerDownTime;
    private bool isDragging;
    private const float DRAG_THRESHOLD = 10f;

    private void Awake()
    {
        Render((Item)null, 0);
        FindParentScrollRect();
        CleanEventTriggerConflict();
    }

    private void FindParentScrollRect()
    {
        if (parentScrollRect == null)
        {
            parentScrollRect = GetComponentInParent<ScrollRect>();
        }
    }

    /// <summary>
    /// Gỡ bỏ EventTrigger tĩnh cũ nếu có để tránh việc EventTrigger nuốt mất sự kiện kéo cuộn (Drag/Scroll).
    /// </summary>
    private void CleanEventTriggerConflict()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger != null)
        {
            Destroy(trigger);
        }
    }

    public void Render(Item item, int count, UnityAction onClick = null, UnityAction onEnter = null, UnityAction onExit = null)
    {
        Render(item ? item.itemIcon : null, count, onClick, onEnter, onExit);
    }

    public void Render(Sprite icon, int count, UnityAction onClick = null, UnityAction onEnter = null, UnityAction onExit = null)
    {
        if (icon == null || count <= 0)
        {
            if (amountCount != null) amountCount.text = "";
            if (imageIcon != null) imageIcon.gameObject.SetActive(false);
            this.onClick = null;
            this.onEnter = null;
            this.onExit = null;
            if (button != null) button.interactable = false;
            return;
        }

        if (amountCount != null) amountCount.text = NumberExtensions.FormatNumber(count, 0);
        if (imageIcon != null)
        {
            imageIcon.sprite = icon;
            imageIcon.gameObject.SetActive(true);
        }

        this.onClick = onClick;
        this.onEnter = onEnter;
        this.onExit = onExit;
        if (button != null) button.interactable = true;
    }

    public void RenderShopItem(Sprite icon, string priceText, FarmUITheme.ButtonState state, Color priceColor, UnityAction onClick = null, UnityAction onEnter = null, UnityAction onExit = null)
    {
        if (imageIcon != null)
        {
            imageIcon.sprite = icon;
            imageIcon.gameObject.SetActive(icon != null);
        }

        if (amountCount != null)
        {
            amountCount.text = priceText;
            amountCount.color = priceColor;
        }

        if (button == null) button = GetComponent<Button>();
        if (button != null)
        {
            FarmUITheme.ApplyButtonStyle(button, state, amountCount);
        }

        this.onClick = onClick;
        this.onEnter = onEnter;
        this.onExit = onExit;

        FindParentScrollRect();
        CleanEventTriggerConflict();
    }

    // ==========================================
    // Xử lý Sự kiện Kéo và Cuộn (Drag & Scroll)
    // ==========================================

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        pointerDownTime = Time.unscaledTime;
        isDragging = false;
        FindParentScrollRect();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Kiểm tra xem người dùng có di chuyển quá ngưỡng kéo hay không
        if (Vector2.Distance(pointerDownPosition, eventData.position) > DRAG_THRESHOLD)
        {
            isDragging = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Nếu người chơi đang kéo cuộn danh sách, KHÔNG kích hoạt mua hàng
        if (isDragging) return;
        if (Vector2.Distance(pointerDownPosition, eventData.position) > DRAG_THRESHOLD) return;
        if (Time.unscaledTime - pointerDownTime > 0.6f) return; // Giữ quá lâu cũng coi là cử chỉ giữ/kéo

        onClick?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        if (parentScrollRect != null)
        {
            parentScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(pointerDownPosition, eventData.position) > DRAG_THRESHOLD)
        {
            isDragging = true;
        }

        if (parentScrollRect != null)
        {
            parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
        }

        StartCoroutine(ResetDragRoutine());
    }

    private IEnumerator ResetDragRoutine()
    {
        yield return new WaitForEndOfFrame();
        isDragging = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnScroll(eventData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit?.Invoke();
    }

    // ==========================================
    // Tương thích ngược với các hàm gọi cũ
    // ==========================================

    public void OnPointerDown()
    {
        pointerDownPosition = Input.mousePosition;
        pointerDownTime = Time.unscaledTime;
        isDragging = false;
    }

    public void OnPointerEnter()
    {
        onEnter?.Invoke();
    }

    public void OnPointerExit()
    {
        onExit?.Invoke();
    }
}
