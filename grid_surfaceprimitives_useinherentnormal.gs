//grid_surface_primitives
#version 330 core
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;
in vec3 normal[];
out vec3 normal_F;

in vec3 position[];
out vec3 position_F;

void main() {

    vec3 flatnormal = normalize(cross(gl_in[1].gl_Position.xyz-gl_in[0].gl_Position.xyz, gl_in[2].gl_Position.xyz-gl_in[0].gl_Position.xyz));
    gl_Position = gl_in[0].gl_Position;
    normal_F = normal[0];
    position_F = position[0];
    EmitVertex();
    gl_Position = gl_in[1].gl_Position;
    normal_F = normal[1];
    position_F = position[1];   
    EmitVertex();
    gl_Position = gl_in[2].gl_Position;
    normal_F = normal[2];
    position_F = position[2];
    EmitVertex();
    EndPrimitive();
}