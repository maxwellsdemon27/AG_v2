using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(PathGroupMaker))]
public class PathGroupMakerInspector :Editor
{
    private PathGroupMaker _script;

    public static SerializedProperty prop_GP;
    public static SerializedProperty prop_TP;
    public static SerializedProperty prop_PS;
    public static SerializedProperty prop_CD;

    public static SerializedProperty prop_pointer_PS;

    private ReorderableList prop_CD_RList;

    #region #Pointer Variables
    private int select_PS = 0;
    private int select_cd = -1;

    #endregion

    public string[] path_Settings = new string[0];

    private void OnEnable()
    {
        if (target != null)
        {
            _script = (PathGroupMaker)target;
        }

        prop_GP = serializedObject.FindProperty("giver_prefab");
        prop_TP = serializedObject.FindProperty("turncircle_prefab");
        prop_PS = serializedObject.FindProperty("pathSettings");

        prop_pointer_PS = serializedObject.FindProperty("editor_PS_Pointer");

        select_PS = prop_pointer_PS.intValue;
       
        PathSettingGet();
        if (select_PS != 0)
        {
            prop_CD = prop_PS.GetArrayElementAtIndex(select_PS - 1).FindPropertyRelative("circleDatas");
            GetReorderableList();
        }
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _script = (PathGroupMaker)target;

        #region #PathGroupMaker編輯介面
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(prop_GP);
        EditorGUILayout.PropertyField(prop_TP);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        select_PS = EditorGUILayout.Popup("Edit Path Setting", select_PS, path_Settings);
        if(select_PS != prop_pointer_PS.intValue)
            PathSettingSelectChange();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Path",GUILayout.Width(100)))
        {
            var newSetting = new PathSetting();
            newSetting.name = "Path_" + (_script.pathSettings.Count + 1);
            _script.pathSettings.Add(newSetting);
            select_PS = _script.pathSettings.Count;
        }

        EditorGUI.BeginDisabledGroup(select_PS == 0);

        if (GUILayout.Button("Delate Path", GUILayout.Width(80)))
        {
            _script.pathSettings.RemoveAt(select_PS - 1);
            select_PS = 0;
        }

        if (GUILayout.Button("90 RTurn", GUILayout.Width(60)))
        {
            var newSetting = new PathSetting();
            var targetSetting = _script.pathSettings[select_PS - 1];

            newSetting.name = targetSetting.name + " 90 RTurn";
            newSetting.start_Pos.x = targetSetting.start_Pos.y;
            newSetting.start_Pos.y = targetSetting.start_Pos.x * -1;
            newSetting.start_R = (targetSetting.start_R + 90) % 360;

            for (int i = 0; i < targetSetting.circleDatas.Count; i++)
            {
                var circle = new CircleData();
                circle.position.x = targetSetting.circleDatas[i].position.y;
                circle.position.y = targetSetting.circleDatas[i].position.x * -1;
                circle.turnMode = targetSetting.circleDatas[i].turnMode;
                newSetting.circleDatas.Add(circle);
            }
            _script.pathSettings.Add(newSetting);
            select_PS = _script.pathSettings.Count;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (select_PS > 0)
        {
            PathSetting pathSetting = _script.pathSettings[select_PS - 1];
            pathSetting.name = EditorGUILayout.TextField("Path Name", pathSetting.name);          
            pathSetting.start_Pos = EditorGUILayout.Vector2Field("Start Position", pathSetting.start_Pos);
            pathSetting.start_R = EditorGUILayout.FloatField("Start R", pathSetting.start_R);
            EditorGUILayout.Space();          
            prop_CD_RList.DoLayoutList();
        }
        PathSettingGet();
        #endregion

        serializedObject.ApplyModifiedProperties();
    }

    public void PathSettingGet()
    {
        var count = _script.pathSettings.Count;

        path_Settings = new string[count + 1];

        path_Settings[0] = "None";

        for (int i = 0; i < count; i++)
        {
            path_Settings[i + 1] = (i+1)+". "+_script.pathSettings[i].name;
        }
    }

    private void PathSettingSelectChange()
    {
        prop_pointer_PS.intValue = select_PS;
        if(select_PS != 0)
        {
            prop_CD = prop_PS.GetArrayElementAtIndex(select_PS - 1).FindPropertyRelative("circleDatas");
            GetReorderableList();
        }
    }

    private void GetReorderableList()
    {
        prop_CD_RList = new ReorderableList(serializedObject, prop_CD, true, true, true, true);
        prop_CD_RList.elementHeight = 52;
        prop_CD_RList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty element = prop_CD.GetArrayElementAtIndex(index);

            Rect arect = rect;
            int spacing = 2;

            arect.height = 22;
            arect.width = 40;

            EditorGUI.LabelField(arect, (index + 1).ToString(), GUI.skin.button);

            arect.y += 2;
            arect.x += arect.width + spacing;
            arect.width = rect.width - 40 - spacing;
            arect.height -= 4;

            EditorGUI.PropertyField(arect, element.FindPropertyRelative("turnMode"));

            arect.y += 26;

            element.FindPropertyRelative("position").vector2Value = EditorGUI.Vector2Field(arect,"" ,element.FindPropertyRelative("position").vector2Value);
        };
        prop_CD_RList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, prop_CD.displayName);
       
    }

}