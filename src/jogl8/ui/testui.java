package jogl8.ui;

import javax.swing.JFrame;
import com.jogamp.opengl.awt.GLCanvas;
import jogl8.sim.gridtrace;

public class testui {
	public testui() {
		JFrame testframe = new JFrame("测试");
		testframe.setSize(500, 500);
		GLCanvas testcanvas = new GLCanvas();
		testcanvas.addGLEventListener(new gridtrace());
		testframe.getContentPane().add(testcanvas);
		testframe.setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);
		testframe.setVisible(true);
	}
}