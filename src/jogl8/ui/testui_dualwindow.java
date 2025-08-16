package jogl8.ui;

import java.awt.event.KeyListener;
import java.awt.event.MouseAdapter;

import javax.swing.JFrame;

import com.jogamp.opengl.awt.GLCanvas;
import com.jogamp.opengl.util.FPSAnimator;

import jogl8.control;
import jogl8.sim.gridtrace;
import jogl8.ui.interactives.AutoAnimationMouse;
import jogl8.ui.interactives.KeyboardListener;
import jogl8.ui.interactives.StandardMouse;
import jogl8.ui.interactives.debugKeyboard;

public class testui_dualwindow {
	public testui_dualwindow() {
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
		//anm.add(testcanvas);
		//anm.start();
		
		JFrame frame2 = new JFrame("渲染窗口");
		frame2.setSize(control.WindowWidth, control.WindowHeight);
		frame2.getContentPane().add(testcanvas);
		frame2.setVisible(true);
		
	}
}
