Shader "Game/UI/Energy Flow"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI Texture", 2D) = "white" {}
        _EnergyTex ("Energy Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FlowVelocity ("Flow Velocity (UV per second)", Vector) = (0.08,0.02,0,0)
        _FillAmount ("Fill Amount", Range(0,1)) = 1
        _WaveAmplitude ("Wave Amplitude", Range(0,0.05)) = 0.008
        _WaveFrequency ("Wave Frequency", Range(0.1,10)) = 2
        _WaveSpeed ("Wave Speed", Range(0,5)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0.0001,0.02)) = 0.002

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
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
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct Attributes
            {
                float4 position : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _EnergyTex;
            // Explicit sampler keeps the source texture's importer settings unchanged.
            SamplerState energy_linear_repeat_sampler;
            float4 _EnergyTex_ST;
            float4 _Color;
            float4 _FlowVelocity;
            float4 _ClipRect;
            float _FillAmount, _WaveAmplitude, _WaveFrequency, _WaveSpeed, _EdgeSoftness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.localPosition = input.position;
                output.position = UnityObjectToClipPos(input.position);
                output.color = input.color * _Color;
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 flowUV = input.uv * _EnergyTex_ST.xy + _EnergyTex_ST.zw - _Time.y * _FlowVelocity.xy;
                float4 color = _EnergyTex.Sample(energy_linear_repeat_sampler, flowUV) * input.color;

                // Surface coordinates stay fixed while the texture flows underneath.
                float phase = 6.2831853 * (_WaveFrequency * input.uv.x - _Time.y * _WaveSpeed);
                float wave = _WaveAmplitude * (0.65 * sin(phase) + 0.35 * sin(phase * 1.73 + _Time.y * _WaveSpeed + 2.3));
                float surface = _FillAmount + wave;
                float fill = smoothstep(-_EdgeSoftness, _EdgeSoftness, surface - input.uv.y);
                if (_FillAmount <= 0) fill = 0;
                if (_FillAmount >= 1) fill = 1;
                color.a *= fill;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.localPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                return color;
            }
            ENDHLSL
        }
    }
}
