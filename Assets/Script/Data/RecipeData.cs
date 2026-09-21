using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Recipe data definition for factories and processing buildings.
/// </summary>
[Serializable]
public class RecipeData
{
    public string recipeId;
    public string recipeName;
    public int inputItemId;
    public int inputAmount;
    public int outputItemId;
    public int outputAmount;
    public float craftDurationSeconds = 30f;
}
