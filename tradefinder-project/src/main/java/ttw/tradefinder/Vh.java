/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nh
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.di
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import javax.swing.BorderFactory;
import javax.swing.JPasswordField;
import ttw.tradefinder.Jd;
import ttw.tradefinder.Nh;
import ttw.tradefinder.YD;
import ttw.tradefinder.di;
import ttw.tradefinder.yf;

public class Vh
implements KeyListener {
    public final /* synthetic */ di I;
    public final /* synthetic */ YD G;
    public final /* synthetic */ JPasswordField D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Vh(di a2, JPasswordField a3, YD a4) {
        Vh a5;
        a5.I = a2;
        a5.D = a3;
        a5.G = a4;
    }

    @Override
    public void keyReleased(KeyEvent a2) {
    }

    @Override
    public void keyTyped(KeyEvent a2) {
    }

    @Override
    public void keyPressed(KeyEvent a2) {
        if (((KeyEvent)a2).getKeyCode() == 10) {
            Vh a3;
            a2 = String.valueOf(a3.D.getPassword());
            if (!((String)(a2 = ((String)a2).trim())).matches(Nh.A((Object)"Ja$\u0017-go\u000f8\u000b!G.au\u0017n{9`$\u0017-e9go\t$\u0016 \ni\u001e"))) {
                a3.D.setBorder(BorderFactory.createLineBorder(Color.red, 2));
                return;
            }
            a3.D.setBorder(BorderFactory.createEmptyBorder());
            Vh vh = a3;
            if (!((String)a2).equals(vh.I.A((String)((Jd)vh.G.I).I))) {
                ((Jd)a3.G.I).I = (long)a3.I.f((String)a2);
                Vh vh2 = a3;
                a3.I.K.A(vh2.G);
                vh2.G.A(yf.k);
            }
        }
    }
}

