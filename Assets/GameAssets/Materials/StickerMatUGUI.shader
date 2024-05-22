Shader "Custom/StickerUGUI"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
        _Color("Color", Color) = (1, 0, 0, 1)
        _Thickness("Thickness", Float) = 5
        
        _StencilComp("_StencilComp", Float) = 8
        _Stencil("_Stencil", Float) = 0
        _StencilOp("_StencilOp", Float) = 0
        _StencilWriteMask("_StencilWriteMask", Float) = 255
        _StencilReadMask("_StencilReadMask", Float) = 255
        _ColorMask("_ColorMask", Float) = 15
        
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalSpriteUnlitSubTarget"
        }
        Pass
        {
            Name "Sprite Unlit"
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest [unity_GUIZTestMode]
        ZWrite Off
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITEUNLIT
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Color;
        float _Thickness;
        float _StencilComp;
        float _Stencil;
        float _StencilOp;
        float _StencilWriteMask;
        float _StencilReadMask;
        float _ColorMask;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.tex, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.samplerstate, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_R_4_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.r;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_G_5_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.g;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_B_6_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.b;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.a;
            float4 _Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4 = _Color;
            UnityTexture2D _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float = _Thickness;
            float _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float;
            Unity_Multiply_float_float(_Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float, 0.01, _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float);
            float2 _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2 = float2(_Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float, 0);
            float2 _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2, _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2);
            float4 _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.tex, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.samplerstate, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2) );
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_R_4_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.r;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_G_5_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.g;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_B_6_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.b;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.a;
            UnityTexture2D _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_b7aae40273264202a516ab84830af2e3_Out_0_Float = _Thickness;
            float _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float;
            Unity_Multiply_float_float(_Property_b7aae40273264202a516ab84830af2e3_Out_0_Float, -0.01, _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float);
            float2 _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2 = float2(_Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float, 0);
            float2 _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2, _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2);
            float4 _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.tex, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.samplerstate, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2) );
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_R_4_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.r;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_G_5_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.g;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_B_6_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.b;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.a;
            float _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float, _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float, _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float);
            float _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float;
            Unity_Clamp_float(_Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float, 0, 1, _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float);
            UnityTexture2D _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float = _Thickness;
            float _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float;
            Unity_Multiply_float_float(_Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float, -0.01, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2 = float2(0, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2, _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2);
            float4 _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.tex, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.samplerstate, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2) );
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_R_4_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.r;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_G_5_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.g;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_B_6_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.b;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.a;
            UnityTexture2D _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float = _Thickness;
            float _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float;
            Unity_Multiply_float_float(_Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float, 0.01, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2 = float2(0, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2, _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2);
            float4 _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.tex, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.samplerstate, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2) );
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_R_4_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.r;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_G_5_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.g;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_B_6_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.b;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.a;
            float _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float, _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float, _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float);
            float _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float;
            Unity_Clamp_float(_Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float, 0, 1, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float);
            float _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float;
            Unity_Add_float(_Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float, _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float);
            float _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float;
            Unity_Clamp_float(_Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float, 0, 1, _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float);
            float _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float;
            Unity_Subtract_float(_Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float, _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float, _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float);
            float4 _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4, (_Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float.xxxx), _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4);
            float4 _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4;
            Unity_Add_float4(_SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4, _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4, _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4);
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_R_1_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[0];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_G_2_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[1];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_B_3_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[2];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[3];
            surface.BaseColor = (_Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4.xyz);
            surface.Alpha = _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Color;
        float _Thickness;
        float _StencilComp;
        float _Stencil;
        float _StencilOp;
        float _StencilWriteMask;
        float _StencilReadMask;
        float _ColorMask;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.tex, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.samplerstate, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_R_4_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.r;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_G_5_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.g;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_B_6_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.b;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.a;
            float4 _Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4 = _Color;
            UnityTexture2D _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float = _Thickness;
            float _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float;
            Unity_Multiply_float_float(_Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float, 0.01, _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float);
            float2 _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2 = float2(_Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float, 0);
            float2 _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2, _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2);
            float4 _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.tex, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.samplerstate, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2) );
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_R_4_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.r;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_G_5_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.g;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_B_6_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.b;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.a;
            UnityTexture2D _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_b7aae40273264202a516ab84830af2e3_Out_0_Float = _Thickness;
            float _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float;
            Unity_Multiply_float_float(_Property_b7aae40273264202a516ab84830af2e3_Out_0_Float, -0.01, _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float);
            float2 _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2 = float2(_Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float, 0);
            float2 _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2, _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2);
            float4 _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.tex, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.samplerstate, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2) );
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_R_4_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.r;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_G_5_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.g;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_B_6_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.b;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.a;
            float _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float, _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float, _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float);
            float _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float;
            Unity_Clamp_float(_Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float, 0, 1, _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float);
            UnityTexture2D _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float = _Thickness;
            float _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float;
            Unity_Multiply_float_float(_Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float, -0.01, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2 = float2(0, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2, _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2);
            float4 _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.tex, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.samplerstate, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2) );
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_R_4_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.r;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_G_5_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.g;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_B_6_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.b;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.a;
            UnityTexture2D _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float = _Thickness;
            float _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float;
            Unity_Multiply_float_float(_Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float, 0.01, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2 = float2(0, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2, _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2);
            float4 _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.tex, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.samplerstate, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2) );
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_R_4_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.r;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_G_5_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.g;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_B_6_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.b;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.a;
            float _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float, _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float, _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float);
            float _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float;
            Unity_Clamp_float(_Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float, 0, 1, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float);
            float _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float;
            Unity_Add_float(_Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float, _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float);
            float _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float;
            Unity_Clamp_float(_Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float, 0, 1, _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float);
            float _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float;
            Unity_Subtract_float(_Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float, _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float, _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float);
            float4 _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4, (_Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float.xxxx), _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4);
            float4 _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4;
            Unity_Add_float4(_SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4, _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4, _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4);
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_R_1_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[0];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_G_2_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[1];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_B_3_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[2];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[3];
            surface.Alpha = _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Color;
        float _Thickness;
        float _StencilComp;
        float _Stencil;
        float _StencilOp;
        float _StencilWriteMask;
        float _StencilReadMask;
        float _ColorMask;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.tex, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.samplerstate, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_R_4_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.r;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_G_5_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.g;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_B_6_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.b;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.a;
            float4 _Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4 = _Color;
            UnityTexture2D _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float = _Thickness;
            float _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float;
            Unity_Multiply_float_float(_Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float, 0.01, _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float);
            float2 _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2 = float2(_Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float, 0);
            float2 _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2, _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2);
            float4 _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.tex, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.samplerstate, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2) );
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_R_4_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.r;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_G_5_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.g;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_B_6_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.b;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.a;
            UnityTexture2D _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_b7aae40273264202a516ab84830af2e3_Out_0_Float = _Thickness;
            float _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float;
            Unity_Multiply_float_float(_Property_b7aae40273264202a516ab84830af2e3_Out_0_Float, -0.01, _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float);
            float2 _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2 = float2(_Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float, 0);
            float2 _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2, _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2);
            float4 _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.tex, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.samplerstate, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2) );
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_R_4_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.r;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_G_5_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.g;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_B_6_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.b;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.a;
            float _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float, _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float, _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float);
            float _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float;
            Unity_Clamp_float(_Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float, 0, 1, _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float);
            UnityTexture2D _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float = _Thickness;
            float _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float;
            Unity_Multiply_float_float(_Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float, -0.01, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2 = float2(0, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2, _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2);
            float4 _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.tex, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.samplerstate, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2) );
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_R_4_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.r;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_G_5_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.g;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_B_6_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.b;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.a;
            UnityTexture2D _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float = _Thickness;
            float _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float;
            Unity_Multiply_float_float(_Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float, 0.01, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2 = float2(0, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2, _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2);
            float4 _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.tex, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.samplerstate, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2) );
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_R_4_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.r;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_G_5_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.g;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_B_6_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.b;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.a;
            float _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float, _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float, _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float);
            float _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float;
            Unity_Clamp_float(_Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float, 0, 1, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float);
            float _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float;
            Unity_Add_float(_Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float, _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float);
            float _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float;
            Unity_Clamp_float(_Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float, 0, 1, _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float);
            float _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float;
            Unity_Subtract_float(_Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float, _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float, _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float);
            float4 _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4, (_Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float.xxxx), _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4);
            float4 _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4;
            Unity_Add_float4(_SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4, _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4, _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4);
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_R_1_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[0];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_G_2_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[1];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_B_3_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[2];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[3];
            surface.Alpha = _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Sprite Unlit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITEFORWARD
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Color;
        float _Thickness;
        float _StencilComp;
        float _Stencil;
        float _StencilOp;
        float _StencilWriteMask;
        float _StencilReadMask;
        float _ColorMask;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.tex, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.samplerstate, _Property_812279357b9f4a3eb574896a72be6af7_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_R_4_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.r;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_G_5_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.g;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_B_6_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.b;
            float _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float = _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4.a;
            float4 _Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4 = _Color;
            UnityTexture2D _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float = _Thickness;
            float _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float;
            Unity_Multiply_float_float(_Property_904b3d4f7b75458e99841b46269d70db_Out_0_Float, 0.01, _Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float);
            float2 _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2 = float2(_Multiply_bab7e90e936945fca2803e81e43f2261_Out_2_Float, 0);
            float2 _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b1724200589b4227bef06ed4e5f676e4_Out_0_Vector2, _TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2);
            float4 _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.tex, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.samplerstate, _Property_ee1f34bda8ff40e88912242eeb6e8c74_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_088cbc3dec4b48b080dac0cb15fe599d_Out_3_Vector2) );
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_R_4_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.r;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_G_5_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.g;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_B_6_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.b;
            float _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float = _SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_RGBA_0_Vector4.a;
            UnityTexture2D _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_b7aae40273264202a516ab84830af2e3_Out_0_Float = _Thickness;
            float _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float;
            Unity_Multiply_float_float(_Property_b7aae40273264202a516ab84830af2e3_Out_0_Float, -0.01, _Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float);
            float2 _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2 = float2(_Multiply_840440c2f8434611b1367f79a3a16566_Out_2_Float, 0);
            float2 _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_f064b5d345ba4388b3b7f4061cffff79_Out_0_Vector2, _TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2);
            float4 _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.tex, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.samplerstate, _Property_c773412c35d74e66b6623e677517b01a_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_53b1a5504ec345bb8290df2843cf2f2a_Out_3_Vector2) );
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_R_4_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.r;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_G_5_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.g;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_B_6_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.b;
            float _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float = _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_RGBA_0_Vector4.a;
            float _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_dfcd6fd959f34723a49f252003d1ee8d_A_7_Float, _SampleTexture2D_1996e37ee33344a0978540e4548a45e4_A_7_Float, _Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float);
            float _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float;
            Unity_Clamp_float(_Add_46d25c7c7c8d4f57adc6a7ddd6790e08_Out_2_Float, 0, 1, _Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float);
            UnityTexture2D _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float = _Thickness;
            float _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float;
            Unity_Multiply_float_float(_Property_faf40d7d1e1e4976896be560229d4587_Out_0_Float, -0.01, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2 = float2(0, _Multiply_92b08293a71e4cf0939507b3ddebb08d_Out_2_Float);
            float2 _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_7f767fd4943242299cc018395c251290_Out_0_Vector2, _TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2);
            float4 _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.tex, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.samplerstate, _Property_86b10f5f5db84c45b59c0f0b617416d5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_3a2f0f4886ad4f9ca900b6c8d6261d31_Out_3_Vector2) );
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_R_4_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.r;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_G_5_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.g;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_B_6_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.b;
            float _SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float = _SampleTexture2D_3341d383dca64db997dada3fb2d19148_RGBA_0_Vector4.a;
            UnityTexture2D _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float _Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float = _Thickness;
            float _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float;
            Unity_Multiply_float_float(_Property_ce897d55187643c48bdc3b60353557b7_Out_0_Float, 0.01, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2 = float2(0, _Multiply_2b5e2e997eaf4d5f855a2fedcd74dbf8_Out_2_Float);
            float2 _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_755997985935479ca1d764e81f0084be_Out_0_Vector2, _TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2);
            float4 _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.tex, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.samplerstate, _Property_abe7f8267b664f21a05033fffd2d8cc1_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_61d1d0fc7d514589977b4f129c5bad0d_Out_3_Vector2) );
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_R_4_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.r;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_G_5_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.g;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_B_6_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.b;
            float _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float = _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_RGBA_0_Vector4.a;
            float _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_3341d383dca64db997dada3fb2d19148_A_7_Float, _SampleTexture2D_072a9811fca94fe4a4d45c32e71e72f8_A_7_Float, _Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float);
            float _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float;
            Unity_Clamp_float(_Add_b792c4340092413cb9aae8b8d63db7c6_Out_2_Float, 0, 1, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float);
            float _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float;
            Unity_Add_float(_Clamp_8aa3459a02a84ae8859c2f0624a67fb8_Out_3_Float, _Clamp_e7a170b5a1154272bd93ebda73a59fa0_Out_3_Float, _Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float);
            float _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float;
            Unity_Clamp_float(_Add_c491c2cbf709442b8b177d8647c039a5_Out_2_Float, 0, 1, _Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float);
            float _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float;
            Unity_Subtract_float(_Clamp_a566e38a726a448e9d24b60613a06490_Out_3_Float, _SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_A_7_Float, _Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float);
            float4 _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_caaa4d4f03b344159d0edd5ac4260ea2_Out_0_Vector4, (_Subtract_1b06c55dc78a460cbfe2e8c549b1ac30_Out_2_Float.xxxx), _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4);
            float4 _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4;
            Unity_Add_float4(_SampleTexture2D_19b2f0d39e2447c592e7f6c2f356085d_RGBA_0_Vector4, _Multiply_7c4212319df3418e8220805fe0d9d71c_Out_2_Vector4, _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4);
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_R_1_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[0];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_G_2_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[1];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_B_3_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[2];
            float _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float = _Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4[3];
            surface.BaseColor = (_Add_7dab113b9c4c48959b3a14a866c819eb_Out_2_Vector4.xyz);
            surface.Alpha = _Split_bdeb4bb99f784f36b4ca14cf31db205b_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}