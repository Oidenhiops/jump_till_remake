// Máscara de círculo para uGUI - Unity 6 / URP
// Uso: Image (pantalla completa) con componente Mask + material con este shader.
//      Los hijos del Image solo se verán DENTRO del círculo.
//  _Radius = 1  -> círculo cubre toda la pantalla (los hijos se ven completos)
//  _Radius = 0  -> círculo cerrado (no se ve nada de los hijos)
//  _Invert = 1  -> los hijos se ven FUERA del círculo en vez de dentro
// El círculo es independiente del aspect ratio y del tamaño del rect.
Shader "UI/CircleWipeMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte (solo si Show Mask Graphic)", Color) = (1,1,1,1)
        _Radius ("Radio (0 = cerrado, 1 = abierto)", Range(0, 1)) = 1
        _Softness ("Suavizado del borde", Range(0, 0.5)) = 0.02
        _Center ("Centro (0-1)", Vector) = (0.5, 0.5, 0, 0)
        [Toggle] _Invert ("Invertir (mostrar fuera del círculo)", Float) = 0

        // Requeridos por el sistema de máscaras de uGUI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 1
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

            // El componente Mask activa esta keyword para recortar por alfa
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float  _Invert;
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

                // Corrección de aspecto
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = (sp - _Center.xy) * float2(aspect, 1.0);

                // Distancia hasta la esquina más lejana (radio=1 cubre toda la pantalla)
                float2 c = _Center.xy;
                float2 farCorner = max(c, 1.0 - c) * float2(aspect, 1.0);
                float maxDist = length(farCorner);

                float dist = length(p);
                float band = _Softness * 2.0;
                float r = _Radius * (maxDist + band);

                // 1 dentro del círculo, 0 fuera. Banda de suavizado por dentro del borde,
                // así con radio 0 no queda ningún punto visible en el centro.
                float inside = 1.0 - smoothstep(r - band, r, dist);
                float mask = lerp(inside, 1.0 - inside, _Invert);

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 col = tex * IN.color;
                col.a *= mask;

                #ifdef UNITY_UI_ALPHACLIP
                    // Solo escribe el stencil (y por tanto muestra hijos) donde hay alfa
                    clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
