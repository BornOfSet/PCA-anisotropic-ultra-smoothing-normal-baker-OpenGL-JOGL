//flat变量完全依据provoking vertex决定
//通过glposition和flat v1 v2 v3在fs中手动插值
//这里负责传入v1 v2 v3

#version 450 core
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;


out flat vec3 MBv1;
out flat vec3 MBv2;
out flat vec3 MBv3;
out vec3 interpolatepos; 

in vec4 onormal[];
out flat vec3 Nv1;
out flat vec3 Nv2;
out flat vec3 Nv3;
out vec3 interpolatenml; 


void main() {

    vec4 v1 = gl_in[0].gl_Position;
    vec4 v2 = gl_in[1].gl_Position;
    vec4 v3 = gl_in[2].gl_Position;

    gl_Position = move * scale * view * v1;
    MBv1 = v1.xyz;
    MBv2 = v2.xyz;
    MBv3 = v3.xyz;
    interpolatepos = v1.xyz;
    Nv1 = onormal[0].xyz;
    Nv2 = onormal[1].xyz;
    Nv3 = onormal[2].xyz;
    interpolatenml = onormal[0].xyz;
    EmitVertex();

    gl_Position = move * scale * view * v2;
    MBv1 = v1.xyz;
    MBv2 = v2.xyz;
    MBv3 = v3.xyz;
    interpolatepos = v2.xyz;
    Nv1 = onormal[0].xyz;
    Nv2 = onormal[1].xyz;
    Nv3 = onormal[2].xyz;
    interpolatenml = onormal[1].xyz;
    EmitVertex();

    gl_Position = move * scale * view * v3;
    MBv1 = v1.xyz;
    MBv2 = v2.xyz;
    MBv3 = v3.xyz;
    interpolatepos = v3.xyz;
    Nv1 = onormal[0].xyz;
    Nv2 = onormal[1].xyz;
    Nv3 = onormal[2].xyz;
    interpolatenml = onormal[2].xyz;
    EmitVertex();
    EndPrimitive();
}