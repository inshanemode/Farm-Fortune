#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to automatically register and switch Farm Fortune app icons.
/// </summary>
[InitializeOnLoad]
public static class SetAppIconTool
{
    static SetAppIconTool()
    {
        EditorApplication.delayCall += ApplyDefaultIcon;
    }

    [MenuItem("FarmFortune/Icons/1. Pixel Art Farmer & Cow Mascot (Active Default)", false, 50)]
    public static void SetPixelMascotIcon()
    {
        ApplyIcon("Assets/Sprites/Icon/icon_pixel_mascot.png");
    }

    [MenuItem("FarmFortune/Icons/2. Pixel Art Harvest Crate & Logo", false, 51)]
    public static void SetPixelCrateIcon()
    {
        ApplyIcon("Assets/Sprites/Icon/icon_pixel_crate.png");
    }

    [MenuItem("FarmFortune/Icons/3. Pixel Art Treasure Chest of Crops", false, 52)]
    public static void SetPixelChestIcon()
    {
        ApplyIcon("Assets/Sprites/Icon/icon_pixel_chest.png");
    }

    [MenuItem("FarmFortune/Icons/4. Minimalist Sprout & Coin", false, 60)]
    public static void SetMinimalSproutIcon()
    {
        ApplyIcon("Assets/Sprites/Icon/icon_minimal_sprout.png");
    }

    [MenuItem("FarmFortune/Icons/5. Flat Golden Wheat & Coins", false, 61)]
    public static void SetFlatWheatIcon()
    {
        ApplyIcon("Assets/Sprites/Icon/icon_flat_wheat.png");
    }

    private static void ApplyDefaultIcon()
    {
        Texture2D[] currentIcons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
        if (currentIcons == null || currentIcons.Length == 0 || currentIcons[0] == null)
        {
            ApplyIcon("Assets/Sprites/Icon/app_icon.png");
        }
    }

    private static void ApplyIcon(string assetPath)
    {
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (icon != null)
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { icon });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new Texture2D[] { icon });
            AssetDatabase.SaveAssets();
            Debug.Log($"[SetAppIconTool] Farm Fortune app icon updated to: {assetPath}");
        }
        else
        {
            Debug.LogWarning($"[SetAppIconTool] Could not load icon at: {assetPath}");
        }
    }
}
#endif
