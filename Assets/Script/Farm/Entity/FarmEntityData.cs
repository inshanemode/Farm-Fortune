using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmEntityData : ScriptableObject
{
    public GameObject entityPrefab;
    
    public Item harvestItem;
    public int harvestItemAmount = 1;

    [Header("Growth settings")]
    [Tooltip("Seconds needed before each harvest.")]
    public int timeToHarvest;

    [Tooltip("How many times this crop can be harvested from one seed before the land becomes empty.")]
    [Min(1)]
    public int totalHarvest = 5;

    [Tooltip("Seconds the crop can stay ready before it withers.")]
    public int timeToDecompose = 3600;
    public virtual GameObject Spawn(Land land)
    {
        GameObject newEntityPrefab = Instantiate(entityPrefab, land.transform.position, Quaternion.identity, land.transform);
        return newEntityPrefab;
    }
}
