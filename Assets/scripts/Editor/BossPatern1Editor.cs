using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(BossPatern1))]
public class BossPatern1Editor : Editor
{
    private void OnSceneGUI()
    {
        var boss = (BossPatern1)target;
        EditorGUI.BeginChangeCheck();
        Vector3 position = Handles.PositionHandle(boss.MouthPosition, boss.transform.rotation);
        if (!EditorGUI.EndChangeCheck()) return;
        if (boss.mouth != null)
        {
            Undo.RecordObject(boss.mouth, "Move boss mouth");
            boss.mouth.position = position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(boss.mouth);
        }
        else
        {
            Undo.RecordObject(boss, "Move boss mouth");
            boss.mouthOffset = boss.transform.InverseTransformPoint(position);
            PrefabUtility.RecordPrefabInstancePropertyModifications(boss);
            EditorUtility.SetDirty(boss);
        }
    }
}
