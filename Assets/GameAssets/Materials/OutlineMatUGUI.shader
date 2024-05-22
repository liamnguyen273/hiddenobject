Shader "Custom/OutlineUGUI"
{
    Properties
    {
        [NoScaleOffset]_MainTex("_MainTex", 2D) = "white" {}
        _Thickness("Thickness", Float) = 2
        
        _Color("Color", Color) = (0, 0, 0, 1)
        _Stencil("_Stencil", Float) = 0
        _StencilComp("_StencilComp", Float) = 8
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
        float _Thickness;
        float4 _Color;
        float _Stencil;
        float _StencilComp;
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
            UnityTexture2D _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float = _Thickness;
            float _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float;
            Unity_Multiply_float_float(_Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float, 0.01, _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float);
            float2 _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2 = float2(_Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float, 0);
            float2 _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2, _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2);
            float4 _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.tex, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.samplerstate, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2) );
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_R_4_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.r;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_G_5_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.g;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_B_6_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.b;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.a;
            UnityTexture2D _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float = _Thickness;
            float _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float;
            Unity_Multiply_float_float(_Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float, -0.01, _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float);
            float2 _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2 = float2(_Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float, 0);
            float2 _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2, _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2);
            float4 _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.tex, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.samplerstate, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2) );
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_R_4_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.r;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_G_5_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.g;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_B_6_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.b;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.a;
            float _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float, _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float, _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float);
            float _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float;
            Unity_Clamp_float(_Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float, 0, 1, _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float);
            UnityTexture2D _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float = _Thickness;
            float _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float;
            Unity_Multiply_float_float(_Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float, -0.01, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2 = float2(0, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2, _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2);
            float4 _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.tex, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.samplerstate, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2) );
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_R_4_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.r;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_G_5_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.g;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_B_6_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.b;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.a;
            UnityTexture2D _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float = _Thickness;
            float _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float;
            Unity_Multiply_float_float(_Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float, 0.01, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2 = float2(0, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2, _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2);
            float4 _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.tex, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.samplerstate, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2) );
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_R_4_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.r;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_G_5_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.g;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_B_6_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.b;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.a;
            float _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float, _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float, _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float);
            float _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float;
            Unity_Clamp_float(_Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float, 0, 1, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float);
            float _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float;
            Unity_Add_float(_Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float, _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float);
            float _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float;
            Unity_Clamp_float(_Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float, 0, 1, _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float);
            UnityTexture2D _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.tex, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.samplerstate, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_R_4_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.r;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_G_5_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.g;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_B_6_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.b;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.a;
            float _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float;
            Unity_Subtract_float(_Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float, _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float, _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float);
            float4 _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4 = _Color;
            float4 _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float.xxxx), _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4, _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4);
            float4 _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4;
            Unity_Add_float4(_Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4, (_SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float.xxxx), _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4);
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_R_1_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[0];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_G_2_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[1];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_B_3_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[2];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[3];
            surface.BaseColor = (_Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4.xyz);
            surface.Alpha = _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float;
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
        float _Thickness;
        float4 _Color;
        float _Stencil;
        float _StencilComp;
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
            UnityTexture2D _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float = _Thickness;
            float _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float;
            Unity_Multiply_float_float(_Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float, 0.01, _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float);
            float2 _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2 = float2(_Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float, 0);
            float2 _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2, _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2);
            float4 _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.tex, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.samplerstate, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2) );
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_R_4_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.r;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_G_5_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.g;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_B_6_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.b;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.a;
            UnityTexture2D _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float = _Thickness;
            float _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float;
            Unity_Multiply_float_float(_Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float, -0.01, _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float);
            float2 _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2 = float2(_Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float, 0);
            float2 _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2, _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2);
            float4 _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.tex, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.samplerstate, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2) );
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_R_4_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.r;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_G_5_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.g;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_B_6_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.b;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.a;
            float _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float, _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float, _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float);
            float _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float;
            Unity_Clamp_float(_Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float, 0, 1, _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float);
            UnityTexture2D _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float = _Thickness;
            float _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float;
            Unity_Multiply_float_float(_Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float, -0.01, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2 = float2(0, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2, _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2);
            float4 _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.tex, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.samplerstate, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2) );
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_R_4_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.r;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_G_5_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.g;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_B_6_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.b;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.a;
            UnityTexture2D _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float = _Thickness;
            float _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float;
            Unity_Multiply_float_float(_Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float, 0.01, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2 = float2(0, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2, _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2);
            float4 _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.tex, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.samplerstate, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2) );
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_R_4_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.r;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_G_5_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.g;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_B_6_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.b;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.a;
            float _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float, _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float, _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float);
            float _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float;
            Unity_Clamp_float(_Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float, 0, 1, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float);
            float _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float;
            Unity_Add_float(_Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float, _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float);
            float _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float;
            Unity_Clamp_float(_Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float, 0, 1, _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float);
            UnityTexture2D _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.tex, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.samplerstate, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_R_4_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.r;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_G_5_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.g;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_B_6_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.b;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.a;
            float _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float;
            Unity_Subtract_float(_Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float, _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float, _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float);
            float4 _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4 = _Color;
            float4 _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float.xxxx), _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4, _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4);
            float4 _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4;
            Unity_Add_float4(_Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4, (_SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float.xxxx), _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4);
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_R_1_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[0];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_G_2_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[1];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_B_3_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[2];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[3];
            surface.Alpha = _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float;
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
        float _Thickness;
        float4 _Color;
        float _Stencil;
        float _StencilComp;
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
            UnityTexture2D _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float = _Thickness;
            float _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float;
            Unity_Multiply_float_float(_Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float, 0.01, _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float);
            float2 _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2 = float2(_Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float, 0);
            float2 _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2, _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2);
            float4 _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.tex, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.samplerstate, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2) );
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_R_4_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.r;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_G_5_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.g;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_B_6_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.b;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.a;
            UnityTexture2D _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float = _Thickness;
            float _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float;
            Unity_Multiply_float_float(_Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float, -0.01, _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float);
            float2 _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2 = float2(_Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float, 0);
            float2 _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2, _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2);
            float4 _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.tex, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.samplerstate, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2) );
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_R_4_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.r;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_G_5_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.g;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_B_6_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.b;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.a;
            float _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float, _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float, _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float);
            float _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float;
            Unity_Clamp_float(_Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float, 0, 1, _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float);
            UnityTexture2D _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float = _Thickness;
            float _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float;
            Unity_Multiply_float_float(_Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float, -0.01, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2 = float2(0, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2, _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2);
            float4 _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.tex, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.samplerstate, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2) );
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_R_4_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.r;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_G_5_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.g;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_B_6_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.b;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.a;
            UnityTexture2D _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float = _Thickness;
            float _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float;
            Unity_Multiply_float_float(_Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float, 0.01, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2 = float2(0, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2, _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2);
            float4 _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.tex, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.samplerstate, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2) );
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_R_4_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.r;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_G_5_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.g;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_B_6_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.b;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.a;
            float _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float, _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float, _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float);
            float _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float;
            Unity_Clamp_float(_Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float, 0, 1, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float);
            float _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float;
            Unity_Add_float(_Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float, _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float);
            float _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float;
            Unity_Clamp_float(_Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float, 0, 1, _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float);
            UnityTexture2D _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.tex, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.samplerstate, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_R_4_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.r;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_G_5_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.g;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_B_6_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.b;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.a;
            float _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float;
            Unity_Subtract_float(_Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float, _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float, _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float);
            float4 _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4 = _Color;
            float4 _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float.xxxx), _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4, _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4);
            float4 _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4;
            Unity_Add_float4(_Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4, (_SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float.xxxx), _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4);
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_R_1_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[0];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_G_2_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[1];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_B_3_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[2];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[3];
            surface.Alpha = _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float;
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
        float _Thickness;
        float4 _Color;
        float _Stencil;
        float _StencilComp;
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
            UnityTexture2D _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float = _Thickness;
            float _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float;
            Unity_Multiply_float_float(_Property_3c8cc2f923644f5d90b5ebe62770e622_Out_0_Float, 0.01, _Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float);
            float2 _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2 = float2(_Multiply_20c0015e41764de9bb6e510f06030c7b_Out_2_Float, 0);
            float2 _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_c7ea33768d2a4c68a0c0d9d134cfc67b_Out_0_Vector2, _TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2);
            float4 _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.tex, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.samplerstate, _Property_58a99d03b5b84f848a37e97ebbf97936_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_dd5d35a9c879471787eb55db8d20c4ce_Out_3_Vector2) );
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_R_4_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.r;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_G_5_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.g;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_B_6_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.b;
            float _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float = _SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_RGBA_0_Vector4.a;
            UnityTexture2D _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float = _Thickness;
            float _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float;
            Unity_Multiply_float_float(_Property_aef7f3e486664ef0a492e55858e723c6_Out_0_Float, -0.01, _Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float);
            float2 _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2 = float2(_Multiply_7ac7619097d641d5895f790b0c4ad6c7_Out_2_Float, 0);
            float2 _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_0c9da3a943df433d819f452c61407896_Out_0_Vector2, _TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2);
            float4 _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.tex, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.samplerstate, _Property_1ff72b8f7f0e4d28bd866692557942e5_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5027bf19878344cab8495533fc934abc_Out_3_Vector2) );
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_R_4_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.r;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_G_5_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.g;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_B_6_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.b;
            float _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float = _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_RGBA_0_Vector4.a;
            float _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_a37fb8d04aa6491ba469feb6659a74ad_A_7_Float, _SampleTexture2D_9e5e157e8c4d446a9e353897422bc258_A_7_Float, _Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float);
            float _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float;
            Unity_Clamp_float(_Add_0f2b6ef4e19b467191c75d0b071006ec_Out_2_Float, 0, 1, _Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float);
            UnityTexture2D _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float = _Thickness;
            float _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float;
            Unity_Multiply_float_float(_Property_679762d2f3ca4f999d36d1b96a0f6d5d_Out_0_Float, -0.01, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2 = float2(0, _Multiply_cec8751d57254097b6a504d5e34f7b0b_Out_2_Float);
            float2 _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_bfa58f2864f4408ba0fb5507200920fc_Out_0_Vector2, _TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2);
            float4 _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.tex, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.samplerstate, _Property_233cd7dd519747138a8fd662ef3a7a5c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_5b4c4a8f622f429c9aaf229440a6de8e_Out_3_Vector2) );
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_R_4_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.r;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_G_5_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.g;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_B_6_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.b;
            float _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float = _SampleTexture2D_55202f26857c4861815cda2eec7c3d62_RGBA_0_Vector4.a;
            UnityTexture2D _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float = _Thickness;
            float _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float;
            Unity_Multiply_float_float(_Property_4fd7e5bda282435390d674fa3b2d7971_Out_0_Float, 0.01, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2 = float2(0, _Multiply_97c007dfdf7649b68c081998c31f1a7f_Out_2_Float);
            float2 _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_487b32476d9644e89f16240631cec8c0_Out_0_Vector2, _TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2);
            float4 _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.tex, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.samplerstate, _Property_490332307dae4b79b1de70bb4538485c_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_373d05df1d2341e58dce8a1055e6b498_Out_3_Vector2) );
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_R_4_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.r;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_G_5_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.g;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_B_6_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.b;
            float _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float = _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_RGBA_0_Vector4.a;
            float _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float;
            Unity_Add_float(_SampleTexture2D_55202f26857c4861815cda2eec7c3d62_A_7_Float, _SampleTexture2D_3dbbff3ef6ce4ae5aba385f92b389107_A_7_Float, _Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float);
            float _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float;
            Unity_Clamp_float(_Add_acdce4d4bfb34c8993a5f190748a5089_Out_2_Float, 0, 1, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float);
            float _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float;
            Unity_Add_float(_Clamp_31385bd4589c41da8af1713e6f4e1fb5_Out_3_Float, _Clamp_31309356d45147798def133c37cfbffe_Out_3_Float, _Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float);
            float _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float;
            Unity_Clamp_float(_Add_41fc6b5ee9c74f99b6c74d8717832e58_Out_2_Float, 0, 1, _Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float);
            UnityTexture2D _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.tex, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.samplerstate, _Property_e376c4bbd1b5442fb1b3a07a1115510b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_R_4_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.r;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_G_5_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.g;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_B_6_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.b;
            float _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float = _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_RGBA_0_Vector4.a;
            float _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float;
            Unity_Subtract_float(_Clamp_50a4c7bc11c14306aee6036661828cd3_Out_3_Float, _SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float, _Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float);
            float4 _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4 = _Color;
            float4 _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_16dc00356f2645c7924458f61f84c469_Out_2_Float.xxxx), _Property_5befa9796d25457583b708a808fa72c5_Out_0_Vector4, _Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4);
            float4 _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4;
            Unity_Add_float4(_Multiply_7afae2a871aa4cf987ac413d2afd8899_Out_2_Vector4, (_SampleTexture2D_a10b1b314b004e108b42a07162d5ec4d_A_7_Float.xxxx), _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4);
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_R_1_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[0];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_G_2_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[1];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_B_3_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[2];
            float _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float = _Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4[3];
            surface.BaseColor = (_Add_6fdb19c4b1514753bdb5f7d1e3c22122_Out_2_Vector4.xyz);
            surface.Alpha = _Split_30aa375b05ca4c4c8e015f2fb4719b59_A_4_Float;
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