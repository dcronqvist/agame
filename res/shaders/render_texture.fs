#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D renderTexture;

void main()
{
	FragColor = texture(renderTexture, TexCoords);
} 