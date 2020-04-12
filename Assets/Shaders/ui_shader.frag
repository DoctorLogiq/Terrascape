#version 400

in vec2 passUV;

out vec4 outFragColour;

uniform float inR;
uniform float inG;
uniform float inB;
uniform float inA;

uniform sampler2D texture1; // diffuse
uniform sampler2D texture2; // mask

void main()
{
    float red_final   = texture(texture1, passUV).r * inR;
    float green_final = texture(texture1, passUV).g * inG;
    float blue_final  = texture(texture1, passUV).b * inB;

    float texture_alpha = texture(texture1, passUV).a * texture(texture2, passUV).r;
    float alpha_final = texture_alpha * inA;
    
    if (alpha_final < 0.05f)
        discard;

    outFragColour = vec4(red_final, green_final, blue_final, alpha_final);
}