#version 450 core

layout (location = 3) in vec3 lowPos;
layout (location = 4) in vec3 lowNorm;
layout (location = 5) in vec2 lowUv;

out vec3 Direc;
out vec3 Orig;



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

//正交化
void main(){
    gl_Position = vec4(lowUv*2-1,0,1);
    Direc = lowNorm;//ray direction
    Orig = lowPos;//ray origin
    N = normalize(vec3(ns[gl_VertexID].v[0], ns[gl_VertexID].v[1], ns[gl_VertexID].v[2]));
    T = normalize(vec3(ts[gl_VertexID].v[0], ts[gl_VertexID].v[1], ts[gl_VertexID].v[2]));
    Nf = normalize(vec3(nsf[gl_VertexID].v[0], nsf[gl_VertexID].v[1], nsf[gl_VertexID].v[2]));
    Tf = normalize(vec3(tsf[gl_VertexID].v[0], tsf[gl_VertexID].v[1], tsf[gl_VertexID].v[2]));

    //我认为，，，插值前后也不会垂直
    float LengthTN = dot(N, T);
    vec3 relengthNormal = LengthTN * N;
    vec3 reangleT = T - relengthNormal;
    T = normalize(reangleT);
}