using System;
using UnityEngine;

/// <summary>
/// Definition for content unlock requirements and progression thresholds.
/// </summary>
[Serializable]
public class UnlockDefinition
{
    public string unlockKey;
    public string displayName;
    public int requiredTotalMoney;
    public int requiredLandCount;
    public int requiredToolLevel;
    public int unlockPrice;
}
