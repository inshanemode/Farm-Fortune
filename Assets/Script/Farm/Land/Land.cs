using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LandInteract))]
public class Land : StateMachine
{
    public IFarmEntity farmEntity;
    public FarmEntityData entityData;

    public float liveTime = 0;
    public float harvestTime = 0;

    public Player owner;
    public WorkerEntity currentWorker;

    private GameObject progressBarObj;
    private SpriteRenderer progressFillRenderer;
    private const float BAR_WIDTH = 2.0f;
    private const float BAR_HEIGHT = 0.22f;

    private void Awake()
    {
        defaultState = "LandFreeState";
        stateList.Add(new LandFreeState(this));
        stateList.Add(new LandGrowState(this));
        stateList.Add(new LandDecomposeState(this));

        CreateProgressBar();
    }

    private void CreateProgressBar()
    {
        progressBarObj = new GameObject("GrowthProgressBar");
        progressBarObj.transform.SetParent(transform, false);
        progressBarObj.transform.localPosition = new Vector3(0, -2.0f, -0.5f);

        Sprite whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Background
        GameObject bgObj = new GameObject("Bg");
        bgObj.transform.SetParent(progressBarObj.transform, false);
        SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whiteSprite;
        bgRenderer.color = new Color(FarmUITheme.InkBrown.r, FarmUITheme.InkBrown.g, FarmUITheme.InkBrown.b, 0.85f);
        bgRenderer.sortingOrder = 20;
        bgObj.transform.localScale = new Vector3(BAR_WIDTH + 0.1f, BAR_HEIGHT + 0.08f, 1f);

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(progressBarObj.transform, false);
        progressFillRenderer = fillObj.AddComponent<SpriteRenderer>();
        progressFillRenderer.sprite = whiteSprite;
        progressFillRenderer.color = FarmUITheme.GreenActive;
        progressFillRenderer.sortingOrder = 21;
        fillObj.transform.localPosition = new Vector3(-BAR_WIDTH * 0.5f, 0, 0);
        // Pivot at left edge
        fillObj.GetComponent<SpriteRenderer>().sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        fillObj.transform.localScale = new Vector3(0, BAR_HEIGHT, 1f);

        progressBarObj.SetActive(false);
    }

    private void UpdateProgressBar()
    {
        if (progressBarObj == null) return;

        if (IsLandEmpty() || entityData == null)
        {
            progressBarObj.SetActive(false);
            return;
        }

        progressBarObj.SetActive(true);
        float progress = Mathf.Clamp01(GetProgress());

        Transform fillTransform = progressFillRenderer.transform;
        fillTransform.localScale = new Vector3(progress * BAR_WIDTH, BAR_HEIGHT, 1f);
        progressFillRenderer.color = GetCropStateColor();
    }

    protected override void Update()
    {
        base.Update();
        UpdateProgressBar();
    }

    public void Init(Player owner)
    {
        this.owner = owner;
    }

    public bool IsLandEmpty()
    {
        return IsStateName("LandFreeState");
    }

    public bool ReadyToHarvest()
    {
        return (IsStateName("LandGrowState") && GetProgress() >= 1);
    }
    
    public bool IsLandDecompose()
    {
        return IsStateName("LandDecomposeState");
    }

    public float GetProgress()
    {
        return (float)liveTime / entityData.timeToHarvest;
    }

    public void Reset()
    {
        foreach (Transform child in transform)
        {
            if (progressBarObj != null && child.gameObject == progressBarObj) continue;
            Destroy(child.gameObject);
        }
        farmEntity = null;
        entityData = null;
    }

    public void GrowPlant(FarmEntityData data)
    {
        farmEntity = data.Spawn(this).GetComponent<IFarmEntity>();
        entityData = data;
        ChangeState("LandGrowState");
        TutorialManager.Instance?.OnCropPlanted();
    }

    public enum CropVisualState
    {
        Empty,
        Growing,
        Ready,
        Withering,
        Dead
    }

    public CropVisualState GetCropState()
    {
        if (IsLandEmpty() || entityData == null) return CropVisualState.Empty;
        if (IsLandDecompose()) return CropVisualState.Dead;
        if (GetProgress() < 1f) return CropVisualState.Growing;

        float overdue = liveTime - entityData.timeToHarvest;
        if (overdue >= entityData.timeToDecompose * 0.5f) return CropVisualState.Withering;
        return CropVisualState.Ready;
    }

    public bool IsWithering()
    {
        return GetCropState() == CropVisualState.Withering;
    }

    public Color GetCropStateColor()
    {
        switch (GetCropState())
        {
            case CropVisualState.Growing: return FarmUITheme.GreenActive;
            case CropVisualState.Ready: return FarmUITheme.GoldReward;
            case CropVisualState.Withering:
                float t = Mathf.PingPong(Time.time * 3f, 1f);
                return Color.Lerp(FarmUITheme.OrangeWithering, FarmUITheme.RedWarning, t);
            case CropVisualState.Dead: return FarmUITheme.GrayLocked;
            default: return FarmUITheme.InkBrown;
        }
    }
}

