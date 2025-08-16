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




layout(std430, binding=8 ) readonly buffer AntiVertexBlock{
    VertexStruct anti_vertices[];
};

layout(std430, binding=9 ) readonly buffer AntiTriangleBlock{
    TriangleStruct anti_triangles[];
};


layout(std430, binding=10 ) readonly buffer RGridRefBlock{
    int Rprimrefs[];
};


layout(std430, binding=11 ) readonly buffer RGridHelperBlock{
    float Rfglobalmin[3];
    float Rfglobalmax[3];
    float Runitsize;
    float Rxcount;
    float Rycount;
    float Rzcount;
    GridHelper Rgrids[];
};




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

    dvec3 A = dvec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    dvec3 B = dvec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    dvec3 C = dvec3(pC.pos[0],pC.pos[1],pC.pos[2]);

    dvec3 Normal1 = dvec3(pA.normal[0],pA.normal[1],pA.normal[2]);
    dvec3 Normal2 = dvec3(pB.normal[0],pB.normal[1],pB.normal[2]);
    dvec3 Normal3 = dvec3(pC.normal[0],pC.normal[1],pC.normal[2]);

    dvec3 reached = Hit.where;

    Vertex r = Vertex(vec3(0), vec3(0), vec3(0));

    dvec3 L1 = A - reached;
    dvec3 L2 = B - reached;
    dvec3 L3 = C - reached;

    dvec3 L4 = B-A;
    dvec3 L5 = C-A;
    double S1 = length(cross(L1,L2));
    double S2 = length(cross(L1,L3));
    double S3 = length(cross(L2,L3));
    double S4 = length(cross(L4,L5));

    double c1 = S3/S4;
    double c2 = S2/S4;
    double c3 = S1/S4;

    double E = 0.0001;
    float llerp = 0;
    if(S1<E||S2<E||S3<E){
        llerp=1;
    }
    double PointSize = 0.001;
    float plerp = 0;
    if(length(L1)<PointSize||length(L2)<PointSize||length(L3)<PointSize){
        plerp = 1;
    }
    
    vec3 col = vec3(0);
    col=mix(col,vec3(1),llerp);
    col=mix(col,vec3(1,0,0),plerp);


    r.pos = vec3(c1*A + c2*B + c3*C);
    r.normal = vec3(c1*Normal1 + c2*Normal2 + c3*Normal3);
    r.overlay = col;

    return r;

}






Vertex Barycentric_SitraAchra(HitInfo Hit){

    if(!Hit.boolean){
        Vertex r = Vertex(vec3(0), vec3(0), vec3(0));
        r.pos = vec3(0);
        r.normal = vec3(5);
        r.overlay = vec3(0,1,0);
        return r;
    }  

    TriangleStruct prim = Hit.id;
    VertexStruct pA = anti_vertices[prim.V1];
    VertexStruct pB = anti_vertices[prim.V2];
    VertexStruct pC = anti_vertices[prim.V3];

    dvec3 A = dvec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    dvec3 B = dvec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    dvec3 C = dvec3(pC.pos[0],pC.pos[1],pC.pos[2]);

    dvec3 Normal1 = dvec3(pA.normal[0],pA.normal[1],pA.normal[2]);
    dvec3 Normal2 = dvec3(pB.normal[0],pB.normal[1],pB.normal[2]);
    dvec3 Normal3 = dvec3(pC.normal[0],pC.normal[1],pC.normal[2]);

    dvec3 reached = Hit.where;

    Vertex r = Vertex(vec3(0), vec3(0), vec3(0));

    dvec3 L1 = A - reached;
    dvec3 L2 = B - reached;
    dvec3 L3 = C - reached;

    dvec3 L4 = B-A;
    dvec3 L5 = C-A;
    double S1 = length(cross(L1,L2));
    double S2 = length(cross(L1,L3));
    double S3 = length(cross(L2,L3));
    double S4 = length(cross(L4,L5));

    double c1 = S3/S4;
    double c2 = S2/S4;
    double c3 = S1/S4;

    double E = 0.0001;
    float llerp = 0;
    if(S1<E||S2<E||S3<E){
        llerp=1;
    }
    double PointSize = 0.001;
    float plerp = 0;
    if(length(L1)<PointSize||length(L2)<PointSize||length(L3)<PointSize){
        plerp = 1;
    }
    
    vec3 col = vec3(0);
    col=mix(col,vec3(1),llerp);
    col=mix(col,vec3(1,0,0),plerp);


    r.pos = vec3(c1*A + c2*B + c3*C);
    r.normal = vec3(c1*Normal1 + c2*Normal2 + c3*Normal3);
    r.overlay = col;

    return r;

}







