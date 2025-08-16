#version 450 core





struct VertexStruct{
    float pos[3];
    float normal[3];
};

struct Vertex{
    vec3 pos;
    vec3 normal;
    vec3 overlay;
};

struct TriangleStruct{
    int V1;
    int V2;
    int V3;
};

struct AdjacentStruct{
    int V1;
    int V2;
    int V3;
    int F1;
    int F2;
    int F3;
};



struct GridHelper{
    float bbmin[3];
    float bbmax[3];
    float start;
    float end;
};

struct HitInfo{
    TriangleStruct id;
    double dist;
    vec3 where;
    bool boolean;
    int tid;
};

layout(std430, binding=0 ) readonly buffer VertexBlock{
    VertexStruct vertices[];
};

layout(std430, binding=1 ) readonly buffer TriangleBlock{
    TriangleStruct triangles[];
};

layout(std430, binding=2 ) readonly buffer GridRefBlock{
    int primrefs[];
};


layout(std430, binding=3 ) readonly buffer GridHelperBlock{
    float fglobalmin[3];
    float fglobalmax[3];
    float unitsize;
    float xcount;
    float ycount;
    float zcount;
    GridHelper grids[];
};

struct adjLocator{
    int start;
    int end;
};

layout(std430, binding=8 ) readonly buffer SparsePointCloud{
    int pointcloud[];
};


layout(std430, binding=9 ) readonly buffer FindPointsByTriangle{
    adjLocator adj[];
};

uniform int rand;


in vec3 N;
in vec3 T;
//in vec3 Nf;
//in vec3 Tf;


in vec3 Direc;
in vec3 Orig;
in vec3 Tangent;

in vec3 flatNormal;
uniform vec3 LIGHTDIRECTION;

layout (location=0) out vec4 scene;



vec4 quatmul(vec4 P, vec4 Q){
    vec3 vQ = Q.xyz;
    float rQ = Q.w;
    vec3 vP = P.xyz;
    float rP = P.w;
    
    float W = rQ*rP-dot(vQ,vP);
    vec3 V = rQ * vP + rP * vQ + cross(vQ,vP);
    
    return vec4(V,W);

}

//解决了一些噪点问题
bool anyOfVec(vec3 f, vec3 s){
    float E = 0.0001;
    if(f.x < s.x+E){return false;}
    if(f.y < s.y+E){return false;}
    if(f.z < s.z+E){return false;}
    return true;
}




float GaussianKernel(vec3 dist){

    float fdist = length(dist);//这个大概是一维高斯
    float x2 = fdist*fdist;
    float a = 0.1;
    return exp(-a * x2);
}



