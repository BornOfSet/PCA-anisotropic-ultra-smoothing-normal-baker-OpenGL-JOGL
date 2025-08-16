package jogl8.sim;

import static assimp.AiPostProcessStep.Triangulate;

import java.awt.image.BufferedImage;
import java.awt.image.WritableRaster;
import java.io.File;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Set;

import javax.imageio.ImageIO;

import com.jogamp.opengl.GL2;
import com.jogamp.opengl.GL3;
import com.jogamp.opengl.GLAutoDrawable;
import com.jogamp.opengl.GLEventListener;
import com.jogamp.opengl.util.FPSAnimator;

import assimp.AiMesh;
import assimp.AiScene;
import assimp.Importer;
import glm_.glm;
import glm_.mat4x4.Mat4;
import glm_.vec3.Vec3;
import glm_.vec4.Vec4;
import jogl8.control;
import jogl8.utils;

public class gridtrace implements GLEventListener{

	
	FPSAnimator Anim;
	public gridtrace(FPSAnimator A) {
		this.Anim = A;
	}
	
	public gridtrace() {
	}
	
	//运行时可以改变
	
	public String mpath = "C:/Users/User/Desktop/newfolder/裤子 高模.obj";
	//public String swirl = "C:/Users/User/Desktop/NewFolder/AVERAGE反.obj";
	public String Lpath =  "C:/Users/User/Desktop/newfolder/裤子 低模.obj";
	public boolean ren_mesh_points =false;
	public boolean ren_grid = false;
	public boolean ren_mesh_faces = true;
	public boolean ren_capsule = false;
	public boolean ren_smooth = true; 
	public float mmm = 0.002f;//mouse movement mult
	public buildgrid builder;
	public int linewidth = 1;
	public int pointsize = 4;
	
	//左边视图的意义:	
	//1.渲染高模法线方向
	//2.渲染高模平滑法线兰伯特
	
	//3.渲染低模法线方向
	//4.渲染低模法线拉力分布
	
	
	//true的话，显示的是平滑的高模
	//false就是低模带法线
	public boolean previewinghigh = false;
	public boolean shownormal = false;
	
	public boolean UVMODE =false ;
	public boolean ren_frame = false	;
	
	public boolean pauserender = true;
	public boolean OUTPUT = true;
	public boolean flsh = false;
	
	//private
	
	private boolean ONCE;
	
	private int mesh;
	private int grid;
	private int backwards;
	private int pp;
	private int sp;
	private int lp;
	private int tp;
	private int rvc;//realvertexcount
	private int tpc;//totalpointscount
	private float yaw;
	private float pitch;
	private int framebuffer;
	private int[] Dimension;
	private int rmi;
	private int normalcount;
	private int sn;
	private int smoothrenhigh;
	private int smoothrenlow;
	private int snlow;
	private int uvshader;
	private int uvframe;
	private int csgrid;
	private int testrender;
	private int raytraceTangent;
	private int displaytangent;
	private int flash;
	
	private int Advanceraytrace;
	private int AdvanceraytraceMIKK;
	
	private int Framebuffer_name;//用于切换framebuffer
	
	private int LVC;//low vertices count low.FaceNum*3
	
	private int testcount;
	private int lownormalcount;
	
	private boolean fuck;

	//for convenience
	private void updateuniforms(GL3 gl , int program, FloatBuffer RB, FloatBuffer TB, FloatBuffer SB) {
		int uv = gl.glGetUniformLocation(program, "view");
		int um = gl.glGetUniformLocation(program, "move");
		int us = gl.glGetUniformLocation(program, "scale");
		gl.glUseProgram(program);
		gl.glUniformMatrix4fv(uv, 1, false, RB);		
		gl.glUniformMatrix4fv(um, 1, false, TB);			
		gl.glUniformMatrix4fv(us, 1, false, SB);	
		gl.glUseProgram(0);
	}
	
	//https://github.com/BornOfSet/JOGL-An-Example-On-How-To-Read-And-Write-SSBO-In-Vertex-Shader-and-Geometry-Shader-Regular-Pipeline
	
	@Override
	public void display(GLAutoDrawable arg0) {
		if(!UVMODE) {
			renderlogic1(arg0);
		}else
		{
			renderlogic2(arg0);
		}
		//utils.LOG(Math.ceil(50.2f - 0.2f));//50.0
		//utils.LOG(new Vec3(-99999,2,0.0058).hashCode());
		//utils.LOG(new Vec3(1234,3432,5450.0058).toString().hashCode());
		//utils.LOG(31*31*Float.hashCode(1234.0f)+31*Float.hashCode(3432.0f)+Float.hashCode(5450.0058f));
		//utils.LOG(Float.hashCode(1234.0f)*31*31);
		//utils.LOG(Float.hashCode(3432.0f));
		//utils.LOG(Float.hashCode(5450.0058f));
	}
	
