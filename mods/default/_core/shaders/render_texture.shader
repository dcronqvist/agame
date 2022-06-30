#VERTEX_SHADER_BEGIN

#version 330 core
layout (location = 0) in vec2 position; // vec2 position
layout (location = 1) in vec2 aTexCoords;

out vec2 TexCoords;

uniform mat4 model;
uniform mat4 projection;

void main()
{
    gl_Position = projection * model * vec4(position.xy, 0.0, 1.0);
	TexCoords = aTexCoords;
}

#VERTEX_SHADER_END

#FRAGMENT_SHADER_BEGIN

#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D renderTexture;
uniform vec4 textureColor;

void main()
{
	FragColor = textureColor * texture(renderTexture, TexCoords);
} 

#FRAGMENT_SHADER_END