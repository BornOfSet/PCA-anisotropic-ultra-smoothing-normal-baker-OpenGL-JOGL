//grid_vertex_shader_mesh
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNorm;
uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;
out vec3 normal;
out vec3 position;


void main(){
    gl_Position = view * vec4(aPos,1);
    gl_Position = move * scale * gl_Position;
    normal =(view * vec4(aNorm,1)).xyz;
    position = aPos;
}