using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la transición de círculo (iris). Ponlo en el Image que usa el material "UI/CircleWipe".
/// Radio 1 = pantalla abierta, 0 = fundido a negro.
/// </summary>
public class CircleWipe : MonoBehaviour
{
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");

    [SerializeField] private float duration = 0.8f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, Range(0f, 1f)] private float startRadius = 1f;

    public Image image;
    private Material material;

    public float Radius
    {
        get => material.GetFloat(RadiusId);
        set => material.SetFloat(RadiusId, Mathf.Clamp01(value));
    }

    private void Awake()
    {
        material = Instantiate(image.material);
        image.material = material;
        Radius = startRadius;
        image.raycastTarget = startRadius < 1f;
    }
    [NaughtyAttributes.Button]
    public void Enter()
    {
        _ = Play(1);
    }
    [NaughtyAttributes.Button]
    public void Exit()
    {
        _ = Play(0);
    }
    private void OnDestroy()
    {
        if (material != null) Destroy(material);
    }

    public async Awaitable Play(float targetRadius)
    {
        await Animate(targetRadius);
    }

    private async Awaitable Animate(float target)
    {
        float from = Radius;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Radius = Mathf.Lerp(from, target, curve.Evaluate(Mathf.Clamp01(t / duration)));
            await Awaitable.NextFrameAsync();
        }
        Radius = target;
        await Awaitable.NextFrameAsync();
    }
}
