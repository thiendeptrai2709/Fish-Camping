#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class RoadPainter : EditorWindow
{
    public GameObject roadPrefab;
    public Transform roadSystemParent;
    public float roadLength = 10f; // Chiều dài mặc định của 1 đoạn đường prefab

    private Vector3 _lastRoadPosition;
    private Quaternion _lastRoadRotation;
    private bool _hasLastRoad = false;

    [MenuItem("Tools/Sieu Thong Minh/Road Painter")]
    public static void ShowWindow() => GetWindow<RoadPainter>("Cọ Vẽ Đường");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("HƯỚNG DẪN:\n1. Chọn Prefab đường và nhóm Road_System.\n2. Click 'Đặt mảnh đường đầu tiên' trên Scene.\n3. Giữ SHIFT + Click chuột trái tiếp theo để vẽ đường nối đuôi tự động!", MessageType.Info);

        roadPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab Đường thẳng:", roadPrefab, typeof(GameObject), false);
        roadSystemParent = (Transform)EditorGUILayout.ObjectField("Nhóm chứa (Road_System):", roadSystemParent, typeof(Transform), true);
        roadLength = EditorGUILayout.FloatField("Chiều dài 1 mảnh đường (m):", roadLength);

        if (GUILayout.Button("Bật chế độ vẽ đường nối đuôi"))
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            _hasLastRoad = false; // Reset lại điểm vẽ
        }

        if (GUILayout.Button("Reset điểm vẽ (Để bắt đầu đường mới)"))
        {
            _hasLastRoad = false;
            Debug.Log("Đã reset điểm vẽ. Hãy click để đặt mảnh đầu tiên cho trục đường mới.");
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Nhấn Shift + Click chuột trái để vẽ
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift && roadPrefab != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 spawnPos = hit.point;
                Quaternion spawnRot = Quaternion.identity;

                if (_hasLastRoad)
                {
                    // Tính toán hướng từ mảnh đường cũ đến điểm vừa click mới
                    Vector3 direction = (hit.point - _lastRoadPosition).normalized;
                    direction.y = 0; // Giữ đường nằm phẳng trên mặt đất

                    // Tính góc xoay cho khít hướng đi
                    spawnRot = Quaternion.LookRotation(direction);

                    // Tính vị trí chính xác nối đuôi dựa trên chiều dài mảnh đường
                    spawnPos = _lastRoadPosition + (direction * roadLength);
                }

                // Tiến hành tạo mảnh đường
                GameObject newRoad = (GameObject)PrefabUtility.InstantiatePrefab(roadPrefab);
                newRoad.transform.position = spawnPos;
                newRoad.transform.rotation = spawnRot;

                if (roadSystemParent != null)
                    newRoad.transform.parent = roadSystemParent;

                Undo.RegisterCreatedObjectUndo(newRoad, "Paint Road");

                // Lưu lại thông tin mảnh này để làm móng cho mảnh tiếp theo
                _lastRoadPosition = spawnPos;
                _lastRoadRotation = spawnRot;
                _hasLastRoad = true;

                e.Use();
            }
        }
    }
}
#endif