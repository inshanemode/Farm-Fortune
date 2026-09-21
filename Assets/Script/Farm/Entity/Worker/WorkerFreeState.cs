public class WorkerFreeState : BaseState
{
    private WorkerEntity entity;
    public WorkerFreeState(WorkerEntity entity) : base("WorkerFreeState")
    {
        this.entity = entity;
    }

    public override void Enter()
    {
        base.Enter();
        entity.target = GameplayManager.Instance.GetRandomSpawn();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (!entity.isMoving)
        {
            entity.LookAt(GameplayManager.Instance.GetRandomSpawn(false));
        }
        if (entity.owner == null) return;
        if (entity.CurrentTask == WorkerManager.WorkerTask.Manual) return;

        Land targetLand = null;
        Land witheringLand = null;
        Land readyHarvestLand = null;
        Land emptyPlantLand = null;

        SeedItem selectedSeed = entity.SelectedSeed;
        bool playerHasSeed = selectedSeed != null && entity.owner.inventory.GetAmount(selectedSeed) > 0;

        for (int i = entity.owner.landManager.availableLands.Count - 1; i >= 0; i--)
        {
            Land land = entity.owner.landManager.availableLands[i];
            if (land.currentWorker != null) continue;

            if (entity.CanPlant && land.IsLandEmpty() && playerHasSeed && emptyPlantLand == null)
            {
                emptyPlantLand = land;
            }

            if (entity.CanHarvest && (land.ReadyToHarvest() || land.IsLandDecompose()))
            {
                if (land.IsWithering())
                {
                    witheringLand = land; // Top priority!
                    break;
                }
                else if (readyHarvestLand == null)
                {
                    readyHarvestLand = land;
                }
            }
        }

        // Priority order: Withering crops > regular ready crops > empty planting land
        if (witheringLand != null) targetLand = witheringLand;
        else if (readyHarvestLand != null) targetLand = readyHarvestLand;
        else if (emptyPlantLand != null) targetLand = emptyPlantLand;

        if (targetLand == null) return;
        entity.currentLand = targetLand;
        entity.ChangeState("WorkerWorkState");
    }
}
