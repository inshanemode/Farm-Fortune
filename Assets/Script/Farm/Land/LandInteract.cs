using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandInteract : Interactable
{
    private Land land;

    private CursorMessage currentMessage;

    protected override void Awake()
    {
        base.Awake();
        land = GetComponent<Land>();
    }


    private void OnEnable()
    { 
    }

    private void OnDisable()
    {
        ClearMessage();
    }

    protected override void OnMouseEnter()
    {
        string title = null, description = null;
        if (land == null) return;

        Land.CropVisualState cropState = land.GetCropState();
        SetColor(land.GetCropStateColor());

        switch (cropState)
        {
            case Land.CropVisualState.Empty:
                title = "Empty Soil";
                description = "Click a seed to plant";
                break;

            case Land.CropVisualState.Growing:
                title = $"{land.entityData.harvestItem.itemName} (Growing)";
                description = $"Grow Progress: {(land.GetProgress() * 100f):0}%\n" +
                    $"Time: {GetTime((int)land.liveTime)} / {GetTime(land.entityData.timeToHarvest)}\n" +
                    $"Harvest Yield: {land.entityData.harvestItemAmount}\n" +
                    $"Harvests Left: {GetHarvestsLeft()} / {land.entityData.totalHarvest}\nRight-click to remove";
                break;

            case Land.CropVisualState.Ready:
                title = $"<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.GoldReward)}>Ready to Harvest: {land.entityData.harvestItem.itemName}</color>";
                description = "Click to harvest!\nRight-click to remove\n" +
                    $"Harvests Left: {GetHarvestsLeft()} / {land.entityData.totalHarvest}\n" +
                    $"Time before withering: {GetTime(Mathf.Max(0, (int)(land.entityData.timeToDecompose * 0.5f - (land.liveTime - land.entityData.timeToHarvest))))}";
                break;

            case Land.CropVisualState.Withering:
                title = $"<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.RedWarning)}>[!] WITHERING: {land.entityData.harvestItem.itemName}</color>";
                description = "<color=#B94A35>Harvest immediately before it decomposes!</color>\n" +
                    $"Decomposes in: {GetTime(Mathf.Max(0, land.entityData.timeToDecompose - (int)(land.liveTime - land.entityData.timeToHarvest)))}\n" +
                    $"Harvests Left: {GetHarvestsLeft()} / {land.entityData.totalHarvest}";
                break;

            case Land.CropVisualState.Dead:
                title = $"<color=#{ColorUtility.ToHtmlStringRGB(FarmUITheme.GrayLocked)}>Dead Crop</color>";
                description = "Crop has decayed. Click to clear soil.";
                break;
        }

        if (title == null || description == null) return;
        currentMessage = new CursorMessage(title, description);
        EventManager.TriggerEvent(new OnCursorMessageRequest(currentMessage, true));
        base.OnMouseEnter();
    }

    private void Update()
    {
        if (land != null && land.IsWithering())
        {
            // Show subtle pulsing warning border when withering
            spriteRenderer.material.SetFloat("_OutlineThickness", 1);
            SetColor(land.GetCropStateColor());
        }
        else if (!isInteractable)
        {
            spriteRenderer.material.SetFloat("_OutlineThickness", 0);
        }

        if (currentMessage != null)
        {
            OnMouseEnter();
        }
    }
    private string GetTime(int seconds)
    {
        return $"{(seconds / 60 > 0 ? (seconds / 60).ToString() + "m " : "")}{(seconds % 60 > 0 ? (seconds % 60).ToString() + "s " : "")}";
    }

    private int GetHarvestsLeft()
    {
        if (land == null || land.entityData == null) return 0;
        return Mathf.Max(0, land.entityData.totalHarvest - (int)land.harvestTime);
    }
    protected override void OnMouseExit()
    {
        ClearMessage();
        base.OnMouseExit();
    }

    private void ClearMessage()
    {
        if (currentMessage != null)
        {
            EventManager.TriggerEvent(new OnCursorMessageRequest(currentMessage, false));
            currentMessage = null;
        }
    }
}
