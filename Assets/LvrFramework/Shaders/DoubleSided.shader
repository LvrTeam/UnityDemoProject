Shader "Demo/DoubleSided"
{
Properties
{
    _Color("Main Color", Color) = (1,1,1,1)
    _MainTex("Texture", 2D) = "white" {}
}
    SubShader
{
    //Ambient pass
    Pass
    {
        Name "BASE"
        Tags {"LightMode" = "Always" /* Upgrade NOTE: changed from PixelOrNone to Always */}
        Color[_PPLAmbient]
        SetTexture[_BumpMap]
        {
            constantColor(.5,.5,.5)
            combine constant lerp(texture) previous
        }
    SetTexture[_MainTex]
{
    constantColor[_Color]
    Combine texture * previous DOUBLE, texture *constant
}

    }

    //Vertex lights
    Pass{
    Name "BASE"
    Tags {"LightMode" = "Vertex"}
    Material
{
    Diffuse[_Color]
    Emission[_PPLAmbient]
    Shininess[_Shininess]
    Specular[_SpecColor]
}

    SeparateSpecular On
    Lighting On
    cull off
    SetTexture[_BumpMap]
    {
        constantColor(.5,.5,.5)
        combine constant lerp(texture) previous
    }
    SetTexture[_MainTex]
    {
        Combine texture *previous DOUBLE, texture *primary
    }
}

}

    FallBack "Diffuse", 1
}
