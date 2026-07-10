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

        // Dò tìm công thức khớp với nguyên liệu truyền vào
        CookingRecipeSO matchedRecipe = FindMatchingRecipe(ingredients);
        if (matchedRecipe == null)
        {
            Debug.Log("<color=red>[Cooking System] Sai công thức! Nấu ra món ăn thất bại.</color>");
            // (Tùy chọn: Bạn có thể return false để cấm nấu, hoặc cho currentCookedResult thành món "Cặn bã")
            return false;
        }

        currentCookedResult = matchedRecipe.resultItem;
        StartCoroutine(CookingProcess());
        return true;
    }
    private CookingRecipeSO FindMatchingRecipe(System.Collections.Generic.List<ItemShapeSO> inputs)
    {
        foreach (var recipe in availableRecipes)
        {
            if (recipe.requiredIngredients.Count != inputs.Count) continue;

            bool isMatch = true;
            List<ItemShapeSO> tempInputs = new List<ItemShapeSO>(inputs);
            foreach (var reqItem in recipe.requiredIngredients)
            {
                if (tempInputs.Contains(reqItem))
                {
                    tempInputs.Remove(reqItem);
                }
                else
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch) return recipe;
        }
        return null;
    }
    private bool DetectCampfire()
    {
        Collider[] hits = Physics.OverlapSphere(groundCheckPoint.position, 0.5f, campfireLayer);
        foreach (var hit in hits)
        {
            currentCampfire = hit.GetComponent<Campfire>();
            if (currentCampfire != null) return true;
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

        Debug.Log($"<color=green>[Cooking System] Đã nấu xong: {currentCookedResult.itemName}</color>");
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
        if (!DetectCampfire())
        {
            Debug.Log("<color=yellow>Không thể nấu: Hãy xây một Lửa trại ở bên dưới trước.</color>");
            return;
        }

        // Đổi isCooking thành kiểm tra State
        if (State == CookingState.Cooking)
        {
            Debug.Log("<color=yellow>Nồi đang nấu rồi!</color>");
            return;
        }

        if (CookingUIManager.Instance != null)
        {
            CookingUIManager.Instance.OpenCookingUI(this);
        }
    }

    public string GetInteractPrompt()
    {
        if (State == CookingState.Finished) return "Lấy thức ăn";
        if (State == CookingState.Cooking) return "Đang nấu...";
        if (DetectCampfire()) return "Nấu ăn";
        return "Thiếu lửa trại";
    }
}