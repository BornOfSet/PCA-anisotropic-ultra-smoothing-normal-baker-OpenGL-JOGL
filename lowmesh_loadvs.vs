//用此shader读取345插槽，加载低模
#version 330 core
layout (location = 3) in vec3 aPos;
layout (location = 4) in vec3 aNorm;
layout (location = 5) in vec2 aUv;

out vec4 onormal;

void main(){
    gl_Position = vec4(aPos,1);//0~1 -> NDC -1~1
    onormal = vec4(aNorm,1);
}