	//渲染选项：
	//兰伯特景观图
	//重叠网格
	//法线景观图
	//
	private void renderlogic2(GLAutoDrawable arg0) {
		GL3 gl = arg0.getGL().getGL3();
		gl.glClear(GL3.GL_COLOR_BUFFER_BIT);			
		gl.glClear(GL3.GL_DEPTH_BUFFER_BIT);
		gl.glBindVertexArray(0);
		gl.glUseProgram(0);

		float xMoveDelta = (control.RealtimeLocation.x - control.StartLocation.x)*mmm;
		float yMoveDelta = (control.RealtimeLocation.y - control.StartLocation.y)*mmm;
		yaw += xMoveDelta;
		pitch += -yMoveDelta;

		Vec3 CameraVector = new Vec3(Math.sin(yaw),pitch,Math.cos(yaw)).normalize();
		

		gl.glViewport(Dimension[0], Dimension[1], Dimension[2], Dimension[3]);
		GL2 degenerate = arg0.getGL().getGL2();
		//degenerate.glColor3f(1, 0, 0);
		//degenerate.glBegin(GL2.GL_LINES);
		//degenerate.glVertex2f(-0.52f, 1.0f);
		//degenerate.glVertex2f(-0.52f, -1.0f);
		//degenerate.glVertex2f(0.52f, 1.0f);
		//degenerate.glVertex2f(0.52f, -1.0f);
		//degenerate.glEnd();
		
		gl.glViewport(Dimension[2]/4, Dimension[1], Dimension[2]/2, Dimension[3]);
		
		
		
		int uvshader = this.uvshader;

		
		if(pauserender&& fuck) {
			Anim.stop();
			uvshader = this.Advanceraytrace;
		}
		

		if(ren_frame) {
			gl.glBindVertexArray(backwards);
			gl.glUseProgram(uvshader);
			//!!!!
			gl.glDrawElements(GL3.GL_TRIANGLES, LVC, GL3.GL_UNSIGNED_INT, 0);		
			gl.glUseProgram(uvframe);
			gl.glDrawElements(GL3.GL_TRIANGLES, LVC, GL3.GL_UNSIGNED_INT, 0);		
		}else {
			gl.glBindVertexArray(backwards);
			gl.glUseProgram(uvshader);
			

			
			//这部分是测试用的，如果光线追踪的结果能够跟随灯光移动，就说明做对了
			int rotateLight = gl.glGetUniformLocation(uvshader, "LIGHTDIRECTION");
			float[] temp =  CameraVector.getArray();
			gl.glUniform3fv(rotateLight, 1, temp, 0);
			gl.glDrawElements(GL3.GL_TRIANGLES, LVC, GL3.GL_UNSIGNED_INT, 0);		
		}
		
		
		if(pauserender && fuck) {
			if(OUTPUT) {
				gl.glViewport(0, 0, 2048, 2048);
				gl.glBindFramebuffer(GL3.GL_FRAMEBUFFER,this.Framebuffer_name);	
				
				gl.glBindVertexArray(backwards);
				gl.glUseProgram(uvshader);
				gl.glDrawElements(GL3.GL_TRIANGLES, LVC, GL3.GL_UNSIGNED_INT, 0);		
				
				
				gl.glReadBuffer(GL3.GL_COLOR_ATTACHMENT0);
				//8-8-8
				ByteBuffer data = ByteBuffer.allocate(2048*2048*3);//If you're going to use RGBA then it's 4 because you have each channel represented by one Byte , a pixel represented by 3/4 channels
				gl.glReadPixels(0, 0, 2048, 2048, GL3.GL_RGB, GL3.GL_UNSIGNED_BYTE, data);
				BufferedImage ImageBuffer = new BufferedImage(2048, 2048, BufferedImage.TYPE_INT_RGB);
				WritableRaster ImageRaster = ImageBuffer.getRaster();
				for(int height = 2048-1;height >= 0; height--) {//Otherwise it is upside down . You should not have height = TargetFrameSize . There's no such a index in Raster
					for(int width = 0;width < 2048; width++) {
						byte r = data.get();
						byte g = data.get();
						byte b = data.get();
						//Byte自动提升(promotion)为int，按位与移除(mask off)补码和符号
						int[] pixel = {r&0xFF,g&0xFF,b&0xFF};
						if(pixel[0]==0 && pixel[1]==0 && pixel[2]==0) {
							pixel[0]=128;//从125修正到128，数字写错了
							pixel[1]=128;
							pixel[2]=255;//初始紫色
						}
						ImageRaster.setPixel(width, height, pixel);
					}
				}
				try {
					ImageIO.write(ImageBuffer,"png",new File("out.png"));
				}catch(IOException e) {
					e.printStackTrace(); //BMP has problem with TYPE_INT_RGB
				}
				System.out.print("  ||  Exporting  ||  ");
			}
		}
		//切换到有内容的framebuffer继续预览效果，不然会黑屏
		gl.glBindFramebuffer(GL3.GL_FRAMEBUFFER,0);	
		
		
		if(pauserender && !fuck) {
			fuck = true;
		}
		
		
		//gl.glDispatchCompute(num, 0, 0);
	}
	
