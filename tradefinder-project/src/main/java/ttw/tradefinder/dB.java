/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.Vc
 *  ttw.tradefinder.dB
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import javax.swing.JTextField;
import ttw.tradefinder.Na;
import ttw.tradefinder.Vc;
import ttw.tradefinder.yf;

public class dB
implements KeyListener {
    public final /* synthetic */ Na I;
    public final /* synthetic */ JTextField G;
    public final /* synthetic */ Vc D;

    @Override
    public void keyReleased(KeyEvent a2) {
    }

    @Override
    public void keyPressed(KeyEvent a2) {
        if (((KeyEvent)a2).getKeyCode() == 10) {
            dB a3;
            a2 = a3.G.getText();
            if (!((String)(a2 = ((String)a2).trim())).equals(a3.I.e)) {
                dB dB2 = a3;
                dB2.I.e = a2;
                dB dB3 = a3;
                dB2.D.G.A(a3.D.I, dB3.D.D.G, a3.D.k);
                dB3.D.k.A(yf.ma);
                dB2.D.G.A();
            }
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ dB(Vc a2, JTextField a3, Na a4) {
        dB a5;
        a5.D = a2;
        a5.G = a3;
        a5.I = a4;
    }

    @Override
    public void keyTyped(KeyEvent a2) {
    }
}

