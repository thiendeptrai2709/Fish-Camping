using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForcedTutorialManager))]
public class ForcedTutorialManagerEditor : Editor
{
    private bool showStagesFold = true;

    public override void OnInspectorGUI()
    {
        // Vẽ inspector mặc định
        DrawDefaultInspector();

        var manager = (ForcedTutorialManager)target;
        if (manager == null) return;

        EditorGUILayout.Space(12);
        EditorGUILayout.BeginVertical("box");

        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.LabelField("🎯 TOOL ĐIỀU KHIỂN NHIỆM VỤ TUTORIAL", headerStyle);
        EditorGUILayout.Space(4);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Bấm PLAY để nhảy trực tiếp giữa các nhiệm vụ trong khi chơi game.", MessageType.Info);
        }

        GUI.backgroundColor = new Color(0.2f, 0.7f, 1f);
        if (GUILayout.Button("🚀 MỞ CỬA SỔ TOOL BẢNG ĐIỀU KHIỂN (FULL WINDOW)", GUILayout.Height(34)))
        {
            TutorialDebugEditorWindow.ShowWindow();
        }
        GUI.backgroundColor = Color.white;

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(6);
            TutorialStage currentStage = manager.GetCurrentStage();

            GUIStyle currentStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = new Color(0.1f, 0.85f, 0.3f) }
            };
            EditorGUILayout.LabelField($"📌 Đang ở: [{(int)currentStage}] {currentStage}", currentStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⏮ Lùi NV", GUILayout.Height(28)))
            {
                int prev = Mathf.Max(0, (int)currentStage - 1);
                manager.ForceSetStage((TutorialStage)prev);
            }

            if (GUILayout.Button("⏭ Kế Tiếp", GUILayout.Height(28)))
            {
                int next = (int)currentStage + 1;
                if (Enum.IsDefined(typeof(TutorialStage), next))
                {
                    manager.ForceSetStage((TutorialStage)next);
                }
            }

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🔄 Reset", GUILayout.Height(28)))
            {
                manager.ResetTutorial();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            showStagesFold = EditorGUILayout.Foldout(showStagesFold, "📋 Danh Sách Nhảy Nhanh Từng Nhiệm Vụ", true);
            if (showStagesFold)
            {
                TutorialStage[] allStages = (TutorialStage[])Enum.GetValues(typeof(TutorialStage));
                foreach (TutorialStage stage in allStages)
                {
                    bool isCur = stage == currentStage;
                    EditorGUILayout.BeginHorizontal();

                    if (isCur)
                    {
                        GUI.backgroundColor = new Color(0.3f, 1f, 0.4f);
                        GUILayout.Label($"★ [{(int)stage}] {stage}", EditorStyles.boldLabel);
                        GUILayout.Label("Đang làm", GUILayout.Width(70));
                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                        GUILayout.Label($"[{(int)stage}] {stage}");
                        if (GUILayout.Button("Nhảy tới", GUILayout.Width(70)))
                        {
                            manager.ForceSetStage(stage);
                        }
                    }

                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUILayout.EndVertical();
    }
}
