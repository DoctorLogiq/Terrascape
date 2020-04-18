#version 400

in vec2 passUV;

out vec4 outFragColour;

uniform float inR;
uniform float inG;
uniform float inB;
uniform float inA;

uniform bool inDebug;

uniform sampler2D inDiffuse;

void main()
{
    float red_final   = texture(inDiffuse, passUV).r * inR;
    float green_final = texture(inDiffuse, passUV).g * inG;
    float blue_final  = texture(inDiffuse, passUV).b * inB;

    float texture_alpha = texture(inDiffuse, passUV).a; // * texture(texture2, passUV).r);
    float alpha_final = texture_alpha * inA;
    
    if (alpha_final < 0.05f) // TODO(LOGIX): Should this check for == 0?
        discard;

    outFragColour = vec4(red_final, green_final, blue_final, alpha_final);
}