using UnityEngine;

public class ShopStock : ScriptableObject
{
    public enum ShopCategory
    {
        Seeds,
        Animals,
        Tools,
        Upgrades
    }

    public int buyPrice = 0;

    public virtual ShopCategory GetCategory()
    {
        return ShopCategory.Seeds;
    }

    public virtual int GetBuyPrice(Player player)
    {
        return buyPrice;
    }
    
    public virtual string GetStockName()
    {
        return "Stock";
    }

    public virtual string GetDescription()
    {
        return "";
    }

    public virtual string GetDescription(Player player)
    {
        return GetDescription();
    }

    public virtual Sprite GetStockIcon()
    {
        return null;
    }

    [Header("Unlock & Limits")]
    public int unlockCost = 0;
    public int maxPurchaseLimit = -1;

    public virtual bool IsUnlocked(Player player)
    {
        if (player == null) return true;
        if (unlockCost <= 0) return true;
        return player.totalMoney >= unlockCost;
    }

    public virtual string GetUnlockRequirementText(Player player)
    {
        if (unlockCost > 0)
        {
            return $"LOCKED\nUnlock at {unlockCost:N0} coins";
        }
        return "LOCKED";
    }

    public virtual void AddStock(Player player)
    {

    }

    public virtual bool IsMaxed(Player player)
    {
        if (maxPurchaseLimit <= 0) return false;
        return false;
    }
}
