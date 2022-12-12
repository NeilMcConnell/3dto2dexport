Shader "Custom/CameraNormalShader" {
    SubShader{
        Pass{
 
        CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
 
        struct v2f {
        fixed4 pos : POSITION; //don't ask why I use fixed point for this... :D
        fixed2 uv : TEXCOORD0; //UVs. (Unless one needs a base diffuse texture, probably won't need this!)
        fixed3 VSNormal : TEXCOORD1; //our view-space normal (V-S Normal)
        fixed4 screenPos : TEXCOORD2; //used for GrabPass coord
        };
 
        v2f vert(appdata_tan v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord.xy;
            o.VSNormal = COMPUTE_VIEW_NORMAL;
            o.screenPos = ComputeScreenPos(o.pos);
            return o;
        }
 
        sampler2D _RefrTex; //refraction texture
        sampler2D _NormalMap;
        fixed4 frag(v2f i) : COLOR
        {
            fixed4 col = fixed4(0,0,0,0);
           
            fixed3 normTex = UnpackNormal(tex2D(_NormalMap, i.uv));
                       
            fixed3 combinedNormals = normTex + i.VSNormal;
            combinedNormals = normalize(combinedNormals );

            fixed3 worldNormals = UnityObjectToWorldNormal(combinedNormals);
            worldNormals = mul((float3x3)UNITY_MATRIX_V, worldNormals);
           
            //col.r = worldNormals.x;
            //col.g = worldNormals.y;
            //col.b = worldNormals.z;
            col.r = combinedNormals.x;
            col.g = combinedNormals.y;
            col.b = combinedNormals.z;
           
            col = normalize(col);
 
            // Convert space from (-1,1) to (0,1)
            col.r = (col.r + 1)*0.5f;
            col.g = (col.g + 1)*0.5f;
            col.b = (col.b + 1)*0.5f;
            col.a = 1;
           
            return col;
        }
            ENDCG
        }
    }
        Fallback "VertexLit"
}