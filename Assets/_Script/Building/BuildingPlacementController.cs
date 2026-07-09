using UnityEngine;

public class BuildingPlacementController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private ActivityEnergyController energyController;
    [SerializeField] private ComfortStatController comfortController;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask buildableSurfaceLayer;
    [SerializeField] private float maxBuildDistance = 8f;
    [SerializeField] private float rotationSpeed = 100f;

    private BuildableItemSO currentItem;
    private Transform currentParentSurface;

    private GameObject currentPreview;
    private bool isPlacing = false;
    private float currentRotationY = 0f;
    private bool isInsideValidZone = true;
    private void Update()
    {
        if (!isPlacing || currentPreview == null) return;

        UpdatePreviewPositionAndRotation();
        HandlePlacementInput();
    }

    public void StartPlacement(BuildableItemSO item)
    {
        CancelPlacement();
        currentItem = item;
        isPlacing = true;

        GameObject previewObj = item.previewPrefab != null ? item.previewPrefab : item.prefab;
        currentPreview = Instantiate(previewObj);

        Collider[] colliders = currentPreview.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        currentItem = null;
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    private void UpdatePreviewPositionAndRotation()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        LayerMask combinedLayer = groundLayer | buildableSurfaceLayer;
        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, combinedLayer))
        {
            currentPreview.transform.position = hit.point;
            currentPreview.SetActive(true);

            BuildableSurface surface = hit.collider.GetComponentInParent<BuildableSurface>();
            currentParentSurface = surface != null ? surface.transform : null;

            if (CampBuildZone.Instance != null)
            {
                isInsideValidZone = CampBuildZone.Instance.IsInsideBuildZone(hit.point);
            }
            else
            {
                isInsideValidZone = true;
            }
        }
        else
        {
            currentPreview.SetActive(false);
            isInsideValidZone = false;
            currentParentSurface = null;
        }

        if (inputHandler != null && inputHandler.RotateItemTriggered)
        {
            float direction = Mathf.Sign(inputHandler.RotateItemValue);
            currentRotationY += rotationSpeed * direction;
        }
        currentPreview.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
    }
    private void HandlePlacementInput()
    {
        if (inputHandler == null || inputHandler.IsUIOpen) return;

        if (inputHandler.InteractTriggered && currentPreview.activeSelf)
        {
            if (!isInsideValidZone)
            {
                Debug.Log("<color=red>[Building System] Vị trí này nằm ngoài diện tích khu trại! Không thể đặt công trình ở đây.</color>");
                return;
            }

            if (energyController != null && energyController.statsManager != null)
            {
                if (energyController.statsManager.GetStatValue(StatType.Energy) < currentItem.energyCost)
                {
                    Debug.Log("<color=red>[Building System] Bạn không đủ Năng lượng để xây công trình này!</color>");
                    return;
                }
            }

            if (energyController != null)
            {
                energyController.TryConsumeEnergy(currentItem.energyCost);
            }

            GameObject placedObj = Instantiate(currentItem.prefab, currentPreview.transform.position, currentPreview.transform.rotation);
            if (currentParentSurface != null)
            {
                placedObj.transform.SetParent(currentParentSurface, true);
            }
            Debug.Log($"<color=green>[Building System] Đã đặt thành công: {currentItem.itemName}</color>");

            if (comfortController != null)
            {
                comfortController.ModifyCampComfort(currentItem.comfortBonus);
                Debug.Log($"<color=cyan>[Building System] Điểm thoải mái khu trại tăng thêm: +{currentItem.comfortBonus}</color>");
            }

            CancelPlacement();
        }
    }
}