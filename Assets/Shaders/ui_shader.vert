#version 400

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec2 inUV;

uniform mat4 inModel;
uniform mat4 inView;
uniform mat4 inProjection;

out vec2 passUV;

void main()
{
	passUV = inUV;
	gl_Position = vec4(inPosition, 1.0) * inModel * inView * inProjection;
}