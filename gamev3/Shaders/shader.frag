#version 330 core

#define MAX_POINT_LIGHTS 8

struct Material {
	sampler2D diffuse;
	sampler2D specular;
	float shininess;
};

struct PointLight {
	vec3 position;

	float strength;

	float constant;
	float linear;
	float quadratic;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct ShadowMaps {
	samplerCube shadowMap;
};

uniform PointLight pointLights[MAX_POINT_LIGHTS];
uniform vec3 viewPos;
uniform Material material;

uniform ShadowMaps shadowMap[MAX_POINT_LIGHTS];

uniform float far_plane;

out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

float ShadowCalculation(vec3 fragPos, PointLight light, samplerCube map)
{
	vec3 fragToLight = fragPos - light.position;
	float currentDepth = length(fragToLight);

	float bias = 0.05;
	float shadow = 0.0;
	int samples = 20;
	float viewDistance = length(viewPos - fragPos);
	float diskRadius = 0.05;

	vec3 sampleOffsetDirections[20] = vec3[]
	(
	   vec3( 1,  1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1,  1,  1), 
	   vec3( 1,  1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1,  1, -1),
	   vec3( 1,  1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1,  1,  0),
	   vec3( 1,  0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1,  0, -1),
	   vec3( 0,  1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0,  1, -1)
	);

	for(int i = 0; i < samples; ++i)
	{
		float closestDepth = texture(map, fragToLight + sampleOffsetDirections[i] * diskRadius).r * far_plane;
		if(currentDepth - bias > closestDepth)
			shadow += 1.0;
	}

	shadow /= float(samples);
	return shadow;
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
	vec3 lightDir = normalize(light.position - fragPos);
	float diff = max(dot(normal, lightDir), 0.0);

	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

	vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
	vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
	vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));

	float distance = length(light.position - fragPos);
	float attenuation = light.strength / (light.constant + light.linear * distance + light.quadratic * distance * distance);

	ambient *= attenuation;
	diffuse *= attenuation;
	specular *= attenuation;

	return (ambient + diffuse + specular);
}

void main()
{
	vec3 norm = normalize(Normal);
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 result = vec3(0.0, 0.0, 0.0);

	for(int i = 0; i < MAX_POINT_LIGHTS; i++)
	{
		float shadow = ShadowCalculation(FragPos, pointLights[i], shadowMap[i].shadowMap);
		result += (1.0 - shadow) * CalcPointLight(pointLights[i], norm, FragPos, viewDir);
	}

	FragColor = vec4(result, 1.0);
}