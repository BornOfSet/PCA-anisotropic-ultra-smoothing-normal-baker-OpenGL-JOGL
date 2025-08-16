//located in the same place as grid_surface_primitives does
#version 330 core
layout(triangles) in;
layout(line_strip, max_vertices = 40) out;
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

    for(int i=1;i<=density;i++){
        float ratio = float(i)/float(density);//禁止int/int，这导致结果取整
        vec3 xk = xo + ratio * (xa - xo);
        vec3 xv = xo + ratio * (xb - xo);
        gl_Position = vec4(xk,1);
        AtVertex = mix(xo, xa, floor(ratio + 0.5));
        EmitVertex();
        gl_Position = vec4(xv,1);
        AtVertex = mix(xo, xb, floor(ratio + 0.5));
        EmitVertex();
        EndPrimitive();
    }
}