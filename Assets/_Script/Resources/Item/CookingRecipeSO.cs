using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCookingRecipe", menuName = "Cooking/Recipe")]
public class CookingRecipeSO : ScriptableObject
{
    public string recipeName;
    public List<ItemShapeSO> requiredIngredients;
    public ItemShapeSO resultItem;
}