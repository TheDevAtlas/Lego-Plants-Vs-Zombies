Shader "Hidden/DepthCopy"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float depth : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.depth = o.pos.z / o.pos.w;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float linearDepth = (i.depth - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y);
                linearDepth = saturate(linearDepth); // clamp 0–1
                return fixed4(linearDepth, linearDepth, linearDepth, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
