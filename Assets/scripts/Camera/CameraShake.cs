using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public void Shake(float duration, float shakeStrength)
    {
        if (!isActiveAndEnabled || duration <= 0f || shakeStrength <= 0f)
            return;

        var cameras = FindObjectsByType<CinemachineCamera>();
        if (cameras.Length == 0)
        {
            Debug.LogWarning("진동을 받을 활성 Cinemachine Camera가 없습니다.", this);
            return;
        }

        foreach (var camera in cameras)
        {
            var listener = camera.GetComponent<CinemachineImpulseListener>();
            if (listener == null)
            {
                listener = camera.gameObject.AddComponent<CinemachineImpulseListener>();
                listener.ChannelMask = 1;
                listener.Gain = 1f;
                listener.Use2DDistance = true;
                listener.UseCameraSpace = true;
                listener.ApplyAfter = CinemachineCore.Stage.Noise;
            }
        }

        // Each event owns its definition so overlapping shakes keep their duration.
        var impulse = new CinemachineImpulseDefinition
        {
            ImpulseChannel = 1,
            ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
            ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Rumble,
            ImpulseDuration = duration
        };
        impulse.CreateEvent(transform.position, new Vector3(0.3f, -1f, 0f).normalized * shakeStrength);
    }

    [ContextMenu("Test Shake (Play Mode)")]
    private void TestShake()
    {
        if (Application.isPlaying)
            Shake(0.3f, 0.5f);
    }
}
