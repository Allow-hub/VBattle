Shader "Custom/MaskArea"
{
    SubShader
    {
        Tags { "Queue"="Geometry-10" }
        ColorMask 0
        ZWrite Off

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass {}
    }
}   