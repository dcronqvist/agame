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

uniform sampler2D renderTexture0;
uniform sampler2D renderTexture1;

void main()
{
	FragColor = vec4((texture(renderTexture0, TexCoords) * texture(renderTexture1, TexCoords)).rgb, 1.0);
} 

#FRAGMENT_SHADER_END