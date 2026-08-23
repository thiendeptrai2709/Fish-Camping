#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class ShopPriceEditorTool
{
    // BẢNG GIÁ CHUẨN CÂN BẰNG
    public static readonly int[] giaCanCau = new int[] { 1500, 2000, 3500, 5000, 9500 };
    public static readonly int[] giaMoiCau = new int[] { 30, 80, 150, 300, 500, 800 };
    public static readonly int[] giaPhaoCau = new int[] { 100, 200, 350, 500, 800, 1200, 1800, 2500 };
    public static readonly int[] giaPhuTung = new int[] { 300, 500, 800, 1200, 1600, 2000, 2500, 3000 };

    static ShopPriceEditorTool()
    {
        EditorApplication.delayCall += () =>
        {
            CapNhatGiaTienTatCaPrefabVaScene(false);
        };
    }

    [MenuItem("Tools/Shop/Cập Nhật Giá Tiền Cho Toàn Bộ Shop UI (Prefab & Scene)")]
    public static void ManualCapNhatGiaTien()
    {
        CapNhatGiaTienTatCaPrefabVaScene(true);
    }

    public static void CapNhatGiaTienTatCaPrefabVaScene(bool showDialog)
    {
        int countUpdated = 0;

        // 1. Quét tất cả các file Prefab liên quan đến Shop trong Project
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/_UI", "Assets/_Frefabs", "Assets/Resources" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Supper_canvas") || path.Contains("Shop_popup") || path.ToLower().Contains("shop"))
            {
                try
                {
                    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                    if (prefabRoot != null)
                    {
                        bool changed = XuLyCapNhatGiaTrenRoot(prefabRoot.transform);
                        if (changed)
                        {
                            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                            countUpdated++;
                            Debug.Log($"<color=green>[ShopPriceEditorTool] Đã cập nhật giá tiền trực tiếp vào Prefab: {path}</color>");
                        }
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ShopPriceEditorTool] Lỗi khi xử lý {path}: {ex.Message}");
                }
            }
        }

        // 2. Quét các GameObject đang mở trong Scene hiện tại
        Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in allCanvases)
        {
            if (canvas == null) continue;
            string cName = canvas.gameObject.name.ToLower();
            if (cName.Contains("supper_canvas") || cName.Contains("shop") || canvas.transform.Find("Scroll View_Cancau") != null)
            {
                bool sceneChanged = XuLyCapNhatGiaTrenRoot(canvas.transform);
                if (sceneChanged)
                {
                    EditorUtility.SetDirty(canvas.gameObject);
                    countUpdated++;
                    Debug.Log($"<color=green>[ShopPriceEditorTool] Đã cập nhật giá tiền vào Scene GameObject: {canvas.gameObject.name}</color>");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (showDialog)
        {
            EditorUtility.DisplayDialog("Cập Nhật Giá Tiền Shop", $"Đã cập nhật thành công giá tiền cho {countUpdated} đối tượng Shop (Cần câu, Mồi, Phao, Phụ tùng)!", "OK");
        }
    }

    public static bool XuLyCapNhatGiaTrenRoot(Transform root)
    {
        if (root == null) return false;
        bool hasChanges = false;

        // 1. Cần câu
        Transform svCancau = TimTransformTheoTen(root, "Scroll View_Cancau") ?? TimTransformTheoTen(root, "Scroll View_CanCau");
        if (svCancau != null)
        {
            List<Transform> templates = LayDanhSachTemplate(svCancau);
            for (int i = 0; i < templates.Count; i++)
            {
                int price = (i < giaCanCau.Length) ? giaCanCau[i] : 1000 * (i + 1);
                if (CapNhatGiaChoTemplate(templates[i], price, "Cần câu " + (i + 1))) hasChanges = true;
            }
        }

        // 2. Mồi câu
        Transform svMoicau = TimTransformTheoTen(root, "Scroll View_Moicau") ?? TimTransformTheoTen(root, "Scroll View_MoiCau");
        if (svMoicau != null)
        {
            List<Transform> templates = LayDanhSachTemplate(svMoicau);
            for (int i = 0; i < templates.Count; i++)
            {
                int price = (i < giaMoiCau.Length) ? giaMoiCau[i] : 100 * (i + 1);
                if (CapNhatGiaChoTemplate(templates[i], price, "Mồi câu " + (i + 1))) hasChanges = true;
            }
        }

        // 3. Phao câu
        Transform svPhao = TimTransformTheoTen(root, "Scroll View_Phao") ?? TimTransformTheoTen(root, "Scroll View_Phaocau") ?? TimTransformTheoTen(root, "Scroll View_PhaoCau");
        if (svPhao != null)
        {
            List<Transform> templates = LayDanhSachTemplate(svPhao);
            for (int i = 0; i < templates.Count; i++)
            {
                int price = (i < giaPhaoCau.Length) ? giaPhaoCau[i] : 200 * (i + 1);
                if (CapNhatGiaChoTemplate(templates[i], price, "Phao câu " + (i + 1))) hasChanges = true;
            }
        }

        // 4. Phụ tùng xe
        Transform svPhutung = TimTransformTheoTen(root, "Scroll View_Phutungxe") ?? TimTransformTheoTen(root, "Scroll View_PhuTungXe");
        if (svPhutung != null)
        {
            List<Transform> templates = LayDanhSachTemplate(svPhutung);
            for (int i = 0; i < templates.Count; i++)
            {
                int price = (i < giaPhuTung.Length) ? giaPhuTung[i] : 500 * (i + 1);
                if (CapNhatGiaChoTemplate(templates[i], price, "Phụ tùng " + (i + 1))) hasChanges = true;
            }
        }

        return hasChanges;
    }

    private static bool CapNhatGiaChoTemplate(Transform template, int price, string itemLabel)
    {
        if (template == null) return false;
        bool modified = false;

        // 1. Tìm thằng con Item_Price
        Transform itemPriceObj = TimTransformTheoTen(template, "Item_Price");
        if (itemPriceObj != null)
        {
            // 2. Tìm thằng con Text (TMP) bên trong Item_Price
            Transform textTMPObj = itemPriceObj.Find("Text (TMP)");
            TextMeshProUGUI tmp = null;
            if (textTMPObj != null)
            {
                tmp = textTMPObj.GetComponent<TextMeshProUGUI>();
            }
            if (tmp == null)
            {
                tmp = itemPriceObj.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (tmp != null)
            {
                string targetPriceStr = price.ToString();
                if (tmp.text != targetPriceStr)
                {
                    tmp.text = targetPriceStr;
                    EditorUtility.SetDirty(tmp);
                    EditorUtility.SetDirty(tmp.gameObject);
                    modified = true;
                    Debug.Log($"[ShopPriceEditorTool] [{itemLabel}] ({template.name}) -> Item_Price -> Text (TMP) = <color=yellow>{targetPriceStr}</color>");
                }
            }
            else
            {
                Text legacyText = itemPriceObj.GetComponentInChildren<Text>(true);
                if (legacyText != null && legacyText.text != price.ToString())
                {
                    legacyText.text = price.ToString();
                    EditorUtility.SetDirty(legacyText);
                    modified = true;
                }
            }
        }

        // 3. Đồng bộ vào ShopItemButton.giaTien nếu có
        ShopItemButton shopItemBtn = template.GetComponent<ShopItemButton>() ?? template.GetComponentInChildren<ShopItemButton>(true);
        if (shopItemBtn != null)
        {
            if (shopItemBtn.giaTien != price)
            {
                shopItemBtn.giaTien = price;
                EditorUtility.SetDirty(shopItemBtn);
                modified = true;
            }
        }

        return modified;
    }

    private static List<Transform> LayDanhSachTemplate(Transform scrollView)
    {
        List<Transform> result = new List<Transform>();
        Transform content = TimTransformTheoTen(scrollView, "Content");
        if (content == null) content = scrollView;

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (child.name.ToLower().Contains("item_template") || child.name.ToLower().Contains("template"))
            {
                result.Add(child);
            }
        }

        // Sắp xếp theo số thứ tự trong tên nếu có (ví dụ: Item_Template, Item_Template (1), Item_Template (2)...)
        result = result.OrderBy(t => LaySoThuTu(t.name)).ToList();
        return result;
    }

    private static int LaySoThuTu(string name)
    {
        if (string.IsNullOrEmpty(name)) return 0;
        if (!name.Contains("(") || !name.Contains(")"))
        {
            return 0; // Item_Template gốc không có số trong ngoặc coi như là 0 (đầu tiên)
        }
        try
        {
            int start = name.IndexOf('(') + 1;
            int end = name.IndexOf(')');
            string numStr = name.Substring(start, end - start).Trim();
            if (int.TryParse(numStr, out int num)) return num;
        }
        catch { }
        return 999;
    }

    private static Transform TimTransformTheoTen(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = TimTransformTheoTen(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
#endif