HitInfo Hit(vec3 Dirf, TriangleStruct prim){

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

    return HitInfo(prim, c, reached, a>=double(0.) && a<=double(1.) && b>=double(0.) && b<=double(1.) && a+b<=double(1.));


}






HitInfo Hit_SitraAchra(vec3 Dirf, TriangleStruct prim){

    dvec3 Dir = dvec3(Dirf);
    VertexStruct pA = anti_vertices[prim.V1];
    VertexStruct pB = anti_vertices[prim.V2];
    VertexStruct pC = anti_vertices[prim.V3];

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

    return HitInfo(prim, c, reached, a>=double(0.) && a<=double(1.) && b>=double(0.) && b<=double(1.) && a+b<=double(1.));


}




HitInfo InsideOneGrid(vec3 Dir, GridHelper which){
    double FLT_MAX = 4096;
    HitInfo hitprim;
    for(int i= int(which.start);i< int(which.end);i++){
        TriangleStruct prim = triangles[primrefs[i]];
        HitInfo info = Hit(Dir, prim);
        if(info.boolean){
            if(info.dist<FLT_MAX){
                FLT_MAX = info.dist;
                hitprim = info;
            }
        }
    }
    return hitprim;
}



HitInfo InsideOneGrid_SitraAchra(vec3 Dir, GridHelper which){
    double FLT_MAX = 4096;
    HitInfo hitprim;
    for(int i= int(which.start);i< int(which.end);i++){
        //不一样的start end设置，不一样的图元索引
        TriangleStruct prim = anti_triangles[Rprimrefs[i]];
        HitInfo info = Hit_SitraAchra(Dir, prim);
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






HitInfo RemasteredSearchForNext_SitraAchra(ivec3 where_id){

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

        //都是同一个模型，只是自旋不一样，那么grid id应该可以通用，但是储存实际内容的grid不一样，grid储存的实际内容不一样
        GridHelper which = Rgrids[startX + startY*ixc + startZ*ixc*iyc];
        vec3 bbmax = vec3(which.bbmax[0], which.bbmax[1], which.bbmax[2]);
        vec3 bbmin = vec3(which.bbmin[0], which.bbmin[1], which.bbmin[2]);

		vec3 GVTmaxraw = (bbmax - Orig)/Dir;
        vec3 GVTminraw = (bbmin - Orig)/Dir;
        vec3 GVTmax = max(GVTmaxraw, GVTminraw);
				

		analyseOutput = InsideOneGrid_SitraAchra(Dir, which);
        

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





    //追踪另一个模型
    HitInfo Ranalyse = RemasteredSearchForNext_SitraAchra(where_id);
    Vertex Rinterpolation = Barycentric_SitraAchra(Ranalyse);

    vec3 sTa = normalize(T);
    vec3 sNo = normalize(N);
    float LengthTN = dot(sTa, sNo);
    vec3 alignedN = LengthTN * sNo;
    vec3 newT = sTa - alignedN;
    mat3 oTBN = mat3(normalize(newT), normalize(cross(newT, sNo)), sNo);
    mat3 fTBN = mat3(normalize(Tf), normalize(cross(Tf, Nf)),normalize(Nf));

    vec3 tspaceN = transpose(oTBN) * (Rinterpolation.normal + interpolation.normal);
    
    scene = vec4((tspaceN+1)*0.5, 1);

    //scene.xyz = normalize(normalize(Rinterpolation.normal) - normalize(interpolation.normal));




}
