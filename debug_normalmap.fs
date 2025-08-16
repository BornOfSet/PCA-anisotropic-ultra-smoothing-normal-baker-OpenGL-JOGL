//debug normalmap fs
//使用grid-simplelambert的法线接口
//目的用于检查pvp管线的法线
//因此无法分类，属于debug类

#version 450 core
in vec3 normal_F;
uniform mat4 view;

layout (location=0) out vec4 color;
void main(){
    vec4 n = transpose(view) * vec4(normal_F,0);
    color = vec4(normalize(n.xyz),1);
}