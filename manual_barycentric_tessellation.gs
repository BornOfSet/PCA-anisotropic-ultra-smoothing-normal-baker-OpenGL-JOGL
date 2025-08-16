//简单的细分GS，辅助插值
//辅助重心插值
//在三角形的重心位置插入额外的顶点，并与已知的三个顶点构成fans
//该中心顶点应该采用一个值，使.....


#version 450 core
layout(triangles) in;
layout(triangle_strip, max_vertices = 5) out;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;

in vec4 onormal[];
out vec3 normal_F;

void main() {

    vec3 p1 = (move * scale * view * gl_in[0].gl_Position).xyz;
    vec3 n1 = (view * vec4(onormal[0].xyz,1)).xyz;
    vec3 p2 = (move * scale * view * gl_in[1].gl_Position).xyz;
    vec3 n2 = (view * vec4(onormal[1].xyz,1)).xyz;
    vec3 p3 = (move * scale * view * gl_in[2].gl_Position).xyz;
    vec3 n3 = (view * vec4(onormal[2].xyz,1)).xyz;


    float dv1p = 1/max(length(p1 - (p1+p2+p3)/3),0.001);
    float dv2p = 1/max(length(p2 - (p1+p2+p3)/3),0.001);
    float dv3p = 1/max(length(p3 - (p1+p2+p3)/3),0.001);
    float e1 = dv1p + dv2p + dv3p;
    vec3 pythagorean = dv1p/e1 * n1 + dv2p/e1 * n2 + dv3p/e1 * n3;

    gl_Position = vec4(p1,1);
    normal_F = n1;
    EmitVertex();

    gl_Position = vec4(p2,1);
    normal_F = n2;
    EmitVertex();

    gl_Position = vec4((p1+p2+p3)/3,1);
    normal_F = pythagorean;
    EmitVertex();

    gl_Position = vec4(p3,1);
    normal_F = n3;
    EmitVertex();

    gl_Position = vec4(p1,1);
    normal_F = n1;
    EmitVertex();

    EndPrimitive();
}