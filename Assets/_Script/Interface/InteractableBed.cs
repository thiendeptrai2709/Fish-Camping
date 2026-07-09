using UnityEngine;

public class InteractableBed : MonoBehaviour, IInteractable
{
    [Header("Sleeping Settings")]
    [SerializeField] private string interactPrompt = "Nhấn [E] để Ngủ";
    [SerializeField] private float sleepRestoreAmount = 100f;
    [SerializeField] private float energyRestoreAmount = 100f;

    private PlayerInteraction playerInteraction;
    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        // Setup layer để làm Outline (giống hệt cấu trúc InteractableTrunk của ông)
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        gameObject.layer = normalLayer;
    }

    public void Interact()
    {
        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        if (playerInteraction != null)
        {
            ForcedSleepController forcedSleep = playerInteraction.GetComponent<ForcedSleepController>();
            if (forcedSleep != null)
            {
                forcedSleep.ResetAllPenalties();
            }

            SleepStatController sleepController = playerInteraction.GetComponent<SleepStatController>();
            CharacterStatsManager statsManager = playerInteraction.GetComponent<CharacterStatsManager>();

            if (sleepController != null)
            {
                sleepController.Sleep(sleepRestoreAmount);
            }

            if (statsManager != null)
            {
                statsManager.ModifyStat(StatType.Energy, energyRestoreAmount);
            }

            Debug.Log("Nhân vật đã ngủ và hồi phục sức khỏe!");
        }
    }

    public string GetInteractPrompt()
    {
        return interactPrompt;
    }

    public void OnFocus()
    {
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}