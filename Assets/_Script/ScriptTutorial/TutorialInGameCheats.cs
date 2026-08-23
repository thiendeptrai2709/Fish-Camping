using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Công cụ In-Game Debug / Cheat để nhảy tới bất kỳ nhiệm vụ Tutorial nào ngay trong game.
/// Phím tắt Bật / Tắt Menu: Phím F10
/// </summary>
public class TutorialInGameCheats : MonoBehaviour
{
    [Header("Cài đặt")]
    [Tooltip("Phím tắt để Bật / Tắt bảng điều khiển trong Game (Mặc định: F10)")]
    public Key toggleKey = Key.F10;

    [Tooltip("Hiển thị nút bấm nhỏ ở góc màn hình để mở bảng không cần phím")]
    public bool showQuickToggleButton = true;

    private bool isVisible = false;
    private Vector2 scrollPos;
    private string searchFilter = "";
    private Rect windowRect = new Rect(20, 40, 440, 520);
    private int selectedTab = 0;
    private readonly string[] tabs = { "Tất cả", "Map 1", "Map 2", "Chung" };

    private void Awake()
    {
        // Đảm bảo không bị trùng lặp nếu scene reload
        TutorialInGameCheats[] existing = FindObjectsByType<TutorialInGameCheats>(FindObjectsSortMode.None);
        if (existing.Length > 1)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            isVisible = !isVisible;
            if (isVisible)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    private void OnGUI()
    {
        // Nút bấm nhỏ mở menu ở góc trên bên trái
        if (showQuickToggleButton && !isVisible)
        {
            GUI.backgroundColor = new Color(0.1f, 0.6f, 1f, 0.85f);
            if (GUI.Button(new Rect(10, 10, 150, 28), "🐛 [F10] Tutorial Tool"))
            {
                isVisible = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            GUI.backgroundColor = Color.white;
            return;
        }

        if (!isVisible) return;

        GUI.skin.window.fontSize = 13;
        GUI.skin.button.fontSize = 12;
        GUI.skin.label.fontSize = 12;

        windowRect = GUI.Window(999123, windowRect, DrawWindowContent, "🎯 TOOL ĐIỀU KHIỂN NHIỆM VỤ TUTORIAL (F10)");
    }

    private void DrawWindowContent(int windowID)
    {
        var manager = ForcedTutorialManager.Instance;
        if (manager == null)
        {
            GUILayout.Label("<color=red>Không tìm thấy ForcedTutorialManager!</color>");
            if (GUILayout.Button("Đóng", GUILayout.Height(28))) isVisible = false;
            GUI.DragWindow();
            return;
        }

        TutorialStage current = manager.GetCurrentStage();

        // 1. Trạng thái hiện tại
        GUILayout.BeginVertical("box");
        GUILayout.Label($"<color=#69F0AE><b>Nhiệm vụ hiện tại:</b></color> [{(int)current}] {current}");
        GUILayout.EndVertical();

        // 2. Các nút điều khiển nhanh
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("⏮ Lùi 1 NV", GUILayout.Height(30)))
        {
            int prev = Mathf.Max(0, (int)current - 1);
            manager.ForceSetStage((TutorialStage)prev);
        }

        if (GUILayout.Button("⏭ Kế Tiếp", GUILayout.Height(30)))
        {
            int next = (int)current + 1;
            if (Enum.IsDefined(typeof(TutorialStage), next))
            {
                manager.ForceSetStage((TutorialStage)next);
            }
        }

        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
        if (GUILayout.Button("✅ Xong Hết", GUILayout.Height(30)))
        {
            manager.ForceSetStage(TutorialStage.Completed);
        }

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("🔄 Reset", GUILayout.Height(30)))
        {
            manager.ResetTutorial();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // 3. Tab và tìm kiếm
        selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.Height(24));

        GUILayout.BeginHorizontal();
        GUILayout.Label("🔍 Tìm:", GUILayout.Width(45));
        searchFilter = GUILayout.TextField(searchFilter);
        if (GUILayout.Button("X", GUILayout.Width(25))) searchFilter = "";
        GUILayout.EndHorizontal();

        // 4. Danh sách toàn bộ nhiệm vụ
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        TutorialStage[] allStages = (TutorialStage[])Enum.GetValues(typeof(TutorialStage));
        foreach (TutorialStage stage in allStages)
        {
            int idx = (int)stage;
            bool isMap1 = idx <= (int)TutorialStage.Quest9_OpenTravelMap;
            bool isMap2 = idx >= (int)TutorialStage.Map2_Quest1_1_OpenMapToCamp && idx <= (int)TutorialStage.Map2_Quest8_HelpGuide;
            bool isGeneral = idx >= (int)TutorialStage.Final_TalkToQuestNPC;

            if (selectedTab == 1 && !isMap1) continue;
            if (selectedTab == 2 && !isMap2) continue;
            if (selectedTab == 3 && !isGeneral) continue;

            string stageName = stage.ToString();
            string friendlyName = GetFriendlyName(stage);

            if (!string.IsNullOrEmpty(searchFilter))
            {
                if (stageName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) < 0 &&
                    friendlyName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
            }

            bool isCur = stage == current;

            GUILayout.BeginHorizontal("box");

            if (isCur)
            {
                GUILayout.Label($"<color=#69F0AE><b>★ [{idx}] {friendlyName}</b></color>");
                GUI.backgroundColor = new Color(0.2f, 1f, 0.4f);
                GUILayout.Button("ĐANG LÀM", GUILayout.Width(85), GUILayout.Height(26));
            }
            else
            {
                GUILayout.Label($"[{idx}] {friendlyName}");
                GUI.backgroundColor = new Color(0.8f, 0.9f, 1f);
                if (GUILayout.Button("▶ Chuyển", GUILayout.Width(85), GUILayout.Height(26)))
                {
                    manager.ForceSetStage(stage);
                }
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.Space(6);
        if (GUILayout.Button("❌ ĐÓNG BẢNG (F10)", GUILayout.Height(30)))
        {
            isVisible = false;
        }

        GUI.DragWindow();
    }

    private string GetFriendlyName(TutorialStage stage)
    {
        switch (stage)
        {
            case TutorialStage.Quest0_WelcomeGame: return "0. Chào Mừng";
            case TutorialStage.Quest1_Movement: return "1. Di Chuyển (WASD)";
            case TutorialStage.Quest1_1_TogglePerspective: return "2. Đổi Góc Nhìn (Y)";
            case TutorialStage.Quest1_2_FindOldTruck: return "3. Tìm Xe Tải Cũ";
            case TutorialStage.Quest2_1_OpenTrunk: return "4. Mở Cốp Xe";
            case TutorialStage.Quest2_2_CloseTrunk: return "5. Đóng Cốp Xe (Tab)";
            case TutorialStage.Quest2_3_InspectCar: return "6. Kiểm Tra Tình Trạng Xe";
            case TutorialStage.Quest2_4_OpenHood: return "7. Mở Nắp Capo";
            case TutorialStage.Quest2_5_RepairEngine: return "8. Sửa Máy & Nước Làm Mát";
            case TutorialStage.Quest2_6_CloseHood: return "9. Đóng Nắp Capo";
            case TutorialStage.Quest3_1_OpenMap: return "10. Mở Bản Đồ (N)";
            case TutorialStage.Quest3_2_ClickShopIcon: return "11. Click Icon Shop";
            case TutorialStage.Quest4_1_EnterVehicle: return "12. Lên Xe Bán Tải";
            case TutorialStage.Quest4_1_1_ToggleRadio: return "13. Bật/Tắt Radio (L)";
            case TutorialStage.Quest4_1_2_RadioControl: return "14. Đổi Bài (K) & Âm Lượng ([ ])";
            case TutorialStage.Quest4_1_3_Headlights: return "15. Đèn Pha (G)";
            case TutorialStage.Quest4_2_DriveToShop: return "16. Lái Xe Đến Shop Đồ Câu";
            case TutorialStage.Quest4_3_ExitVehicle: return "17. Xuống Xe (E)";
            case TutorialStage.Quest5_1_OpenShopMenu: return "18. Mở Shop Anh Cần Thủ";
            case TutorialStage.Quest5_2_CloseShopMenu: return "19. Đóng Shop (E)";
            case TutorialStage.Quest6_1_OpenMapUpgrade: return "20. Mở Map Tìm Gara Xe";
            case TutorialStage.Quest6_2_OpenUpgradeMenu: return "21. Mở Menu Gara Bác Thợ Máy";
            case TutorialStage.Quest6_3_CloseUpgradeMenu: return "22. Đóng Menu Gara (Z)";
            case TutorialStage.Quest8_1_DriveToGasStation: return "23. Lái Xe Đến Cây Xăng";
            case TutorialStage.Quest8_2_TalkToGasNPC: return "24. Gặp Chú Bán Xăng";
            case TutorialStage.Quest8_3_RefuelVehicle: return "25. Nạp Đầy Xăng Xe (F)";
            case TutorialStage.Quest8_4_BuyGasCanister: return "26. Mua Can Xăng (F)";
            case TutorialStage.Quest8_5_CheckFuelInTrunk: return "27. Mở Cốp Xem Can Xăng";
            case TutorialStage.Quest9_OpenTravelMap: return "28. Mở Travel Map Đi Map 2 (M)";
            case TutorialStage.Map2_Quest1_1_OpenMapToCamp: return "29. Mở Map Xem Điểm Trại (N)";
            case TutorialStage.Map2_Quest1_2_GoToCampSite: return "30. Đi Đến Điểm Cắm Trại";
            case TutorialStage.Map2_Quest1_3_ExitVehicle: return "31. Xuống Xe Điểm Trại (E)";
            case TutorialStage.Map2_Quest2_1_OpenBackpack: return "32. Mở Balo (Tab)";
            case TutorialStage.Map2_Quest2_2_EquipRod: return "33. Lắp Cần Câu";
            case TutorialStage.Map2_Quest2_3_EquipBaitAndBobber: return "34. Lắp Mồi & Phao";
            case TutorialStage.Map2_Quest2_4_CloseBackpack: return "35. Đóng Balo (Tab)";
            case TutorialStage.Map2_Quest3_WalkToLakeSide: return "36. Đi Ra Sát Bờ Hồ";
            case TutorialStage.Map2_Quest4_1_WindUpRod: return "37. Vào Thế Vung Cần";
            case TutorialStage.Map2_Quest4_2_TimingPower: return "38. Căn Lực Quăng Xuống Nước";
            case TutorialStage.Map2_Quest4_3_ReelFish: return "39. Canh Phao & Giật Cá";
            case TutorialStage.Map2_Quest4_4_KeepOrReleaseFish: return "40. Xem 3D & Nhận / Thả Cá";
            case TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish: return "41. Mở Balo Xem Cá";
            case TutorialStage.Map2_Quest5_0_GoToTentCampArea: return "42. Về Khu Lều Trại";
            case TutorialStage.Map2_Quest5_1_OpenBuildMenu: return "43. Mở Bảng Xây Dựng (B)";
            case TutorialStage.Map2_Quest5_2_PlaceFirewood: return "44. Đặt Đống Củi";
            case TutorialStage.Map2_Quest5_3_PlaceCookingRack: return "45. Đặt Giá Treo Bếp";
            case TutorialStage.Map2_Quest5_4_CookFish: return "46. Nướng Cá Trên Lửa";
            case TutorialStage.Map2_Quest5_5_EatFish: return "47. Ăn Cá Nướng";
            case TutorialStage.Map2_Quest5_6_PlaceLamp: return "48. Đặt Chiếc Đèn (B)";
            case TutorialStage.Map2_Quest5_7_SleepInTent: return "49. Ngủ Trong Lều";
            case TutorialStage.Map2_Quest6_BackToTown: return "50. Lái Xe Về Thị Trấn";
            case TutorialStage.Map2_Quest7_FishLog: return "51. Sổ Tay Nhật Ký Cá (J)";
            case TutorialStage.Map2_Quest8_HelpGuide: return "52. Hướng Dẫn Phím Bấm (P)";
            case TutorialStage.Final_TalkToQuestNPC: return "53. Gặp Cậu Chủ Làng";
            case TutorialStage.Completed: return "54. Hoàn Tất Toàn Bộ Tutorial";
            default: return stage.ToString();
        }
    }
}
