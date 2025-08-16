//grid_simple_lambert
#version 330 core
in vec3 normal_F;
in vec3 position_F;
uniform mat4 view;
layout (location=0) out vec4 color;
void main(){
    //gl_FragDepth = 1-gl_FragCoord.z;
    //gl_FragDepth = (apos.z+1)/2;
    vec4 light = vec4(0,0,-1,1);//hmmm we don't need view* here. Since we are not going to fix the light direction making it always at one front of the object , which can be done by rotating both of them (sync them)
    color = vec4(vec3(dot(normal_F, normalize(light.xyz))*0.9),1);
    //color = vec4(position_F,1);
}