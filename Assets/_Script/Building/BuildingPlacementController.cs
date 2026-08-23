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
    private Transform mainCameraTransform;
    private LayerMask combinedLayer;

    private void Awake()
    {
        if (Camera.main != null) mainCameraTransform = Camera.main.transform;
        combinedLayer = groundLayer | buildableSurfaceLayer;
    }

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

        if (CampBuildZone.Instance != null) CampBuildZone.Instance.ToggleZoneVisual(true);
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

        if (CampBuildZone.Instance != null) CampBuildZone.Instance.ToggleZoneVisual(false);
    }

    private void UpdatePreviewPositionAndRotation()
    {
        if (mainCameraTransform == null)
        {
            if (Camera.main != null) mainCameraTransform = Camera.main.transform;
            else return;
        }

        Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
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
                isInsideValidZone = false;
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

            GameObject placedObj = Instantiate(currentItem.prefab, currentPreview.transform.position, currentPreview.transform.rotation); //[cite: 10]
            if (currentParentSurface != null) //[cite: 10]
            {
                placedObj.transform.SetParent(currentParentSurface, true); //[cite: 10]
            }
            Debug.Log($"<color=green>[Building System] Đã đặt thành công: {currentItem.itemName}</color>"); //[cite: 10]

            // --- LƯU ĐỒ VÀO DỮ LIỆU MAP TẠI ĐÂY ---
            if (BuildingSaveManager.Instance != null)
            {
                BuildingSaveManager.Instance.SavePlacedItem(currentItem, currentPreview.transform.position, currentPreview.transform.rotation);
            }

            if (comfortController != null) //[cite: 10]
            {
                comfortController.ModifyCampComfort(currentItem.comfortBonus); //[cite: 10]
                Debug.Log($"<color=cyan>[Building System] Điểm thoải mái khu trại tăng thêm: +{currentItem.comfortBonus}</color>"); //[cite: 10]
            }

            if (currentItem != null)
            {
                string iName = (currentItem.itemName ?? "").ToLower();
                string aName = (currentItem.name ?? "").ToLower();
                if (iName.Contains("camp") || iName.Contains("củi") || aName.Contains("camp") || aName.Contains("cui"))
                {
                    ForcedTutorialManager.Instance?.NotifyFirewoodPlaced();
                }
                else if (iName.Contains("cook") || iName.Contains("treo") || aName.Contains("cook") || aName.Contains("treo"))
                {
                    ForcedTutorialManager.Instance?.NotifyCookingRackPlaced();
                }
                else if (iName.Contains("lamp") || iName.Contains("đèn") || iName.Contains("den") || aName.Contains("lamp") || aName.Contains("den") || aName.Contains("lantern"))
                {
                    ForcedTutorialManager.Instance?.NotifyLampPlaced();
                }
            }

            CancelPlacement(); //[cite: 10]
        }
    }
}