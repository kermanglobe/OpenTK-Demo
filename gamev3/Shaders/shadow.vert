#version 330 core
in vec3 vPosition;

uniform mat4 model;

void main()
{
	gl_Position = vec4(vPosition, 1.0) * model;
}