	private void renderlogic1(GLAutoDrawable arg0){
		//DrawModuels are fixed . Available are only enum-options
		GL3 gl = arg0.getGL().getGL3();
		gl.glClear(GL3.GL_COLOR_BUFFER_BIT);			
		gl.glClear(GL3.GL_DEPTH_BUFFER_BIT);
		gl.glBindVertexArray(0);
		gl.glUseProgram(0);

		gl.glViewport(Dimension[0], Dimension[1], Dimension[2], Dimension[3]);
		GL2 degenerate = arg0.getGL().getGL2();//如果任何一个具有core标题的着色器被使用，那么旧固定管线就无法继续使用
		//degenerate.glColor3f(1, 0, 0);
		//degenerate.glBegin(GL2.GL_LINES);
		//degenerate.glVertex2f(0.0f, 1.0f);
		//degenerate.glVertex2f(0.0f, -1.0f);
		//degenerate.glEnd();
		//xoffset , yoffset , width , adaptiveheight
		
		
		gl.glViewport(Dimension[2]/2, Dimension[1], Dimension[2]/2, Dimension[3]);
		
		//transformation
		float xMoveDelta = (control.RealtimeLocation.x - control.StartLocation.x)*mmm;
		float yMoveDelta = (control.RealtimeLocation.y - control.StartLocation.y)*mmm;
		yaw += xMoveDelta;
		pitch += -yMoveDelta;
		pitch = Math.min(pitch,   6);
		pitch = Math.max(pitch, -6);
		//默认状态下是单位矩阵
		Vec3 UP = new Vec3(0,1,0);
		Vec3 CameraVector = new Vec3(Math.sin(yaw),pitch,Math.cos(yaw)).normalize();
		Vec3 CameraRight = UP.cross(CameraVector).normalize();
		Vec3 CameraUp = CameraVector.cross(CameraRight).normalize();
		Vec3 CameraMovement = control.Translation;
		Mat4 M = new Mat4(
				new Vec4(CameraRight,  0),
				new Vec4(CameraUp,  	  0),
				new Vec4(CameraVector, 0),
				new Vec4(0,0,0,1)
		);
		float l = control.HardScale;
		Mat4 T = new Mat4(
				new Vec4(l,0,0,0),
				new Vec4(0,l,0,0),
				new Vec4(0,0,l,0),
				new Vec4(CameraMovement,1)
		);
		M = M.transpose();
		Vec3 scale = control.Resize;
		Mat4 S = new Mat4(1);
		S = glm.INSTANCE.scale(S,  scale);
		FloatBuffer RBuffer = utils.GetBuffer4x4(M);
		FloatBuffer SBuffer = utils.GetBuffer4x4(S);
		FloatBuffer TBuffer = utils.GetBuffer4x4(T);
		updateuniforms(gl, pp, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, sp, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, lp, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, tp, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, sn, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, snlow, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, smoothrenlow, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, smoothrenhigh, RBuffer, TBuffer, SBuffer);
		updateuniforms(gl, displaytangent, RBuffer, TBuffer, SBuffer);

		updateuniforms(gl, flash, RBuffer, TBuffer, SBuffer);

		int rand = gl.glGetUniformLocation(flash, "rand");
		int R = (int)Math.floor(Math.random() * testcount/3);
		//utils.LOG(R);
		
		if(flsh) {
			//gl.glBindVertexArray(mesh);//为什么要绑定这个？drawelement....哦，叫drawarray
	
			
			gl.glBindVertexArray(rmi);
			gl.glUseProgram(sp);
			gl.glDrawElements(GL3.GL_TRIANGLES, rvc, GL3.GL_UNSIGNED_INT, 0);
			
			gl.glBindVertexArray(0);

			gl.glUseProgram(flash);
			gl.glUniform1i(rand, R);
			gl.glPointSize(16);
			gl.glDrawArrays(GL3.GL_POINTS,  0, testcount);	
			return;
		}

//rvc这里没问题，rvc指的是高模面数*3。我怀疑用高模dc次数调用低模绘制，会导致利用未初始化的空间
		//render in progress
		gl.glPointSize(pointsize);

		if(ren_mesh_points) {
			gl.glBindVertexArray(mesh);
			gl.glUseProgram(pp);
			gl.glDrawElements(GL3.GL_POINTS, rvc , GL3.GL_UNSIGNED_INT, 0);
		}
		if(ren_grid) {
			gl.glLineWidth(linewidth);
			gl.glBindVertexArray(grid);
			gl.glUseProgram(lp);
			//grid geometry
			int ps = gl.glGetUniformLocation(lp, "pointsize");
			gl.glUniform1f(ps, this.builder.unitsize/2);
			gl.glDrawArrays(GL3.GL_POINTS, 0, tpc);
		}
		if(ren_mesh_faces) {
			gl.glLineWidth(linewidth);
			gl.glBindVertexArray(rmi);
			gl.glUseProgram(sp);
			gl.glDrawElements(GL3.GL_TRIANGLES, rvc, GL3.GL_UNSIGNED_INT, 0);
		}
		if(ren_capsule) {
			gl.glLineWidth(linewidth);
			gl.glBindVertexArray(mesh);
			gl.glUseProgram(tp);
			gl.glDrawElements(GL3.GL_TRIANGLES, rvc, GL3.GL_UNSIGNED_INT, 0);		
		}


		
		//附属屏幕
		gl.glViewport(Dimension[0], Dimension[1], Dimension[2]/2, Dimension[3]);
		//显示模型
		if(this.previewinghigh) {
			gl.glBindVertexArray(rmi);
			gl.glUseProgram(smoothrenhigh);
			gl.glDrawElements(GL3.GL_TRIANGLES, rvc, GL3.GL_UNSIGNED_INT, 0);
			//显示法线与否
			if(this.shownormal) {
				gl.glUseProgram(sn);//依赖ssbo
				gl.glDrawArrays(GL3.GL_POINTS, 0, normalcount);
			}
		}else {
			gl.glBindVertexArray(backwards);
			gl.glUseProgram(smoothrenlow);
			//gl.glUniform1i(gl.glGetUniformLocation(smoothrenlow, "text0"), 0);
			//!!!!
			gl.glDrawElements(GL3.GL_TRIANGLES, LVC, GL3.GL_UNSIGNED_INT, 0);

			//显示法线与否
			if(this.shownormal) {
				gl.glUseProgram(displaytangent);
				gl.glDrawArrays(GL3.GL_POINTS, 0, lownormalcount);
				
			}
		}

		
	}

