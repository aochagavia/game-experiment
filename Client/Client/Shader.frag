#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 1) in vec2 fsin_TexturePosition;

layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform sampler2D SkeletonTexture;

void main()
{
    fsout_Color = texture(SkeletonTexture, fsin_TexturePosition);
}