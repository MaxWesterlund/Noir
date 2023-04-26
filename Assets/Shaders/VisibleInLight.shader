Shader "Custom/VisibleInLight" {
    Properties {
        [MainColor]
        _MainColor("Color", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { 
            "Queue"="Geometry+2"
        }
        LOD 100
        
        Lighting Off
        Fog { Mode Off }

        Stencil {
            Ref 1
            Comp Equal
        }

        Pass {
            Color [_MainColor]
        }
    }
}
