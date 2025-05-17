#version 330 core

struct ShadowMatrices {
	mat4 matrix;
};

layout (triangles) in;
layout (triangle_strip, max_vertices = 18) out;

uniform ShadowMatrices matrices[6];

out vec4 FragPos;

void main()
{
	for (int face = 0; face < 6; ++face)
	{
		gl_Layer = face;

		for (int i = 0; i < 3; ++i)
		{
			FragPos = gl_in[i].gl_Position;
			gl_Position = FragPos * matrices[face].matrix;
			EmitVertex();
		}

		EndPrimitive();
	}
}