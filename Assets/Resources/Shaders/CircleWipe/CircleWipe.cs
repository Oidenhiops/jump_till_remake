using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la transición de círculo (iris). Ponlo en el Image que usa el material
/// "UI/CircleWipe" (panel) o "UI/CircleWipeMask" (con componente Mask).
/// Radio 1 = pantalla abierta, 0 = fundido a negro / cerrado.
/// </summary>
public class CircleWipe : MonoBehaviour
{
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int CenterId = Shader.PropertyToID("_Center");

    [SerializeField] private float duration = 0.8f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, Range(0f, 1f)] private float startRadius = 1f;

    public Image image;
    private Material material;
    private float radius;

    public float Radius
    {
        get => radius;
        set
        {
            radius = Mathf.Clamp01(value);
            SetFloat(RadiusId, radius);
        }
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

    /// <summary>Centra el círculo en un punto del mundo (por ejemplo la posición del jugador).</summary>
    public void SetCenterFromWorld(Vector3 worldPos, Camera cam)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        SetVector(CenterId, new Vector4(vp.x, vp.y, 0, 0));
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

    // --- Escritura de propiedades ---
    // Con un componente Mask (o si el Image está dentro de otra máscara) uGUI NO dibuja con
    // image.material, sino con una copia interna con el stencil configurado: materialForRendering.
    // Esa copia se crea una sola vez, así que hay que escribir la propiedad en las dos.

    private void SetFloat(int id, float value)
    {
        material.SetFloat(id, value);
        Material rendering = image.materialForRendering;
        if (rendering != null && rendering != material) rendering.SetFloat(id, value);
    }

    private void SetVector(int id, Vector4 value)
    {
        material.SetVector(id, value);
        Material rendering = image.materialForRendering;
        if (rendering != null && rendering != material) rendering.SetVector(id, value);
    }
}
