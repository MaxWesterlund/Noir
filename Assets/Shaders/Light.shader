Shader "Custom/Light" {
    Properties {
        [MainColor]
        _MainColor("Color", Color) = (1, 1, 1, 0.5)
    }
    SubShader {
        Tags { 
            "Queue"="Geometry+1"
        }
        LOD 100
        
        ZWrite Off
        Lighting Off
        Fog { Mode Off }

        Blend SrcAlpha OneMinusSrcAlpha

        Stencil {
            Ref 1
            Pass Replace
        }

        Pass {
            Color [_MainColor]
        }
    }
}
