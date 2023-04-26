Shader "Custom/VisibleInLight" {
    Properties {
        [MainColor]
        _MainColor("Color", Color) = (1, 1, 1, 0.5)
    }
    SubShader {
        Tags { 
            "Queue"="Geometry+2" 
        }
        LOD 100
        
        Lighting Off
        Fog { Mode Off }

        Blend SrcAlpha OneMinusSrcAlpha

        Stencil {
            Ref 1
            Comp equal
        }

        Pass {
            Color [_MainColor]
            // CGPROGRAM
            // #pragma vertex vert
            // #pragma fragment frag

            // #include "UnityCG.cginc"

            // struct appdata {
            //     float4 vertex : POSITION;
            // };

            // struct v2f {
            //     float4 vertex : SV_POSITION;
            // };

            // v2f vert (appdata v) {
            //     v2f o;
            //     o.vertex = UnityObjectToClipPos(v.vertex);
            //     return o;
            // }

            // float4 _MainColor;
            // fixed4 frag (v2f i) : SV_Target {
            //     return _MainColor;
            // }
            // ENDCG
        }
    }
}
