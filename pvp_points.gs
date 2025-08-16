    //pvp_passnormal
//特殊GS，它实际上什么事也没干，只是保证接口名的一致性，以便于和grid系列兰伯特渲染器对接
#version 450 core
layout(triangles) in;
layout(points, max_vertices = 3) out;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;

in vec4 onormal[];
out vec3 normal_F;

void main() {

    gl_Position = move * scale * view * gl_in[0].gl_Position;
    normal_F = (view * vec4(onormal[0].xyz,1)).xyz;
    EmitVertex();
    gl_Position = move * scale * view * gl_in[1].gl_Position;
    normal_F = (view * vec4(onormal[1].xyz,1)).xyz;
    EmitVertex();
    gl_Position = move * scale * view * gl_in[2].gl_Position;
    normal_F = (view * vec4(onormal[2].xyz,1)).xyz;
    EmitVertex();
    EndPrimitive();
}