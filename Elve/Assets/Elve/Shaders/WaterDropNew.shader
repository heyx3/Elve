Shader "Water/Drop New"
{
	Properties
	{
        _RadiusScale("Radius Scale", Float) = 1.5
        _StrengthDropoff("Strength Dropoff", Float) = 1.0
	}

	SubShader
	{
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
			"PreviewType"="Plane" 
        }

        Cull Off
        Lighting Off
        ZWrite On
        Fog { Mode Off }
		Blend One One
		
        Pass
		{

			CGPROGRAM
				#pragma target 5.0
				
				#pragma vertex vert
				#pragma geometry geom
				#pragma fragment frag

				#pragma enable_d3d11_debug_symbols

				#include "UnityCG.cginc"


				float _RadiusScale;
                float _StrengthDropoff;

                struct WaterDrop
                {
	                float2 pos, vel;
	                float radius;
                };
                uniform StructuredBuffer<WaterDrop> dropsBuffer;


				struct GS_INPUT
				{
					float4 PosAndRadius : SV_POSITION;
				};
				struct F_INPUT
				{
					float4 Pos : SV_POSITION;
                    float4 WorldPosAndCenter : TEXCOORD0;
                    float2 RadiusAndRadiusDiff : TEXCOORD1;
				};

				GS_INPUT vert(uint id : SV_VertexID)
				{
					GS_INPUT output;
                    WaterDrop d = dropsBuffer[id];
                    output.PosAndRadius = float4(d.pos.x, d.pos.y, d.radius, 1.0);
					return output;
				}
				[maxvertexcount(4)]
				void geom(point GS_INPUT input[1], inout TriangleStream<F_INPUT> outStream)
				{
					F_INPUT output;

                    float2 pos = input[0].PosAndRadius.xy;
                    float4 pos4 = float4(pos.x, pos.y, 0.0, 1.0);

                    float radius = input[0].PosAndRadius.z;

                    float3 consts = float3(0.0, radius * _RadiusScale, 0.0);
                    consts.x = -consts.y;

                    float2 radii = float2(radius, consts.y - radius);

					output.Pos = mul(UNITY_MATRIX_VP, pos4 + consts.xxzz);
					output.WorldPosAndCenter.xy = pos + consts.xx;
                    output.WorldPosAndCenter.zw = pos;
                    output.RadiusAndRadiusDiff = radii;
					outStream.Append(output);

					output.Pos = mul(UNITY_MATRIX_VP, pos4 + consts.xyzz);
					output.WorldPosAndCenter.xy = pos + consts.xy;
                    output.WorldPosAndCenter.zw = pos;
                    output.RadiusAndRadiusDiff = radii;
					outStream.Append(output);

					output.Pos = mul(UNITY_MATRIX_VP, pos4 + consts.yxzz);
					output.WorldPosAndCenter.xy = pos + consts.yx;
                    output.WorldPosAndCenter.zw = pos;
                    output.RadiusAndRadiusDiff = radii;
					outStream.Append(output);

					output.Pos = mul(UNITY_MATRIX_VP, pos4 + consts.yyzz);
					output.WorldPosAndCenter.xy = pos + consts.yy;
                    output.WorldPosAndCenter.zw = pos;
                    output.RadiusAndRadiusDiff = radii;
					outStream.Append(output);
				}
				fixed4 frag(F_INPUT input) : COLOR0
				{
                    float radius = input.RadiusAndRadiusDiff.x,
                          radiusDiff = input.RadiusAndRadiusDiff.y;
                    float2 myPos = input.WorldPosAndCenter.xy,
                           centerPos = input.WorldPosAndCenter.zw;

                    float dist = distance(myPos, centerPos);

                    //Output 1 if dist is less than radius.
                    //Output [0, 1] if dist is between radius and scaled radius.
                    //Output 0 if dist is greater than scaled radius.
                    float alpha = 1.0 - saturate((dist - radius) / radiusDiff);
                    alpha = pow(alpha, _StrengthDropoff);

                    return fixed4(alpha, alpha, alpha, alpha);
				}
			ENDCG
		}
	}
}