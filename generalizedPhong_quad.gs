//我们最终要构造出类似扇形的排布
//扇形的0索引是空间内的一个动点Vdyn = Barycentric(V1,V2,V3)
//动点所运动的领域，也就是包括它的多边形，是一个凸多边形，构成它的顶点不应该理解成三角形链的形式，而是线链，线链所封闭的面积即所求领域
//我在此之前错误地理解成要在三角形的中心增加一个重心点（A/3+B/3+C/3)，但不应该是这样的，这里决定的是轮廓而非动点
//我给出两种方案
//1.对三角形算
//2.将三角形扩展成多边形算
//扩展方法：
//V4 = (V2-V1) + (V3-V1) + V1

#version 450 core
layout(triangles) in;
layout(triangle_strip, max_vertices = 4) out;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;


out flat vec3 priver1;
out flat vec3 priver2;
out flat vec3 priver3;
out vec3 absoluteworldpos; 


void main() {

    vec4 v1 = gl_in[0].gl_Position;
    vec4 v2 = gl_in[1].gl_Position;
    vec4 v3 = gl_in[2].gl_Position;

    gl_Position = move * scale * view * v1;
    absoluteworldpos = v1.xyz;
    EmitVertex();

    gl_Position = move * scale * view * v2;
    absoluteworldpos = v2.xyz;
    EmitVertex();

    gl_Position = move * scale * view * v3;
    absoluteworldpos = v3.xyz;
    priver1 = v1.xyz;
    priver2 = v2.xyz;
    priver3 = v3.xyz;
    EmitVertex();

    vec4 v4 = v3 + v2 - v1;
    gl_Position = move * scale * view * v4;
    absoluteworldpos = v4.xyz;
    priver1 = v2.xyz;
    priver2 = v3.xyz;
    priver3 = v4.xyz;
    EmitVertex();
    EndPrimitive();

}