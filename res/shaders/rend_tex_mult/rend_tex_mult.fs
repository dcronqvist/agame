#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D renderTexture0;
uniform sampler2D renderTexture1;

void main()
{
	FragColor = vec4((texture(renderTexture0, TexCoords) * texture(renderTexture1, TexCoords)).rgb, 1.0);
} 