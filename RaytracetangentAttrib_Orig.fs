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

//也是一样的理由要改一下数据类型
//vec3 is always rounded up to vec4
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

//vec3和vec3数组的align都是4N，这里使用std430布局下的float[3]可以正确读取数据
//std430：align不再为4N，能够正确读取数组内；本数组的长度没有任何padding，使得数组外下一个成员的alignoffset能够严丝合缝地等于baseoffset
//原数据是以float的格式储存的，因此就以float类型来读取。转化成int类型的时候不会保持数值相等
//如果要使得interface block可以同时读出float 和int，那么java里面要设置是bytebuffer
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

//注意，这个是0，因为删掉了GS
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


Vertex Barycentric(HitInfo Hit){

    if(!Hit.boolean){
        Vertex r = Vertex(vec3(0), vec3(0), vec3(0));
        r.pos = vec3(0);
        r.normal = normalize(cross(dFdx(Orig), dFdy(Orig)));
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

    vec3 L1 = A - reached;
    vec3 L2 = B - reached;
    vec3 L3 = C - reached;

    vec3 L4 = B-A;
    vec3 L5 = C-A;
    //面积坐标
    float S1 = length(cross(L1,L2));
    float S2 = length(cross(L1,L3));
    float S3 = length(cross(L2,L3));
    float S4 = length(cross(L4,L5));

    float c1 = S3/S4;
    float c2 = S2/S4;
    float c3 = S1/S4;

    //显示网格，debug用途
    //为啥点显示不出来呢
    //z到底是怎么样的？
    float E = 0.008;
    float llerp = 0;
    if(S1<E||S2<E||S3<E){
        llerp=1;
    }
    float PointSize = 0.008;
    float plerp = 0;
    if(length(L1)<PointSize||length(L2)<PointSize||length(L3)<PointSize){
        plerp = 1;
    }
    
    vec3 col = vec3(0);
    col=mix(col,vec3(1),llerp);
    col=mix(col,vec3(1,0,0),plerp);

    //if hit.boolean == true
    //指的是存在碰撞
    //这里做出条件区分，以区分周围的黑背景底色和物体
    //DEBUG条件:重心插值公式c1*A + c2*B + c3*C==reached
    //感觉是reached算的有问题，跟A B C 相差很大。A没有x的地方，reached全是X，但这个地方理应X接近0的
    r.pos = c1*A + c2*B + c3*C;
    r.normal = c1*Normal1 + c2*Normal2 + c3*Normal3;
    r.overlay = col;

    return r;

}



HitInfo Hit(vec3 Dir, TriangleStruct prim){
    VertexStruct pA = vertices[prim.V1];
    VertexStruct pB = vertices[prim.V2];
    VertexStruct pC = vertices[prim.V3];

    vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    vec3 B = vec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    vec3 C = vec3(pC.pos[0],pC.pos[1],pC.pos[2]);

    //Given raydirection & rayorigin , we have to calculate where this ray may intersect with plane
    //Plane is defined by triangle . For an arbitary point on that plane , P = a(P3-P1) + b(P2-P1) + P1
    //P is also on ray, so we have P = RO + i*RD
    //RO - P1 = a(P3-P1) + b(P2-P1) - i*RD
    vec3 e1 = C-A;
    vec3 e2 = B-A;
    vec3 res = Orig-A;
    //通过使用double，解决了明明在三角形上或内却被透视的问题，没有了噪点
    //妈的，我知道为什么pos有问题了。当初我复制的时候这里是direc，不是翻转的dir。你妈的
    //鸡巴，没解决
    //过点A作法线为ABxDir的平面，AC在平面的法线上的投影长度为det
    double det = dot(e1,cross(e2,Dir));
    double deta = dot(res,cross(e2,Dir));
    double detb = dot(e1,cross(res,Dir));
    //作平面ABO，AC在平面ABO的法线n上的投影长度，为deti
    //如果deti等于0，AC就是ABO的法线，AC垂直AB
    //我脑瘫了，cross出来的是他妈的法线，deti等于0的话，就是AC垂直法线，那ABO平面就是ABC平面，O本身就落在ABC平面里，自然不需要再步进
    double deti = dot(e1,cross(e2,res));//他妈的，抄错了。这个是res不是RD
    double a = deta/det;
    double b = detb/det;
    double c = -deti/det;
    //奇怪，要说方向得normalize，我也干了啊
    vec3 reached = Orig + float(c)*Dir;

    bool inside = false;
    //The interesting thing is , the algorithm we're going to use to test being in triangle , matches our definitions of 
    //P = a(P3-P1) + b(P2-P1) + P1
    //That is , a∈[0,1] , b∈[0,1] , a+b∈[0,1]
    if(a>=0. && a<=1. && b>=0. && b<=1. && a+b<=1.){
        inside = true;
    }
    return HitInfo(prim, c, reached, inside);


}


HitInfo InsideOneGrid(vec3 Dir, GridHelper which){
    double FLT_MAX = 4096;
    HitInfo hitprim;// false->遍历所有图元发现不存在碰撞
    for(int i= int(which.start);i< int(which.end);i++){
        TriangleStruct prim = triangles[primrefs[i]];
        HitInfo info = Hit(Dir, prim);
        if(info.boolean){
            if(info.dist<FLT_MAX){
                FLT_MAX = info.dist;
                hitprim = info;
                //info.boolean表示在三角形内，它一定是true，因此可以直接用过来，使得hitprim.boolean等于true，但这时候意味着存在碰撞
            }
        }
    }
    return hitprim;
}


HitInfo SearchForNext(float Tdir, vec3 Dir, float Control, vec3 GlobalMin, HitInfo OldAnalyse){
    HitInfo analyseOutput;
    float LastgridExit = Control;
    int safety = 0;//这行代码救了老命了，如果orig是0，dir是0，就是我加了Gs，接口名字变了，FS里面查不到，这时候就是死循环了
    while(safety < 100){
        safety++;
        //在进入这个函数之前，我希望你记住，开头我们就开始算下一个格子了，因此请保证你在调用此函数前已经算过了old analyse,也就是上一个格子
        //这里只是简单地根据position来判断这个格子在不在整体aabb内
        //请注意， Orig + Dir*(LastgridExit+0.0001);这段代码是控制退出的根本保障
        vec3 NextgridEntry = Orig + Dir*(LastgridExit+Tdir*0.0001);
        vec3 originaldata = ceil((NextgridEntry-GlobalMin)/unitsize);
        //cnm这里有个死循环，要不是debug我还真发现不了
        //在0边界上有问题。originaldata取0的时候，意味着unitsize < NextgridEntry-GlobalMin < 0，但是ceil到0.
        //要么让它<=0，要么把ceil((NextgridEntry-GlobalMin)/unitsize);的ceil去掉
        //否则在0边界上它会永远循环下去，因为它总是无法<0。而且GVTmax算来算去都是同样的
        //不能去掉ceil，那后面就算崩了
        //呵呵，我想的很美好，实际上少了个NextgridEntry-GlobalMin)/unitsize<0的时候originaldata取-1的操作
        //所以这个程序之前是14.0111算15跳出
        //-0.0001算0继续循环
        bool X = originaldata.x <= 0 || originaldata.x > xcount;
        bool Y = originaldata.y <= 0 || originaldata.y > ycount;
        bool Z = originaldata.z <= 0 || originaldata.z > zcount;
        if(X||Y||Z){
            //这个格子不存在，返回已知存在的最后一个格子
            //不能这么id1d>=xcount*ycount*zcount判断，因为错位
            //不能在此之前max(vec3(0,0,0),ceil((NextgridEntry-GlobalMin)/unitsize)-1.0);，因为-2也被算作0了
            analyseOutput = OldAnalyse;
            break;
        }else{
            //我怀疑这里得出的analyse还是等于OldAnalyse
            //哦，是因为我们没有做判断就强行算next analyse
            vec3 id = max(vec3(0,0,0),originaldata-1.0);//结合上述bool规则判断，得到的数据范围在0~count-1，而实际上的数组范围在[0,count)，因此没问题
            float id1d = id.x + id.y*xcount + id.z*xcount*ycount;
            GridHelper which = grids[int(id1d)];
            vec3 bbmax = vec3(which.bbmax[0], which.bbmax[1], which.bbmax[2]);
            vec3 bbmin = vec3(which.bbmin[0], which.bbmin[1], which.bbmin[2]);
            vec3 GVTmaxraw = (bbmax - Orig)/Dir;
            vec3 GVTminraw = (bbmin - Orig)/Dir;
            vec3 GVTmax = max(GVTmaxraw, GVTminraw);
            HitInfo analyse = InsideOneGrid(Dir, which);
            if(!analyse.boolean){
                //如果这个格子里面没有碰到任何一个图元
                //dir, globalmin 都相等，不需要递归
                //analyse既然啥都没碰到，也不需要更新
                //每次递归更新的特征是LastgridExit
                //特征：递归的退出条件，每轮都不一样，其他的每轮都是一样的
                LastgridExit = min( GVTmax.x, min(GVTmax.y,GVTmax.z));
            }else{
                //碰到图元了，它一定是最近的，那么返回
                analyseOutput = analyse;
                break;
            }
        }
    }
    //如果出现如(LastgridExit-Tdir*0.0001）的情况，下一个只不过是往里面走，算下来总是等于这一个，导致无限循环的情况，那么analyse全部给黑色
    //safety是有用的，如果改成上面这个情况就会不断递归本身，需要safety
    //不过其实是没用的。因为在main里面已经保证min = min(max, min)，换言之，tmax一定大于tmin，因此这里给正数是ok的，但是如果模型本身就是反着的，要避免死循环
    return analyseOutput;
}


void main(){
    
    //计算光线在整个立方体上的落点
    vec3 globalmin = vec3(fglobalmin[0], fglobalmin[1], fglobalmin[2]);
    vec3 globalmax = vec3(fglobalmax[0], fglobalmax[1], fglobalmax[2]);
    vec3 Dir = -normalize(N);
    vec3 VTmin = (globalmin - Orig)/Dir;
    vec3 VTmax = (globalmax - Orig)/Dir;
    VTmin = min(VTmax, VTmin);
    float Tmin = max(  VTmin.x ,max( VTmin.y,VTmin.z) );
    vec3 where = Orig + Dir * Tmin;//这个where考虑到了低模发射器的坐标在高模grid里面的情况，这个情况下它会向后找到延长线最处进入的位置，也就是将光线从射线扩展为直线
    //增加了从fglobalmax转化为globalmax的步骤，截止到where变量，一切都已经修复。现在opengl不带padding地读取缓冲

    //为什么where_id会有负数？因为where是射线和整个AABB接触的入点，而在CPU方面，我们算的是AABB内一点的ceil，那么显然前者因为始终在面上而产生0/0.2=0
    //但是我们不能为了舍入y=-1到y=0而强迫第一个格子里面的所有点在ceil(x)到1后并没有调整到index=0上
	
    //未修正的
    //ivec3 where_id = max(ivec3(0,0,0),ivec3(ceil((Orig-globalmin)/unitsize)) - 1);

    //修正的
    ivec3 where_id = max(ivec3(0,0,0),ivec3(ceil(Orig/unitsize)) - ivec3(ceil(globalmin/unitsize)) - 1);


    //接下来研究一下int和float的转换
    //允许对float使用int(value)转化成等数值的int类型
    //但是不允许对IEEE754标准储存的float SSBO以int类型来读
    //显然，这一切的根源都在于，你往FloatBuffer里面put一个int变量，那么会产生类型转化把int编码成等面额的float，此时它是float形式组织的，因此shader用int去解释就会出错
    //但是glsl里的int()方法在转化的时候确实保留面额
    //如果要Debug where_id_1D，那么请scene = vec4(float(where_id_1D)/1000);
    //int where_id_1D = int(where_id.x) + int(where_id.y)*int(xcount) + int(where_id.z)* int(xcount*ycount);
    int where_id_1D = where_id.x + where_id.y*int(xcount) + where_id.z*int(xcount)*int(ycount);

    GridHelper which = grids[int(where_id_1D)];
    vec3 bbmax = vec3(which.bbmax[0], which.bbmax[1], which.bbmax[2]);
    vec3 bbmin = vec3(which.bbmin[0], which.bbmin[1], which.bbmin[2]);
    vec3 GVTmaxraw = (bbmax - Orig)/Dir;//V是vector的意思。G指的是grid
    vec3 GVTminraw = (bbmin - Orig)/Dir;
    //经过测试，which.start正确了
    //但是为什么GwhereEnter和where不是一致的？
   
    vec3 GVTmax = max(GVTmaxraw, GVTminraw);
    vec3 GVTmin = min(GVTmaxraw, GVTminraw);
    //GVTmax GVTmin这两行代码也被证明是必需的
    //这两行有点问题
    //参见图1
   
    float GTmax = min( GVTmax.x, min(GVTmax.y,GVTmax.z));
    //修复了GTmin来自GVTmax的问题
    //但是为什么GTmin和Tmin不相等，导致GWhereEnter更亮?
    //这个问题也很简单，因为GWhereEnter计算的是距离你最近的“内膜”，而Tmin的时候只看得到最外围的一个包裹壳，根本不存在“内膜”，简单来说，就是现在算的是一个非流形立方体
    //上述错误。典中典之unitsize没/2
    //妈的，不只是这样。我曹，我搞明白了。我CPU端的globalmax和globalmin放得是模型的AABB+delta量
    //但实际上这个东西不能保证最大值-最小值可以被unitsize整除
    //所以，在将unitsize纳入计算，作为递增量从min+unit/2的起点开始一路加到最后的时候，新得到的边界是不等于globalmax这个不能被整除的边界的
    float GTmin = max( GVTmin.x, max(GVTmin.y,GVTmin.z));
    vec3 GWhereLeave = Orig + Dir*GTmax;
    vec3 GWhereEnter = Orig + Dir*GTmin;

    float Tdir = GTmax - GTmin;
    Tdir = Tdir / abs(Tdir);
    
    //Debug:理应GWhereEnter==where
    
    
    HitInfo analyse = InsideOneGrid(Dir, which);
    //这里加上判断
    if(!analyse.boolean){
        //analyse = SearchForNext(Tdir, Dir, GTmax , globalmin, analyse);
    }

    //scene.xyz = GWhereEnter;
    Vertex interpolation = Barycentric(analyse);



    //平坦TBN
    vec3 Ta = normalize(Tf);
    vec3 No = normalize(Nf);
    vec3 Bi = normalize(cross(Ta, No));
    mat3 fTBN = mat3(Ta,Bi,No);



    mat3 oTBN = mat3(T, cross(T, N), N);

    vec3 tspaceN = transpose(oTBN) * normalize(interpolation.normal);
    //vec3 MikkNormal = normalize(interpolation.normal.x * T + interpolation.normal.y * cross(T, N) + interpolation.normal.z * N);

    scene = vec4((tspaceN+1)*0.5, 1);
    //scene.xyz = MikkNormal; 
    //scene.xyz = sNo;
    scene.xyz = normalize(interpolation.normal);
    //scene.xyz = interpolation.overlay;
    //scene.xyz = transpose(sTBN) * vec3(0,1,0) * .5 + .5;
    //注意，这里有个灰域问题
    //如果某个空间，低模占据了，但高模没有占据，在TBN矩阵变换的时候，会产生灰色，相当于0*0.5 + 0.5
    //我想把它改成 0 0 0.5
    //咳咳，什么鬼，那只是因为那个地方光追的时候就没找到东西
    //好，现在如果光追没有碰到格子内的任一个图元，都会返回flatnormal
    //但是它能不能估算成就近的格子？这样模型在贴上法线贴图后，边角不会有瑕疵
    
    //有个问题，就是这样只能追踪uv块对应的区域，区域里面射空的地方都是0 0 .5
    //但是FS初始色是黑色，因为这些地方不被光栅化
}