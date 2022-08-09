#VERTEX_SHADER_BEGIN

#version 330 core
layout (location = 0) in vec4 vertex; // vec2 position, vec2 texCoords
layout (location = 1) in vec4 color;

out vec2 TexCoords;
out vec4 Color;

uniform mat4 model;
uniform mat4 projection;

void main()
{
	TexCoords = vertex.zw;
    Color = color;
    gl_Position = projection * vec4(vertex.xy, 0.0, 1.0);
}

#VERTEX_SHADER_END

#FRAGMENT_SHADER_BEGIN

#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec4 Color;

uniform sampler2D text;

void main()
{
	vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, TexCoords).r);
    FragColor = Color * sampled;
} 

#FRAGMENT_SHADER_END