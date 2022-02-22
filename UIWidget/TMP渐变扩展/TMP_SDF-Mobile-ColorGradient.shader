// Simplified SDF shader:
// - No Shading Option (bevel / bump / env map)
// - No Glow Option
// - Softness is applied on both side of the outline

Shader "TextMeshPro/Mobile/ColorGradient" {

Properties {
	_FaceColor			("Face Color", Color) = (1,1,1,1)
	_FaceDilate			("Face Dilate", Range(-1,1)) = 0

    _UseColorGradient   ("Use Gradient", float) = 0
    _ColorGradientTex   ("Gradient Tex", 2D) = "white" {}

	_OutlineColor		("Outline Color", Color) = (0,0,0,1)
	_OutlineWidth		("Outline Thickness", Range(0,1)) = 0
	_OutlineSoftness	("Outline Softness", Range(0,1)) = 0

	_UnderlayColor		("Border Color", Color) = (0,0,0,.5)
	_UnderlayOffsetX 	("Border OffsetX", Range(-1,1)) = 0
	_UnderlayOffsetY 	("Border OffsetY", Range(-1,1)) = 0
	_UnderlayDilate		("Border Dilate", Range(-1,1)) = 0
	_UnderlaySoftness 	("Border Softness", Range(0,1)) = 0

	_UnderlayColor2		("Border2 Color", Color) = (0,0,0,.5)
	_UnderlayOffsetX2 	("Border2 OffsetX", Range(-1,1)) = 0
	_UnderlayOffsetY2 	("Border2 OffsetY", Range(-1,1)) = 0
	_UnderlayDilate2	("Border2 Dilate", Range(-1,1)) = 0
	_UnderlaySoftness2 	("Border2 Softness", Range(0,1)) = 0

	_WeightNormal		("Weight Normal", float) = 0
	_WeightBold			("Weight Bold", float) = .5

	_ShaderFlags		("Flags", float) = 0
	_ScaleRatioA		("Scale RatioA", float) = 1
	_ScaleRatioB		("Scale RatioB", float) = 1
	_ScaleRatioC		("Scale RatioC", float) = 1

	_MainTex			("Font Atlas", 2D) = "white" {}
	_TextureWidth		("Texture Width", float) = 512
	_TextureHeight		("Texture Height", float) = 512
	_GradientScale		("Gradient Scale", float) = 5
	_ScaleX				("Scale X", float) = 1
	_ScaleY				("Scale Y", float) = 1
	_PerspectiveFilter	("Perspective Correction", Range(0, 1)) = 0.875
	_Sharpness			("Sharpness", Range(-1,1)) = 0

	_VertexOffsetX		("Vertex OffsetX", float) = 0
	_VertexOffsetY		("Vertex OffsetY", float) = 0

	_ClipRect			("Clip Rect", vector) = (-32767, -32767, 32767, 32767)
	_MaskSoftnessX		("Mask SoftnessX", float) = 0
	_MaskSoftnessY		("Mask SoftnessY", float) = 0
	
	_StencilComp		("Stencil Comparison", Float) = 8
	_Stencil			("Stencil ID", Float) = 0
	_StencilOp			("Stencil Operation", Float) = 0
	_StencilWriteMask	("Stencil Write Mask", Float) = 255
	_StencilReadMask	("Stencil Read Mask", Float) = 255
	
	_ColorMask			("Color Mask", Float) = 15
}

SubShader {
	Tags 
	{
		"Queue"="Transparent"
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
	}


	Stencil
	{
		Ref [_Stencil]
		Comp [_StencilComp]
		Pass [_StencilOp] 
		ReadMask [_StencilReadMask]
		WriteMask [_StencilWriteMask]
	}

	Cull [_CullMode]
	ZWrite Off
	Lighting Off
	Fog { Mode Off }
	ZTest [unity_GUIZTestMode]
	Blend One OneMinusSrcAlpha
	ColorMask [_ColorMask]

	Pass {
		CGPROGRAM
		#pragma vertex VertShader
		#pragma fragment PixShader
		#pragma shader_feature __ OUTLINE_ON
		#pragma shader_feature __ UNDERLAY_ON UNDERLAY_INNER

		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		#pragma multi_compile __ UNITY_UI_ALPHACLIP

		#include "UnityCG.cginc"
		#include "UnityUI.cginc"
		#include "TMPro_Properties.cginc"

		uniform float4	_UnderlayColor2;
		uniform half    _UnderlayOffsetX2;
		uniform half    _UnderlayOffsetY2;
		uniform half    _UnderlayDilate2;
		uniform half    _UnderlaySoftness2;

		struct vertex_t {
			UNITY_VERTEX_INPUT_INSTANCE_ID
			float4	vertex			: POSITION;
			float3	normal			: NORMAL;
			fixed4	color			: COLOR;
			float2	texcoord0		: TEXCOORD0;
			float2	texcoord1		: TEXCOORD1;
		};

		struct pixel_t {
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
			float4	vertex			: SV_POSITION;
			fixed4	faceColor		: COLOR;
			fixed4	outlineColor	: COLOR1;
			float4	texcoord0		: TEXCOORD0;			// Texture UV, Mask UV
			half4	param			: TEXCOORD1;			// Scale(x), BiasIn(y), BiasOut(z), Bias(w)
			half4	mask			: TEXCOORD2;			// Position in clip space(xy), Softness(zw)
			half2   gradientUV      : TEXCOORD3;	
			float4	texcoord1		: TEXCOORD4;			// Texture UV, alpha, reserved  inner outlay
			#if (UNDERLAY_ON | UNDERLAY_INNER)
			half3	underlayParam	: TEXCOORD5;			// Scale(x), Bias(y)
			float4	texcoord2		: TEXCOORD6;			// outer outlay
			#endif
		};


		pixel_t VertShader(vertex_t input)
		{
			pixel_t output;

			UNITY_INITIALIZE_OUTPUT(pixel_t, output);
			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_TRANSFER_INSTANCE_ID(input, output);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
			
			float bold = step(input.texcoord1.y, 0);

			float4 vert = input.vertex;
			vert.x += _VertexOffsetX;
			vert.y += _VertexOffsetY;
			float4 vPosition = UnityObjectToClipPos(vert);

			float2 pixelSize = vPosition.w;
			pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
			
			float scale = rsqrt(dot(pixelSize, pixelSize));
			scale *= abs(input.texcoord1.y) * _GradientScale * (_Sharpness + 1);
			if(UNITY_MATRIX_P[3][3] == 0) scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(input.normal.xyz), normalize(WorldSpaceViewDir(vert)))));

			float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
			weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5;

			float layerScale = scale;
			float layerScale2 = scale;

			scale /= 1 + (_OutlineSoftness * _ScaleRatioA * scale);
			float bias = (0.5 - weight) * scale - 0.5;
			float outline = _OutlineWidth * _ScaleRatioA * 0.5 * scale;

			float opacity = input.color.a;
			#if (UNDERLAY_ON | UNDERLAY_INNER)
			opacity = 1.0;
			#endif

			fixed4 faceColor = _FaceColor;
            if(_UseColorGradient < 0.5)
            {
                faceColor *= fixed4(input.color.rgb, opacity);
            }
			faceColor.rgb *= faceColor.a;

			fixed4 outlineColor = _OutlineColor;
			outlineColor.a *= opacity;
			outlineColor.rgb *= outlineColor.a;
			outlineColor = lerp(faceColor, outlineColor, sqrt(min(1.0, (outline * 2))));

			#if (UNDERLAY_ON | UNDERLAY_INNER)

			layerScale /= 1 + ((_UnderlaySoftness2 * _ScaleRatioC) * layerScale);
			float layerBias = (.5 - weight) * layerScale - .5 - ((_UnderlayDilate2 * _ScaleRatioC) * .5 * layerScale);
			float layerOutline = _OutlineWidth * _ScaleRatioC * 0.5 * layerScale;

			float x = -(_UnderlayOffsetX2 * _ScaleRatioC) * _GradientScale / _TextureWidth;
			float y = -(_UnderlayOffsetY2 * _ScaleRatioC) * _GradientScale / _TextureHeight;
			float2 layerOffset = float2(x, y);

			layerScale2 /= 1 + ((_UnderlaySoftness * _ScaleRatioC) * layerScale2);
			float layerBias2 = (.5 - weight) * layerScale2 - .5 - ((_UnderlayDilate * _ScaleRatioC) * .5 * layerScale2);
			//float layerOutline2 = _OutlineWidth * _ScaleRatioC * 0.5 * layerScale2;

			float x2 = -(_UnderlayOffsetX * _ScaleRatioC) * _GradientScale / _TextureWidth;
			float y2 = -(_UnderlayOffsetY * _ScaleRatioC) * _GradientScale / _TextureHeight;
			float2 layerOffset2 = float2(x2, y2);
			#endif

			// Generate UV for the Masking Texture
			float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
			float2 maskUV = (vert.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);

			// Populate structure for pixel shader
			output.vertex = vPosition;
			output.faceColor = faceColor;
			output.outlineColor = outlineColor;
			output.texcoord0 = float4(input.texcoord0.x, input.texcoord0.y, maskUV.x, maskUV.y);
			output.param = half4(scale, bias - outline, bias + outline, bias);
			output.mask = half4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + pixelSize.xy));
			output.gradientUV = input.color.rg;
			#if (UNDERLAY_ON || UNDERLAY_INNER)
			output.texcoord1 = float4(input.texcoord0 + layerOffset, input.color.a, 0);
			output.underlayParam = half3(layerScale, layerBias, layerOutline);
			output.texcoord2 = float4(input.texcoord0 + layerOffset2, layerScale2, layerBias2);
			#else
			output.texcoord1 = float4(0, 0, input.color.a, 0);
			#endif

			return output;
		}


		// PIXEL SHADER
		fixed4 PixShader(pixel_t input) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(input);
			half d = tex2D(_MainTex, input.texcoord0.xy).a * input.param.x;
			half4 c = input.faceColor * saturate(d - input.param.w);
            
			#if UNDERLAY_ON
			// 这里使用一组和 face 参数不同的变量
			half d2 = tex2D(_MainTex, input.texcoord1.xy).a * input.underlayParam.x;
			half layerParamY = input.underlayParam.y + input.underlayParam.z;
			half layerParamZ = input.underlayParam.z + input.underlayParam.y;
			half4 c2 = input.faceColor * saturate(d2 - input.underlayParam.y);
			#endif

			if(_UseColorGradient > 0.5)
			{
                half4 g = tex2D(_ColorGradientTex, float2(0, input.gradientUV.x));
                c *= g;
            }

			#if UNDERLAY_ON
			half sd = saturate(d2 - layerParamZ);
			d2 = tex2D(_MainTex, input.texcoord1.xy).a * input.underlayParam.x;
			float4 inlineColor = float4(_UnderlayColor2.rgb * _UnderlayColor2.a, _UnderlayColor2.a) * (1 - saturate(d2 - input.underlayParam.y)) * sd * c.a;// * (1 - c2.a)
			// 叠加的效果不好，改成下面的过渡
			//c = float4(c.rgb * saturate(1 - inlineColor.a) + inlineColor.rgb, c.a);
			c = float4(lerp(c.rgb, _UnderlayColor2.rgb, _UnderlayColor2.a * c.a * (1 - sd) *(1 - saturate(d2 - input.underlayParam.y))), c.a);
			#endif

			#ifdef OUTLINE_ON
			c = lerp(input.outlineColor, c, saturate(d - input.param.z));
			c *= saturate(d - input.param.y);
			#endif

			#if UNDERLAY_ON
			d = tex2D(_MainTex, input.texcoord2.xy).a * input.texcoord2.z;
			c += float4(_UnderlayColor.rgb * _UnderlayColor.a, _UnderlayColor.a) * saturate(d - input.texcoord2.w) * (1 - c.a);
			#endif

			// Alternative implementation to UnityGet2DClipping with support for softness.
			#if UNITY_UI_CLIP_RECT
			half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
			c *= m.x * m.y;
			#endif

			//#if (UNDERLAY_ON)
			c *= input.texcoord1.z;
			//#endif

			#if UNITY_UI_ALPHACLIP
			clip(c.a - 0.001);
			#endif

			return c;
		}
		ENDCG
	}
}

CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUIColorGradient"
}
