package jogl8.sim;

import assimp.AiMesh;
import glm_.vec3.Vec3;
import jogl8.utils;

public class buildgrid {
	
	AiMesh self;
	float minmove = -1f;
	float maxmove = 1f;
	float unitsize =.5f;
	float auto = 5;
	
	//不允许更改
	private boolean useAuto = false;
	
	public buildgrid(AiMesh x) {
		self = x;
	}
	
	//used in geometry proessing
	public Vec3[] points;
	public Vec3 GlobalMin;
	public Vec3 GlobalMax;
	int xcount; 
	int ycount;
	int zcount;
	
	//per component lesser
	private void percless(Vec3 receive, Vec3 p) {
		float a = p.getX();
		float b = p.getY();
		float c = p.getZ();
		float u = receive.getX();
		float v = receive.getY();
		float w = receive.getZ();
		if(a<=u) receive.setX(a);
		if(b<=v) receive.setY(b);
		if(c<=w) receive.setZ(c);
	}
	
	//per component greater
	private void percgreat(Vec3 receive, Vec3 p) {
		float a = p.getX();
		float b = p.getY();
		float c = p.getZ();
		float u = receive.getX();
		float v = receive.getY();
		float w = receive.getZ();
		if(a>=u) receive.setX(a);
		if(b>=v) receive.setY(b);
		if(c>=w) receive.setZ(c);
	}
	
	//
	public Vec3[] func() {
		
		//找到总体AABB，无精度损失
		//在这个过程中，顶点本身是多少float，边界就是同样的数字
		Vec3 max = new Vec3(Float.MIN_VALUE);
		Vec3 min = new Vec3(Float.MAX_VALUE);
		for(int i=0;i<self.getNumVertices();i++) {
			Vec3 p = self.getVertices().get(i);
			percless(min,p);
			percgreat(max,p);
		}

		//不要计算range，这会导致精度损失。
		//譬如0.6并不是完全的0.6，而是0.60002
		//0.3也不是完全的0.3，可能是0.2995
		//这样导致算得的实际长度0.300004超出0.6-0.3=0.3
		//这导致这样的长度需要不止3个0.99925(0.1)来表示
		//在range本身具有误差的情况下，unitsize除法贡献的误差“重复贡献误差”
		//ceil不会因为多一步除法变成+2。哪怕没了除法它依然会+1
		//max = max.plus(maxmove);
		//min = min.plus(minmove);
		
		Vec3 range = max.minus(min);
		//要是把它改成floor....呃，你要知道我们左边的0是固定的，改成floor就相当于右边凭空少了一格
		//xcount = (int) Math.ceil(range.getX()/unitsize);//计算的就是盒子的数量，不是挡板的数量
		//ycount = (int) Math.ceil(range.getY()/unitsize);
		//zcount = (int) Math.ceil(range.getZ()/unitsize);
		
		//这里的xcount不保证为整数，因为range不保证为倍数
		xcount = utils.Envelop(range.getX(), unitsize);
		ycount = utils.Envelop(range.getY(), unitsize);
		zcount = utils.Envelop(range.getZ(), unitsize);
		Vec3 H[] = new Vec3[xcount*ycount*zcount];
		Vec3 start = min.plus(unitsize/2);
		
		//max = new Vec3(Float.MIN_VALUE);
		//min = new Vec3(Float.MAX_VALUE);
		
		//MAXIMUM=xcount-1
		for(int x=0;x<xcount;x++) {
			for(int y=0;y<ycount;y++) {
				for(int z=0;z<zcount;z++) {
					Vec3 delta = new Vec3(x*unitsize,y*unitsize,z*unitsize);
					int count = x + y*xcount + z*ycount*xcount;
					H[count] = start.plus(delta);

					//xc-1 + xc*(yc-1) + yc*xc*(zc-1) = xc-1 + xc*yc - xc + yc*xc*zc - yc*xc = xc*yc*zc-1
					//percless(min,H[count]);
					//percgreat(max,H[count]);
				}
			}
		}
		//utils.logv(min.getArray(),"FloatingPoints");
		//utils.logv(max.getArray(),"FloatingPoints");
		//GlobalMin = min.minus(unitsize/2);
		//GlobalMax = max.plus(unitsize/2);
		GlobalMin = min;
		GlobalMax = min.plus(new Vec3(xcount,ycount,zcount).times(unitsize));
		//utils.LOG(xcount + " " + ycount + " " + zcount);
		utils.LOG(xcount + "储存的    误差" + GlobalMax.minus(GlobalMin).getX()/unitsize + "找到整数" + utils.Clean(GlobalMax.minus(GlobalMin).getX()/unitsize));
		utils.LOG(ycount + "储存的    误差" + GlobalMax.minus(GlobalMin).getY()/unitsize + "找到整数" + utils.Clean(GlobalMax.minus(GlobalMin).getY()/unitsize));
		utils.LOG(zcount + "储存的    误差" + GlobalMax.minus(GlobalMin).getZ()/unitsize + "找到整数" + utils.Clean(GlobalMax.minus(GlobalMin).getZ()/unitsize));
		//公式1：(最远中心坐标新max值+一半unitsize-边界框左下值旧min值)/unitsize=ceil(....)
		//公式2：(最远中心坐标新max值-最近中心坐标值新min值)/unitsize=ceil-1
		this.points = H;
		return H;
	}
}
