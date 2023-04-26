Shader "Custom/FlatColor" {
    Properties {
        [MainColor]
        _MainColor("Color", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { 
            "Queue"="Geometry" 
        }
        LOD 100
        
        Lighting Off
        Fog { Mode Off }

        Pass {
            Color [_MainColor]
        }
    }
}
