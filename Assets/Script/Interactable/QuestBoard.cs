using System.Collections;
using UnityEngine;

/// <summary>
/// In-world interactive Quest / Bulletin Board placed in the farm yard
/// between the campfire and Stella's shop.
/// Clicking or pressing 'Q' opens the QuestBoardUI modal.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class QuestBoard : Interactable
{
    public static QuestBoard Instance { get; private set; }

    [Header("Configuration")]
    public Vector3 defaultYardPosition = new Vector3(1.5f, 13.0f, 0f);
    public Color hoverOutlineColor = new Color(0.957f, 0.725f, 0.259f, 1.0f); // GoldReward

    private CursorMessage hoverMessage;
    private Transform bobbingIcon;
    private Vector3 initialIconPos;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneListener()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "Menu") return;
            EnsureQuestBoardInScene();
        };
    }

    public static void EnsureQuestBoardInScene()
    {
        QuestBoard existing = FindObjectOfType<QuestBoard>();
        if (existing == null)
        {
            GameObject boardObj = new GameObject("QuestBoard");
            boardObj.transform.position = new Vector3(1.5f, 13.0f, 0f);
            boardObj.AddComponent<QuestBoard>();
            Debug.Log("[QuestBoard] Auto-spawned QuestBoard in farm yard at (1.5, 13.0).");
        }
    }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        base.Awake();

        ConfigureComponents();
        CreateBobbingBadge();
    }

    private void ConfigureComponents()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Load sprite from Resources or fallback
        if (spriteRenderer.sprite == null)
        {
            Sprite s = Resources.Load<Sprite>("QuestBoard");
            if (s != null)
            {
                spriteRenderer.sprite = s;
            }
        }

        // Setup sorting layer to match farm ground and shop (Layer 2)
        spriteRenderer.sortingLayerName = "Ground";
        spriteRenderer.sortingOrder = 1;

        // Ensure collider covers the board
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.isTrigger = false;
            col.size = new Vector2(2.0f, 2.2f);
            col.offset = new Vector2(0f, 0.1f);
        }
    }

    private void CreateBobbingBadge()
    {
        // Cute floating notice indicator above the board
        GameObject badgeObj = new GameObject("NoticeBadge", typeof(SpriteRenderer));
        badgeObj.transform.SetParent(transform, false);
        badgeObj.transform.localPosition = new Vector3(0f, 1.45f, 0f);

        SpriteRenderer sr = badgeObj.GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Ground";
        sr.sortingOrder = 3;

        // Try load WheatSeed or item icon as small badge sprite, or generate simple marker
        Sprite badgeSprite = Resources.Load<Sprite>("Sprites/Item/Wheat/Wheat");
        if (badgeSprite != null)
        {
            sr.sprite = badgeSprite;
            badgeObj.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }

        bobbingIcon = badgeObj.transform;
        initialIconPos = bobbingIcon.localPosition;
    }

    private void OnEnable()
    {
        EventManager.StartListening<OnPlayerClick>(HandlePlayerClick);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnPlayerClick>(HandlePlayerClick);
    }

    private void Update()
    {
        // Gentle bobbing effect for the badge
        if (bobbingIcon != null)
        {
            float bob = Mathf.Sin(Time.time * 3.5f) * 0.08f;
            bobbingIcon.localPosition = initialIconPos + new Vector3(0f, bob, 0f);
        }
    }

    protected override void OnMouseEnter()
    {
        SetColor(hoverOutlineColor);

        hoverMessage = new CursorMessage("Bảng Nhiệm Vụ", "Nhấp chuột hoặc nhấn phím [Q] để xem mục tiêu & tiến độ");
        EventManager.TriggerEvent(new OnCursorMessageRequest(hoverMessage, true));

        base.OnMouseEnter();
    }

    protected override void OnMouseExit()
    {
        if (hoverMessage != null)
        {
            EventManager.TriggerEvent(new OnCursorMessageRequest(hoverMessage, false));
            hoverMessage = null;
        }

        base.OnMouseExit();
    }

    private void OnMouseDown()
    {
        // Direct Unity click event
        Interact();
    }

    private void HandlePlayerClick(EventParam param)
    {
        OnPlayerClick clickParam = param as OnPlayerClick;
        if (clickParam == null || !clickParam.player.Equals(Player.main)) return;
        if (clickParam.IsEventCanceled()) return;
        if (!isInteractable) return;

        clickParam.CancelEvent();
        Interact();
    }

    public void Interact()
    {
        QuestBoardUI.Instance?.Toggle();
    }
}
