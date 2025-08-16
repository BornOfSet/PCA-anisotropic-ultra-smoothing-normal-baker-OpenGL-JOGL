//可以进行更多调整，比如置换
#version 450 core
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;

in vec3 opos[];
in vec4 onormal[];
out vec3 normal_F;
out vec3 pos_F;


void main() {

    gl_Position = gl_in[0].gl_Position;
    normal_F = onormal[0].xyz;
    pos_F = opos[0];
    EmitVertex();
    gl_Position = gl_in[1].gl_Position;
    normal_F = onormal[1].xyz;
    pos_F = opos[1];
    EmitVertex();
    gl_Position = gl_in[2].gl_Position;
    normal_F = onormal[2].xyz;
    pos_F = opos[2];
    EmitVertex();
    EndPrimitive();
}