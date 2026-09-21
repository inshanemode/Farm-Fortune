using UnityEngine;
using UnityEngine.EventSystems;
class PickItemState : BaseState
{
    private UserInput input;
    private Camera camera;
    private bool waitingForInitialRelease;
    private bool isDragging;
    private Vector2 dragStartPosition;
    public PickItemState(UserInput input) : base("PickItemState")
    {
        this.input = input;
        this.camera = Camera.main;
        modules.Add(new CameraControlModule(input));
    }
    public override void Enter()
    {
        base.Enter();
        waitingForInitialRelease = Input.GetMouseButton(0);
        isDragging = false;
        dragStartPosition = Input.mousePosition;
        EventManager.StartListening<OnInventoryUpdate>(OnInventoryUpdate);
    }

    public override void Exit()
    {
        base.Exit();
        input.holdItem = null;
        EventManager.StopListening<OnInventoryUpdate>(OnInventoryUpdate);
    }

    private void OnInventoryUpdate(EventParam param)
    {
        if (input.holdItem == null) return;
        OnInventoryUpdate eventParam = param as OnInventoryUpdate;
        if (eventParam.player.Equals(input.targetPlayer))
        {
            //Check for main player
            if (!eventParam.updatedInventory.TryGetValue(input.holdItem, out int value) || value == 0)
            {
                OnItemDrop dropEvent = new OnItemDrop(null, input.targetPlayer, camera.ScreenToWorldPoint(Input.mousePosition));
                EventManager.TriggerEvent(dropEvent);
                input.ResetState();
                return;
            }
        }

    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (input.holdItem == null) input.ResetState();

        if (waitingForInitialRelease)
        {
            if (Input.GetMouseButton(0) && Vector2.Distance(dragStartPosition, Input.mousePosition) > 8f)
            {
                waitingForInitialRelease = false;
                isDragging = true;
            }
            else if (!Input.GetMouseButton(0))
            {
                waitingForInitialRelease = false;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            TryUseSelectedItem();
            dragStartPosition = Input.mousePosition;
            isDragging = false;
        }

        if (Input.GetMouseButton(0) && Vector2.Distance(dragStartPosition, Input.mousePosition) > 8f)
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            DropItem();
        }
    }

    private void TryUseSelectedItem()
    {
        Vector2 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        if (input.holdItem is SeedItem)
        {
            Land land = LandManager.GetLand(mousePos, Global.LAND_SIZE / 2 * 0.6f);
            if (land != null && land.IsLandEmpty())
            {
                DropItemAt(mousePos);
                return;
            }
        }

        foreach (Collider2D collider in Physics2D.OverlapPointAll(mousePos))
        {
            if (collider.GetComponent<Shop>() == null) continue;
            DropItemAt(mousePos);
            return;
        }
    }

    private void DropItemAt(Vector2 position)
    {
        OnItemDrop dropEvent = new OnItemDrop(input.holdItem, input.targetPlayer, position);
        EventManager.TriggerEvent(dropEvent);
    }

    private void DropItem()
    {
        DropItemAt(camera.ScreenToWorldPoint(Input.mousePosition));
    }
}
