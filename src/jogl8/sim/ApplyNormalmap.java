package jogl8.sim;

import java.awt.image.BufferedImage;
import java.awt.image.WritableRaster;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import java.util.HashMap;
import java.util.List;

import javax.imageio.ImageIO;

import com.jogamp.opengl.GL3;

import assimp.AiMesh;
import glm_.vec2.Vec2;
import glm_.vec3.Vec3;
import jogl8.utils;
import jogl8.sim.geometryprocessor.VecHash;

public class ApplyNormalmap {
	
	public String Gsdfr = "C:/Users/User/Desktop/NewFolder/粗糙低模/粗糙低模光滑高模_八猴烘焙_无padding.jpg";//fTBN解码
	
	public String imagePath = "C:/Users/User/Desktop/NewFolder/光滑低模/高模平滑性被烘焙_soft.jpg";//八猴在解码法线贴图上有一些独特的方式
	//这张图里面有一些不连续区域，产生渲染瑕疵，但八猴没有这个问题，它怎么编码就怎么解码，所以对冲了
	//可能是切线加权平均的算法不同
	
	public String f54y2 = "C:/Users/User/Desktop/NewFolder/渲染输出/可变编码/光追方向_光滑_sTBN_光滑高模.png";//按照我们自己的编码方式来解码

	public String g5 = "C:/Users/User/Desktop/NewFolder/渲染输出/可变编码/光追方向_光滑_fTBN.png";
	
	public String sd13 = "C:/Users/User/Desktop/NewFolder/数据对比/sta.png";
	
	public String sama = "C:/Users/User/Desktop/NewFolder/渲染输出/修复的/up.png";
	
	public String sd3 = "C:/Users/User/Desktop/NewFolder/UP向量/sTBN但是全都没有normalize.png";
	
	
	public String ff = "C:/Users/User/Desktop/NewFolder/最终解决.png";
	
	public String debug = "C:/Users/User/Desktop/NewFolder/测试低模_oTBN.png";
	
	public String fs = "C:/Users/User/Desktop/NewFolder/测试低模_oTBN_未归一.png";

	public String ohan = "C:/Users/User/Desktop/NewFolder/UV测试/otbn切.png";

