using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CrossfadeSlideshow))]
[CanEditMultipleObjects]
public class CrossfadeSlideshowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "nextScenePath");

        SerializedProperty pathProperty = serializedObject.FindProperty("nextScenePath");
        SceneAsset selectedScene = pathProperty.hasMultipleDifferentValues
            ? null
            : AssetDatabase.LoadAssetAtPath<SceneAsset>(pathProperty.stringValue);

        EditorGUI.showMixedValue = pathProperty.hasMultipleDifferentValues;
        EditorGUI.BeginChangeCheck();
        SceneAsset newScene = EditorGUILayout.ObjectField("마지막 사진 다음 씬", selectedScene, typeof(SceneAsset), false) as SceneAsset;
        if (EditorGUI.EndChangeCheck())
            pathProperty.stringValue = newScene == null ? string.Empty : AssetDatabase.GetAssetPath(newScene);
        EditorGUI.showMixedValue = false;

        serializedObject.ApplyModifiedProperties();

        if (string.IsNullOrEmpty(pathProperty.stringValue))
        {
            EditorGUILayout.HelpBox("씬을 지정하지 않으면 사진 목록을 계속 반복합니다.", MessageType.Info);
        }
        else if (!IsEnabledInBuild(pathProperty.stringValue))
        {
            EditorGUILayout.HelpBox("선택한 씬을 Build Settings에 추가하고 활성화해야 빌드에서도 이동할 수 있습니다.", MessageType.Warning);
        }
    }

    private static bool IsEnabledInBuild(string path)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && scene.path == path)
                return true;
        }

        return false;
    }
}
