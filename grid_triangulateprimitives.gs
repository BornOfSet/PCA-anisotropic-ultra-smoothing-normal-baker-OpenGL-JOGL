//located in the same place as grid_surface_primitives does
#version 330 core
layout(triangles) in;
layout(line_strip, max_vertices = 4) out;
in vec3 normal[];
out vec3 normal_F;

in vec3 position[];
out vec3 position_F;

out vec3 AtVertex;

void main() {

    int density = 20;

    //point x=(0,0,0)
    vec3 xo = gl_in[0].gl_Position.xyz;
    vec3 xa = gl_in[1].gl_Position.xyz;
    vec3 xb = gl_in[2].gl_Position.xyz;

    gl_Position = vec4(xo,1);
    EmitVertex();
    gl_Position = vec4(xa,1);
    EmitVertex();
    gl_Position = vec4(xb,1);
    EmitVertex();
    gl_Position = vec4(xo,1);
    EmitVertex();
    EndPrimitive();
}