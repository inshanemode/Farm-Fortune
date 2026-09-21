using System;
using System.Collections.Generic;
using UnityEngine;

public enum OrderState
{
    Available = 0,
    Active = 1,
    Completed = 2,
    Expired = 3,
    Cancelled = 4
}

/// <summary>
/// Runtime instance of an order with customer identity, countdown, and requirements.
/// </summary>
[Serializable]
public class OrderInstance
{
    public string orderId;
    public string customerName;
    public string orderTitle;
    public string customerNote;
    public List<OrderRequirement> requirements = new List<OrderRequirement>();
    public float totalDuration = 300f;
    public float timeRemaining = 300f;
    public int rewardCoins;
    public int rewardReputation = 10;
    public OrderState state = OrderState.Available;

    public OrderInstance()
    {
        orderId = Guid.NewGuid().ToString().Substring(0, 8);
    }

    public void Accept()
    {
        if (state == OrderState.Available)
        {
            state = OrderState.Active;
            NotificationUI.Show("Đã Nhận Đơn Hàng", $"Đơn hàng của {customerName} đang được thực hiện!");
        }
    }

    public bool CanDeliver(Player player)
    {
        if (player == null || player.inventory == null) return false;
        if (requirements == null || requirements.Count == 0) return false;

        foreach (var req in requirements)
        {
            if (!req.IsSatisfied(player)) return false;
        }
        return true;
    }

    public bool TryDeliver(Player player)
    {
        if (state != OrderState.Active) return false;
        if (!CanDeliver(player)) return false;

        // Deduct items
        foreach (var req in requirements)
        {
            Item item = req.GetItem();
            if (item != null)
            {
                player.inventory.AddItem(item, -req.requiredAmount);
            }
        }

        // Grant rewards
        player.AddMoney(rewardCoins);
        FeedbackManager.Instance?.PlayCoinSound();
        FeedbackManager.Instance?.SpawnCoinText(rewardCoins);

        state = OrderState.Completed;

        NotificationUI.Show("Giao Hàng Thành Công!", $"+{rewardCoins:N0} xu & +{rewardReputation} Điểm Danh Tiếng từ {customerName}!");

        return true;
    }

    public void Cancel()
    {
        if (state == OrderState.Active)
        {
            state = OrderState.Cancelled;
            NotificationUI.Show("Đã Hủy Đơn Hàng", $"Đã hủy đơn của {customerName}.");
        }
    }

    public bool UpdateTimer(float dt)
    {
        if (state != OrderState.Active) return false;

        timeRemaining -= dt;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            state = OrderState.Expired;
            NotificationUI.Show("Đơn Hàng Hết Hạn", $"Đơn hàng của {customerName} đã quá hạn giao!");
            return true; // Expired
        }
        return false;
    }

    public string GetFormattedTimeRemaining()
    {
        if (timeRemaining <= 0) return "Hết giờ";
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public OrderEntrySaveData ToSaveData()
    {
        OrderEntrySaveData data = new OrderEntrySaveData
        {
            orderId = orderId,
            customerName = customerName,
            orderTitle = orderTitle,
            customerNote = customerNote,
            requirements = new List<OrderRequirement>(requirements),
            rewardMoney = rewardCoins,
            rewardReputation = rewardReputation,
            totalDuration = totalDuration,
            timeRemaining = timeRemaining,
            state = (int)state,
            isCompleted = (state == OrderState.Completed)
        };

        // Populate fallback legacy fields for compatibility
        if (requirements.Count > 0)
        {
            data.requiredItemId = requirements[0].itemId;
            data.requiredAmount = requirements[0].requiredAmount;
        }

        return data;
    }

    public static OrderInstance FromSaveData(OrderEntrySaveData data)
    {
        if (data == null) return null;

        OrderInstance inst = new OrderInstance
        {
            orderId = string.IsNullOrEmpty(data.orderId) ? Guid.NewGuid().ToString().Substring(0, 8) : data.orderId,
            customerName = string.IsNullOrEmpty(data.customerName) ? "Khách Hàng Quen" : data.customerName,
            orderTitle = string.IsNullOrEmpty(data.orderTitle) ? "Đơn Nông Sản" : data.orderTitle,
            customerNote = data.customerNote,
            rewardCoins = data.rewardMoney,
            rewardReputation = data.rewardReputation > 0 ? data.rewardReputation : 10,
            totalDuration = data.totalDuration > 0 ? data.totalDuration : 300f,
            timeRemaining = data.timeRemaining > 0 ? data.timeRemaining : 300f,
            state = (OrderState)data.state
        };

        if (data.requirements != null && data.requirements.Count > 0)
        {
            inst.requirements = new List<OrderRequirement>(data.requirements);
        }
        else if (data.requiredItemId > 0 && data.requiredAmount > 0)
        {
            // Legacy fallback
            inst.requirements.Add(new OrderRequirement(data.requiredItemId, data.requiredAmount));
        }

        if (data.isCompleted) inst.state = OrderState.Completed;

        return inst;
    }
}
