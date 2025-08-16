#version 450 core

layout (location = 3) in vec3 lowPos;
layout (location = 4) in vec3 lowNorm;
layout (location = 5) in vec2 lowUv;

out vec3 Direc;
out vec3 Orig;

void main(){
    gl_Position = vec4(lowUv*2-1,0,1);
    Direc = lowNorm;//ray direction
    Orig = lowPos;//ray origin
}