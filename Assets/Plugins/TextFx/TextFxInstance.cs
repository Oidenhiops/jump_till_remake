using System;
using UnityEngine;

/// <summary>
/// Un efecto activo aplicado a un rango de letras.
/// Se puede crear desde el inspector o por codigo con los helpers estaticos.
/// </summary>
[Serializable]
public class TextFxInstance
{
    public TextFxType type = TextFxType.Wave;

    [Tooltip("Primer caracter afectado. -1 = desde el inicio.")]
    public int from = -1;

    [Tooltip("Ultimo caracter afectado (inclusive). -1 = hasta el final.")]
    public int to = -1;

    [Tooltip("Intensidad del movimiento en unidades locales de TMP (o factor de escala segun el efecto).")]
    public float amplitude = 8f;

    [Tooltip("Que tan rapido oscila.")]
    public float speed = 6f;

    [Tooltip("Desfase entre letras contiguas. 0 = todas se mueven igual.")]
    public float frequency = 0.35f;

    [Tooltip("Color del efecto. Solo lo usan los efectos que lo necesitan (Outline).")]
    public Color color = Color.black;

    /// <summary>Identificador opcional para poder quitar el efecto luego con RemoveEffect(id).</summary>
    [HideInInspector] public string id;

    // ------------------------------------------------------------ ciclo de vida
    //
    // Estado de runtime: NO se serializa. Si se marcara solo con [HideInInspector], Unity lo
    // guardaria igual y, al agregar un elemento nuevo en el inspector (boton "+"), lo crearia
    // con todo en cero: stopAt = 0 => IsStopping = true y el efecto se quitaria en el primer frame.

    /// <summary>Momento en que el efecto entro en juego. -1 = lo pone TextFx en el primer frame.</summary>
    [NonSerialized] public float startTime = -1f;

    /// <summary>Momento a partir del cual el efecto debe terminar. -1 = infinito.</summary>
    [NonSerialized] public float stopAt = -1f;

    /// <summary>Minimo de oscilaciones completas antes de terminar (solo si stopAt >= 0).</summary>
    [NonSerialized] public int stopCycles = 1;

    /// <summary>Si es true espera a cerrar la oscilacion en curso antes de apagarse.</summary>
    [NonSerialized] public bool finishCycle = true;

    /// <summary>Rampa final para volver a la posicion base sin salto. 0 = corte seco.</summary>
    [NonSerialized] public float fadeOut = 0.12f;

    /// <summary>Peso interno 0..1 que usa la rampa final. No tocar a mano.</summary>
    [NonSerialized] public float weight = 1f;

    /// <summary>True si ya se le pidio terminar.</summary>
    public bool IsStopping => stopAt >= 0f;

    /// <summary>Duracion de una oscilacion completa en segundos. 0 = el efecto no es ciclico.</summary>
    public float CyclePeriod
    {
        get
        {
            float s = Mathf.Abs(speed);
            if (s <= 0.0001f) return 0f;

            switch (type)
            {
                case TextFxType.Wave:
                case TextFxType.Wiggle:
                case TextFxType.Bounce:
                case TextFxType.Pulse:
                case TextFxType.Swing:
                case TextFxType.Blink:
                    return 2f * Mathf.PI / s;

                case TextFxType.Rainbow:
                    return 1f / s;

                default:
                    return 0f; // Shake (ruido) y Outline (estatico) no tienen ciclo
            }
        }
    }

    /// <summary>
    /// True si el elemento fue creado con el boton "+" del inspector y todavia esta en ceros
    /// (Unity no aplica los valores por defecto de la clase en ese caso).
    /// </summary>
    public bool IsBlank => amplitude == 0f && speed == 0f && frequency == 0f;

    /// <summary>Copia los valores por defecto de su tipo (amplitud, velocidad, desfase, rango completo).</summary>
    public void ResetToDefaults()
    {
        TextFxInstance d = Default(type);
        from = d.from;
        to = d.to;
        amplitude = d.amplitude;
        speed = d.speed;
        frequency = d.frequency;
        color = d.color;
    }

    /// <summary>True si esta letra entra en el rango del efecto.</summary>
    public bool Covers(int charIndex, int totalChars)
    {
        int a = from < 0 ? 0 : from;
        int b = to < 0 ? totalChars - 1 : to;
        return charIndex >= a && charIndex <= b;
    }

    public TextFxInstance Clone()
    {
        return new TextFxInstance
        {
            type = type,
            from = from,
            to = to,
            amplitude = amplitude,
            speed = speed,
            frequency = frequency,
            color = color,
            id = id,
            stopCycles = stopCycles,
            finishCycle = finishCycle,
            fadeOut = fadeOut,
        };
    }

    /// <summary>Valores por defecto sensatos para cada tipo de efecto.</summary>
    public static TextFxInstance Default(TextFxType type, int from = -1, int to = -1)
    {
        TextFxInstance fx = new TextFxInstance { type = type, from = from, to = to };

        switch (type)
        {
            case TextFxType.Wave:
                fx.amplitude = 8f; fx.speed = 6f; fx.frequency = 0.35f; break;
            case TextFxType.Shake:
                fx.amplitude = 3f; fx.speed = 40f; fx.frequency = 0f; break;
            case TextFxType.Wiggle:
                fx.amplitude = 5f; fx.speed = 3f; fx.frequency = 0.5f; break;
            case TextFxType.Bounce:
                fx.amplitude = 14f; fx.speed = 5f; fx.frequency = 0.5f; break;
            case TextFxType.Pulse:
                fx.amplitude = 0.15f; fx.speed = 5f; fx.frequency = 0.3f; break;
            case TextFxType.Swing:
                fx.amplitude = 12f; fx.speed = 5f; fx.frequency = 0.4f; break;
            case TextFxType.Rainbow:
                fx.amplitude = 1f; fx.speed = 0.6f; fx.frequency = 0.08f; break;
            case TextFxType.Blink:
                fx.amplitude = 0.6f; fx.speed = 6f; fx.frequency = 0f; break;
            case TextFxType.Outline:
                // amplitude = grosor (0..1). Pasar de ~0.3 recorta si el font asset tiene poco padding.
                fx.amplitude = 0.2f; fx.speed = 0f; fx.frequency = 0f; fx.color = Color.black; break;
        }

        return fx;
    }
}

/// <summary>Color asignado a un rango concreto de letras.</summary>
[Serializable]
public class TextColorRange
{
    public int from;
    public int to;
    public Color colorA = Color.white;
    public Color colorB = Color.white;

    /// <summary>Si es true, interpola de colorA a colorB a lo largo del rango.</summary>
    public bool gradient;

    public bool Covers(int charIndex) => charIndex >= from && charIndex <= to;

    public Color Evaluate(int charIndex)
    {
        if (!gradient || to <= from) return colorA;
        float t = Mathf.InverseLerp(from, to, charIndex);
        return Color.Lerp(colorA, colorB, t);
    }
}