	@Override
	public void dispose(GLAutoDrawable arg0) {
		
	}

	@Override
	public void init(GLAutoDrawable arg0) {
		GL3 gl = arg0.getGL().getGL3();
		
		
       // gl.glEnable(gl.GL_CULL_FACE);
      //  gl.glCullFace(gl.GL_FRONT);
		//gl.glPolygonMode(GL3.GL_FRONT_AND_BACK , gl.GL_LINE);
		
		
		//该framebuffer应该只用于出图模式，而非预览模式
		//在导出前记得初始化颜色为0 0 0.5
		IntBuffer rtRenderTarget = IntBuffer.allocate(1);
		IntBuffer rtTexture = IntBuffer.allocate(1);
		gl.glGenFramebuffers(1, rtRenderTarget);
		
		gl.glGenTextures(1, rtTexture);
		gl.glBindTexture(GL3.GL_TEXTURE_2D, rtTexture.get(0));
		gl.glTexImage2D(GL3.GL_TEXTURE_2D, 0, GL3.GL_RGB, 2048, 2048, 0, GL3.GL_RGB, GL3.GL_UNSIGNED_BYTE, null);
		gl.glTexParameteri(GL3.GL_TEXTURE_2D, GL3.GL_TEXTURE_MAG_FILTER, GL3.GL_NEAREST);
		gl.glTexParameteri(GL3.GL_TEXTURE_2D, GL3.GL_TEXTURE_MIN_FILTER, GL3.GL_NEAREST);
		
		gl.glBindFramebuffer(GL3.GL_FRAMEBUFFER, rtRenderTarget.get(0));	
		gl.glFramebufferTexture(GL3.GL_FRAMEBUFFER, GL3.GL_COLOR_ATTACHMENT0, rtTexture.get(0), 0);
		utils.getErrors(gl,"glCheckFramebufferStatus: " + gl.glCheckFramebufferStatus(GL3.GL_FRAMEBUFFER));
		
		IntBuffer Control = IntBuffer.allocate(1);
		Control.put(GL3.GL_COLOR_ATTACHMENT0);
		Control.clear();
		gl.glDrawBuffers(1, Control);

		
		gl.glBindFramebuffer(GL3.GL_FRAMEBUFFER, 0);	
		gl.glBindTexture(GL3.GL_TEXTURE_2D, 0);

		Framebuffer_name = rtRenderTarget.get(0);
		
		
		
		
		
		
		//gl.glDepthRange (1,0);
        gl.glEnable(GL3.GL_DEPTH_TEST);
     //   gl.glClearDepth(0);
	//	gl.glClear(GL3.GL_DEPTH_BUFFER_BIT);
		
        gl.glDepthFunc(GL3.GL_LESS);
		AiScene rootscene = new Importer().readFile(mpath , Triangulate.i);//PATH
		//参见《关于元素绘制....的规范》
		//多余的顶点缺少面来承载，如果以默认所有面都是三角面的标准创建顶点缓冲，终会导致溢出
		Integer x = rootscene.getNumMeshes();
		utils.log("LOG_MeshCount:"+x.toString());
		AiMesh workmesh = rootscene.getMeshes().get(0);
		int nvx = workmesh.getNumVertices();
		int nE = workmesh.getNumFaces();
		IntBuffer evx = IntBuffer.allocate(nE*3);
		FloatBuffer vtx = FloatBuffer.allocate(nvx*6);
		for(int i=0;i<nvx;i++) {
			Vec3 position = workmesh.getVertices().get(i);
			Vec3 normal = workmesh.getNormals().get(i);
			vtx.put(position.getArray());
			vtx.put(normal.getArray());
		};
		vtx.clear();//do not forget
		for(int i=0;i<nE;i++) {
			List<List<Integer>> faces = workmesh.getFaces();
			evx.put(utils.toarray(faces.get(i)));
		}
		evx.clear();
		
		//preparations
		IntBuffer loc = IntBuffer.allocate(2);
		IntBuffer aloc = IntBuffer.allocate(2);
		IntBuffer eloc = IntBuffer.allocate(1);
		gl.glGenBuffers(2, loc);
		gl.glGenVertexArrays(2, aloc);
		gl.glGenBuffers(1, eloc);
		
		//mesh
		this.mesh = aloc.get();
		gl.glBindVertexArray(this.mesh);
		gl.glBindBuffer(GL3.GL_ARRAY_BUFFER, loc.get());//vao stores binding . so bind vao before all bindings
		gl.glBufferData(GL3.GL_ARRAY_BUFFER, vtx.capacity()*Float.BYTES, vtx, GL3.GL_STATIC_DRAW);
		gl.glEnableVertexAttribArray(0);//position/ vertexattrib reads from arraybuffer , so bind arraybuffer before creating attribute for it
		gl.glVertexAttribPointer(0, 3, GL3.GL_FLOAT, true, 6*Float.BYTES, 0);
		gl.glEnableVertexAttribArray(1);//normal
		gl.glVertexAttribPointer(1, 3, GL3.GL_FLOAT, true, 6*Float.BYTES, 3*Float.BYTES);
		gl.glBindBuffer(GL3.GL_ELEMENT_ARRAY_BUFFER, eloc.get());
		gl.glBufferData(GL3.GL_ELEMENT_ARRAY_BUFFER, evx.capacity()*Integer.BYTES, evx, GL3.GL_STATIC_DRAW);
		this.rvc = evx.capacity();//real vertices count
		//关于元素绘制时VS 总调用次数的规范：等于图元数量*3，代表每个图元是含三个点的三角形
		
		//grid
		buildgrid bg = new buildgrid(workmesh);
		Vec3[] points = bg.func();
		this.builder = bg;
		FloatBuffer geometry = FloatBuffer.allocate(points.length*3);
		for(int i=0;i<points.length;i++) {
			geometry.put(points[i].getArray());
		}
		geometry.clear();
		this.grid = aloc.get();
		gl.glBindVertexArray(this.grid);
		gl.glBindBuffer(GL3.GL_ARRAY_BUFFER, loc.get());
		gl.glBufferData(GL3.GL_ARRAY_BUFFER, geometry.capacity()*Float.BYTES, geometry, GL3.GL_STATIC_DRAW);
		gl.glEnableVertexAttribArray(2);
		gl.glVertexAttribPointer(2, 3, GL3.GL_FLOAT, true, 3*Float.BYTES, 0);
		gl.glBindVertexArray(0);
		this.tpc = points.length;
		
		//shader
		pp = gl.glCreateProgram();//draw module : DensedPointsCluster  CODE: dm.dpc
		sp = gl.glCreateProgram();//draw module : StandardLitSurface    CODE: dm.sls
		lp = gl.glCreateProgram();//draw module : CleanWireFrame         CODE: dm.cwf
		tp = gl.glCreateProgram();//triangulate wire frame
		//可能允许用户自己配置着色器
		universalshaderloader.SetPipeline(gl, pp, "grid_vertexshader_mesh.vs",  "grid_pointprimitives.gs",          "grid_giveflattencolor_red.fs");
		universalshaderloader.SetPipeline(gl, lp, "grid_vertexshader_grid.vs",    "grid_lineprimitives.gs",             "grid_giveflattencolor_white.fs");
		universalshaderloader.SetPipeline(gl, tp, "grid_vertexshader_mesh.vs",  "grid_triangulateprimitives.gs", "grid_giveflattencolor_red.fs");

		utils.getErrors(gl, "Initiation-I");
				
		//预览部分结束
		//接下来研究光追
		
		//Load Mesh
		utils.LOG("加载低模");
		AiScene lowmeshscene  = new Importer().readFile(this.Lpath , Triangulate.i);
		AiMesh Mesh = lowmeshscene.getMeshes().get(0);
		utils.log(lowmeshscene.getMeshes().size() + " 个网格存在于 " + this.Lpath);
		
		List<float[]> UVs = Mesh.getTextureCoords().get(0);
		utils.log("Mesh.hasTextureCoords(0) = " + Mesh.hasTextureCoords(0));
		
		int VertexNum = Mesh.getNumVertices();
		int FaceNum = Mesh.getNumFaces();
		List<Vec3> Vertices = Mesh.getVertices();
		List<Vec3> Normals = Mesh.getNormals();
		
		List<List<Integer>> Faces = Mesh.getFaces();

		//顶点数据
		FloatBuffer output = FloatBuffer.allocate(VertexNum*8); 
		lownormalcount = VertexNum;
		for(int i = 0;i<VertexNum;i++) {
			Vec3 v1 = Vertices.get(i);
			Vec3 v2 = Normals.get(i);
			output.put(v1.toFloatArray());
			output.put(v2.toFloatArray());
			
			float[] v3 = UVs.get(i);
			output.put(v3[0]);
			output.put(v3[1]);
		}
		output.flip();
		
		//索引（图元顶点!=实际顶点，实际顶点可以被压缩到少于图元所要求的顶点的数量）
		IntBuffer indices = IntBuffer.allocate(FaceNum*3); 
		for(int i = 0;i<FaceNum;i++) {
			List<Integer> face = Faces.get(i);
			for(int j = 0;j<3;j++) {
				indices.put(face.get(j));
			}
		}
		indices.flip();
		
		this.LVC = FaceNum*3;

		
		//绑定
		IntBuffer backwards_VAO = IntBuffer.allocate(1);
		gl.glGenVertexArrays(1, backwards_VAO);
		this.backwards = backwards_VAO.get();
		gl.glBindVertexArray(this.backwards);
		
		IntBuffer backwards_VBO = IntBuffer.allocate(1);
		gl.glGenBuffers(1, backwards_VBO);
		gl.glBindBuffer(GL3.GL_ARRAY_BUFFER, backwards_VBO.get());
		gl.glBufferData(GL3.GL_ARRAY_BUFFER, 	 VertexNum * 8 * Float.BYTES, 	 output, 	GL3.GL_STATIC_DRAW);
		
		IntBuffer backwards_EBO = IntBuffer.allocate(1);
		gl.glGenBuffers(1, backwards_EBO);
		gl.glBindBuffer(GL3.GL_ELEMENT_ARRAY_BUFFER, backwards_EBO.get());
		gl.glBufferData(GL3.GL_ELEMENT_ARRAY_BUFFER,    FaceNum  *  3 * Integer.BYTES,		indices, 		GL3.GL_STATIC_DRAW);

		
		gl.glEnableVertexAttribArray(3);
		gl.glVertexAttribPointer(3, 3, GL3.GL_FLOAT, false, 8*Float.BYTES, 0);
		gl.glEnableVertexAttribArray(4);
		gl.glVertexAttribPointer(4, 3, GL3.GL_FLOAT, false, 8*Float.BYTES, 3*Float.BYTES);
		gl.glEnableVertexAttribArray(5);
		gl.glVertexAttribPointer(5, 2, GL3.GL_FLOAT, false, 8*Float.BYTES, 6*Float.BYTES);
		
		gl.glBindVertexArray(0);
		
		//低模导入完毕
		
		//接下来我们要重新处理最开始的高模
		//因为assimp不会为我们自动合并顶点
		//我们会在这里合并相同点，并且生成对应的平滑法线
		//如果我们要用哈希表为模型分配pos：index：normal的映射关系
		//不是简单地拿出两张哈希对应两个映射关系可以搞定的
		//因为我们的法线不是唾手可得的，而是需要通过图元cross计算出来
		//所以我们要模拟GS管线，逐图元处理顶点，然后将顶点自加法线
		//但现在我们有一份特别的重拓扑方案
		//它可以在边缘上插值
		
		utils.LOG("合并高模顶点");
		geometryprocessor gp = new geometryprocessor(workmesh, this.builder);
		FloatBuffer commonvbo = gp.CreateBufferObject();
		IntBuffer commonebo = gp.RemappingIndices();
		
		
/*
		AiScene Reversed = new Importer().readFile(swirl , Triangulate.i);
		AiMesh Rworkmesh = Reversed.getMeshes().get(0);
		geometryprocessor Rgp = new geometryprocessor(Rworkmesh, this.builder);
		FloatBuffer swirlVBO = Rgp.CreateBufferObject();
		IntBuffer swirlEBO = Rgp.RemappingIndices();
		
		//载入反旋模型
		//Binding-8,9
		IntBuffer SWIRLBO = IntBuffer.allocate(2);
		gl.glGenBuffers(2, SWIRLBO);
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, SWIRLBO.get(0));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, swirlVBO.capacity()*Float.BYTES, swirlVBO, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 8, SWIRLBO.get(0));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, SWIRLBO.get(1));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, swirlEBO.capacity()*Integer.BYTES, swirlEBO, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 9, SWIRLBO.get(1));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		//finished
*/
		
		
		this.normalcount = gp.IndexRemap.size();
		
		//Programmable vertex pulling
		//由位置和法线组成的单个SSBO
		IntBuffer ssbo = IntBuffer.allocate(1);
		gl.glGenBuffers(1, ssbo);
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, ssbo.get(0));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, commonvbo.capacity()*Float.BYTES, commonvbo, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 0, ssbo.get(0));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);

		IntBuffer rmvao = IntBuffer.allocate(1);
		gl.glGenVertexArrays(1, rmvao);
		gl.glBindVertexArray(rmvao.get(0));
		this.rmi = rmvao.get(0);

		//重新绑定element buffer 以产生正确连接的图元
		IntBuffer pvpebo = IntBuffer.allocate(1);
		gl.glGenBuffers(1, pvpebo);
		gl.glBindBuffer(GL3.GL_ELEMENT_ARRAY_BUFFER, pvpebo.get(0));
		gl.glBufferData(GL3.GL_ELEMENT_ARRAY_BUFFER, commonebo.capacity()*Integer.BYTES, commonebo, GL3.GL_STATIC_DRAW);
		gl.glBindVertexArray(0);

		//渲染管线的组成：1.兰伯特  2.法线显示
		smoothrenlow = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, smoothrenlow, "lowmesh.vs", "applynormal.fs");
		sn = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, sn,                     "pvp_standardvs.vs",  "pvp_shownormal.gs", "grid_giveflattencolor_red.fs");
		smoothrenhigh = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, smoothrenhigh, "pvp_standardvs.vs",  "pvp_points.gs", "grid_simplelambert.fs");
		
		////////////////////////////////////
		//here
		universalshaderloader.SetPipeline(gl, sp,"pvp_standardvs.vs",  "pvp_passnormal.gs", "grid_simplelambert.fs");
		
		snlow = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, snlow,                "lowmesh_loadvs.vs",  "pvp_shownormal.gs", "grid_giveflattencolor_red.fs");

		utils.getErrors(gl, "programmable vertices pulling"); 

		
		
		//show uv
		uvshader = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, uvshader,      "lowmesh_loadvsasuv.vs",  "lowmesh_flatten.gs", "lowmesh_blank.fs");

		uvframe = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, uvframe,      "lowmesh_loadvsasuv.vs",  "grid_triangulateprimitives.gs", "grid_giveflattencolor_red.fs");

		
		//compute shader
		//用来构建网格
		
		//csgrid = gl.glCreateProgram();
		//等等，你确定？遍历图元然后算出来网格是没问题的，但是这些图元都要放进网格里面去
		//多个物体依次压栈的操作无法并行
		//并行的时候，多个线程写入的同一个东西不能真的是同一个
		//所以我们都是对屏幕算cs的
		
		gp.GridToBuffer();
		IntBuffer GBO = gp.IG;
		FloatBuffer GH = gp.FG;
		
		
		/*
		//另一个模型
		Rgp.GridToBuffer();
		IntBuffer RGBO = Rgp.IG;
		FloatBuffer RGH = Rgp.FG;
*/
		
		//programmed ebo pipeline
		IntBuffer eboss=IntBuffer.allocate(1);
		gl.glGenBuffers(1, eboss);
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, eboss.get(0));
		//写入eboss
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, commonebo.capacity()*Integer.BYTES, commonebo, GL3.GL_DYNAMIC_COPY);
		//绑定到indexed array上
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 1, eboss.get(0));
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		//优化思路：把matrices用uniform buffer表达。
		//以及，做个类封装bindbufferbase，手动防呆避免你写出重复的binding来
		
		testcount = commonebo.capacity();
		
		//grid
		IntBuffer gss =IntBuffer.allocate(2);
		gl.glGenBuffers(2, gss);
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, gss.get(0));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, GBO.capacity()*Integer.BYTES, GBO, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 2, gss.get(0));
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, gss.get(1));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, GH.capacity()*Float.BYTES, GH, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 3, gss.get(1));
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		
		//平行网格grid
