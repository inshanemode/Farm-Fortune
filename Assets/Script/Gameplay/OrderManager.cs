using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comprehensive Order & Delivery system managing available order boards,
/// active countdown contracts, customer relationships, and farm reputation.
/// </summary>
public class OrderManager : MonoBehaviour
{
    private static OrderManager instance;
    public static OrderManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<OrderManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("OrderManager");
                    instance = go.AddComponent<OrderManager>();
                }
            }
            return instance;
        }
    }

    [Header("Reputation")]
    public int farmReputation = 0;
    public int ReputationLevel => 1 + (farmReputation / 50);

    [Header("Orders")]
    [SerializeField] private List<OrderInstance> availableOrders = new List<OrderInstance>();
    [SerializeField] private List<OrderInstance> activeOrders = new List<OrderInstance>();

    public IReadOnlyList<OrderInstance> AvailableOrders => availableOrders;
    public IReadOnlyList<OrderInstance> ActiveOrders => activeOrders;

    public int completedOrdersCount { get; private set; } = 0;

    public const int MAX_AVAILABLE_ORDERS = 4;
    public const int MAX_ACTIVE_ORDERS = 3;

    private readonly string[] customerNames = new string[]
    {
        "Bác Ba Nông Dân",
        "Đầu Bếp Pierre",
        "Cô Tư Bánh Mì",
        "Bà Bảy Chợ Quê",
        "Chủ Khách Sạn Hưng Thịnh"
    };

    private readonly string[] customerNotes = new string[]
    {
        "Nông trại chúng tôi cần bổ sung nguồn hàng gấp!",
        "Nhà hàng đang chuẩn bị tiệc tối cho thực khách VIP.",
        "Tiệm bánh cần nguyên liệu tươi mới trong ngày.",
        "Chợ phiên hôm nay rất đông khách hỏi mua.",
        "Khách sạn 5 sao yêu cầu tiêu chuẩn nông sản cao nhất."
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (availableOrders.Count == 0)
        {
            RefreshAvailableOrders();
        }
    }

    private void Update()
    {
        // Countdown timer for active orders
        float dt = Time.deltaTime;
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            OrderInstance order = activeOrders[i];
            if (order != null && order.state == OrderState.Active)
            {
                bool expired = order.UpdateTimer(dt);
                if (expired)
                {
                    // Penalty for expiration
                    farmReputation = Mathf.Max(0, farmReputation - 5);
                    activeOrders.RemoveAt(i);
                    OrderUI.Instance?.Refresh();
                }
            }
        }
    }

    public void RefreshAvailableOrders()
    {
        availableOrders.RemoveAll(o => o == null || o.state != OrderState.Available);

        while (availableOrders.Count < MAX_AVAILABLE_ORDERS)
        {
            OrderInstance newOrder = GenerateRandomOrder();
            if (newOrder != null)
            {
                availableOrders.Add(newOrder);
            }
            else
            {
                break;
            }
        }
    }

    public OrderInstance GenerateRandomOrder()
    {
        if (ItemManager.Instance == null || ItemManager.Instance.itemList == null || ItemManager.Instance.itemList.Count == 0)
        {
            return null;
        }

        // Gather candidates (crops / harvest items)
        List<Item> candidates = new List<Item>();
        foreach (Item it in ItemManager.Instance.itemList)
        {
            if (it != null && it.sellPrice > 0 && !(it is SeedItem))
            {
                candidates.Add(it);
            }
        }

        if (candidates.Count == 0)
        {
            foreach (Item it in ItemManager.Instance.itemList)
            {
                if (it != null) candidates.Add(it);
            }
        }

        if (candidates.Count == 0) return null;

        int custIdx = UnityEngine.Random.Range(0, customerNames.Length);
        string customer = customerNames[custIdx];
        string note = customerNotes[custIdx];

        int reqCount = UnityEngine.Random.Range(1, Mathf.Min(3, candidates.Count + 1));
        List<OrderRequirement> reqs = new List<OrderRequirement>();
        int totalValue = 0;

        List<Item> shuffled = new List<Item>(candidates);
        // Shuffle
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, shuffled.Count);
            Item tmp = shuffled[i];
            shuffled[i] = shuffled[rnd];
            shuffled[rnd] = tmp;
        }

        for (int i = 0; i < reqCount && i < shuffled.Count; i++)
        {
            Item item = shuffled[i];
            int amount = UnityEngine.Random.Range(3, 10) + (ReputationLevel * 2);
            reqs.Add(new OrderRequirement(item.id, amount));
            totalValue += (item.sellPrice > 0 ? item.sellPrice : 50) * amount;
        }

        int rewardMoney = Mathf.RoundToInt(totalValue * UnityEngine.Random.Range(1.8f, 2.4f)) + 500;
        int repReward = 10 + (ReputationLevel * 5);
        float duration = UnityEngine.Random.Range(240f, 480f); // 4 - 8 minutes

        OrderInstance order = new OrderInstance
        {
            orderId = Guid.NewGuid().ToString().Substring(0, 8),
            customerName = customer,
            customerNote = note,
            orderTitle = reqs.Count == 1 ? $"Cung cấp {reqs[0].requiredAmount}x {reqs[0].GetItem()?.itemName}" : $"Đơn hàng nông sản tổng hợp ({reqs.Count} món)",
            requirements = reqs,
            totalDuration = duration,
            timeRemaining = duration,
            rewardCoins = rewardMoney,
            rewardReputation = repReward,
            state = OrderState.Available
        };

        return order;
    }

    public bool AcceptOrder(OrderInstance order)
    {
        if (order == null || order.state != OrderState.Available) return false;

        if (activeOrders.Count >= MAX_ACTIVE_ORDERS)
        {
            NotificationUI.Show("Đã Đầy Đơn Hàng!", $"Bạn chỉ có thể thực hiện tối đa {MAX_ACTIVE_ORDERS} đơn cùng lúc.", 3f);
            return false;
        }

        availableOrders.Remove(order);
        order.Accept();
        activeOrders.Add(order);

        OrderUI.Instance?.Refresh();
        return true;
    }

    public bool TryCompleteOrder(string orderId, Player player)
    {
        OrderInstance order = activeOrders.Find(o => o.orderId == orderId && o.state == OrderState.Active);
        if (order == null || player == null) return false;

        if (!order.TryDeliver(player))
        {
            NotificationUI.Show("Chưa Đủ Nông Sản!", "Hãy kiểm tra lại số lượng vật phẩm yêu cầu trong túi đồ.", 2.5f);
            return false;
        }

        completedOrdersCount++;
        farmReputation += order.rewardReputation;
        activeOrders.Remove(order);

        GoalManager.Instance?.OnOrderCompleted();
        OrderUI.Instance?.Refresh();

        return true;
    }

    public void CancelOrder(string orderId)
    {
        OrderInstance order = activeOrders.Find(o => o.orderId == orderId);
        if (order != null)
        {
            order.Cancel();
            activeOrders.Remove(order);
            farmReputation = Mathf.Max(0, farmReputation - 2);
            OrderUI.Instance?.Refresh();
        }
    }

    public OrderSaveData GetSaveData()
    {
        OrderSaveData data = new OrderSaveData
        {
            farmReputation = farmReputation,
            completedOrdersCount = completedOrdersCount,
            activeOrders = new List<OrderEntrySaveData>(),
            availableOrders = new List<OrderEntrySaveData>()
        };

        foreach (var order in activeOrders)
        {
            if (order != null) data.activeOrders.Add(order.ToSaveData());
        }

        foreach (var order in availableOrders)
        {
            if (order != null) data.availableOrders.Add(order.ToSaveData());
        }

        return data;
    }

    public void LoadOrderData(OrderSaveData data)
    {
        activeOrders.Clear();
        availableOrders.Clear();

        if (data != null)
        {
            farmReputation = data.farmReputation;
            completedOrdersCount = data.completedOrdersCount;

            if (data.activeOrders != null)
            {
                foreach (var entry in data.activeOrders)
                {
                    OrderInstance inst = OrderInstance.FromSaveData(entry);
                    if (inst != null) activeOrders.Add(inst);
                }
            }

            if (data.availableOrders != null)
            {
                foreach (var entry in data.availableOrders)
                {
                    OrderInstance inst = OrderInstance.FromSaveData(entry);
                    if (inst != null) availableOrders.Add(inst);
                }
            }
        }

        if (availableOrders.Count == 0)
        {
            RefreshAvailableOrders();
        }

        Debug.Log($"[OrderManager] Loaded {activeOrders.Count} active, {availableOrders.Count} available orders. Reputation: {farmReputation}.");
    }

    // Legacy debug helper
    public OrderEntrySaveData CreateSampleOrder()
    {
        OrderInstance inst = GenerateRandomOrder();
        if (inst != null)
        {
            availableOrders.Add(inst);
            return inst.ToSaveData();
        }
        return null;
    }
}
