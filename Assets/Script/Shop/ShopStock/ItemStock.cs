using UnityEngine;

[CreateAssetMenu(fileName = "Shop Stock", menuName = "Shop/Create Shop Item", order = 1)]
public class ItemStock : ShopStock
{
    public Item item;
    public int buyAmount = 10;
    [Tooltip("Use Animals for livestock such as cows; otherwise use the selected shop category.")]
    public bool isAnimalStock;
    public ShopCategory category = ShopCategory.Seeds;

    public override ShopCategory GetCategory()
    {
        return isAnimalStock ? ShopCategory.Animals : category;
    }

    public override string GetStockName()
    {
        return item.itemName;
    }

    public override string GetDescription()
    {
        string res = $"Price: {buyPrice} coins / {buyAmount} units";
        if(item is SeedItem)
        {
            res += "\n\nClick a seed, then click empty soil to plant\nYou can also drag it to the soil";
        }
        return res;
    }

    public override Sprite GetStockIcon()
    {
        return item.itemIcon;
    }

    public override void AddStock(Player player)
    {
        player.inventory.AddItem(item, buyAmount);
    }
}
