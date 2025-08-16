package jogl8.ui;

import java.awt.event.KeyListener;
import java.awt.event.MouseAdapter;

import javax.swing.JFrame;

import com.jogamp.opengl.awt.GLCanvas;
import com.jogamp.opengl.util.FPSAnimator;

import jogl8.control;
import jogl8.utils;
import jogl8.sim.gridtrace;

public class testui_pausable {
//不要为特定的功能创建仅能在某个地方引用一次的模块
	public class Pausablethread extends Thread{
		FPSAnimator a;
		GLCanvas c;
		public Pausablethread(FPSAnimator anm, GLCanvas can){
			this.a = anm;
			this.c = can;
		}
	    public void run() {
	        while (true) {
	            try {
		            if(control.StopAnimator) {
		            	utils.LOG("StopAnimator");
		            	a.stop();
		            	control.RenewGeometryShader = true;
		            	this.c.display();
		            	control.RenewGeometryShader = false;
		            	Thread.sleep(2000);//2s
        				utils.LOG("ResumeAnimator\n");
        				a.start();
        				 control.StopAnimator = false;
		            }else {
		                Thread.sleep(500);//0.5s
		            }
	            } catch (InterruptedException e) {
	                e.printStackTrace();
	            }
	        }
	    }
	}
	
	public testui_pausable() {
		FPSAnimator anm = new FPSAnimator(30);
		JFrame testframe = new JFrame("测试");
		testframe.setSize(control.WindowWidth, control.WindowHeight);
		GLCanvas testcanvas = new GLCanvas();
		interactives ic = new interactives();
		interactives.AutoAnimationMouse drunkmouse = ic.new AutoAnimationMouse();
		interactives.StandardMouse standardmouse = ic.new StandardMouse();
		interactives.KeyboardListener standardkeyboard = ic.new KeyboardListener();
		interactives.debugKeyboard debugkeyboard = ic.new debugKeyboard();
		
		//....
		MouseAdapter  mouse = standardmouse;
		KeyListener keylistener = standardkeyboard;
		//....
		testcanvas.addKeyListener(keylistener);
		testcanvas.addMouseMotionListener(mouse);
		testcanvas.addMouseListener(mouse);
		
		testcanvas.addGLEventListener(new gridtrace());
		testframe.getContentPane().add(testcanvas);
		testframe.setVisible(true);
		anm.add(testcanvas);
		anm.start();
		
		Pausablethread animatorlistener = new Pausablethread(anm, testcanvas);
		animatorlistener.start();

	}
}
