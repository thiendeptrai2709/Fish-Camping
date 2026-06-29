#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PropsSpawner : EditorWindow
{
    public List<GameObject> propsPrefabs = new List<GameObject>();
    public Transform propsSystemParent;

    [Header("Cấu Hình Cọ Quẹt")]
    public float brushSpacing = 2f;

    public float customRotationX = 0f;
    public bool randomRotation = true;
    public float baseRotationY = 0f;
    public float customRotationZ = 0f;

    [Header("Cấu Hình Phóng To / Thu Nhỏ")]
    public bool enableRandomScale = true;
    public float scaleMin = 0.8f;
    public float scaleMax = 1.2f;

    private int _selectedIndex = 0;
    private Vector3 _lastPaintPosition = Vector3.zero;

    // Biến trạng thái bật/tắt cọ vẽ rảnh tay
    private bool _isPaintingActive = false;

    [MenuItem("Tools/Sieu Thong Minh/Props Spawner")]
    public static void ShowWindow() => GetWindow<PropsSpawner>("Cọ Vẽ Đồ Trang Trí");

    void CleanList()
    {
        propsPrefabs.RemoveAll(item => item == null);
        Repaint();
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("HƯỚNG DẪN CHẾ ĐỘ RẢNH TAY (ĐÃ ĐỔI PHÍM):\n1. Di chuột vào cửa sổ Scene và nhấn phím 'H' để Bật / Tắt nhanh cọ vẽ đồ trang trí.\n2. Khi cọ BẬT (Màu Xanh), chỉ cần ĐÈ IM CHUỘT TRÁI để rải đạo cụ.\n3. Thuật toán tự động tính toán kích thước ép chân đế bám sát mặt đất 100%!", MessageType.Info);

        propsSystemParent = (Transform)EditorGUILayout.ObjectField("Nhóm chứa (Props_System):", propsSystemParent, typeof(Transform), true);
        brushSpacing = EditorGUILayout.Slider("Độ dày khi rê cọ (m):", brushSpacing, 0.5f, 15f);

        GUILayout.Label("CẤU HÌNH XOAY ĐỒ VẬT:", EditorStyles.boldLabel);
        customRotationX = EditorGUILayout.FloatField("Xoay nghiêng/ngửa Trục X (Độ):", customRotationX);

        randomRotation = EditorGUILayout.Toggle("Xoay tròn Y ngẫu nhiên (360 độ):", randomRotation);
        if (!randomRotation)
        {
            baseRotationY = EditorGUILayout.FloatField("Góc xoay cố định Y (Độ):", baseRotationY);
        }
        customRotationZ = EditorGUILayout.FloatField("Xoay nghiêng bên Trục Z (Độ):", customRotationZ);

        GUILayout.Space(5);
        enableRandomScale = EditorGUILayout.Toggle("Kích thước ngẫu nhiên:", enableRandomScale);
        if (enableRandomScale)
        {
            scaleMin = EditorGUILayout.FloatField("Scale nhỏ nhất:", scaleMin);
            scaleMax = EditorGUILayout.FloatField("Scale lớn nhất:", scaleMax);
        }

        GUILayout.Space(10);
        SerializedObject so = new SerializedObject(this);
        SerializedProperty prefabsProperty = so.FindProperty("propsPrefabs");
        EditorGUILayout.PropertyField(prefabsProperty, new GUIContent("Danh sách đồ trang trí Prefab:"), true);
        so.ApplyModifiedProperties();

        if (GUILayout.Button("Làm sạch và sắp xếp lại danh sách (Sửa lỗi sai món)"))
        {
            CleanList();
        }

        if (propsPrefabs.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("CHỌN MÓN ĐỂ RẢI:", EditorStyles.boldLabel);
            string[] options = new string[propsPrefabs.Count];
            for (int i = 0; i < propsPrefabs.Count; i++)
            {
                options[i] = propsPrefabs[i] != null ? propsPrefabs[i].name : "Trống (Lỗi)";
            }
            _selectedIndex = GUILayout.SelectionGrid(_selectedIndex, options, 3);
        }

        GUILayout.Space(15);

        // Cấu hình hiển thị màu sắc trực quan cho nút trạng thái
        GUI.backgroundColor = _isPaintingActive ? Color.green : Color.red;
        string buttonText = _isPaintingActive ? "TRẠNG THÁI: CỌ ĐANG BẬT (Bấm H để Tắt)" : "TRẠNG THÁI: CỌ ĐANG TẮT (Bấm H để Bật)";

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

        // BẤM PHÍM 'H' KHI ĐANG DI CHUỘT TRÊN SCENE ĐỂ BẬT/TẮT NHANH CỌ
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.H)
        {
            TogglePainting();
            e.Use();
            return;
        }

        // Nếu trạng thái cọ đang tắt thì chặn toàn bộ logic vẽ phía dưới
        if (!_isPaintingActive) return;

        // Khi cọ đang bật: Chỉ cần đè chuột trái để quẹt rê (Không phụ thuộc phím Shift)
        if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
        {
            if (propsPrefabs.Count == 0 || _selectedIndex >= propsPrefabs.Count || propsPrefabs[_selectedIndex] == null)
            {
                CleanList();
                return;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (_lastPaintPosition != Vector3.zero && Vector3.Distance(_lastPaintPosition, hit.point) < brushSpacing)
                {
                    return;
                }

                GameObject prefab = propsPrefabs[_selectedIndex];
                GameObject newProp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                // 1. Áp dụng góc xoay và scale trước để tính toán kích thước thực tế
                float rotY = randomRotation ? Random.Range(0f, 360f) : baseRotationY;
                newProp.transform.rotation = Quaternion.Euler(customRotationX, rotY, customRotationZ);

                if (enableRandomScale)
                {
                    float randScale = Random.Range(scaleMin, scaleMax);
                    newProp.transform.localScale = prefab.transform.localScale * randScale;
                }

                // 2. THUẬT TOÁN TỰ KHỚP CHÂN ĐẾ (SNAP TO GROUND SURFACES)
                newProp.transform.position = hit.point;

                MeshRenderer renderer = newProp.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    float bottomY = renderer.bounds.min.y;
                    float pivotY = newProp.transform.position.y;
                    float offsetY = pivotY - bottomY;

                    newProp.transform.position = hit.point + new Vector3(0, offsetY, 0);
                }

                if (propsSystemParent != null)
                    newProp.transform.parent = propsSystemParent;

                Undo.RegisterCreatedObjectUndo(newProp, "Spawn Prop ranh tay");

                _lastPaintPosition = hit.point;
                e.Use();
            }
        }
    }
}
#endif