Shader "ElveGame/WaterQuad"
{
	Properties
	{
		[PerRendererData] _MainTex ("Water Pass Texture", 2D) = "white" {}

        _WaterColor("Water Color", Color) = (0.1, 0.4, 1.0, 0.4)
        _Tint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        _Z("Z", Range(0.0, 1.0)) = 0.0

        _WaterThinness("Water Thinness", Range(0.0, 1.0)) = 0.0
        _LandscapeAlphaThreshold("Discard Landscape Alpha Threshold", Range(0.0, 1.0)) = 0.01
	}

	SubShader
	{
		Tags
		{
            "Queue" = "Geometry"
            "IgnoreProjector" = "True"
            "RenderType" = "Opaque"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite On
		Fog { Mode Off }
		Blend One Zero

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY DISCARDLANDSCAPEALPHA_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};

            fixed _Z;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = IN.vertex;
                OUT.vertex.z = _Z;
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;

				return OUT;
			}

			sampler2D _MainTex, _Landscape;
            fixed4 _Tint, _WaterColor;
            float _LandscapeAlphaThreshold, _WaterThinness;

			fixed4 frag(v2f IN) : COLOR
			{
                fixed4 waterCol = tex2D(_MainTex, IN.texcoord);
                clip(waterCol.a - _WaterThinness);

				return _WaterColor * _Tint;
			}
		ENDCG
		}
	}
}
