#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PrefabPainter : EditorWindow
{
    public GameObject prefabToPaint;
    public Transform parentObject;
    public float radius = 5f;
    public int count = 5;

    [MenuItem("Tools/Sieu Thong Minh/Prefab Painter")]
    public static void ShowWindow() => GetWindow<PrefabPainter>("Cọ Vẽ Cây");

    void OnGUI()
    {
        prefabToPaint = (GameObject)EditorGUILayout.ObjectField("Prefab Cây/Đá:", prefabToPaint, typeof(GameObject), false);
        parentObject = (Transform)EditorGUILayout.ObjectField("Nhóm chứa (Tree_System):", parentObject, typeof(Transform), true);
        radius = EditorGUILayout.Slider("Bán kính cọ:", radius, 1f, 50f);
        count = EditorGUILayout.IntSlider("Số lượng mỗi lần quẹt:", count, 1, 20);
        
        if (GUILayout.Button("Bật chế độ quẹt chuột (Giữ Shift + Click Scene)"))
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                for (int i = 0; i < count; i++)
                {
                    Vector2 rand = Random.insideUnitCircle * radius;
                    Vector3 spawnPos = hit.point + new Vector3(rand.x, 0, rand.y);
                    if (prefabToPaint != null)
                    {
                        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPaint);
                        obj.transform.position = spawnPos;
                        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        if (parentObject != null) obj.transform.parent = parentObject;
                        Undo.RegisterCreatedObjectUndo(obj, "Paint Prefab");
                    }
                }
            }
            e.Use();
        }
    }
}
#endif