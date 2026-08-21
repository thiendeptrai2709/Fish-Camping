using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingRack : MonoBehaviour, IInteractable
{
    [SerializeField] private float cookingDuration = 5f;
    [SerializeField] private Slider cookingProgressSlider;
    [SerializeField] private LayerMask campfireLayer;
    [SerializeField] private Transform groundCheckPoint;

    [SerializeField] private List<CookingRecipeSO> availableRecipes; // Danh sách công thức

    public enum CookingState { Idle, Cooking, Finished }
    public CookingState State { get; private set; } = CookingState.Idle;

    private Campfire currentCampfire;
    private ItemShapeSO currentCookedResult;

    private void Start()
    {
        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.gameObject.SetActive(false);
        }
    }

    public bool TryStartCooking(System.Collections.Generic.List<ItemShapeSO> ingredients)
    {
        if (State != CookingState.Idle) return false;

        if (ingredients == null || ingredients.Count == 0)
        {
            Debug.Log("<color=yellow>[Cooking System] Cần ít nhất 1 nguyên liệu để nấu!</color>");
            return false;
        }

        // 1. Dò tìm công thức khớp với nguyên liệu truyền vào
        CookingRecipeSO matchedRecipe = FindMatchingRecipe(ingredients);
        if (matchedRecipe != null && matchedRecipe.resultItem != null)
        {
            currentCookedResult = matchedRecipe.resultItem;
        }
        else
        {
            // 2. Tự động tìm kiếm món ăn nướng tương ứng với loại cá (Dynamic Fallback cho mọi loại cá Map 1, 2, 3, 4)
            currentCookedResult = FindFallbackCookedFood(ingredients);
        }

        if (currentCookedResult == null)
        {
            Debug.LogWarning("<color=red>[Cooking System] Không tìm thấy món ăn phù hợp cho nguyên liệu này!</color>");
            return false;
        }

        StartCoroutine(CookingProcess());
        return true;
    }

    private bool IsSameItem(ItemShapeSO a, ItemShapeSO b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;
        if (!string.IsNullOrEmpty(a.itemID) && !string.IsNullOrEmpty(b.itemID) && a.itemID.Equals(b.itemID, System.StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.IsNullOrEmpty(a.name) && !string.IsNullOrEmpty(b.name) && a.name.Equals(b.name, System.StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.IsNullOrEmpty(a.itemName) && !string.IsNullOrEmpty(b.itemName) && a.itemName.Equals(b.itemName, System.StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private CookingRecipeSO FindMatchingRecipe(System.Collections.Generic.List<ItemShapeSO> inputs)
    {
        if (availableRecipes != null && availableRecipes.Count > 0)
        {
            foreach (var recipe in availableRecipes)
            {
                if (recipe == null || recipe.requiredIngredients == null) continue;
                if (recipe.requiredIngredients.Count != inputs.Count) continue;

                bool isMatch = true;
                List<ItemShapeSO> tempInputs = new List<ItemShapeSO>(inputs);
                foreach (var reqItem in recipe.requiredIngredients)
                {
                    ItemShapeSO found = tempInputs.Find(x => IsSameItem(x, reqItem));
                    if (found != null)
                    {
                        tempInputs.Remove(found);
                    }
                    else
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch) return recipe;
            }
        }

        // Thử tìm trong toàn bộ Resources / Project CookingRecipeSO
        CookingRecipeSO[] allRecipes = Resources.FindObjectsOfTypeAll<CookingRecipeSO>();
        if (allRecipes != null && allRecipes.Length > 0)
        {
            foreach (var recipe in allRecipes)
            {
                if (recipe == null || recipe.requiredIngredients == null) continue;
                if (recipe.requiredIngredients.Count != inputs.Count) continue;

                bool isMatch = true;
                List<ItemShapeSO> tempInputs = new List<ItemShapeSO>(inputs);
                foreach (var reqItem in recipe.requiredIngredients)
                {
                    ItemShapeSO found = tempInputs.Find(x => IsSameItem(x, reqItem));
                    if (found != null)
                    {
                        tempInputs.Remove(found);
                    }
                    else
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch) return recipe;
            }
        }

        return null;
    }

    private ItemShapeSO FindFallbackCookedFood(System.Collections.Generic.List<ItemShapeSO> inputs)
    {
        if (inputs == null || inputs.Count == 0) return null;
        ItemShapeSO firstItem = inputs[0];
        if (firstItem == null) return null;

        // Trích xuất số ID của cá (ví dụ "fish1", "Fish 1", "fish15")
        string idStr = !string.IsNullOrEmpty(firstItem.itemID) ? firstItem.itemID : firstItem.name;
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(idStr, @"\d+");
        string fishNum = match.Success ? match.Value : "1";

        // Tìm kiếm FoodSO trong Project / Resources
        FoodSO[] allFoods = Resources.FindObjectsOfTypeAll<FoodSO>();
        if (allFoods != null && allFoods.Length > 0)
        {
            // 1. Tìm đúng Food có số tương ứng (ví dụ FoodFIsh1, FoodFIsh2...)
            foreach (var food in allFoods)
            {
                if (food == null) continue;
                string foodName = food.name.ToLower();
                string foodID = (food.itemID ?? "").ToLower();
                if (foodName.Contains("foodfish" + fishNum) || foodID.Contains("foodfish" + fishNum) ||
                    foodName.Contains("fishfood" + fishNum) || foodID.Contains("fishfood" + fishNum) ||
                    foodName.Contains("fish" + fishNum) || foodID.Contains("fish" + fishNum))
                {
                    return food;
                }
            }

            // 2. Tìm bất kỳ món cá nướng FoodFIsh
            foreach (var food in allFoods)
            {
                if (food != null && (food.name.ToLower().Contains("fish") || food.name.ToLower().Contains("food")))
                {
                    return food;
                }
            }

            // 3. Fallback món đầu tiên
            if (allFoods.Length > 0 && allFoods[0] != null) return allFoods[0];
        }

        return null;
    }
    private static readonly Collider[] campfireHitBuffer = new Collider[8];

    private bool DetectCampfire()
    {
        Vector3 checkPos = groundCheckPoint != null ? groundCheckPoint.position : (transform.position + Vector3.down * 0.35f);
        int mask = campfireLayer.value != 0 ? campfireLayer.value : ~0;

        int hitCount = Physics.OverlapSphereNonAlloc(checkPos, 1.5f, campfireHitBuffer, mask);
        for (int i = 0; i < hitCount; i++)
        {
            var hit = campfireHitBuffer[i];
            if (hit == null) continue;
            currentCampfire = hit.GetComponent<Campfire>() ?? hit.GetComponentInParent<Campfire>() ?? hit.GetComponentInChildren<Campfire>();
            if (currentCampfire != null) return true;
        }

        // Fallback: Tìm lửa trại xung quanh trong bán kính 2.5m
        Collider[] nearby = Physics.OverlapSphere(transform.position, 2.5f);
        if (nearby != null)
        {
            for (int i = 0; i < nearby.Length; i++)
            {
                if (nearby[i] == null) continue;
                currentCampfire = nearby[i].GetComponent<Campfire>() ?? nearby[i].GetComponentInParent<Campfire>() ?? nearby[i].GetComponentInChildren<Campfire>();
                if (currentCampfire != null) return true;
            }
        }

        // Fallback 2: Tìm Campfire trong scene nếu ở gần
        Campfire anyCampfire = Object.FindFirstObjectByType<Campfire>();
        if (anyCampfire != null && Vector3.Distance(transform.position, anyCampfire.transform.position) <= 4.5f)
        {
            currentCampfire = anyCampfire;
            return true;
        }

        return false;
    }

    private IEnumerator CookingProcess()
    {
        State = CookingState.Cooking;
        if (currentCampfire != null) currentCampfire.SetFireActive(true);
        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.gameObject.SetActive(true);
            cookingProgressSlider.value = 0f;
        }

        float timer = 0f;
        while (timer < cookingDuration)
        {
            timer += Time.deltaTime;
            if (cookingProgressSlider != null)
            {
                cookingProgressSlider.value = timer / cookingDuration;
            }
            yield return null;
        }

        if (currentCampfire != null) currentCampfire.SetFireActive(false);
        if (cookingProgressSlider != null) cookingProgressSlider.gameObject.SetActive(false);
        State = CookingState.Finished;

        Debug.Log($"<color=green>[Cooking System] Đã nấu xong: {currentCookedResult?.itemName}</color>");

        // Tự động cập nhật tiến độ nhiệm vụ nấu ăn
        if (QuestManager.Instance != null && currentCookedResult != null)
        {
            QuestManager.Instance.NotifyCookingFinished(currentCookedResult);
        }
    }

    public ItemShapeSO GetCookedFood()
    {
        return currentCookedResult;
    }

    public void ClearCookedFood()
    {
        currentCookedResult = null;
        State = CookingState.Idle;
    }

    public void OnFocus()
    {
    }

    public void OnLoseFocus()
    {
    }

    public void Interact()
    {
        if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanInteractWith(this))
        {
            Debug.LogWarning("<color=yellow>[Cooking Rack] Chưa thể nấu ăn lúc này hoặc ở map này!</color>");
            return;
        }

        if (!DetectCampfire())
        {
            Debug.Log("<color=yellow>[Cooking Rack] Hãy đặt một Lửa trại ở bên dưới trước khi nấu ăn.</color>");
            return;
        }

        if (State == CookingState.Cooking)
        {
            Debug.Log("<color=yellow>[Cooking Rack] Đang trong quá trình nấu chín thức ăn...</color>");
            return;
        }

        if (CookingUIManager.Instance != null)
        {
            CookingUIManager.Instance.OpenCookingUI(this);
        }
        else
        {
            CookingUIManager ui = Object.FindFirstObjectByType<CookingUIManager>(FindObjectsInactive.Include);
            if (ui != null)
            {
                ui.OpenCookingUI(this);
            }
        }
    }

    public string GetInteractPrompt()
    {
        if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanInteractWith(this))
        {
            return "Chưa mở khóa nấu ăn";
        }
        if (State == CookingState.Finished) return "[Chuột Trái] Lấy thức ăn";
        if (State == CookingState.Cooking) return "Đang nấu...";
        if (DetectCampfire()) return "[Chuột Trái] Nấu ăn";
        return "Cần lửa trại bên dưới";
    }
}