Vertex Volumetric(HitInfo Hit){

//表面应该是球状的，没有三角形瑕疵
//理论上加一圈迭代就能消灭三角形了啊
//为什么要加到12才有效果？

    TriangleStruct prim = Hit.id;
    VertexStruct pA = vertices[prim.V1];
    VertexStruct pB = vertices[prim.V2];
    VertexStruct pC = vertices[prim.V3];

    vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    vec3 B = vec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    vec3 C = vec3(pC.pos[0],pC.pos[1],pC.pos[2]);
    vec3 Normal1 = vec3(pA.normal[0],pA.normal[1],pA.normal[2]);
    vec3 Normal2 = vec3(pB.normal[0],pB.normal[1],pB.normal[2]);
    vec3 Normal3 = vec3(pC.normal[0],pC.normal[1],pC.normal[2]);

    vec3 point = Hit.where; //要对空间中的任意一点插值
    vec3 sum = vec3(0);

    Vertex result = Vertex(vec3(0), vec3(0), vec3(0));
    for(int i = adj[Hit.tid].start; i < adj[Hit.tid].end; i++){
        int p = pointcloud[i];
        VertexStruct pD = vertices[p];
        vec3 D = vec3(pD.pos[0],pD.pos[1],pD.pos[2]);
        vec3 Normal2 = vec3(pD.normal[0],pD.normal[1],pD.normal[2]);

        sum += GaussianKernel(point - D) * Normal2;

    }

    //高斯系数好像没什么影响

    //我觉得不是它们权重太高的缘故，它们的权重其实不高，在空间内假设都是混合均匀的。问题是这些空间有些会往左，有些往右，这是三角形面不同所带来的不同连接性造成的
    //妈的，那些边界球为什么不会被衰减掉
    //看看半径
    //呸，我他妈傻逼，没有开启uvmode
    //我真是脑瘫。再来逐个测试一遍
    //迭代次数完全不用管，因为高斯球能消掉它

    //高斯a太高表面会错位
    //太低会有棱

    //他妈的，果然是这三排的权重太高了 
    //我就说，这可是钟型曲线，这么近，影响非常大的
    //lol wtf，如果你把这三个点屏蔽掉了，那么效果就很怪异，这样全他妈的好像折边了一样
    //我收回我的话，这大概和下面这三行没有关系，是高斯a的锅，a太大了就是距离太小了，结果就折边了
    //这三行屏蔽掉，然后换个小a，就好一些了
    //但我想要凸起圆球状的曲面
    //好了，清晰了
    //迭代+12
    //a=0.1
    //屏蔽下面三行
    //效果很棒


    //接下来新开一个文件测试两个东西
    //迭代0，查询并对空间内所有点平滑
    //凸起圆球...

    //sum += GaussianKernel(point - A) * Normal1;
    //sum += GaussianKernel(point - B) * Normal2;
    //sum += GaussianKernel(point - C) * Normal3;


    result.normal = normalize(sum);
    return result;

}



Vertex TEST(HitInfo Hit){



    TriangleStruct prim = Hit.id;
    VertexStruct pA = vertices[prim.V1];
    VertexStruct pB = vertices[prim.V2];
    VertexStruct pC = vertices[prim.V3];

    vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    vec3 B = vec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    vec3 C = vec3(pC.pos[0],pC.pos[1],pC.pos[2]);


    vec3 point = Hit.where; //要对空间中的任意一点插值


    Vertex result = Vertex(vec3(0), vec3(0), vec3(0));
    for(int i = adj[Hit.tid].start; i < adj[Hit.tid].end; i++){
        int p = pointcloud[i];
        VertexStruct pD = vertices[p];
        vec3 D = vec3(pD.pos[0],pD.pos[1],pD.pos[2]);

        result.pos += D;
    }
    result.pos /= adj[Hit.tid].end - adj[Hit.tid].start;

    return result;

}







HitInfo Hit(vec3 Dirf, TriangleStruct prim, int id){

    dvec3 Dir = dvec3(Dirf);
    VertexStruct pA = vertices[prim.V1];
    VertexStruct pB = vertices[prim.V2];
    VertexStruct pC = vertices[prim.V3];

    dvec3 A = dvec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    dvec3 B = dvec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    dvec3 C = dvec3(pC.pos[0],pC.pos[1],pC.pos[2]);

    //Given raydirection & rayorigin , we have to calculate where this ray may intersect with plane
    //Plane is defined by triangle . For an arbitary point on that plane , P = a(P3-P1) + b(P2-P1) + P1
    //P is also on ray, so we have P = RO + i*RD
    //RO - P1 = a(P3-P1) + b(P2-P1) - i*RD
    dvec3 e1 = C-A;
    dvec3 e2 = B-A;
    dvec3 res = Orig-A;


    double det = dot(e1,cross(e2,Dir));
    double deta = dot(res,cross(e2,Dir));
    double detb = dot(e1,cross(res,Dir));
    
    double deti = dot(e1,cross(e2,res));
    double a = deta/det;
    double b = detb/det;
    double c = -deti/det;
    vec3 reached = Orig + float(c)*Dirf;

    //The interesting thing is , the algorithm we're going to use to test being in triangle , matches our definitions of 
    //P = a(P3-P1) + b(P2-P1) + P1

    return HitInfo(prim, c, reached, a>=double(0.) && a<=double(1.) && b>=double(0.) && b<=double(1.) && a+b<=double(1.), primrefs[id]);


}


