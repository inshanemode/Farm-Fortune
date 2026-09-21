using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data definition for order types and requirements.
/// </summary>
[Serializable]
public class OrderDefinition
{
    public string orderId;
    public string customerName;
    public string orderTitle;
    public string customerNote;
    public List<OrderRequirement> requirements = new List<OrderRequirement>();
    public float durationSeconds = 300f;
    public int rewardCoins;
    public int rewardReputation = 10;
    public int rewardExp;

    // Legacy fields for backward compatibility
    public int requiredItemId;
    public int requiredAmount;
}
