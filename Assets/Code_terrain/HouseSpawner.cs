#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class HouseSpawner : EditorWindow
{
    public List<GameObject> housePrefabs = new List<GameObject>();
    public Transform houseSystemParent;

    [Header("Cài đặt khoảng cách rải nhà")]
    public float spacingBetweenHouses = 15f; // Khoảng cách tối thiểu giữa các căn nhà khi rê chuột
    public float customRotationY = 0f;       // Hướng mặt nhà quay ra

    private int _selectedIndex = 0;
    private Vector3 _lastPaintPosition = Vector3.zero;

    [MenuItem("Tools/Sieu Thong Minh/House Spawner")]
    public static void ShowWindow() => GetWindow<HouseSpawner>("Cọ Spam Nhà");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("HƯỚNG DẪN SPAM NHÀ LIÊN TỤC:\n1. Kéo các mẫu nhà vào danh sách.\n2. Chọn mẫu nhà muốn xây.\n3. Giữ SHIFT + ĐÈ IM CHUỘT TRÁI RỒI DI CHUỘT trên Scene để rải nhà phố siêu tốc!", MessageType.Info);

        houseSystemParent = (Transform)EditorGUILayout.ObjectField("Nhóm chứa (House_System):", houseSystemParent, typeof(Transform), true);
        spacingBetweenHouses = EditorGUILayout.Slider("Khoảng cách giữa 2 nhà (m):", spacingBetweenHouses, 5f, 50f);
        customRotationY = EditorGUILayout.FloatField("Góc xoay mặt nhà Y (Độ):", customRotationY);

        GUILayout.Space(10);
        SerializedObject so = new SerializedObject(this);
        SerializedProperty prefabsProperty = so.FindProperty("housePrefabs");
        EditorGUILayout.PropertyField(prefabsProperty, new GUIContent("Danh sách mẫu nhà Prefab:"), true);
        so.ApplyModifiedProperties();

        if (housePrefabs.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("CHỌN MẪU NHÀ ĐỂ QUẸT:", EditorStyles.boldLabel);
            string[] options = new string[housePrefabs.Count];
            for (int i = 0; i < housePrefabs.Count; i++)
            {
                options[i] = housePrefabs[i] != null ? housePrefabs[i].name : "Trống";
            }
            _selectedIndex = GUILayout.SelectionGrid(_selectedIndex, options, 2);
        }

        GUILayout.Space(15);
        if (GUILayout.Button("Bật chế độ quẹt nhà liên tục"))
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            _lastPaintPosition = Vector3.zero; // Reset điểm cũ
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Hoạt động khi giữ SHIFT và đè chuột trái (MouseDown hoặc MouseDrag)
        if (e.modifiers == EventModifiers.Shift && e.button == 0)
        {
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (housePrefabs.Count == 0 || _selectedIndex >= housePrefabs.Count || housePrefabs[_selectedIndex] == null) return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Kiểm tra khoảng cách an toàn, tránh nhà đẻ đè lên nhau thành một đống rác
                    if (_lastPaintPosition != Vector3.zero)
                    {
                        if (Vector3.Distance(_lastPaintPosition, hit.point) < spacingBetweenHouses)
                        {
                            return; // Chưa đi đủ khoảng cách để xây căn tiếp theo
                        }
                    }

                    // Tiến hành tạo nhà
                    GameObject prefab = housePrefabs[_selectedIndex];
                    GameObject newHouse = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                    // Hút sát xuống mặt đất tại điểm chuột di qua
                    newHouse.transform.position = hit.point;
                    newHouse.transform.rotation = Quaternion.Euler(0, customRotationY, 0);

                    if (houseSystemParent != null)
                        newHouse.transform.parent = houseSystemParent;

                    Undo.RegisterCreatedObjectUndo(newHouse, "Spam House");

                    // Lưu vị trí căn nhà vừa đẻ để tính khoảng cách cho căn sau
                    _lastPaintPosition = hit.point;

                    e.Use();
                }
            }
        }
    }
}
#endif