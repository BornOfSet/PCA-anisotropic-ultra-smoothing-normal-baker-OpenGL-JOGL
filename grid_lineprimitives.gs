//grid_line_primitives
#version 330 core
layout(points) in;
layout(line_strip, max_vertices = 20) out;
uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;
uniform float pointsize;

void main() {
    vec4 inputp = gl_in[0].gl_Position;
    gl_Position = inputp + move * scale * view * vec4(pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,-pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,-pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    EndPrimitive();

    gl_Position = inputp + move * scale * view *  vec4(pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,-pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,-pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    EndPrimitive();

    gl_Position = inputp + move * scale * view * vec4(pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,-pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,-pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    EndPrimitive();

    gl_Position = inputp + move * scale * view *  vec4(-pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,-pointsize,-pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,-pointsize,pointsize,0);
    EmitVertex(); 
    gl_Position = inputp + move * scale * view *  vec4(-pointsize,pointsize,pointsize,0);
    EmitVertex(); 
    EndPrimitive();

}