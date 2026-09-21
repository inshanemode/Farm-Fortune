using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WorkerEntity : StateMachine
{
    [HideInInspector]
    public Player owner;
    [HideInInspector]
    public Land currentLand;
    [HideInInspector]
    public Vector2 target;
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public Animator animator;

    [Header("Stats")]
    public float speed = 1f;
    public float workTime = 120f;

    [Header("Task")]
    [SerializeField] private WorkerManager.WorkerTask currentTask = WorkerManager.WorkerTask.Manual;
    [SerializeField] private SeedItem selectedSeed;

    public WorkerManager.WorkerTask CurrentTask => currentTask;
    public SeedItem SelectedSeed => selectedSeed;
    public bool CanPlant => currentTask == WorkerManager.WorkerTask.PlantOnly || currentTask == WorkerManager.WorkerTask.PlantAndHarvest;
    public bool CanHarvest => currentTask == WorkerManager.WorkerTask.HarvestOnly || currentTask == WorkerManager.WorkerTask.PlantAndHarvest;

    public void SetTask(WorkerManager.WorkerTask task)
    {
        currentTask = task;
    }

    public void SetSeed(SeedItem seed)
    {
        selectedSeed = seed;
    }

    public void Init(Player owner)
    {
        this.owner = owner;
    }

    public string GetStatusText()
    {
        if (CurrentStateName == "WorkerWorkState") return "Working";
        if (currentTask == WorkerManager.WorkerTask.Manual) return "Idle";

        if (owner != null)
        {
            bool hasSeed = selectedSeed != null && owner.inventory != null && owner.inventory.GetAmount(selectedSeed) > 0;
            if (CanPlant && !hasSeed) return "No Seed";

            if (owner.landManager != null)
            {
                if (CanPlant)
                {
                    bool hasEmptyLand = false;
                    foreach (Land l in owner.landManager.availableLands)
                    {
                        if (l != null && l.IsLandEmpty()) { hasEmptyLand = true; break; }
                    }
                    if (!hasEmptyLand) return "No Empty Land";
                }

                if (CanHarvest)
                {
                    bool hasHarvestable = false;
                    foreach (Land l in owner.landManager.availableLands)
                    {
                        if (l != null && (l.ReadyToHarvest() || l.IsLandDecompose())) { hasHarvestable = true; break; }
                    }
                    if (!hasHarvestable) return "Waiting";
                }
            }
        }

        return "Idle";
    }

    public Color GetStatusColor()
    {
        string status = GetStatusText();
        switch (status)
        {
            case "Working": return FarmUITheme.GoldReward;
            case "Idle": return FarmUITheme.GreenActive;
            case "No Seed": return FarmUITheme.RedWarning;
            case "No Empty Land": return FarmUITheme.WoodBrown;
            case "Waiting": return FarmUITheme.WoodBrown;
            default: return FarmUITheme.TextDark;
        }
    }

    public float GetCurrentProgress()
    {
        if (CurrentStateName == "WorkerWorkState")
        {
            WorkerWorkState workState = GetState("WorkerWorkState") as WorkerWorkState;
            if (workState != null) return workState.Progress * 100f;
        }
        return 0f;
    }


    private void Awake()
    {
        target = transform.position;

        animator = GetComponent<Animator>();
        defaultState = "WorkerFreeState";
        stateList.Add(new WorkerFreeState(this));
        stateList.Add(new WorkerWorkState(this));

    }

    protected override void Update()
    {
        base.Update();
        
        Vector2 direction = target - (Vector2)transform.position;
        isMoving = direction.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);
        if (!isMoving) return;
        direction.Normalize();
        animator.SetFloat("horizontal", direction.x);
        animator.SetFloat("vertical", direction.y);

        transform.position = (Vector2)transform.position + direction * speed * Time.deltaTime;
    }
    
    public void LookAt(Vector2 target)
    {
        Vector2 direction = target - (Vector2)transform.position;
        direction.Normalize();
        animator.SetFloat("horizontal", direction.x);
        animator.SetFloat("vertical", direction.y);
    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
        target = pos;
    }

    public void ApplyUpgrades(float speedMultiplier, float efficiencyMultiplier)
    {
        speed = 1f * Mathf.Max(0.5f, speedMultiplier);
        workTime = 120f / Mathf.Max(0.5f, efficiencyMultiplier);
    }

    public void RestoreFromSave(WorkerSaveData data, Land assignedLand)
    {
        if (data == null) return;
        SetPosition(new Vector2(data.posX, data.posY));
        SetTask((WorkerManager.WorkerTask)data.currentTask);

        if (data.selectedSeedId >= 0)
        {
            SeedItem seed = ItemManager.GetItem(data.selectedSeedId) as SeedItem;
            SetSeed(seed);
        }
        else
        {
            SetSeed(null);
        }

        if (data.speed > 0) speed = data.speed;
        if (data.workTime > 0) workTime = data.workTime;

        if (assignedLand != null && data.stateName == "WorkerWorkState")
        {
            currentLand = assignedLand;
            ChangeState("WorkerWorkState");
        }
        else
        {
            ChangeState("WorkerFreeState");
        }
    }
}

