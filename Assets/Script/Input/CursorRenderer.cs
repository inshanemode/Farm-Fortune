using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorRenderer : MonoBehaviour
{
    public static CursorRenderer Instance;

    [Header("Cursor Icon")]
    public Sprite normalCursor;
    public Sprite pressCursor;
    public Image cursorSprite;

    [Header("Item")]
    public Image itemSprite;

    [Header("Cursor Message")]
    public GameObject cursorMessage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    
    private CursorMessage currentMessage;
    private int messageIndex = 0;

    private Player targetPlayer;

    private Camera uiCamera;

    void Start()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Use the native macOS/Unity cursor instead of drawing a second cursor in-game.
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
        if (cursorSprite != null)
        {
            cursorSprite.gameObject.SetActive(false);
            cursorSprite.raycastTarget = false;
        }
        if (itemSprite != null)
        {
            itemSprite.raycastTarget = false;
        }

        uiCamera = Camera.main;
        targetPlayer = Player.main;
    }

    private void OnEnable()
    {
        EventManager.StartListening<OnItemPickUp>(OnItemPickUp);
        EventManager.StartListening<OnItemDrop>(OnItemDrop);
        EventManager.StartListening<OnCursorMessageRequest>(OnCursorMessageRequest);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnItemPickUp>(OnItemPickUp);
        EventManager.StopListening<OnItemDrop>(OnItemDrop);
        EventManager.StopListening<OnCursorMessageRequest>(OnCursorMessageRequest);
    }

    private void OnItemPickUp(EventParam param)
    {
        OnItemPickUp eventParam = param as OnItemPickUp;
        if (eventParam == null || eventParam.player != targetPlayer || itemSprite == null) return;
        itemSprite.gameObject.SetActive(true);
        if (cursorSprite != null) cursorSprite.gameObject.SetActive(false);
        itemSprite.sprite = eventParam.item.itemIcon;
    }

    private void OnItemDrop(EventParam param)
    {
        OnItemDrop eventParam = param as OnItemDrop;
        if (eventParam == null || eventParam.player != targetPlayer || itemSprite == null) return;
        itemSprite.gameObject.SetActive(false);
    }

    private void OnCursorMessageRequest(EventParam param)
    {
        OnCursorMessageRequest eventParam = param as OnCursorMessageRequest;
        if (eventParam.show)
        {
            if (messageIndex > eventParam.message.priority) return;
            currentMessage = eventParam.message;
            cursorMessage.SetActive(true);

            titleText.text = eventParam.message.title;
            descriptionText.text = eventParam.message.description; 
            descriptionText.gameObject.SetActive(descriptionText.text.Length > 0);
        }
        else if(eventParam.message.Equals(currentMessage))
        {
            currentMessage = null;
            cursorMessage.SetActive(false);
        }
    }

    public void SetMessageIndex(int index)
    {
        messageIndex = index;
        if (currentMessage == null || currentMessage.priority >= index) return;
        currentMessage = null;
        cursorMessage.SetActive(false);
    }

    void Update()
    {
        // Keep the native system cursor visible at all times.
        Cursor.visible = true;

        if (uiCamera == null) uiCamera = Camera.main;
        if (uiCamera == null) return;

        Vector3 pos = Input.mousePosition;
        pos.z = uiCamera.transform.position.z;
        transform.position = pos;

        // The root still follows the mouse so tooltip messages and dragged items
        // remain positioned correctly, while the custom cursor icon stays hidden.
    }
}
