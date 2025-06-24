using System;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;

namespace Unity.Cinemachine
{
    [ExecuteAlways, SaveDuringPlay]
    [CameraPipeline(CinemachineCore.Stage.Aim)]
    [AddComponentMenu("Cinemachine/Custom/Spline LookAt Zone (Distance Based)")]
    [DisallowMultipleComponent]
    public class CinemachineSplineLookAtZone : CinemachineComponentBase
    {
        [Serializable]
        public struct LookZone
        {
            public Transform target;
            public TrackerZoneVisualizer zoneVisualizer;
            public AnimationCurve fadeCurve;
        }

        public LookZone[] zones;

        public override bool IsValid => enabled;
        public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

        public override void MutateCameraState(ref CameraState state, float deltaTime)
        {
            if (!TryGetComponent(out CinemachineSplineDolly dolly) || dolly.Spline == null)
                return;

            float position = dolly.CameraPosition;

            LookZone? activeZone = null;
            float activeWeight = 0f;

            foreach (var zone in zones)
            {
                if (zone.target == null || zone.zoneVisualizer == null)
                    continue;

                var v = zone.zoneVisualizer;
                float weight = 0f;

                if (position < v.fadeInStart || position > v.fadeOutEnd)
                {
                    weight = 0f;
                }
                else if (position >= v.fullStart && position <= v.fullEnd)
                {
                    weight = 1f;
                }
                else if (position >= v.fadeInStart && position < v.fullStart)
                {
                    float tFadeIn = Mathf.InverseLerp(v.fadeInStart, v.fullStart, position);
                    weight = zone.fadeCurve.Evaluate(tFadeIn);
                }
                else if (position > v.fullEnd && position <= v.fadeOutEnd)
                {
                    float tFadeOut = Mathf.InverseLerp(v.fullEnd, v.fadeOutEnd, position);
                    weight = zone.fadeCurve.Evaluate(1f - tFadeOut);
                }

                // Stärke berücksichtigen
                weight *= v.effectStrength;

                if (weight > activeWeight)
                {
                    activeWeight = weight;
                    activeZone = zone;
                }
            }

            if (activeZone.HasValue && activeWeight > 0.001f)
            {
                Vector3 dir = activeZone.Value.target.position - state.RawPosition;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Vector3 up = state.ReferenceUp;
                    if (Vector3.Cross(dir, up).sqrMagnitude < 0.0001f)
                        up = Vector3.up;

                    Quaternion lookRotation = Quaternion.LookRotation(dir.normalized, up);
                    state.RawOrientation = Quaternion.Slerp(state.RawOrientation, lookRotation, activeWeight);
                    state.ReferenceLookAt = activeZone.Value.target.position;
                }
            }
        }
    }
}





