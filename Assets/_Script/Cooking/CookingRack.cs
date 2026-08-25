using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingRack : MonoBehaviour, IInteractable
{
    [Header("Thời gian nấu chín món ăn (Cooking Duration)")]
    [Tooltip("Thời gian cần thiết để nấu chín món ăn (tính bằng giây)")]
    [SerializeField] private float cookingDuration = 10f;
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
        string idStr = (!string.IsNullOrEmpty(firstItem.itemID) ? firstItem.itemID : firstItem.name).ToLower();
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(idStr, @"\d+");
        string fishNum = match.Success ? match.Value : "1";

        // 1. Tìm trong availableRecipes của chính CookingRack
        if (availableRecipes != null && availableRecipes.Count > 0)
        {
            foreach (var recipe in availableRecipes)
            {
                if (recipe != null && recipe.resultItem != null)
                {
                    string resName = recipe.resultItem.name.ToLower();
                    string resID = (recipe.resultItem.itemID ?? "").ToLower();
                    if (resName.Contains("foodfish" + fishNum) || resID.Contains("foodfish" + fishNum) ||
                        resName.Contains("fishfood" + fishNum) || resID.Contains("fishfood" + fishNum) ||
                        resName.Contains("fish" + fishNum) || resID.Contains("fish" + fishNum))
                    {
                        return recipe.resultItem;
                    }
                }
            }
        }

        // 2. Tìm kiếm FoodSO trong Project / Resources
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

    private Campfire FindClosestCampfire()
    {
        // 1. Ưu tiên Campfire trong phân cấp con/cha của chính CookingRack này
        Campfire selfCampfire = GetComponentInChildren<Campfire>(true) ?? GetComponentInParent<Campfire>();
        if (selfCampfire != null) return selfCampfire;

        Vector3 basePos = groundCheckPoint != null ? groundCheckPoint.position : (transform.position + Vector3.down * 0.35f);

        // 2. Tìm tất cả Campfire trong Scene và chọn cái có khoảng cách GẦN NHẤT với chân giá nấu này
        Campfire[] allCampfires = Object.FindObjectsByType<Campfire>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Campfire closest = null;
        float minDistance = float.MaxValue;

        foreach (var cf in allCampfires)
        {
            if (cf == null) continue;
            float dist = Vector3.Distance(basePos, cf.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = cf;
            }
        }

        // Chỉ chấp nhận lửa trại nằm ngay dưới hoặc sát giá nấu này (bán kính <= 3.0m)
        if (closest != null && minDistance <= 3.0f)
        {
            return closest;
        }

        return closest;
    }

    private bool DetectCampfire()
    {
        currentCampfire = FindClosestCampfire();
        if (currentCampfire != null)
        {
            Vector3 basePos = groundCheckPoint != null ? groundCheckPoint.position : (transform.position + Vector3.down * 0.35f);
            if (Vector3.Distance(basePos, currentCampfire.transform.position) <= 3.0f)
            {
                return true;
            }
        }

        // Ở Map 3 và Map 4 (Camping tự do): Luôn cho phép nấu ăn
        string currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentActiveScene.Contains("Map3") || currentActiveScene.Contains("Swamp") || 
            currentActiveScene.Contains("Map4") || currentActiveScene.Contains("Ocean"))
        {
            return true;
        }

        return currentCampfire != null;
    }

    private void EnsureCampfireReference()
    {
        if (currentCampfire == null)
        {
            currentCampfire = FindClosestCampfire();
        }
    }

    private IEnumerator CookingProcess()
    {
        State = CookingState.Cooking;
        EnsureCampfireReference();
        if (currentCampfire != null)
        {
            currentCampfire.SetFireActive(true);
        }

        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.gameObject.SetActive(true);
            cookingProgressSlider.value = 0f;
        }

        float timer = 0f;
        while (timer < cookingDuration)
        {
            timer += Time.deltaTime;
            EnsureCampfireReference();
            if (currentCampfire != null)
            {
                currentCampfire.SetFireActive(true);
            }

            if (cookingProgressSlider != null)
            {
                cookingProgressSlider.value = timer / cookingDuration;
            }
            yield return null;
        }

        if (currentCampfire != null)
        {
            currentCampfire.SetFireActive(false);
        }
        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.gameObject.SetActive(false);
        }
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
        EnsureCampfireReference();
        if (currentCampfire != null)
        {
            currentCampfire.SetFireActive(false);
        }
        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.gameObject.SetActive(false);
            cookingProgressSlider.value = 0f;
        }
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

        if (State == CookingState.Finished && currentCookedResult == null)
        {
            // Tự phục hồi trạng thái nếu trước đó đã lấy thức ăn
            ClearCookedFood();
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

        CookingUIManager ui = CookingUIManager.Instance ?? Object.FindFirstObjectByType<CookingUIManager>(FindObjectsInactive.Include);
        if (ui != null)
        {
            ui.OpenCookingUI(this);
        }
    }

    public string GetInteractPrompt()
    {
        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanInteractWith(this))
        {
            return isVietnamese ? "Chưa mở khóa nấu ăn" : "Cooking not unlocked yet";
        }
        if (State == CookingState.Finished) return isVietnamese ? "[Chuột Trái] Lấy thức ăn" : "[Left Click] Take Food";
        if (State == CookingState.Cooking) return isVietnamese ? "Đang nấu..." : "Cooking...";
        if (DetectCampfire()) return isVietnamese ? "[Chuột Trái] Nấu ăn" : "[Left Click] Cook";
        return isVietnamese ? "Cần lửa trại bên dưới" : "Campfire needed underneath";
    }
}