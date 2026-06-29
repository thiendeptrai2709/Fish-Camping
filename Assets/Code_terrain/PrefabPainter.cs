#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PrefabPainter : EditorWindow
{
    public GameObject prefabToPaint;
    public Transform parentObject;
    public float radius = 5f;
    public int count = 5;

    // --- CẤU HÌNH XOAY 3 TRỤC TÙY BIẾN ---
    public float customRotationX = 0f;     // Chỉnh nghiêng/ngửa (nếu mô hình bị nằm ngang)
    public bool randomRotationY = true;    // Xoay tròn la bàn ngẫu nhiên trên trục Y cho tự nhiên
    public float baseRotationY = 0f;       // Góc xoay cố định Y nếu không chọn ngẫu nhiên
    public float customRotationZ = 0f;     // Chỉnh nghiêng trái/phải

    // --- CẤU HÌNH PHÓNG TO / THU NHỎ (SCALE) ---
    public bool enableRandomScale = true;  // Bật scale ngẫu nhiên cho cây cối tự nhiên
    public float scaleMin = 0.8f;
    public float scaleMax = 1.2f;
    public float customScaleAll = 1.0f;    // Kích thước cố định áp dụng nếu không chọn ngẫu nhiên

    private Vector3 _lastPaintPosition = Vector3.zero;
    private float _spacingBetweenBrushes = 3f;
    private bool _isPaintingActive = false;

    [MenuItem("Tools/Sieu Thong Minh/Prefab Painter")]
    public static void ShowWindow() => GetWindow<PrefabPainter>("Cọ Vẽ Cây");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("CHẾ ĐỘ RẢNH TAY TÍCH HỢP XOAY 3 TRỤC & SCALE:\n1. Di chuột vào cửa sổ Scene và nhấn phím 'G' để Bật (Xanh) / Tắt (Đỏ) cọ.\n2. Khi cọ BẬT, chỉ cần ĐÈ IM CHUỘT TRÁI để quẹt.\n3. Nếu cây bị đổ rạp, hãy gõ 90 hoặc -90 vào ô Trục X / Z.", MessageType.Info);

        prefabToPaint = (GameObject)EditorGUILayout.ObjectField("Prefab Cây/Đá:", prefabToPaint, typeof(GameObject), false);
        parentObject = (Transform)EditorGUILayout.ObjectField("Nhóm chứa (Tree_System):", parentObject, typeof(Transform), true);
        radius = EditorGUILayout.Slider("Bán kính cọ rải:", radius, 1f, 50f);
        count = EditorGUILayout.IntSlider("Số lượng mỗi lần quẹt:", count, 1, 20);
        _spacingBetweenBrushes = EditorGUILayout.Slider("Độ thưa khi rê (m):", _spacingBetweenBrushes, 1f, 20f);

        GUILayout.Space(5);
        // --- CẤU HÌNH GÓC XOAY ---
        GUILayout.Label("CẤU HÌNH XOAY BIẾN ĐỔI 3 TRỤC:", EditorStyles.boldLabel);
        customRotationX = EditorGUILayout.FloatField("Xoay nghiêng Trục X (Độ):", customRotationX);
        randomRotationY = EditorGUILayout.Toggle("Xoay tròn Y ngẫu nhiên (360°):", randomRotationY);
        if (!randomRotationY)
        {
            baseRotationY = EditorGUILayout.FloatField("Góc xoay Y cố định:", baseRotationY);
        }
        customRotationZ = EditorGUILayout.FloatField("Xoay nghiêng Trục Z (Độ):", customRotationZ);

        GUILayout.Space(5);
        // --- CẤU HÌNH SCALE ---
        GUILayout.Label("CẤU HÌNH KÍCH THƯỚC (SCALE):", EditorStyles.boldLabel);
        enableRandomScale = EditorGUILayout.Toggle("Kích thước ngẫu nhiên:", enableRandomScale);
        if (enableRandomScale)
        {
            scaleMin = EditorGUILayout.FloatField("  Scale tối thiểu:", scaleMin);
            scaleMax = EditorGUILayout.FloatField("  Scale tối đa:", scaleMax);
        }
        else
        {
            customScaleAll = EditorGUILayout.FloatField("  Scale cố định (Gốc = 1):", customScaleAll);
        }

        GUILayout.Space(10);

        // Hiển thị trạng thái màu sắc nút
        GUI.backgroundColor = _isPaintingActive ? Color.green : Color.red;
        string buttonText = _isPaintingActive ? "TRẠNG THÁI: CỌ ĐANG BẬT (Bấm G để Tắt)" : "TRẠNG THÁI: CỌ ĐANG TẮT (Bấm G để Bật)";

        if (GUILayout.Button(buttonText, GUILayout.Height(35)))
        {
            TogglePainting();
        }
        GUI.backgroundColor = Color.white;
    }

    void TogglePainting()
    {
        _isPaintingActive = !_isPaintingActive;
        if (_isPaintingActive)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            _lastPaintPosition = Vector3.zero;
        }
        else
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

    void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Bấm phím G trong khi di chuột trên Scene để bật/tắt nhanh
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.G)
        {
            TogglePainting();
            e.Use();
            return;
        }

        if (!_isPaintingActive) return;

        // Đè im chuột trái để quẹt rải cụm vật thể
        if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (e.type == EventType.MouseDrag && _lastPaintPosition != Vector3.zero)
                {
                    if (Vector3.Distance(_lastPaintPosition, hit.point) < _spacingBetweenBrushes)
                    {
                        return;
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    Vector2 rand = Random.insideUnitCircle * radius;
                    Vector3 spawnPos = hit.point + new Vector3(rand.x, 0, rand.y);

                    // Tìm độ cao mặt Terrain
                    if (Physics.Raycast(new Ray(spawnPos + Vector3.up * 50f, Vector3.down), out RaycastHit groundHit))
                    {
                        spawnPos.y = groundHit.point.y;
                    }

                    if (prefabToPaint != null)
                    {
                        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPaint);

                        // 1. Tính toán góc quay tích hợp 3 trục X, Y, Z độc lập
                        float rotY = randomRotationY ? Random.Range(0f, 360f) : baseRotationY;
                        obj.transform.rotation = Quaternion.Euler(customRotationX, rotY, customRotationZ);

                        // 2. Tính toán tỉ lệ phóng to / thu nhỏ (Scale)
                        if (enableRandomScale)
                        {
                            float randScale = Random.Range(scaleMin, scaleMax);
                            obj.transform.localScale = prefabToPaint.transform.localScale * randScale;
                        }
                        else
                        {
                            obj.transform.localScale = prefabToPaint.transform.localScale * customScaleAll;
                        }

                        // 3. Đặt vị trí bám bề mặt thông minh (Chống lơ lửng / lún đất do thay đổi scale)
                        obj.transform.position = spawnPos;
                        MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>();
                        if (renderer != null)
                        {
                            float bottomY = renderer.bounds.min.y;
                            float pivotY = obj.transform.position.y;
                            float offsetY = pivotY - bottomY;
                            obj.transform.position = spawnPos + new Vector3(0, offsetY, 0);
                        }

                        if (parentObject != null) obj.transform.parent = parentObject;

                        Undo.RegisterCreatedObjectUndo(obj, "Paint Prefab Advance");
                    }
                }

                _lastPaintPosition = hit.point;
                e.Use();
            }
        }
    }
}
#endif