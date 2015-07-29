Shader "Voxel/Block"
{
	Properties
	{
		_MainTex	 ("Tilemap",				 2D)	 = "white" { }
		_MainTexSize ("Tilemap dimensions (2D)", Vector) = (256.0, 256.0, 0.0, 0.0)
		_TileSize	 ("Tile's pixel size",		 Float)  = 32.0
	}

	SubShader
	{
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Transparent+1" 
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
		
        Pass
		{

			CGPROGRAM
				#pragma target 5.0
				
				#pragma vertex vert
				#pragma geometry geom
				#pragma fragment frag

				#pragma enable_d3d11_debug_symbols

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4	  _MainTexSize;
				float	  _TileSize;

				struct VS_INPUT
				{
					float3 Pos : POSITION;
					float2 PixelMin: TEXCOORD0;
				};
				struct GS_INPUT
				{
					float4 Pos : SV_POSITION;
					float2 UVMin : TEXCOORD0;
				};
				struct F_INPUT
				{
					float4 Pos : SV_POSITION;
					float2 UV :	TEXCOORD0;
				};

				GS_INPUT vert(VS_INPUT input)
				{
					GS_INPUT output;
					output.Pos = float4(input.Pos, 1.0);
					output.UVMin = input.PixelMin * (1.0 / _MainTexSize.xy);
					output.UVMin.y = 1.0 - output.UVMin.y;
					return output;
				}
				[maxvertexcount(4)]
				void geom(point GS_INPUT input[1], inout TriangleStream<F_INPUT> outStream)
				{
					F_INPUT output;

					float2 tileSize = (1.0 / _MainTexSize.xy) * _TileSize;

					output.Pos = mul(UNITY_MATRIX_MVP, input[0].Pos);
					output.UV = input[0].UVMin;
					outStream.Append(output);

					output.Pos = mul(UNITY_MATRIX_MVP, input[0].Pos + float4(0.0, 1.0, 0.0, 0.0));
					output.UV = input[0].UVMin + float2(0.0, -tileSize.y);
					outStream.Append(output);

					output.Pos = mul(UNITY_MATRIX_MVP, input[0].Pos + float4(1.0, 0.0, 0.0, 0.0));
					output.UV = input[0].UVMin + float2(tileSize.x, 0.0);
					outStream.Append(output);

					output.Pos = mul(UNITY_MATRIX_MVP, input[0].Pos + float4(1.0, 1.0, 0.0, 0.0));
					output.UV = input[0].UVMin + float2(tileSize.x, -tileSize.y);
					outStream.Append(output);
				}
				fixed4 frag(F_INPUT input) : COLOR0
				{
					return tex2D(_MainTex, input.UV);
				}
			ENDCG
		}
	}
}