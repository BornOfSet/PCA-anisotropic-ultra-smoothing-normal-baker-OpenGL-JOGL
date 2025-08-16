#version 330

in vec3 meshNormal;
in vec2 uv;

uniform sampler2D text0;

out vec4 col;
uniform mat4 view;
in vec3 N;
in vec3 T;
in vec3 Nf;
in vec3 Tf;

void main(){
    vec4 sample = texture(text0, uv);
    vec3 tangentspace = sample.xyz * 2 - 1;
    vec3 Ta = normalize(T);//normalize(T)有问题，导致极化
    vec3 No = normalize(N);
    //注意，如果是八猴导入的，需要-cross。如果是内循环自我消化，那么编码解码的符号一致，不需要-cross
    vec3 Bi = normalize(cross(Ta, No));
    //等等！叉乘出来的模长不为1！是1x1xsin！你妈！
    //不对啊，sin90不就是1吗
    mat3 Rotationmatrix = mat3(Ta,Bi,No);
    //正交的
    float LengthTN = dot(Ta, No);
    vec3 alignedN = LengthTN * No;
    vec3 newT = Ta - alignedN;
    mat3 oTBN = mat3(normalize(newT), normalize(cross(newT, No)), No);

    //mat3 fTBN = mat3(normalize(Tf), normalize(cross(Tf, Nf)),normalize(Nf));

    vec3 worldspace = oTBN * tangentspace;
    //worldspace = normalize(worldspace);
    col.xyz = vec3(dot((view * vec4(worldspace,0)).xyz,vec3(0,0,-1)));
    //col.xyz = abs(clamp(Ta,0,1) - sample.xyz) * 200;
    //col.xyz = normalize(meshNormal) * Rotationmatrix *0.5 + 0.5;
    //col.xyz = abs(transpose(Rotationmatrix) * vec3(0,1,0) * .5 + .5 - sample.xyz) * 200;
    //col.xyz = oTBN * transpose(oTBN) * vec3(0,1,0);

    //object space
    //col = view * vec4(tangentspace,0);

    //ol.xyz = sample.xyz;
    //col = sample;
}