/*
using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine
{
    [ExecuteAlways, SaveDuringPlay]
    [CameraPipeline(CinemachineCore.Stage.Aim)]
    [AddComponentMenu("Cinemachine/Custom/Spline LookAt Zone (Distance Based)")]
    [DisallowMultipleComponent]
    public class CinemachineSplineLookAtZone : CinemachineComponentBase
    {
        [Serializable]
        public struct LookZone
        {
            public Transform target;

            [Tooltip("Referenz zu einem TrackerZoneVisualizer-Empty, das alle wichtigen Werte enthält")]
            public TrackerZoneVisualizer zoneVisualizer;

            [Tooltip("Kurve für Ein- und Ausblendung (0-1 Bereich, gespiegelte Ease)")]
            public AnimationCurve fadeCurve;
        }

        public LookZone[] zones;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (zones != null)
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    if (zones[i].fadeCurve == null || zones[i].fadeCurve.keys.Length == 0)
                    {
                        zones[i].fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    }
                }
            }
        }

        public override bool IsValid => enabled && TryGetComponent(out CinemachineSplineDolly dolly);

        public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

        public override void MutateCameraState(ref CameraState state, float deltaTime)
        {
            if (!TryGetComponent(out CinemachineSplineDolly dolly) || dolly.Spline == null)
                return;

            float position = dolly.CameraPosition;

            LookZone? activeZone = null;
            float activeWeight = 0f;

            foreach (var zone in zones)
            {
                if (zone.target == null || zone.zoneVisualizer == null || zone.zoneVisualizer.splineContainer == null)
                    continue;

                var visualizer = zone.zoneVisualizer;
                var spline = visualizer.splineContainer.Spline;
                if (spline == null)
                    continue;

                // Berechne Distanz auf der Spline des Visualizer-Punkts
                Unity.Mathematics.float3 nearest;
                float t;
                UnityEngine.Splines.SplineUtility.GetNearestPoint(spline, visualizer.transform.position, out nearest, out t);

                float totalLength = UnityEngine.Splines.SplineUtility.CalculateLength(spline, visualizer.splineContainer.transform.localToWorldMatrix);
                float fullEffectCenter = totalLength * t;

                // Hole Werte aus Visualizer
                float fullEffectLength = visualizer.fullEffectLength;
                float fadeInDistance = visualizer.fadeInDistance;
                float fadeOutDistance = visualizer.fadeOutDistance;

                float fullStart = fullEffectCenter - fullEffectLength / 2f;
                float fullEnd = fullEffectCenter + fullEffectLength / 2f;
                float fadeInStart = fullStart - fadeInDistance;
                float fadeOutEnd = fullEnd + fadeOutDistance;

                float weight = 0f;

                if (position < fadeInStart || position > fadeOutEnd)
                {
                    weight = 0f;
                }
                else if (position >= fullStart && position <= fullEnd)
                {
                    weight = 1f;
                }
                else if (position >= fadeInStart && position < fullStart)
                {
                    float tFadeIn = Mathf.InverseLerp(fadeInStart, fullStart, position);
                    weight = zone.fadeCurve.Evaluate(tFadeIn);
                }
                else if (position > fullEnd && position <= fadeOutEnd)
                {
                    float tFadeOut = Mathf.InverseLerp(fullEnd, fadeOutEnd, position);
                    weight = zone.fadeCurve.Evaluate(1f - tFadeOut);
                }

                if (weight > activeWeight)
                {
                    activeWeight = weight;
                    activeZone = zone;
                }
            }

            if (activeZone.HasValue && activeWeight > 0.001f)
            {
                Vector3 dir = activeZone.Value.target.position - state.RawPosition;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Vector3 up = state.ReferenceUp;
                    if (Vector3.Cross(dir, up).sqrMagnitude < 0.0001f)
                        up = Vector3.up;

                    Quaternion lookRotation = Quaternion.LookRotation(dir.normalized, up);
                    state.RawOrientation = Quaternion.Slerp(state.RawOrientation, lookRotation, activeWeight);
                    state.ReferenceLookAt = activeZone.Value.target.position;
                }
            }
        }
    }
}














using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine
{
    [ExecuteAlways, SaveDuringPlay]
    [CameraPipeline(CinemachineCore.Stage.Aim)]
    [AddComponentMenu("Cinemachine/Custom/Spline LookAt Zone (Distance Based)")]
    [DisallowMultipleComponent]
    public class CinemachineSplineLookAtZone : CinemachineComponentBase
    {
        [Serializable]
        public struct LookZone
        {
            public Transform target;

            [Tooltip("Mittelpunkt des 100% Tracking Bereichs (Meter auf der Spline-Distanz)")]
            public float fullEffectCenter;

            [Tooltip("Länge des Bereichs mit 100% Tracking (Meter)")]
            public float fullEffectLength;

            [Tooltip("Dauer des Fade-In (Meter vor vollem Effekt)")]
            public float fadeInDistance;

            [Tooltip("Dauer des Fade-Out (Meter nach vollem Effekt)")]
            public float fadeOutDistance;

            [Tooltip("Kurve für Ein- und Ausblendung (0-1 Bereich, gespiegelte Ease)")]
            public AnimationCurve fadeCurve;
        }

        public LookZone[] zones;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (zones != null)
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    if (zones[i].fadeCurve == null || zones[i].fadeCurve.keys.Length == 0)
                    {
                        zones[i].fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    }
                }
            }
        }

        public override bool IsValid => enabled && TryGetComponent(out CinemachineSplineDolly dolly);

        public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

        public override void MutateCameraState(ref CameraState state, float deltaTime)
        {
            if (!TryGetComponent(out CinemachineSplineDolly dolly) || dolly.Spline == null)
                return;

            float position = dolly.CameraPosition;

            LookZone? activeZone = null;
            float activeWeight = 0f;

            foreach (var zone in zones)
            {
                if (zone.target == null) continue;

                // Berechnung der Grenzen
                float fullStart = zone.fullEffectCenter - zone.fullEffectLength / 2f;
                float fullEnd = zone.fullEffectCenter + zone.fullEffectLength / 2f;

                float fadeInStart = fullStart - zone.fadeInDistance;
                float fadeOutEnd = fullEnd + zone.fadeOutDistance;

                float weight = 0f;

                if (position < fadeInStart || position > fadeOutEnd)
                {
                    weight = 0f; // außerhalb aller Bereiche
                }
                else if (position >= fullStart && position <= fullEnd)
                {
                    weight = 1f; // voller Effekt
                }
                else if (position >= fadeInStart && position < fullStart)
                {
                    // Fade-In (0 -> 1)
                    float t = Mathf.InverseLerp(fadeInStart, fullStart, position);
                    weight = zone.fadeCurve.Evaluate(t);
                }
                else if (position > fullEnd && position <= fadeOutEnd)
                {
                    // Fade-Out (1 -> 0)
                    float t = Mathf.InverseLerp(fullEnd, fadeOutEnd, position);
                    // Spiegelung der Kurve (reverse)
                    weight = zone.fadeCurve.Evaluate(1f - t);
                }

                if (weight > activeWeight)
                {
                    activeWeight = weight;
                    activeZone = zone;
                }
            }

            if (activeZone.HasValue && activeWeight > 0.001f)
            {
                Vector3 dir = activeZone.Value.target.position - state.RawPosition;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Vector3 up = state.ReferenceUp;
                    if (Vector3.Cross(dir, up).sqrMagnitude < 0.0001f)
                        up = Vector3.up;

                    Quaternion lookRotation = Quaternion.LookRotation(dir.normalized, up);
                    state.RawOrientation = Quaternion.Slerp(state.RawOrientation, lookRotation, activeWeight);
                    state.ReferenceLookAt = activeZone.Value.target.position;
                }
            }
        }
    }
}


















using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine
{
    [ExecuteAlways, SaveDuringPlay]
    [CameraPipeline(CinemachineCore.Stage.Aim)]
    [AddComponentMenu("Cinemachine/Custom/Spline LookAt Zone (Distance Based)")]
    [DisallowMultipleComponent]
    public class CinemachineSplineLookAtZone : CinemachineComponentBase
    {
        [Serializable]
        public struct LookZone
        {
            public Transform target;
            public float startDistance;
            public float endDistance;
            public AnimationCurve weightCurve;  // nicht direkt initialisiert
        }

        public LookZone[] zones;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (zones != null)
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    // Falls weightCurve null ist, auf Standard setzen
                    if (zones[i].weightCurve == null || zones[i].weightCurve.keys.Length == 0)
                    {
                        zones[i].weightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    }
                }
            }
        }

        public override bool IsValid => enabled && TryGetComponent(out CinemachineSplineDolly dolly);

        public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

        public override void MutateCameraState(ref CameraState state, float deltaTime)
        {
            if (!TryGetComponent(out CinemachineSplineDolly dolly) || dolly.Spline == null)
                return;

            float position = dolly.CameraPosition;

            LookZone? activeZone = null;
            float activeWeight = 0f;

            foreach (var zone in zones)
            {
                if (zone.target == null) continue;
                if (position < zone.startDistance || position > zone.endDistance) continue;

                float zoneLength = zone.endDistance - zone.startDistance;
                if (zoneLength <= 0.001f) continue;

                float t = Mathf.InverseLerp(zone.startDistance, zone.endDistance, position);

                // Aufteilen in zwei Hälften: hinsehen (0–0.5), wegsehen (0.5–1)
                float curveT = t <= 0.5f
                    ? Mathf.InverseLerp(0f, 0.5f, t)                     // 0 → 1
                    : Mathf.InverseLerp(1f, 0.5f, t);                    // 0 → 1 rückwärts

                float weight = zone.weightCurve.Evaluate(curveT);

                if (weight > activeWeight)
                {
                    activeZone = zone;
                    activeWeight = weight;
                }
            }

            if (activeZone.HasValue && activeWeight > 0.001f)
            {
                Vector3 dir = activeZone.Value.target.position - state.RawPosition;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Vector3 up = state.ReferenceUp;
                    if (Vector3.Cross(dir, up).sqrMagnitude < 0.0001f)
                        up = Vector3.up;

                    Quaternion lookRotation = Quaternion.LookRotation(dir.normalized, up);
                    state.RawOrientation = Quaternion.Slerp(state.RawOrientation, lookRotation, activeWeight);
                    state.ReferenceLookAt = activeZone.Value.target.position;
                }
            }
        }
    }
}
*/