using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneLoadEditor : EditorWindow
{
    private static List<string> sceneList;
    private static List<string> sceneName;
    private static string firstActiveScene;
    private const string KEY_PREVIOUS_SCENE = "EDITOR_KEY_PREVIOUS_SCENE_PATH";
    private const int COLUMN_COUNT = 2;
    private static readonly bool INCLUDE_INACTIVE = true;

    [MenuItem("Custom/载入场景")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow<SceneLoadEditor>("载入指定场景");
        InitWindow();
    }

    private static void InitWindow()
    {
        sceneList = new List<string>();
        sceneName = new List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
            if (firstActiveScene == null && scene.enabled)
            {
                firstActiveScene = scene.path;
            }
            if (INCLUDE_INACTIVE || scene.enabled)
            {
                sceneList.Add(scene.path);
                string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
                name = name.Substring(0, name.IndexOf('.'));
                sceneName.Add(name);
            }
        }
    }

    private void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(12);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(12);
            if (GUILayout.Button("停止运行", GUILayout.Width(90), GUILayout.Height(30)))
            {
                EditorApplication.isPlaying = false;
                this.Repaint();
                string previousPath = PlayerPrefs.GetString(KEY_PREVIOUS_SCENE);
                if (previousPath != null)
                {
                    EditorSceneManager.OpenScene(previousPath);
                    PlayerPrefs.DeleteKey(KEY_PREVIOUS_SCENE);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        else
        {
            if (sceneList == null)
            {
                InitWindow();
            }
            EditorGUILayout.BeginVertical();
            GUILayout.Space(12);
            #region 运行游戏
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(12);
            if (GUILayout.Button("运行游戏", GUILayout.Width(90), GUILayout.Height(30)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    PlayerPrefs.SetString(KEY_PREVIOUS_SCENE, EditorSceneManager.GetActiveScene().name);
                    EditorSceneManager.OpenScene(firstActiveScene);
                    EditorApplication.isPlaying = true;
                    this.Repaint();
                }
            }
            GUILayout.Space(12);
            if (GUILayout.Button("刷新窗口", GUILayout.Width(90), GUILayout.Height(30)))
            {
                InitWindow();
            }
            EditorGUILayout.EndHorizontal();
            #endregion
            #region 载入场景部分
            GUILayout.Space(20);
            int i = 0;
            for (; i < sceneList.Count; i++)
            {
                if (i % COLUMN_COUNT == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                GUILayout.Space(12);
                if (GUILayout.Button(sceneName[i], GUILayout.Width(90), GUILayout.Height(30)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(sceneList[i]);
                    }
                }
                if (i % COLUMN_COUNT == COLUMN_COUNT - 1)
                {
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(12);
                }
            }
            if (i % COLUMN_COUNT != 0)
            {
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(12);
            }
            #endregion
            EditorGUILayout.EndVertical();
        }
    }
}