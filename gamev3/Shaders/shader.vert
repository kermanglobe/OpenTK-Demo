#version 330 core

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 FragPos;
out vec2 TexCoords;

void main(void)
{
	gl_Position = vec4(vPosition, 1.0) * model * view * projection;
	FragPos = vec3(vec4(vPosition, 1.0) * model);
	Normal = vNormal * mat3(transpose(inverse(model)));
	TexCoords = vTexCoord;
}