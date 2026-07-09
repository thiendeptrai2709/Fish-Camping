using UnityEngine;

public class CharacterHandVisual : MonoBehaviour
{
    [SerializeField] private Transform handSocketTransform;
    private GameObject currentRodModel;
    private GameObject currentBobberModel;
    private BobberSO currentBobberData;
    private Transform tipSocketTransform;
    private PlayerAnimation playerAnimation;

    private void Awake()
    {
        playerAnimation = GetComponentInParent<PlayerAnimation>();
    }

    public void EquipItemVisual(ItemShapeSO itemShape)
    {
        if (itemShape is FishingRodSO)
        {
            RemoveRodVisual();
            if (itemShape.equippedModelPrefab == null) return;

            currentRodModel = Instantiate(itemShape.equippedModelPrefab, handSocketTransform);
            currentRodModel.transform.localPosition = Vector3.zero;
            currentRodModel.transform.localRotation = Quaternion.identity;

            Transform[] allChildren = currentRodModel.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name == "TipSocket")
                {
                    tipSocketTransform = child;
                    break;
                }
            }
            if (tipSocketTransform == null) tipSocketTransform = currentRodModel.transform;

            if (playerAnimation != null) playerAnimation.SetHoldingItemState(true);

            if (currentBobberData != null) SpawnBobberModel();
        }
        else if (itemShape is BobberSO bobberSO)
        {
            currentBobberData = bobberSO;
            SpawnBobberModel();
        }
    }

    private void SpawnBobberModel()
    {
        RemoveBobberModelOnly();
        if (currentBobberData == null || currentBobberData.bobberPrefab == null || tipSocketTransform == null) return;

        currentBobberModel = Instantiate(currentBobberData.bobberPrefab, tipSocketTransform);
        currentBobberModel.transform.localPosition = Vector3.zero;
        currentBobberModel.transform.localRotation = Quaternion.identity;
    }

    public void RemoveItemVisual(ItemShapeSO itemShape)
    {
        if (itemShape is FishingRodSO)
        {
            RemoveRodVisual();
        }
        else if (itemShape is BobberSO)
        {
            currentBobberData = null;
            RemoveBobberModelOnly();
        }
    }

    private void RemoveRodVisual()
    {
        RemoveBobberModelOnly();
        if (currentRodModel != null)
        {
            Destroy(currentRodModel);
            currentRodModel = null;
        }
        tipSocketTransform = null;
        if (playerAnimation != null) playerAnimation.SetHoldingItemState(false);
    }

    private void RemoveBobberModelOnly()
    {
        if (currentBobberModel != null)
        {
            Destroy(currentBobberModel);
            currentBobberModel = null;
        }
    }

    public void SetBobberVisualActive(bool isActive)
    {
        if (currentBobberModel != null)
        {
            currentBobberModel.SetActive(isActive);
        }
    }

    public void ClearCurrentVisual()
    {
        currentBobberData = null;
        RemoveRodVisual();
    }

    public Transform GetTipSocketTransform() => tipSocketTransform;
}