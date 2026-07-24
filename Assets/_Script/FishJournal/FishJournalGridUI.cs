using UnityEngine;

public class FishJournalGridUI : MonoBehaviour
{
    [SerializeField] private FishDatabaseSO fishDatabase;
    [SerializeField] private GameObject fishCardPrefab;
    [SerializeField] private Transform cardsContainer;

    public void RefreshGrid()
    {
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        if (fishDatabase == null || fishDatabase.allFishes == null) return;

        foreach (FishSO fish in fishDatabase.allFishes)
        {
            if (fish != null && !string.IsNullOrEmpty(fish.itemID))
            {
                GameObject cardObj = Instantiate(fishCardPrefab, cardsContainer);
                FishCardUI cardUI = cardObj.GetComponent<FishCardUI>();

                FishRecord record = FishJournalManager.Instance != null
                    ? FishJournalManager.Instance.GetRecord(fish.itemID)
                    : null;

                if (cardUI != null)
                {
                    cardUI.Setup(fish, record);
                }
            }
        }
    }
}