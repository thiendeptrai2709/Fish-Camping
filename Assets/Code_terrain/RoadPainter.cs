#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class RoadPainter : EditorWindow
{
    public GameObject roadPrefab;
    public Transform roadSystemParent;
    public float roadLength = 10f;
    public float roadHeight = 2f;

    [Tooltip("Xoay bù góc mặc định của Prefab nếu đường bị ngược hướng")]
    public float rotationOffset = 0f;

    public enum PaintMode { Free, Lock_X, Lock_Z, Lock_Y_Up, Lock_Y_Down }
    public PaintMode currentMode = PaintMode.Free;

    private Vector3 _lastRoadPosition;
    private Quaternion _lastRoadRotation;
    private bool _hasLastRoad = false;

    [MenuItem("Tools/Sieu Thong Minh/Road Painter")]
    public static void ShowWindow() => GetWindow<RoadPainter>("Cọ Vẽ Đường");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("SỬA LỖI XOAY MẢNH ĐƯỜNG:\nNếu các mảng đen không liền nhau (bị xoay ngang), hãy thử nhập vào ô 'Góc xoay bù Y' các giá trị là 90 hoặc -90 để đưa nó về đúng hướng thẳng tiến nhé!", MessageType.Info);

        roadPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab Đường thẳng:", roadPrefab, typeof(GameObject), false);
        roadSystemParent = (Transform)EditorGUILayout.ObjectField("Nhóm chứa (Road_System):", roadSystemParent, typeof(Transform), true);
        roadLength = EditorGUILayout.FloatField("Chiều dài mảnh X/Z (m):", roadLength);
        roadHeight = EditorGUILayout.FloatField("Chiều cao mỗi bậc Y (m):", roadHeight);

        // Ô nhập góc xoay cứu cánh
        rotationOffset = EditorGUILayout.FloatField("Góc xoay bù Y (Độ):", rotationOffset);

        GUILayout.Space(5);
        currentMode = (PaintMode)EditorGUILayout.EnumPopup("Chế độ khóa hướng:", currentMode);
        GUILayout.Space(5);

        if (GUILayout.Button("Bật chế độ vẽ đường nối đuôi"))
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            _hasLastRoad = false;
        }

        if (GUILayout.Button("Reset điểm vẽ (Để bắt đầu đường mới)"))
        {
            _hasLastRoad = false;
            Debug.Log("Đã reset điểm vẽ trục đường mới.");
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && e.shift && roadPrefab != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 spawnPos = hit.point;
                Quaternion spawnRot = Quaternion.identity;

                // Tính toán góc quay chuẩn kết hợp với Góc xoay bù
                Quaternion baseOffset = Quaternion.Euler(0, rotationOffset, 0);

                if (!_hasLastRoad)
                {
                    spawnPos = hit.point;
                    if (currentMode == PaintMode.Lock_X)
                        spawnRot = Quaternion.Euler(0, 90, 0) * baseOffset;
                    else
                        spawnRot = Quaternion.identity * baseOffset;
                }
                else
                {
                    switch (currentMode)
                    {
                        case PaintMode.Lock_Z:
                            spawnRot = _lastRoadRotation;
                            float signZ = (hit.point.z - _lastRoadPosition.z) >= 0 ? 1f : -1f;
                            spawnPos = _lastRoadPosition + new Vector3(0, 0, roadLength * signZ);
                            break;

                        case PaintMode.Lock_X:
                            spawnRot = Quaternion.Euler(0, 90, 0) * baseOffset;
                            float signX = (hit.point.x - _lastRoadPosition.x) >= 0 ? 1f : -1f;
                            spawnPos = _lastRoadPosition + new Vector3(roadLength * signX, 0, 0);
                            break;

                        case PaintMode.Lock_Y_Up:
                            spawnRot = _lastRoadRotation;
                            spawnPos = _lastRoadPosition + new Vector3(0, roadHeight, 0);
                            break;

                        case PaintMode.Lock_Y_Down:
                            spawnRot = _lastRoadRotation;
                            spawnPos = _lastRoadPosition + new Vector3(0, -roadHeight, 0);
                            break;

                        default:
                            Vector3 direction = (hit.point - _lastRoadPosition).normalized;
                            direction.y = 0;
                            spawnRot = Quaternion.LookRotation(direction) * baseOffset;
                            spawnPos = _lastRoadPosition + (direction * roadLength);
                            break;
                    }
                }

                GameObject newRoad = (GameObject)PrefabUtility.InstantiatePrefab(roadPrefab);
                newRoad.transform.position = spawnPos;
                newRoad.transform.rotation = spawnRot;

                if (roadSystemParent != null)
                    newRoad.transform.parent = roadSystemParent;

                Undo.RegisterCreatedObjectUndo(newRoad, "Paint Road");

                _lastRoadPosition = spawnPos;
                _lastRoadRotation = spawnRot;
                _hasLastRoad = true;

                e.Use();
            }
        }
    }
}
#endif