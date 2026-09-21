using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Foundation for factory and production building management and save/load state.
/// </summary>
public class ProductionManager : MonoBehaviour
{
    public static ProductionManager Instance { get; private set; }

    [SerializeField]
    private List<FactorySaveData> factories = new List<FactorySaveData>();
    public IReadOnlyList<FactorySaveData> Factories => factories;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public ProductionSaveData GetSaveData()
    {
        ProductionSaveData data = new ProductionSaveData
        {
            factories = new List<FactorySaveData>(factories)
        };
        return data;
    }

    public void LoadProductionData(ProductionSaveData data)
    {
        factories.Clear();
        if (data != null && data.factories != null)
        {
            factories.AddRange(data.factories);
        }
        Debug.Log($"[ProductionManager] Loaded {factories.Count} factory records.");
    }
}
