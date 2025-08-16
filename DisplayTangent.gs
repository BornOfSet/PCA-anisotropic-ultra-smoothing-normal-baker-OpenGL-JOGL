//SSBO中提取法切

#version 450 core
layout(points) in;
layout(line_strip, max_vertices = 4) out;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;

in flat int ID[];
in vec3 POS[];
in vec2 uv[];

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


void main() {
    float dist = 0.5;
    gl_Position = move * scale * view * vec4(POS[0],1);
    EmitVertex(); 
    gl_Position = move * scale * view * (vec4(POS[0],1) + dist * vec4(ts[ID[0]].v[0], ts[ID[0]].v[1], ts[ID[0]].v[2],0));
    EmitVertex(); 
    EndPrimitive();

    gl_Position = move * scale * view * vec4(POS[0],1);
    EmitVertex(); 
    gl_Position = move * scale * view * (vec4(POS[0],1) + 0.5 * dist * vec4(ns[ID[0]].v[0], ns[ID[0]].v[1], ns[ID[0]].v[2],0));
    EmitVertex(); 
    EndPrimitive();
}