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


layout(std430, binding=8 ) readonly buffer AdjTriangleBlock{
    TriangleStruct adj[];
};


in vec3 N;
in vec3 T;
in vec3 Nf;
in vec3 Tf;


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







Vertex Barycentric(HitInfo Hit){

    if(!Hit.boolean){
        Vertex r = Vertex(vec3(0), vec3(0), vec3(0));
        r.pos = vec3(0);
        r.normal = vec3(5);
        r.overlay = vec3(0,1,0);
        return r;
    }  

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

    vec3 reached = Hit.where;

    Vertex r = Vertex(vec3(0), vec3(0), vec3(0));

    //hyper bary
    VertexStruct pD = vertices[adj[Hit.tid].V1];//antiC
    VertexStruct pE = vertices[adj[Hit.tid].V2];//antiB
    VertexStruct pF = vertices[adj[Hit.tid].V3];//antiA

    vec3 D = vec3(pD.pos[0],pD.pos[1],pD.pos[2]);
    vec3 E = vec3(pE.pos[0],pE.pos[1],pE.pos[2]);
    vec3 F = vec3(pF.pos[0],pF.pos[1],pF.pos[2]);

    vec3 Normal4 = vec3(pD.normal[0],pD.normal[1],pD.normal[2]);
    vec3 Normal5 = vec3(pE.normal[0],pE.normal[1],pE.normal[2]);
    vec3 Normal6 = vec3(pF.normal[0],pF.normal[1],pF.normal[2]);

    vec3 r_A = reached - A;
    vec3 r_B = reached - B;
    vec3 r_C = reached - C;
    vec3 r_D = reached - D;
    vec3 r_E = reached - E;
    vec3 r_F = reached - F;

    //
    float rA_AC = dot(r_A, C-A)/dot(C-A,C-A);
    float rC_AC = dot(r_C, A-C)/dot(C-A,C-A);
    //这俩加起来应该是1? |ra|*|ca|*cos(rac) + |rc|*|ac|*cos(rca) = |ac| * (cos(rac)*|ra| + cos(rca)*|rc|)


    r.overlay = vec3(rA_AC+rC_AC);
    /////哈哈哈哈哈哈哈，我完成了，哈哈哈哈好，这真的是1，，哈哈哈哈好，我有邻接信息了，哈哈哈哈，我会发siggraph，我一定会成功，啊啊哈哈哈哈哈，我肯定会成功


    float rB_BE = dot(r_B, E-B)/dot(E-B,E-B);
    float rE_BE = dot(r_E, B-E)/dot(E-B,E-B);


    float rA_AB = dot(r_A, B-A)/dot(A-B,A-B);
    float rB_AB = dot(r_B, A-B)/dot(A-B,A-B);


    float rD_CD = dot(r_D, C-D)/dot(C-D,C-D);
    float rC_CD = dot(r_C, D-C)/dot(C-D,C-D);


    float rC_CB = dot(r_C, B-C)/dot(B-C,B-C);
    float rB_CB = dot(r_B, C-B)/dot(B-C,B-C);


    float rA_AF = dot(r_A, F-A)/dot(F-A,F-A);
    float rF_AF = dot(r_F, A-F)/dot(F-A,F-A);

    //C在A点一定是0，但在AB的中点不一定是0?
    
    //长表达式重复ABC
    //r.pos = vec3(rF_AF*A + rE_BE*B + rD_CD*C + rC_CD*D + rB_BE*E + rA_AF*F + rB_CB*C + rC_CB*B + rA_AB*B + rB_AB*A + rA_AC*C + rC_AC*A)/6;

    //短表达式插六项ABCDEF
    r.normal = vec3(rF_AF*Normal1 + rE_BE*Normal2 + rD_CD*Normal3 + rC_CD*Normal4 + rB_BE*Normal5 + rA_AF*Normal6)/3;
    //基于正确的线性插值
    r.pos = vec3(rF_AF*A + rE_BE*B + rD_CD*C + rC_CD*D + rB_BE*E + rA_AF*F)/3;


    //超短三角表达式仅ABC
    //r.normal = vec3(rF_AF*Normal1 + rE_BE*Normal2 + rD_CD*Normal3);

    //巡回ABC
    //r.normal = rB_CB*Normal3 + rC_CB*Normal2 + rA_AB*Normal2 + rB_AB*Normal1 + rA_AC*Normal3 + rC_AC*Normal1;


    //合并ABC
    //r.normal = rA_AC*rB_CB*Normal3 + rA_AB*rC_CB*Normal2 + rC_AC*rB_AB*Normal1;


    //双三角形
    //r.normal = normalize(vec3(rC_CD*Normal4 + rB_BE*Normal5 + rA_AF*Normal6));
    //r.normal += vec3(rF_AF*Normal1 + rE_BE*Normal2 + rD_CD*Normal3);


    return r;

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

    return HitInfo(prim, c, reached, a>=double(0.) && a<=double(1.) && b>=double(0.) && b<=double(1.) && a+b<=double(1.), id);


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
    Vertex interpolation = Barycentric(analyse);


    vec3 sTa = normalize(T);
    vec3 sNo = normalize(N);
    float LengthTN = dot(sTa, sNo);
    vec3 alignedN = LengthTN * sNo;
    vec3 newT = sTa - alignedN;
    mat3 oTBN = mat3(normalize(newT), normalize(cross(newT, sNo)), sNo);
    mat3 fTBN = mat3(normalize(Tf), normalize(cross(Tf, Nf)),normalize(Nf));

    vec3 tspaceN = transpose(oTBN) * normalize(interpolation.normal);

    scene = vec4((tspaceN+1)*0.5, 1);
    //scene.xyz = interpolation.pos;

}
