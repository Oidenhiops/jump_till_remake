// Shader de transición "iris" / círculo para UI (Canvas) - Unity 6 / URP
// Uso: asígnalo a un Material y ponlo en un Image que cubra toda la pantalla.
//  _Radius = 1  -> pantalla completamente abierta (no se ve nada del panel)
//  _Radius = 0  -> fundido total (se ve la textura/color, normalmente negro)
//  Valores intermedios -> se ve un círculo centrado, independiente del aspect ratio.
Shader "UI/CircleWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura del panel", 2D) = "white" {}
        _Color ("Tinte", Color) = (0,0,0,1)
        _Radius ("Radio (0 = cerrado, 1 = abierto)", Range(0, 1)) = 1
        _Softness ("Suavizado del borde", Range(0, 0.5)) = 0.02
        _Center ("Centro (0-1)", Vector) = (0.5, 0.5, 0, 0)

        // Requeridos por el sistema de máscaras de uGUI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _Center;
                float  _Radius;
                float  _Softness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color      = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Coordenadas de pantalla en 0..1, independientes del tamaño del rect
                float2 sp = IN.screenPos.xy / IN.screenPos.w;

                // Corrección de aspecto: el círculo se mide en "unidades de alto de pantalla"
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = (sp - _Center.xy) * float2(aspect, 1.0);

                // Distancia máxima hasta la esquina más lejana (para que radio=1 descubra toda la pantalla)
                float2 c = _Center.xy;
                float2 farCorner = max(c, 1.0 - c) * float2(aspect, 1.0);
                float maxDist = length(farCorner);

                float dist = length(p);
                float band = _Softness * 2.0;
                // El radio incluye la banda de suavizado, para que en 1 la pantalla quede totalmente limpia
                float r = _Radius * (maxDist + band);

                // La banda de suavizado queda por DENTRO del círculo: el borde exterior (opaco) está en r.
                // Así con radio 0 el panel es 100% opaco incluso en el centro.
                // 0 dentro del círculo (transparente), 1 fuera (se ve el panel)
                float mask = smoothstep(r - band, r, dist);

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 col = tex * IN.color;
                col.a *= mask;
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