HitInfo InsideOneGrid(vec3 Dir, GridHelper which){
    double FLT_MAX = 4096;
    HitInfo hitprim;
    for(int i= int(which.start);i< int(which.end);i++){
        TriangleStruct prim = triangles[primrefs[i]];
        HitInfo info = Hit(Dir, prim, i);
        if(info.boolean){
            if(info.dist<FLT_MAX){
                FLT_MAX = info.dist;
                hitprim = info;
            }
        }
    }
    return hitprim;
}








HitInfo RemasteredSearchForNext(ivec3 where_id){

    int startX = (where_id.x);
    int startY = (where_id.y);
    int startZ = (where_id.z);

    int ixc = int(xcount);
    int iyc = int(ycount);

    int depth = 50;
    vec3 Dir = -normalize(N);
    HitInfo analyseOutput;


    int stepX = Dir.x > 0 ? 1 : -1;
    int stepY = Dir.y > 0 ? 1 : -1;
    int stepZ = Dir.z > 0 ? 1 : -1;
	
	bool passTarget = false;

    for(int i = 0;i<depth;i++){

        bool X = startX < 0 || startX >= ixc;
        bool Y = startY < 0 || startY >= iyc;
        bool Z = startZ < 0 || startZ >= int(zcount);

        if(X||Y||Z){
            break;
        }

        GridHelper which = grids[startX + startY*ixc + startZ*ixc*iyc];
        vec3 bbmax = vec3(which.bbmax[0], which.bbmax[1], which.bbmax[2]);
        vec3 bbmin = vec3(which.bbmin[0], which.bbmin[1], which.bbmin[2]);

		vec3 GVTmaxraw = (bbmax - Orig)/Dir;
        vec3 GVTminraw = (bbmin - Orig)/Dir;
        vec3 GVTmax = max(GVTmaxraw, GVTminraw);
				

		analyseOutput = InsideOneGrid(Dir, which);
        

		if(!analyseOutput.boolean){
			if(GVTmax.x<GVTmax.z){
				if(GVTmax.x<GVTmax.y){
					//x<y x<z
					startX += stepX;
				}
				else{
					//y<x<z
					startY += stepY;
				}
			}
			else{
				if(GVTmax.z<GVTmax.y){
					//z<x z<y
					startZ += stepZ;
				}
				else{
					//y<z<x
					startY += stepY;
				}    
			}

		}else{
			break;
		}


        
    }

    return analyseOutput;
}


void main(){
    
    vec3 globalmin = vec3(fglobalmin[0], fglobalmin[1], fglobalmin[2]);
    vec3 globalmax = vec3(fglobalmax[0], fglobalmax[1], fglobalmax[2]);
    vec3 Dir = -normalize(N);
    vec3 VTmin = (globalmin - Orig)/Dir;
    vec3 VTmax = (globalmax - Orig)/Dir;
    VTmin = min(VTmax, VTmin);
    float Tmin = max(  VTmin.x ,max( VTmin.y,VTmin.z) );


    vec3 where = Orig + Dir * Tmin;
	ivec3 where_id;
	ivec3 shootAbroad = max(ivec3(0,0,0), ivec3(min(floor((where-globalmin)/unitsize),vec3(xcount-1,ycount-1,zcount-1))));
	ivec3 shootInside = max(ivec3(0,0,0), ivec3(min(floor((Orig -globalmin)/unitsize),vec3(xcount-1,ycount-1,zcount-1))));
	where_id = anyOfVec(Orig,globalmin)&&anyOfVec(globalmax,Orig)?shootInside:shootAbroad;


    HitInfo analyse = RemasteredSearchForNext(where_id);
    Vertex interpolation = Volumetric(analyse);


    vec3 sTa = normalize(T);
    vec3 sNo = normalize(N);
    float LengthTN = dot(sTa, sNo);
    vec3 alignedN = LengthTN * sNo;
    vec3 newT = sTa - alignedN;
    mat3 oTBN = mat3(normalize(newT), normalize(cross(newT, sNo)), sNo);

    vec3 tspaceN = transpose(oTBN) * (interpolation.normal);
    scene = vec4((tspaceN+1)*0.5, 1);
}
