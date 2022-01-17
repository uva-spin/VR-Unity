Shader "Quark Sphere Glow"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1, 1, 1, 1)
        _ReadMask("Stencil Mask", Int) = 0
        //[Toggle(_ALPHAPREMULTIPLY_ON)] _PremultipliedAlpha("Premultiplied alpha", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }

        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Stencil {
            Ref 0
            ReadMask [_ReadMask]
            Comp Equal
        }

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHAPREMULTIPLY_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                float4 normalOS          : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float fogCoord  : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD2;
                float3 positionOS : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput =  GetVertexNormalInputs(input.normalOS.xyz);

                output.vertex = vertexInput.positionCS;
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                output.normalWS = normalInput.normalWS.xyz;
                output.positionWS = vertexInput.positionWS;
                output.positionOS = input.positionOS.xyz;

                return output;
            }

            float3 FresnelSchlick(float cosTheta, float3 F0)
            {
                return float3(F0 + (float3(1.0f, 1.0f, 1.0f) - F0) * pow(1.0f - cosTheta, 5.0f));
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half3 color = _Color.rgb;
                half alpha = _Color.a;

                color = MixFog(color, input.fogCoord);
                alpha = OutputAlpha(alpha);

                float3 viewDirWS = GetCameraPositionWS() - input.positionWS;
                float NdotL = dot(normalize(input.normalWS), normalize(viewDirWS));

                float coeff = saturate(NdotL * NdotL * NdotL * NdotL * NdotL * NdotL);
                //coeff = coeff * saturate((cnoise(input.positionOS * 2.0f + _Time.xyz * 1.0f) + 1.0f) / 2.0f + 0.3f);
                //float koeff = 1.0f - FresnelSchlick(NdotL, 0.6f);

                color = color * coeff;
                alpha = alpha * coeff;

#ifdef _ALPHAPREMULTIPLY_ON
                color *= alpha;
#endif
                return half4(color, alpha);
            }
            ENDHLSL
        }
        Pass
        {
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaUnlit

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
