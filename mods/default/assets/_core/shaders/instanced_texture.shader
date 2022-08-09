#VERTEX_SHADER_BEGIN

#version 330 core
layout (location = 0) in vec2 position;
layout (location = 1) in mat4 instanceMatrix;
layout (location = 5) in vec2 uvCoords[6];

out vec2 TexCoords;

uniform mat4 model;
uniform mat4 projection;

void main()
{
	TexCoords = uvCoords[gl_VertexID % 6];
    gl_Position = projection * instanceMatrix * vec4(position.xy, 0.0, 1.0);
}

#VERTEX_SHADER_END

#FRAGMENT_SHADER_BEGIN

#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D image;
uniform vec4 textureColor;

void main()
{
    FragColor = textureColor * texture(image, TexCoords);
} 

#FRAGMENT_SHADER_END