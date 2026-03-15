/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.di
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.event.FocusEvent;
import java.awt.event.FocusListener;
import javax.swing.BorderFactory;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JPasswordField;
import ttw.tradefinder.Ie;
import ttw.tradefinder.Jd;
import ttw.tradefinder.YD;
import ttw.tradefinder.di;
import ttw.tradefinder.yf;

public class pi
implements FocusListener {
    public final /* synthetic */ YD k;
    public final /* synthetic */ JButton I;
    public final /* synthetic */ di G;
    public final /* synthetic */ JPasswordField D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ pi(di a2, JPasswordField a3, JButton a4, YD a5) {
        pi a6;
        pi pi2 = a6;
        pi2.G = a2;
        pi2.D = a3;
        a6.I = a4;
        a6.k = a5;
    }

    @Override
    public void focusLost(FocusEvent a2) {
        pi a3;
        a2 = String.valueOf(a3.D.getPassword());
        if (!((String)(a2 = ((String)a2).trim())).matches(Ie.A((Object)"oD\u00012\bBJ*\u001d.\u0004b\u000bDP2K^\u001cE\u00012\b@\u001cBJ,\u00013\u0005/L;"))) {
            a3.D.setBorder(BorderFactory.createLineBorder(Color.red, 2));
            return;
        }
        a3.D.setBorder(BorderFactory.createEmptyBorder());
        pi pi2 = a3;
        if (!((String)a2).equals(pi2.G.A((String)((Jd)pi2.k.I).I))) {
            ((Jd)a3.k.I).I = (long)a3.G.f((String)a2);
            pi pi3 = a3;
            a3.G.K.A(pi3.k);
            pi3.k.A(yf.k);
        }
    }

    @Override
    public void focusGained(FocusEvent a22) {
        pi a3;
        boolean a22 = a3.D.getEchoChar() == '*';
        if (!a22) {
            return;
        }
        pi pi2 = a3;
        pi2.I.setIcon(new ImageIcon(a3.G.e));
        pi2.D.setEchoChar('\u0000');
    }
}

