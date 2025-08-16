//flat变量完全依据provoking vertex决定
//通过glposition和flat v1 v2 v3在fs中手动插值

#version 450 core
in flat vec3 MBv1;
in flat vec3 MBv2;
in flat vec3 MBv3;
in vec3 interpolatepos; 
in flat vec3 Nv1;
in flat vec3 Nv2;
in flat vec3 Nv3;
in vec3 interpolatenml; 

uniform mat4 view;

layout (location=0) out vec4 color;

void main(){
    float p12 = length(cross(MBv1 - interpolatepos, MBv2 - interpolatepos));
    float p23 = length(cross(MBv2 - interpolatepos, MBv3 - interpolatepos));
    float p13 = length(cross(MBv1 - interpolatepos, MBv3 - interpolatepos));
    float coe3 = p12 / (p12 + p23 + p13);
    float coe1 = p23 / (p12 + p23 + p13);
    float coe2 = p13 / (p12 + p23 + p13);
    vec3 barycentric = coe3 * (Nv3) + coe2 * (Nv2) + coe1 * (Nv1);
    vec3 position = coe3 * MBv3 + coe2 * MBv2 + coe1 * MBv1;
    vec3 surfacenormal = normalize(barycentric);
    vec3 light = vec3(0,0,-1);
    //float intensity = dot(light, (view * vec4(surfacenormal,1)).xyz); 
    //如果给出一个平面，平面上处处受光强度一致。我们不希望归一化表面法线，因为这导致平面上出现圆斑，使得平面具有曲面特性，考虑到事实上两侧的法线有更少的分量用在接收几乎垂直的光照上
    float intensity = dot(light, (view * vec4(surfacenormal,1)).xyz);
    //color = vec4(barycentric,1);
    //以上是标准的重心坐标插值，接下来尝试绝对距离插值

    //距离x越大权重f(x)越小
    //函数图像在第一象限
    float dv1p = 1/max(length(MBv1 - interpolatepos),0.001);
    float dv2p = 1/max(length(MBv2 - interpolatepos),0.001);
    float dv3p = 1/max(length(MBv3 - interpolatepos),0.001);
    float e1 = dv1p + dv2p + dv3p;
    //系数之和等于1
    //不要误会了，我们是在几个值之间插值，而并非是因为红色+绿色得到了黄色，是因为黄色本来就是被插的边界之一
    //是啊，右下角应该是(1,-1)，左上角是(-1,1)，所以右下角只知道x=1红色，左上角只知道y=1绿色，而右上角是(1,1)所以是黄色，而左下角则是(-1,-1)
    vec3 pythagorean = dv1p/e1 * Nv1 + dv2p/e1 * Nv2 + dv3p/e1 * Nv3;
    float luminance = dot(light, (view * vec4(pythagorean,1)).xyz);
    //color = vec4(intensity);
    pythagorean *= 10;
    color = vec4(pow(pythagorean.x,10),pow(pythagorean.y,10),pow(pythagorean.z,10),1);
    //绝对距离插值无法过渡
    //尝试两者结合
    float mixure = 20;
    float v1pOn1 = mixure;
    float v2pOn1 = 1/max(length(MBv2 - MBv1),0.001);
    float v3pOn1 = 1/max(length(MBv3 - MBv1),0.001);
    float sumOn1 = v1pOn1 + v2pOn1 + v3pOn1;
    vec3 NewNv1 = v1pOn1/sumOn1 * Nv1 + v2pOn1/sumOn1 * Nv2 + v3pOn1/sumOn1 * Nv3;

    float v1pOn2 = 1/max(length(MBv1 - MBv2),0.001);
    float v2pOn2 = mixure;
    float v3pOn2 = 1/max(length(MBv3 - MBv2),0.001);
    float sumOn2 = v1pOn2 + v2pOn2 + v3pOn2;
    vec3 NewNv2 = v1pOn2/sumOn2 * Nv1 + v2pOn2/sumOn2 * Nv2 + v3pOn2/sumOn2 * Nv3;

    float v1pOn3 = 1/max(length(MBv1 - MBv3),0.001);
    float v2pOn3 = 1/max(length(MBv2 - MBv3),0.001);
    float v3pOn3 = mixure;
    float sumOn3 = v1pOn3 + v2pOn3 + v3pOn3;
    vec3 NewNv3 = v1pOn3/sumOn3 * Nv1 + v2pOn3/sumOn3 * Nv2 + v3pOn3/sumOn3 * Nv3;

    vec3 COMBINE = coe3 * normalize(NewNv3) + coe2 * normalize(NewNv2) + coe1 * normalize(NewNv1);
    float testbrightness = dot(light, (view * vec4(COMBINE,1)).xyz);

    //color = vec4(intensity-testbrightness);
    //呃，一项奇怪的边缘检测算法，看起来接近于上述两者的混合

    //为什么插位置没有问题？
    float envy1 = (dot(cross(Nv2, Nv3), interpolatenml));
    float envy2 = (dot(cross(Nv3, Nv1), interpolatenml));
    float envy3 = (dot(cross(Nv1, Nv2), interpolatenml));
    float test3 = envy3 / (envy1 + envy2 + envy3);
    float test1 = envy1 / (envy1 + envy2 + envy3);
    float test2 = envy2 / (envy1 + envy2 + envy3);
    vec3 originational = test3 * (Nv3) + test2 * (Nv2) + test1 * (Nv1);
    float bv = dot(light, (view * vec4(originational,1)).xyz);
    //byd这就插回去了？
    color= vec4(testbrightness);

}