	public void run(GL3 gl) throws IOException {
		BufferedImage myPicture = ImageIO.read(new File("out.png"));
		ByteBuffer imagebuffer = ByteBuffer.allocate(myPicture.getWidth()* myPicture.getHeight()*3);
		
		/*
		前后尺寸不一样...writer有压缩
		
		ByteArrayOutputStream tobytes = new ByteArrayOutputStream();

		boolean state=false;
	    try {
	    	state = ImageIO.write(myPicture, "png", tobytes);
	    	utils.LOG("size of output stream" + tobytes.size());
	    } catch (IOException ex) {
	    }
	    */
		
		//先W后H会旋转90度
		//180的情况是W从0~max还是max~0的方向决定的
		for(int W = 0; W<myPicture.getWidth(); W++) {
			for(int H=0; H<myPicture.getHeight(); H++) {
				int ARGB = myPicture.getRGB(H, myPicture.getWidth()-1-W);//从WH改成HW
				imagebuffer.put((byte) ((ARGB >> 16)&0xFF));
				imagebuffer.put((byte) ((ARGB >> 8)&0xFF));
				imagebuffer.put((byte) ((ARGB >> 0)&0xFF));
				//utils.LOG(((ARGB >> 16)&0xFF )+"  "+ ((ARGB >> 8)&0xFF) + "  "+ ((ARGB >> 0)&0xFF));
				/*128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
128  127  255
==(0.5,0.5,1)
*/
//				imagebuffer.put((byte) (ARGB & 0xFFFFFF00));
			}
		}
		
		imagebuffer.clear();
		utils.LOG(imagebuffer);
		
		IntBuffer textureloc = IntBuffer.allocate(1);
		gl.glGenTextures(1, textureloc);
		gl.glActiveTexture(GL3.GL_TEXTURE0);
		gl.glBindTexture(GL3.GL_TEXTURE_2D, textureloc.get());
		gl.glTexImage2D(GL3.GL_TEXTURE_2D, 0, GL3.GL_RGB, myPicture.getWidth(), myPicture.getHeight(), 0, GL3.GL_RGB, GL3.GL_UNSIGNED_BYTE, imagebuffer);
		
	//卧槽，好他妈的怪，不加这些就是全黑
		//好吧，理性地分析，纹理图中的纹素和屏幕上的像素之间几乎从来不存在一一对应的关系 因此，纹理图像在进行纹理贴图的时候总是被拉伸或收缩。 glsl中有一个函数texture()，可以通过从拉伸或收缩的纹理图中计算颜色片段来对贴图进行调节
		gl.glTexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_S, gl.GL_REPEAT);   
		gl.glTexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_T, gl.GL_REPEAT);
		gl.glTexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_LINEAR);
		gl.glTexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_LINEAR);
		utils.getErrors(gl, imagePath);
	}
	
	
	VecHash NormalRemap = new VecHash();
	VecHash TangentRemap = new VecHash();
	
	//Vec3[] FlatN;
	//Vec3[] FlatT;
	
	
	public void GenerateTangentData(GL3 gl, AiMesh lowmeshscene) {
		int invocounts = lowmeshscene.getNumFaces();
		int Vcounts = lowmeshscene.getNumVertices();

		
	//	FlatN = new Vec3[Vcounts];
	//	FlatT = new Vec3[Vcounts];

		for(int i=0;i<invocounts;i++) {
			List<Integer> triangle = lowmeshscene.getFaces().get(i);
			Vec3 A = lowmeshscene.getVertices().get(triangle.get(0));
			Vec3 B = lowmeshscene.getVertices().get(triangle.get(1));
			Vec3 C = lowmeshscene.getVertices().get(triangle.get(2));
		
			float[] Auv = lowmeshscene.getTextureCoords().get(0).get(triangle.get(0));
			float[] Buv = lowmeshscene.getTextureCoords().get(0).get(triangle.get(1));
			float[] Cuv = lowmeshscene.getTextureCoords().get(0).get(triangle.get(2));
			Vec3[] list = new Vec3[]{A,B,C};
			LoopPerTriangle(i,list, Auv, Buv, Cuv , triangle);
		}
		
		FloatBuffer ProPullingNormal = FloatBuffer.allocate(Vcounts*3);
		FloatBuffer ProPullingTangent = FloatBuffer.allocate(Vcounts*3);
		
	//	FloatBuffer ProPullingNormalFlat = FloatBuffer.allocate(Vcounts*3);
	//	FloatBuffer ProPullingTangentFlat = FloatBuffer.allocate(Vcounts*3);
		
		for(int i=0;i<Vcounts;i++) {
			Vec3 this_v = lowmeshscene.getVertices().get(i);
			Vec3 N = NormalRemap.get(this_v).normalize();
			Vec3 T = TangentRemap.get(this_v).normalize();
			ProPullingNormal.put(N.getArray());
			ProPullingTangent.put(T.getArray());
	//		ProPullingNormalFlat.put(FlatN[i].getArray());
	//		ProPullingTangentFlat.put(FlatT[i].getArray());
		}
		ProPullingNormal.clear();
		ProPullingTangent.clear();
		
		//ProPullingNormalFlat.clear();
	//	ProPullingTangentFlat.clear();
		
		IntBuffer ssbo = IntBuffer.allocate(4);
		gl.glGenBuffers(4, ssbo);
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, ssbo.get(0));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, ProPullingNormal.capacity()*Float.BYTES, ProPullingNormal, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 4, ssbo.get(0));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, ssbo.get(1));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, ProPullingTangent.capacity()*Float.BYTES, ProPullingTangent, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 5, ssbo.get(1));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		/*
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, ssbo.get(2));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, ProPullingNormalFlat.capacity()*Float.BYTES, ProPullingNormalFlat, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 6, ssbo.get(2));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, ssbo.get(3));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, ProPullingTangentFlat.capacity()*Float.BYTES, ProPullingTangentFlat, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 7, ssbo.get(3));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);*/
	}
	
	public double calArea(Vec3 base, Vec3 p1, Vec3 p2) {
		double cosC = (p1.minus(base).normalize()).dot(p2.minus(base).normalize());
		double sinC = Math.sqrt(1.0 - cosC * cosC);
		return p1.length() * p2.length() * sinC;
	}
	
	
	public void LoopPerTriangle(int primitiveIndex, Vec3[] in, float[] Auv, float[] Buv, float[] Cuv, List<Integer> tri) {
		Vec3 V02 = in[0].minus(in[2]);
		Vec3 V12 = in[1].minus(in[2]);
		Vec3 flatnormal = V02.cross(V12).normalize();
		
		Vec2 U02 = new Vec2(Auv[0]-Cuv[0], Auv[1]-Cuv[1]); 
		Vec2 U12 = new Vec2(Buv[0]-Cuv[0], Buv[1]-Cuv[1]); 
	    float det = (U02.getX() * U12.getY() - U02.getY() * U12.getX());

	//    FlatN[tri.get(0)] = flatnormal;
	//    FlatN[tri.get(1)] = flatnormal;
	//    FlatN[tri.get(2)] = flatnormal;

		Vec3 facetanget = (V02.times(U12.getY()).minus( V12.times(U02.getY()) )).div(det);
		facetanget.normalize(facetanget);
	  //  FlatT[tri.get(0)] = facetanget;
	  //  FlatT[tri.get(1)] = facetanget;
	//    FlatT[tri.get(2)] = facetanget;

		{
			Vec3 position = in[0];
			Vec3 basevec = NormalRemap.getdv(position);
			Vec3 basevecT = TangentRemap.getdv(position);
			double t = geometryprocessor.calradian(in[0], in[1], in[2]);
			double s = 1;
			Vec3 weightflat = flatnormal.times(t*s);
			Vec3 weightTang = facetanget.times(t*s);
			NormalRemap.put(position, basevec.plus(weightflat));
			TangentRemap.put(position, basevecT.plus(weightTang));
		}
		
		{
			Vec3 position = in[1];
			Vec3 basevec = NormalRemap.getdv(position);
			Vec3 basevecT = TangentRemap.getdv(position);
			double t = geometryprocessor.calradian(in[1], in[2], in[0]);
			double s =1;
			Vec3 weightflat = flatnormal.times(t*s);
			Vec3 weightTang = facetanget.times(t*s);
			NormalRemap.put(position, basevec.plus(weightflat));
			TangentRemap.put(position, basevecT.plus(weightTang));
		}
		
		{
			Vec3 position = in[2];
			Vec3 basevec = NormalRemap.getdv(position);
			Vec3 basevecT = TangentRemap.getdv(position);
			double t = geometryprocessor.calradian(in[2], in[1], in[0]);
			double s = 1;
			Vec3 weightflat = flatnormal.times(t*s);
			Vec3 weightTang = facetanget.times(t*s);
			NormalRemap.put(position, basevec.plus(weightflat));
			TangentRemap.put(position, basevecT.plus(weightTang));
		}
		
		
	}
	
	
}
