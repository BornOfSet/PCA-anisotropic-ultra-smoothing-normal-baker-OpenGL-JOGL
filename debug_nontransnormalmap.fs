//只有正对着法线才能显示为蓝色
//事实上应该是显示为-1
//因为只有背面才是1
//xy依据从左到右从下到上
//z不能直指向屏幕里面，不然是不可见的
//记住，没有白色。如果蓝色和红色并存，则蓝色取代红色，因为它现在完全平行视野，没有多余的分量分配给xy方向
//该渲染模式适合用来判断一条直线上的法线是否同样方向
//也可以用来判断一个顶点的法线到底朝什么方向
//应该使用ws操控
#version 450 core
in vec3 normal_F;
uniform mat4 move;

layout (location=0) out vec4 color;
void main(){
    //w非1，因为我们想让法线的偏移领先模型实际的剔除
    vec4 tn = move * vec4(normal_F,-1.2);
    float contrast = 0.0f;
    if(-tn.z > 0.92){
        contrast = 1.0f;
    }
    contrast += pow(-tn.z,8);
    color = vec4(normal_F.xy,contrast,1);
}