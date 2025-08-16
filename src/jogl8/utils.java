package jogl8;

import java.nio.FloatBuffer;
import java.util.List;

import com.jogamp.opengl.GL3;

import glm_.mat4x4.Mat4;
import glm_.vec3.Vec3;

public class utils {
	public static void log(String s) {
		System.out.print(s+"\n");
	}
	public static int[] toarray(List<Integer> list) {
		int x[] = new int[list.size()];
		for(int i = 0;i<list.size();i++) {
			x[i]=list.get(i);
		}
		return x;
	}
	public static void logv(float[] x,String r) {
		System.out.print(r + ":  ");
		for(int i=0;i<x.length;i++) {
			System.out.print(String.valueOf(x[i]) + "  ");
		}
		log("Over");
	}
	public static void getErrors(GL3 gl,String additional) {
		int x;
		while(( x = gl.glGetError()) != GL3.GL_NO_ERROR) {
			System.out.println(x + "  " + additional);
		}
		System.out.println("Stack Cleared: " + x + "   " + additional + " Is Finished \n");
	}
	public static void LOG(Object s) {
		System.out.print(String.valueOf(s)+"\n");
	}
	public static FloatBuffer GetBuffer4x4(Mat4 matrix) {
		FloatBuffer fb = FloatBuffer.allocate(16);
		fb.put(matrix.v00());fb.put(matrix.v01());fb.put(matrix.v02());fb.put(matrix.v03());
		fb.put(matrix.v10());fb.put(matrix.v11());fb.put(matrix.v12());fb.put(matrix.v13());
		fb.put(matrix.v20());fb.put(matrix.v21());fb.put(matrix.v22());fb.put(matrix.v23());
		fb.put(matrix.v30());fb.put(matrix.v31());fb.put(matrix.v32());fb.put(matrix.v33());
		fb.flip();
		return fb;
	}
	public static Vec3 f3min(Vec3 A, Vec3 B) {
		Vec3 R = new Vec3(0,0,0);
		R.setX(Math.min(A.getX(), B.getX()));
		R.setY(Math.min(A.getY(), B.getY()));
		R.setZ(Math.min(A.getZ(), B.getZ()));
		return R;
	}
	public static Vec3 f3max(Vec3 A, Vec3 B) {
		Vec3 R = new Vec3(0,0,0);
		R.setX(Math.max(A.getX(), B.getX()));
		R.setY(Math.max(A.getY(), B.getY()));
		R.setZ(Math.max(A.getZ(), B.getZ()));
		return R;
	}
	
	//找最近的整数
	//这个函数应该仅被用于当明确知道所处理的float应该是整数的时候
	//比如v=range/unitsize
	//实际ID应该取1-1~count-1
	public static int Clean(float v) {
		double C = Math.abs(Math.ceil(v)-v);
		double F = Math.abs(Math.floor(v)-v);
		if(C<0.001) {
			return (int)Math.ceil(v);
		}else {
			return (int)Math.floor(v);
		}
	}
	
	//将空间中任意一点转化成id
	//最大值需要-1  min(max,end-1)
	//其他方面不用改动
	//有问题，Count如果是49.99998，但实际上是50，那这个格子就是归到49了
	//对，就是这样设计的
	public static int[] Sort(Vec3 p, Vec3 min, float size, int[] maxcount) {
		Vec3 Spans = p.minus(min);
		Vec3 Count = Spans.div(size);
		int array[] = {Math.min((int)Math.floor(Count.getX()),maxcount[0]),
								Math.min((int)Math.floor(Count.getY()),maxcount[1]),
								Math.min((int)Math.floor(Count.getZ()),maxcount[2])};
		return array;
	}
	
	//用若干个尺寸一致的格子包裹住该体积
	//容差小于E的情况下不为该体积建立格子
	//50.000021-E -> ceil to 50
	//这个函数的意思是输入任意点，作到minaabb的长度，这个长度可以分成多少个size
	//当然，分是没法分成整数的。所以那就叫A.B*size=Length
	//floor一下找到它的商
	//ceil等于商+1
	//
	public static int Envelop(float Length, float size) {
		float E = 0.0001f;
		return (int)Math.ceil(Length/size-E);
	}
	
}
