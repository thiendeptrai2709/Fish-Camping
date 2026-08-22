using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MapUIManager : MonoBehaviour
{
    public static MapUIManager Instance { get; private set; }
    public bool IsOpen => mapUIPanel != null && mapUIPanel.activeSelf;

    [SerializeField] private GameObject mapUIPanel;
    [SerializeField] private MapInteractionManager mapInteractionManager;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private VehicleInput vehicleInput;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private MonoBehaviour freeLookCamera;
    [SerializeField] private GameObject smallMapUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (mapUIPanel != null && mapUIPanel.activeSelf)
        {
            ToggleMap();
        }
    }

    /* Đóng map và trả lại điều khiển khi load xong scene mới */
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mapUIPanel != null && mapUIPanel.activeSelf)
        {
            ToggleMap();
        }
    }

    private void Start()
    {
        if (mapUIPanel != null)
        {
            mapUIPanel.SetActive(false);
        }
    }

    private void Update()
    {
        bool isPlayerMapTriggered = playerInputHandler != null && playerInputHandler.enabled && playerInputHandler.MapTriggered;
        bool isVehicleMapTriggered = vehicleInput != null && vehicleInput.enabled && vehicleInput.MapTriggered;

        if (isPlayerMapTriggered || isVehicleMapTriggered)
        {
            ToggleMap();
        }
    }

    public void OpenMap()
    {
        if (mapUIPanel != null && !mapUIPanel.activeSelf)
        {
            ToggleMap();
        }
    }

    public void CloseMap()
    {
        if (mapUIPanel != null && mapUIPanel.activeSelf)
        {
            ToggleMap();
        }
    }

    [SerializeField] private TMPro.TextMeshProUGUI txtMapFooterHint;

    private void EnsureFooterHintBar()
    {
        if (mapUIPanel == null) return;

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (txtMapFooterHint == null)
        {
            Transform hintTrans = mapUIPanel.transform.Find("Txt_MapFooterHint");
            if (hintTrans != null)
            {
                txtMapFooterHint = hintTrans.GetComponent<TMPro.TextMeshProUGUI>();
            }
            else
            {
                GameObject footerObj = new GameObject("Txt_MapFooterHint", typeof(TMPro.TextMeshProUGUI));
                footerObj.transform.SetParent(mapUIPanel.transform, false);
                RectTransform rt = footerObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0, 24);
                rt.sizeDelta = new Vector2(950, 38);

                txtMapFooterHint = footerObj.GetComponent<TMPro.TextMeshProUGUI>();
                txtMapFooterHint.fontSize = 16;
                txtMapFooterHint.fontStyle = TMPro.FontStyles.Bold;
                txtMapFooterHint.alignment = TMPro.TextAlignmentOptions.Center;
                txtMapFooterHint.color = new Color(1f, 0.88f, 0.45f, 0.95f);
            }
        }

        if (txtMapFooterHint != null)
        {
            txtMapFooterHint.text = isVietnamese 
                ? "💡 <b>Mẹo:</b> Nhấp chuột vào khu vực có <b>Ổ Khóa 🔒</b> để xem danh sách nhiệm vụ mở Map mới!"
                : "💡 <b>Tip:</b> Click on regions with a <b>Lock 🔒</b> to view requirements to unlock new Maps!";
            txtMapFooterHint.gameObject.SetActive(true);
            txtMapFooterHint.transform.SetAsLastSibling();
        }
    }

    private void ToggleMap()
    {
        if (mapUIPanel != null)
        {
            bool isActive = !mapUIPanel.activeSelf;

            // Nếu chuẩn bị mở Map, tự động đóng các UI khác
            if (isActive)
            {
                if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanOpenMap())
                {
                    return; // Khóa mở Bản đồ khi chưa tới bước hướng dẫn
                }

                if (BackpackController.Instance != null && BackpackController.Instance.IsOpen)
                    BackpackController.Instance.CloseBackpack();

                if (BuildingUIManager.Instance != null && BuildingUIManager.Instance.IsOpen)
                    BuildingUIManager.Instance.CloseBuildingUI();

                if (FishJournalUI.Instance != null && FishJournalUI.Instance.IsOpen)
                    FishJournalUI.Instance.ToggleJournal();

                if (ShopManager.Instance != null && ShopManager.Instance.shopPanel != null && ShopManager.Instance.shopPanel.activeInHierarchy)
                    ShopManager.Instance.DongShop();

                FishingController fc = Object.FindFirstObjectByType<FishingController>();
                if (fc != null && fc.IsBusyFishing())
                {
                    fc.AutoStowRodToBackpack();
                }
            }

            mapUIPanel.SetActive(isActive);
            if (playerInputHandler != null) playerInputHandler.IsUIOpen = isActive;

            if (smallMapUI != null)
            {
                smallMapUI.SetActive(!isActive);
            }

            /* Reset trạng thái phóng to bản đồ khi mở lên và làm mới ổ khóa */
            if (isActive)
            {
                if (mapInteractionManager != null)
                {
                    mapInteractionManager.RestoreMapInstantly();
                }

                EnsureFooterHintBar();

                MapRegion[] regions = mapUIPanel.GetComponentsInChildren<MapRegion>(true);
                foreach (MapRegion r in regions)
                {
                    if (r != null) r.UpdateLockState();
                }
            }
            else
            {
                if (MapRequirementPopupUI.Instance != null)
                {
                    MapRequirementPopupUI.Instance.Hide();
                }
            }

            if (playerMovement != null)
            {
                playerMovement.enabled = !isActive;
            }

            if (freeLookCamera != null)
            {
                MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
                if (cm3Input != null) cm3Input.enabled = !isActive;

                MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
                if (cm2Input != null) cm2Input.enabled = !isActive;
            }
            if (vehicleInput != null)
            {
                vehicleInput.IsUIOpen = isActive;
            }

            if (playerCursor != null)
            {
                playerCursor.SetCursorState(!isActive);
            }
        }
    }
}