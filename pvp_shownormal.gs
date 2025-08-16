//pvp_shownormal
//显示法线，专业的GS

#version 450 core
layout(points) in;
layout(line_strip, max_vertices = 2) out;

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;

in vec4 onormal[];

void main() {
    float dist = .5;
    gl_Position = move * scale * view * gl_in[0].gl_Position;
    EmitVertex(); 
    gl_Position = move * scale * view * (gl_in[0].gl_Position + dist * vec4(onormal[0].xyz,0));
    EmitVertex(); 
    EndPrimitive();
}