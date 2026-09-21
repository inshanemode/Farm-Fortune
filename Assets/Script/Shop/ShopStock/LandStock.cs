using UnityEngine;

[CreateAssetMenu(fileName = "Land Stock", menuName = "Shop/Create Land Item", order = 1)]
public class LandStock : ShopStock
{
    public Sprite landSprite;
    public int buyAmount = 1;

    [Header("Dynamic price")]
    [Tooltip("Number of lands the player starts with. The first extra land costs the base price.")]
    public int startingLandCount = 3;
    [Tooltip("Extra coins added to the price after each land purchase.")]
    public int priceIncreasePerLand = 250;

    public override ShopCategory GetCategory()
    {
        return ShopCategory.Tools;
    }

    public override int GetBuyPrice(Player player)
    {
        if (player == null) return buyPrice;
        int purchasedLandCount = Mathf.Max(0, player.totalLand - startingLandCount);
        return buyPrice + purchasedLandCount * priceIncreasePerLand;
    }

    public override string GetStockName()
    {
        return "Extend land";
    }

    public override string GetDescription(Player player)
    {
        string res = $"Price: {GetBuyPrice(player)} coins / {buyAmount} land";

        res += $"\nNext land costs +{priceIncreasePerLand} coins\n\nExtending your land to grow more crops";
        return res;
    }

    public override Sprite GetStockIcon()
    {
        return landSprite;
    }

    public override void AddStock(Player player)
    {
        player.AddLand(1);
    }
}
