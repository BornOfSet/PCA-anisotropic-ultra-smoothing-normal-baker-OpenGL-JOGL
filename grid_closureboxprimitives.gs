//located in the same place as grid_line_primitives does
#version 330 core
layout(points) in;
layout(triangle_strip, max_vertices = 16) out;
uniform mat4 view;
uniform mat4 scale;
uniform float pointsize;

void main() {
    vec4 inputp = gl_in[0].gl_Position;
    gl_Position = inputp + view * scale * vec4(pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(pointsize,-pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,-pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(pointsize,-pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,-pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,-pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + view * scale * vec4(-pointsize,-pointsize,-pointsize,0);
    EmitVertex(); 

    EndPrimitive();



}