package jogl8.ui;

import java.awt.event.KeyListener;
import java.awt.event.MouseAdapter;

import javax.swing.JFrame;

import com.jogamp.opengl.awt.GLCanvas;
import com.jogamp.opengl.util.FPSAnimator;

import jogl8.control;
import jogl8.sim.gridtrace;


//区别：引入了control.window***尺寸参数
//在gridtrace中支持glviewport分屏
//添加了键盘修改接口


public class testui_nextstage  {
	public testui_nextstage() {
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
	}
}