/*
		IntBuffer sda =IntBuffer.allocate(2);
		gl.glGenBuffers(2, sda);
		
		IntBuffer debug =IntBuffer.allocate(1);
		gl.glGetIntegerv(gl.GL_MAX_FRAGMENT_SHADER_STORAGE_BLOCKS, debug);
		utils.LOG(debug.get() + "最大绑定");
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, sda.get(0));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, RGBO.capacity()*Integer.BYTES, RGBO, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 10, sda.get(0));
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, sda.get(1));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, RGH.capacity()*Float.BYTES, RGH, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 11, sda.get(1));
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		*/
		testrender = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, testrender,      "ProgrammableUVLoading.vs", "Raytrace.fs");
		//需要新建一个渲染窗口，它只调用一次display，打开后用于渲染光追
		
		raytraceTangent = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, raytraceTangent,      "ProgrammableUVLoading.vs","FlatEncode.gs",  "Raytracetangent.fs");
		
		//AVERAGE?
		Advanceraytrace = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, Advanceraytrace,      "ProgrammableUVLoading_Advanced.vs", "RaytraceRemastered_aniso.fs");
		
		
		//只要unit0的texture2d有东西就行了，无所谓这个东西是什么时候加的，shader最后总归能找得到
		ApplyNormalmap app = new ApplyNormalmap();
		try {
			app.run(gl);
		} catch (IOException e) {
			e.printStackTrace();
		}
		
		app.GenerateTangentData(gl, Mesh);
		
		displaytangent = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, displaytangent,                     "lowmesh.vs", "displaytangent.gs", "grid_giveflattencolor_red.fs");
		
		
		//根据Mikk文档
		//在VS里面生成切线和法线，并且保证是normalize的
		//在FS里面对插值后的切线和法线不再normalize
		AdvanceraytraceMIKK = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, AdvanceraytraceMIKK,      "ProgrammableUVLoading_Advanced_MIKK.vs", "RaytraceRemastered_MIKK.fs");
	
		GridforPoints pointsGrid = new GridforPoints(gp.IndexRemap, this.builder);
		
		IntBuffer gfb =IntBuffer.allocate(2);
		gl.glGenBuffers(2, gfb);
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, gfb.get(0));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, pointsGrid.GridBO.capacity()*Integer.BYTES, pointsGrid.GridBO, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 8, gfb.get(0));
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, gfb.get(1));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, pointsGrid.GridHelper.capacity()*Integer.BYTES, pointsGrid.GridHelper, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 9, gfb.get(1));
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);

		/*
		IntBuffer adjinfo = gp.adj;

		IntBuffer locator = gp.adj_locator;
		
		IntBuffer www =IntBuffer.allocate(2);
		gl.glGenBuffers(2, www);
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, www.get(0));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, adjinfo.capacity()*Integer.BYTES, adjinfo, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 8, www.get(0));
				
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, www.get(1));
		gl.glBufferData(GL3.GL_SHADER_STORAGE_BUFFER, locator.capacity()*Integer.BYTES, locator, GL3.GL_DYNAMIC_COPY);
		gl.glBindBufferBase(GL3.GL_SHADER_STORAGE_BUFFER, 9, www.get(1));
		
		gl.glBindBuffer(GL3.GL_SHADER_STORAGE_BUFFER, 0);
		
		flash = gl.glCreateProgram();
		universalshaderloader.SetPipeline(gl, flash,                     "void.vs", "void.gs","void.fs");
		
		*/
	}

	@Override
	public void reshape(GLAutoDrawable arg0, int arg1, int arg2, int arg3, int arg4) {
		this.Dimension = new int[] {arg1, arg2, arg3, arg4} ;
		//bind frame buffer 1 (custom)
		//gl viewport
		//bind frame buffer 0
	}

}
