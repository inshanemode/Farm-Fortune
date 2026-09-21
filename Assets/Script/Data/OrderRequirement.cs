using System;
using UnityEngine;

/// <summary>
/// Represents an individual item requirement within an order.
/// </summary>
[Serializable]
public class OrderRequirement
{
    public int itemId;
    public int requiredAmount;
    public int deliveredAmount;

    public OrderRequirement() { }

    public OrderRequirement(int itemId, int requiredAmount, int deliveredAmount = 0)
    {
        this.itemId = itemId;
        this.requiredAmount = requiredAmount;
        this.deliveredAmount = deliveredAmount;
    }

    public Item GetItem()
    {
        return ItemManager.GetItem(itemId);
    }

    public int GetOwnedAmount(Player player)
    {
        if (player == null || player.inventory == null) return 0;
        Item item = GetItem();
        if (item == null) return 0;
        return player.inventory.GetAmount(item);
    }

    public int GetMissingAmount(Player player)
    {
        return Mathf.Max(0, requiredAmount - GetOwnedAmount(player));
    }

    public bool IsSatisfied(Player player)
    {
        return GetOwnedAmount(player) >= requiredAmount;
    }
}
