using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    public enum WorkerTask
    {
        Manual,
        PlantOnly,
        HarvestOnly,
        PlantAndHarvest
    }

    public GameObject workerPrefab;

    private List<WorkerEntity> workerList;
    private Player targetPlayer;
    public IReadOnlyList<WorkerEntity> Workers => workerList;

    private void Awake()
    {
        workerList = new List<WorkerEntity>();
        targetPlayer = GetComponent<Player>();
    }

    public void SetTask(WorkerEntity worker, WorkerTask task)
    {
        if (worker == null || !workerList.Contains(worker)) return;
        worker.SetTask(task);
    }

    public void SetSeed(WorkerEntity worker, SeedItem seed)
    {
        if (worker == null || !workerList.Contains(worker)) return;
        worker.SetSeed(seed);
    }

    private void OnEnable()
    {
        EventManager.StartListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
    }

    private void OnPlayerStatUpdate(EventParam param)
    {
        OnPlayerStatUpdate eventParam = param as OnPlayerStatUpdate; 
        if (!eventParam.player.Equals(targetPlayer)) return;
        CreateWorker(eventParam.player.totalWorker - workerList.Count);
    }

    public void CreateWorker(int totalWorker)
    {
        int prevLand = workerList.Count;
        if (totalWorker > 0)
        {
            //Adding land
            for (int i = 0; i < totalWorker; i++)
            {
                WorkerEntity newWorker = Instantiate(workerPrefab, GameplayManager.Instance.GetRandomSpawn(), Quaternion.identity, null).GetComponent<WorkerEntity>();
                newWorker.Init(targetPlayer);
                workerList.Add(newWorker);
            }
        }
        else
        {
            //Remove land
            for (int i = 0; i < Mathf.Abs(totalWorker); i++)
            {
                if (workerList.Count == 0) break;
                int index = workerList.Count - 1 - i;
                WorkerEntity land = workerList[index];
                workerList.RemoveAt(index);
                Destroy(land.gameObject);
            }
        }
    }

}
