#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpeedProfile))]
public class SpeedProfileEditor : Editor

{
    private const float PreviewHeight = 100f;
    private const float Padding = 10f;

    public override void OnInspectorGUI()
    {
        SpeedProfile profile = (SpeedProfile)target;

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            profile.RegenerateSections();
            EditorUtility.SetDirty(profile);
        }

        GUILayout.Space(20);
        EditorGUILayout.LabelField("Preview Graph", EditorStyles.boldLabel);

        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - Padding * 2, PreviewHeight);
        GUI.Box(rect, GUIContent.none);

        if (profile.speedSections == null || profile.speedSections.Count < 2)
        {
            EditorGUI.LabelField(rect, "Not enough data to preview.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        Handles.BeginGUI();

        float minX = profile.speedSections[0].startDistance;
        float maxX = profile.speedSections[^1].startDistance;

        float minY = 0.5f; // Preview Min
        float maxY = 2f;   // Preview Max

        Vector2 prev = Vector2.zero;
        for (int i = 0; i < profile.speedSections.Count; i++)
        {
            var s = profile.speedSections[i];

            float tX = Mathf.InverseLerp(minX, maxX, s.startDistance);
            float tY = Mathf.InverseLerp(minY, maxY, s.speedMultiplier);

            Vector2 point = new Vector2(
                Mathf.Lerp(rect.xMin + Padding, rect.xMax - Padding, tX),
                Mathf.Lerp(rect.yMax - Padding, rect.yMin + Padding, tY)
            );

            if (i > 0)
            {
                Handles.color = Color.cyan;
                Handles.DrawLine(prev, point);
            }

            Handles.color = Color.white;
            Handles.DrawSolidDisc(point, Vector3.forward, 3f);

            prev = point;
        }

        Handles.EndGUI();
    }
}
#endif





