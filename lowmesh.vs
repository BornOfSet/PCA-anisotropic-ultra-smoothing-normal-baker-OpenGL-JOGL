//用此shader读取345插槽，加载低模
#version 450 core
layout (location = 3) in vec3 aPos;
layout (location = 4) in vec3 aNorm;
layout (location = 5) in vec2 aUv;

out vec3 meshNormal;
out vec2 uv;
out vec3 POS;
out flat int ID;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;

struct VertexStruct{
    float v[3];
};


layout(std430, binding=4 ) readonly buffer A{
    VertexStruct[] ns;
};

layout(std430, binding=5 ) readonly buffer B{
    VertexStruct[] ts;
};

layout(std430, binding=6 ) readonly buffer C{
    VertexStruct[] nsf;
};

layout(std430, binding=7 ) readonly buffer D{
    VertexStruct[] tsf;
};

out vec3 N;
out vec3 T;
out vec3 Nf;
out vec3 Tf;

void main(){
    gl_Position = move * scale * view * vec4(aPos,1);
    uv = aUv;
    meshNormal = aNorm;
    POS = aPos;
    ID = gl_VertexID;
    N = vec3(ns[gl_VertexID].v[0], ns[gl_VertexID].v[1], ns[gl_VertexID].v[2]);
    T = vec3(ts[gl_VertexID].v[0], ts[gl_VertexID].v[1], ts[gl_VertexID].v[2]);
    Nf = vec3(nsf[gl_VertexID].v[0], nsf[gl_VertexID].v[1], nsf[gl_VertexID].v[2]);
    Tf = vec3(tsf[gl_VertexID].v[0], tsf[gl_VertexID].v[1], tsf[gl_VertexID].v[2]);
}