/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Ti
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.di
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Component;
import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import javax.swing.BorderFactory;
import javax.swing.JOptionPane;
import javax.swing.JTextField;
import ttw.tradefinder.Jd;
import ttw.tradefinder.Nc;
import ttw.tradefinder.YD;
import ttw.tradefinder.cc;
import ttw.tradefinder.di;
import ttw.tradefinder.go;
import ttw.tradefinder.yf;

public class Ti
implements KeyListener {
    public final /* synthetic */ YD k;
    public final /* synthetic */ di I;
    public final /* synthetic */ Nc G;
    public final /* synthetic */ JTextField D;

    @Override
    public void keyTyped(KeyEvent a2) {
    }

    @Override
    public void keyReleased(KeyEvent a2) {
    }

    @Override
    public void keyPressed(KeyEvent a2) {
        if (((KeyEvent)a2).getKeyCode() == 10) {
            Ti a3;
            a2 = a3.D.getText();
            if (((String)(a2 = ((String)a2).trim())).startsWith(cc.A((Object)"%")) || ((String)a2).startsWith(go.A("y"))) {
                Ti ti = a3;
                ti.D.setText((String)((Jd)ti.k.I).k);
                JOptionPane.showMessageDialog((Component)a3.G, cc.A((Object)"\u0005`-g22/w1a#u'ab|-fbb0}4{&w&"));
                return;
            }
            if (!((String)a2).matches(go.A("\nSd%mU/=x9aup"))) {
                a3.D.setBorder(BorderFactory.createLineBorder(Color.red, 2));
                return;
            }
            a3.D.setBorder(BorderFactory.createEmptyBorder());
            if (!((String)a2).equals(((Jd)a3.k.I).k)) {
                ((Jd)a3.k.I).k = (float)a2;
                Ti ti = a3;
                a3.I.K.A(ti.k);
                ti.k.A(yf.k);
            }
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Ti(di a2, JTextField a3, YD a4, Nc a5) {
        Ti a6;
        Ti ti = a6;
        ti.I = a2;
        ti.D = a3;
        a6.k = a4;
        a6.G = a5;
    }
}