/*
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SpeedProfile))]
public class SpeedProfilePreviewEditor : Editor
{
    private const float PreviewHeight = 100f;
    private const float Padding = 10f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpeedProfile profile = (SpeedProfile)target;

        GUILayout.Space(20);
        EditorGUILayout.LabelField("Preview Graph", EditorStyles.boldLabel);

        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - Padding * 2, PreviewHeight);
        GUI.Box(rect, GUIContent.none);

        if (profile.speedSections == null || profile.speedSections.Count < 2)
        {
            EditorGUI.LabelField(rect, "Not enough data to preview.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        Handles.BeginGUI();

        float minX = profile.speedSections[0].startDistance;
        float maxX = profile.speedSections[^1].startDistance;

        float minY = 0.5f; // Minimum multiplier for preview scale
        float maxY = 2f;   // Max expected multiplier

        Vector2 prev = Vector2.zero;
        for (int i = 0; i < profile.speedSections.Count; i++)
        {
            var s = profile.speedSections[i];

            float tX = Mathf.InverseLerp(minX, maxX, s.startDistance);
            float tY = Mathf.InverseLerp(minY, maxY, s.speedMultiplier);

            Vector2 point = new Vector2(
                Mathf.Lerp(rect.xMin + Padding, rect.xMax - Padding, tX),
                Mathf.Lerp(rect.yMax - Padding, rect.yMin + Padding, tY)
            );

            if (i > 0)
            {
                Handles.color = Color.cyan;
                Handles.DrawLine(prev, point);
            }

            Handles.color = Color.white;
            Handles.DrawSolidDisc(point, Vector3.forward, 3f);

            prev = point;
        }

        Handles.EndGUI();
    }
}
#endif






#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(SpeedProfile))]
public class SpeedProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpeedProfile profile = (SpeedProfile)target;

        EditorGUILayout.LabelField("Speed Events", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Event"))
        {
            profile.speedEvents.Add(new HighLevelSpeedEvent());
        }

        for (int i = 0; i < profile.speedEvents.Count; i++)
        {
            var evt = profile.speedEvents[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Event {i}", EditorStyles.miniBoldLabel);

            evt.label = EditorGUILayout.TextField("Label", evt.label);
            evt.startDistance = EditorGUILayout.FloatField("Start Distance", evt.startDistance);
            evt.length = EditorGUILayout.FloatField("Length", evt.length);
            evt.easeIn = EditorGUILayout.FloatField("Ease In", evt.easeIn);
            evt.easeOut = EditorGUILayout.FloatField("Ease Out", evt.easeOut);
            evt.speedMultiplier = EditorGUILayout.FloatField("Speed Multiplier", evt.speedMultiplier);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▲", GUILayout.Width(30)) && i > 0)
            {
                var tmp = profile.speedEvents[i];
                profile.speedEvents[i] = profile.speedEvents[i - 1];
                profile.speedEvents[i - 1] = tmp;
                break;
            }

            if (GUILayout.Button("▼", GUILayout.Width(30)) && i < profile.speedEvents.Count - 1)
            {
                var tmp = profile.speedEvents[i];
                profile.speedEvents[i] = profile.speedEvents[i + 1];
                profile.speedEvents[i + 1] = tmp;
                break;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove"))
            {
                profile.speedEvents.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // Generiere die eigentlichen Sections neu
        if (GUI.changed)
        {
            profile.RegenerateSections();
            EditorUtility.SetDirty(profile);
        }
    }
}
#endif

















#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SpeedProfile))]
public class SpeedProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpeedProfile profile = (SpeedProfile)target;

        // Optional: automatisch sortieren nach StartDistance
        profile.speedSections = profile.speedSections.OrderBy(s => s.startDistance).ToList();

        EditorGUILayout.LabelField("Speed Events (Distance in meters)", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Event Section"))
        {
            profile.speedSections.Add(new SpeedSection());
        }

        for (int i = 0; i < profile.speedSections.Count; i++)
        {
            var section = profile.speedSections[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Event {i}", EditorStyles.miniBoldLabel);

            section.label = EditorGUILayout.TextField("Label", section.label);
            section.startDistance = EditorGUILayout.FloatField("Start Distance", section.startDistance);
            section.length = EditorGUILayout.FloatField("Length", section.length);
            section.easeIn = EditorGUILayout.FloatField("Ease In", section.easeIn);
            section.easeOut = EditorGUILayout.FloatField("Ease Out", section.easeOut);
            section.speedMultiplier = EditorGUILayout.FloatField("Speed Multiplier", section.speedMultiplier);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = i > 0;
            if (GUILayout.Button("▲", GUILayout.Width(30)))
            {
                var tmp = profile.speedSections[i];
                profile.speedSections[i] = profile.speedSections[i - 1];
                profile.speedSections[i - 1] = tmp;
                GUI.enabled = true;
                break;
            }

            GUI.enabled = i < profile.speedSections.Count - 1;
            if (GUILayout.Button("▼", GUILayout.Width(30)))
            {
                var tmp = profile.speedSections[i];
                profile.speedSections[i] = profile.speedSections[i + 1];
                profile.speedSections[i + 1] = tmp;
                GUI.enabled = true;
                break;
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove"))
            {
                profile.speedSections.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(profile);
        }
    }
}
#endif










#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SpeedProfile))]
public class SpeedProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpeedProfile profile = (SpeedProfile)target;

        // Optional: automatisch sortieren nach StartDistance
        profile.speedSections = profile.speedSections.OrderBy(s => s.startDistance).ToList();

        EditorGUILayout.LabelField("Speed Sections (Distance in meters)", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Section"))
        {
            profile.speedSections.Add(new SpeedSection());
        }

        for (int i = 0; i < profile.speedSections.Count; i++)
        {
            var section = profile.speedSections[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Section {i}", EditorStyles.miniBoldLabel);

            section.label = EditorGUILayout.TextField("Label", section.label);
            section.startDistance = EditorGUILayout.FloatField("Start Distance", section.startDistance);
            section.speedMultiplier = EditorGUILayout.FloatField("Speed Multiplier", section.speedMultiplier);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = i > 0;
            if (GUILayout.Button("▲", GUILayout.Width(30)))
            {
                var tmp = profile.speedSections[i];
                profile.speedSections[i] = profile.speedSections[i - 1];
                profile.speedSections[i - 1] = tmp;
                GUI.enabled = true;
                break;
            }

            GUI.enabled = i < profile.speedSections.Count - 1;
            if (GUILayout.Button("▼", GUILayout.Width(30)))
            {
                var tmp = profile.speedSections[i];
                profile.speedSections[i] = profile.speedSections[i + 1];
                profile.speedSections[i + 1] = tmp;
                GUI.enabled = true;
                break;
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove"))
            {
                profile.speedSections.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(profile);
        }
    }
}
#endif






#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SpeedProfile))]
public class SpeedProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SpeedProfile profile = (SpeedProfile)target;

        EditorGUILayout.LabelField("Speed Sections (Distance in meters)", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Section"))
        {
            profile.speedSections.Add(new SpeedSection());
        }

        for (int i = 0; i < profile.speedSections.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Section {i}", EditorStyles.miniBoldLabel);
            profile.speedSections[i].startDistance = EditorGUILayout.FloatField("Start Distance", profile.speedSections[i].startDistance);
            profile.speedSections[i].speedMultiplier = EditorGUILayout.FloatField("Speed Multiplier", profile.speedSections[i].speedMultiplier);

            if (GUILayout.Button("Remove"))
            {
                profile.speedSections.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(profile);
        }
    }
}
#endif
*/