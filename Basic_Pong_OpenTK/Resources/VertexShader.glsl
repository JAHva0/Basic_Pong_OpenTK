#version 330

layout(location = 0) in vec4 position;

smooth out vec4 theColor;

uniform vec2 location;

layout(std140) uniform CameraMatricies
{
	mat4 View;
	mat4 Perspective;
};

void main()
{
	gl_Position = (Perspective * View) * (position + vec4(location, 0.0, 1.0));
	theColor = vec4(1.0, 1.0, 1.0, 1.0);
}