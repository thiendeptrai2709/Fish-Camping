using System;
using UnityEditor;
using UnityEngine;

public class TutorialDebugEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string searchFilter = "";
    private int selectedCategory = 0; // 0: Tất cả, 1: Map 1, 2: Map 2, 3: Kết thúc
    private readonly string[] categoryTabs = { "Tất cả", "Map 1 (Thị Trấn)", "Map 2 (Pine Lake)", "Trạng Thái Chung" };

    [MenuItem("Tools/Fish-Camping/🎯 Tutorial Stage Controller (Trình Điều Khiển Nhiệm Vụ)", false, 10)]
    public static void ShowWindow()
    {
        var window = GetWindow<TutorialDebugEditorWindow>("Tutorial Controller");
        window.minSize = new Vector2(420, 560);
        window.Show();
    }

    private void OnInspectorUpdate()
    {
        // Tự động vẽ lại cửa sổ khi đang Play Mode để cập nhật real-time trạng thái nhiệm vụ
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        DrawHeader();

        if (!Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("⚠️ Hãy bấm PLAY (Chạy Game) để sử dụng công cụ nhảy nhiệm vụ Tutorial trực tiếp trong thời gian thực.", MessageType.Info);
            return;
        }

        var manager = ForcedTutorialManager.Instance;
        if (manager == null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("❌ Không tìm thấy ForcedTutorialManager trong Scene hiện tại!", MessageType.Warning);
            return;
        }

        DrawCurrentStageStatus(manager);
        DrawQuickControlButtons(manager);
        DrawCategoryAndSearch();
        DrawQuestList(manager);
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(8);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.LabelField("🎯 BẢNG ĐIỀU KHIỂN NHIỆM VỤ TUTORIAL", titleStyle);
        EditorGUILayout.Space(4);
    }

    private void DrawCurrentStageStatus(ForcedTutorialManager manager)
    {
        TutorialStage currentStage = manager.GetCurrentStage();

        EditorGUILayout.BeginVertical("box");
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            normal = { textColor = new Color(0.2f, 0.9f, 0.3f) }
        };

        EditorGUILayout.LabelField($"📌 Nhiệm vụ hiện tại: [{(int)currentStage}] {currentStage}", labelStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4);
    }

    private void DrawQuickControlButtons(ForcedTutorialManager manager)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚡ Thao Tác Nhanh", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⏮ Lùi 1 NV", GUILayout.Height(28)))
        {
            int prev = Mathf.Max(0, (int)manager.GetCurrentStage() - 1);
            manager.ForceSetStage((TutorialStage)prev);
        }

        if (GUILayout.Button("⏭ Bỏ Qua (Kế Tiếp)", GUILayout.Height(28)))
        {
            int next = (int)manager.GetCurrentStage() + 1;
            if (Enum.IsDefined(typeof(TutorialStage), next))
            {
                manager.ForceSetStage((TutorialStage)next);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
        if (GUILayout.Button("✅ Hoàn Thành Hết", GUILayout.Height(26)))
        {
            manager.ForceSetStage(TutorialStage.Completed);
        }

        GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
        if (GUILayout.Button("🔄 Reset Về Đầu", GUILayout.Height(26)))
        {
            manager.ResetTutorial();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4);
    }

    private void DrawCategoryAndSearch()
    {
        selectedCategory = GUILayout.Toolbar(selectedCategory, categoryTabs, GUILayout.Height(24));
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("🔍 Tìm kiếm:", GUILayout.Width(65));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            searchFilter = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private void DrawQuestList(ForcedTutorialManager manager)
    {
        TutorialStage currentStage = manager.GetCurrentStage();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        TutorialStage[] allStages = (TutorialStage[])Enum.GetValues(typeof(TutorialStage));

        foreach (TutorialStage stage in allStages)
        {
            int stageIndex = (int)stage;
            bool isMap1 = stageIndex <= (int)TutorialStage.Quest9_OpenTravelMap;
            bool isMap2 = stageIndex >= (int)TutorialStage.Map2_Quest1_1_OpenMapToCamp && stageIndex <= (int)TutorialStage.Map2_Quest8_HelpGuide;
            bool isGeneral = stageIndex >= (int)TutorialStage.Final_TalkToQuestNPC;

            if (selectedCategory == 1 && !isMap1) continue;
            if (selectedCategory == 2 && !isMap2) continue;
            if (selectedCategory == 3 && !isGeneral) continue;

            string stageName = stage.ToString();
            string friendlyName = GetFriendlyQuestName(stage);

            if (!string.IsNullOrEmpty(searchFilter))
            {
                bool matchName = stageName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchFriendly = friendlyName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!matchName && !matchFriendly) continue;
            }

            bool isCurrent = stage == currentStage;

            EditorGUILayout.BeginHorizontal("box");

            if (isCurrent)
            {
                GUI.backgroundColor = new Color(0.2f, 1f, 0.4f);
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.BeginVertical();
            GUIStyle questTitleStyle = isCurrent ? EditorStyles.boldLabel : EditorStyles.label;
            EditorGUILayout.LabelField($"[{stageIndex}] {friendlyName}", questTitleStyle);
            EditorGUILayout.LabelField($"    <color=#888888>{stageName}</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            EditorGUILayout.EndVertical();

            if (isCurrent)
            {
                GUI.backgroundColor = new Color(0.4f, 1f, 0.5f);
                GUILayout.Button("★ ĐANG LÀM", GUILayout.Width(100), GUILayout.Height(32));
            }
            else
            {
                GUI.backgroundColor = new Color(0.8f, 0.9f, 1f);
                if (GUILayout.Button("▶ Chuyển Tới", GUILayout.Width(100), GUILayout.Height(32)))
                {
                    manager.ForceSetStage(stage);
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private string GetFriendlyQuestName(TutorialStage stage)
    {
        switch (stage)
        {
            // MAP 1
            case TutorialStage.Quest0_WelcomeGame: return "0. Chào Mừng Người Chơi";
            case TutorialStage.Quest1_Movement: return "1. Di Chuyển (WASD + Shift)";
            case TutorialStage.Quest1_1_TogglePerspective: return "2. Đổi Góc Nhìn (Y)";
            case TutorialStage.Quest1_2_FindOldTruck: return "3. Tìm Xe Tải Cũ";
            case TutorialStage.Quest2_1_OpenTrunk: return "4. Mở Cốp Xe";
            case TutorialStage.Quest2_2_CloseTrunk: return "5. Đóng Cốp Xe (Tab)";
            case TutorialStage.Quest2_3_InspectCar: return "6. Kiểm Tra Tình Trạng Xe";
            case TutorialStage.Quest2_4_OpenHood: return "7. Mở Nắp Capo";
            case TutorialStage.Quest2_5_RepairEngine: return "8. Sửa Động Cơ & Châm Nước";
            case TutorialStage.Quest2_6_CloseHood: return "9. Đóng Nắp Capo";
            case TutorialStage.Quest3_1_OpenMap: return "10. Mở Bản Đồ (N)";
            case TutorialStage.Quest3_2_ClickShopIcon: return "11. Click Icon Shop Trên Map";
            case TutorialStage.Quest4_1_EnterVehicle: return "12. Lên Xe Bán Tải";
            case TutorialStage.Quest4_1_1_ToggleRadio: return "13. Bật / Tắt Radio (L)";
            case TutorialStage.Quest4_1_2_RadioControl: return "14. Đổi Bài (K) & Âm Lượng ([ ])";
            case TutorialStage.Quest4_1_3_Headlights: return "15. Bật / Tắt Đèn Pha (G)";
            case TutorialStage.Quest4_2_DriveToShop: return "16. Lái Xe Đến Shop Đồ Câu";
            case TutorialStage.Quest4_3_ExitVehicle: return "17. Xuống Xe (E)";
            case TutorialStage.Quest5_1_OpenShopMenu: return "18. Mở Shop Anh Cần Thủ";
            case TutorialStage.Quest5_2_CloseShopMenu: return "19. Đóng Menu Shop (E)";
            case TutorialStage.Quest6_1_OpenMapUpgrade: return "20. Mở Map Tìm Bác Thợ Máy";
            case TutorialStage.Quest6_2_OpenUpgradeMenu: return "21. Mở Menu Nâng Cấp Xe (Gara)";
            case TutorialStage.Quest6_3_CloseUpgradeMenu: return "22. Đóng Menu Gara (Z)";
            case TutorialStage.Quest8_1_DriveToGasStation: return "23. Lái Xe Đến Cây Xăng";
            case TutorialStage.Quest8_2_TalkToGasNPC: return "24. Nói Chuyện Với Chú Bán Xăng";
            case TutorialStage.Quest8_3_RefuelVehicle: return "25. Nạp Đầy Xăng Xe (F)";
            case TutorialStage.Quest8_4_BuyGasCanister: return "26. Mua Can Xăng Dự Trữ (F)";
            case TutorialStage.Quest8_5_CheckFuelInTrunk: return "27. Mở Cốp Kiểm Tra Can Xăng";
            case TutorialStage.Quest9_OpenTravelMap: return "28. Mở Bản Đồ Du Lịch Sang Map 2 (M)";

            // MAP 2
            case TutorialStage.Map2_Quest1_1_OpenMapToCamp: return "29. Mở Map Xem Điểm Cắm Trại (N)";
            case TutorialStage.Map2_Quest1_2_GoToCampSite: return "30. Lái Xe Đến Điểm Cắm Trại";
            case TutorialStage.Map2_Quest1_3_ExitVehicle: return "31. Xuống Xe Tại Điểm Cắm Trại (E)";
            case TutorialStage.Map2_Quest2_1_OpenBackpack: return "32. Mở Balo (Tab)";
            case TutorialStage.Map2_Quest2_2_EquipRod: return "33. Lắp Cần Câu Vào Ô Trang Bị";
            case TutorialStage.Map2_Quest2_3_EquipBaitAndBobber: return "34. Lắp Mồi Câu & Phao Câu";
            case TutorialStage.Map2_Quest2_4_CloseBackpack: return "35. Đóng Balo (Tab)";
            case TutorialStage.Map2_Quest3_WalkToLakeSide: return "36. Đi Ra Sát Bờ Hồ";
            case TutorialStage.Map2_Quest4_1_WindUpRod: return "37. Click Vào Thế Vung Cần";
            case TutorialStage.Map2_Quest4_2_TimingPower: return "38. Căn Lực Quăng Xuống Nước";
            case TutorialStage.Map2_Quest4_3_ReelFish: return "39. Canh Phao & Giật Cần Kéo Cá";
            case TutorialStage.Map2_Quest4_4_KeepOrReleaseFish: return "40. Soi Cá 3D & Nhận / Thả Cá";
            case TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish: return "41. Mở Balo Xem Cá (Tab)";
            case TutorialStage.Map2_Quest5_0_GoToTentCampArea: return "42. Đi Về Khu Vực Lều Trại";
            case TutorialStage.Map2_Quest5_1_OpenBuildMenu: return "43. Mở Bảng Xây Dựng (B)";
            case TutorialStage.Map2_Quest5_2_PlaceFirewood: return "44. Đặt Đống Củi";
            case TutorialStage.Map2_Quest5_3_PlaceCookingRack: return "45. Đặt Giá Treo Nấu Ăn";
            case TutorialStage.Map2_Quest5_4_CookFish: return "46. Nướng Cá Trên Bếp Lửa";
            case TutorialStage.Map2_Quest5_5_EatFish: return "47. Ăn Cá Nướng Hồi Thể Lực";
            case TutorialStage.Map2_Quest5_6_PlaceLamp: return "48. Đặt Chiếc Đèn Chiếu Sáng (B)";
            case TutorialStage.Map2_Quest5_7_SleepInTent: return "49. Ngủ Trong Lều Hồi Phục";
            case TutorialStage.Map2_Quest6_BackToTown: return "50. Lái Xe Về Lại Thị Trấn";
            case TutorialStage.Map2_Quest7_FishLog: return "51. Mở Sổ Tay Nhật Ký Cá (J)";
            case TutorialStage.Map2_Quest8_HelpGuide: return "52. Mở Hướng Dẫn Phím Bấm (P)";

            // GENERAL
            case TutorialStage.Final_TalkToQuestNPC: return "53. Gặp Cậu Chủ Làng Nhận Nhiệm Vụ";
            case TutorialStage.Completed: return "54. Đã Hoàn Thành Toàn Bộ Tutorial";
            default: return stage.ToString();
        }
    }
}
