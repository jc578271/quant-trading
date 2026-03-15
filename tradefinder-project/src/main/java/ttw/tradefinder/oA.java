/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.ab
 *  ttw.tradefinder.oA
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.ra
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JButton;
import ttw.tradefinder.Na;
import ttw.tradefinder.Nc;
import ttw.tradefinder.YD;
import ttw.tradefinder.ab;
import ttw.tradefinder.rH;
import ttw.tradefinder.ra;
import ttw.tradefinder.yf;

public class oA
implements ActionListener {
    public final /* synthetic */ YD e;
    public final /* synthetic */ JButton i;
    public final /* synthetic */ rH k;
    public final /* synthetic */ Nc I;
    public final /* synthetic */ ab G;
    public final /* synthetic */ Nc D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        oA a3;
        a2 = new Na();
        int n2 = ((ra)a3.e.I).A((Na)a2);
        oA oA2 = a3;
        oA oA3 = a3;
        oA2.G.A().A(oA3.G.A().g(), a3.k.G, a3.e);
        oA2.e.A(yf.ma);
        if (((ra)oA3.e.I).A() >= ab.I) {
            oA oA4 = a3;
            a3.I.f((Component)oA4.i);
            oA4.i.setEnabled(false);
        }
        oA oA5 = a3;
        oA oA6 = a3;
        oA oA7 = a3;
        oA5.D.A(oA5.I, oA6.G.A(oA6.k, n2, (Na)a2, oA7.I, oA7.D, a3.e));
        oA5.G.j(a3.k.G);
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ oA(ab a2, YD a3, rH a4, Nc a5, JButton a6, Nc a7) {
        oA a8;
        oA oA2 = a8;
        oA oA3 = a8;
        oA3.G = a2;
        oA3.e = a3;
        oA2.k = a4;
        oA2.I = a5;
        a8.i = a6;
        a8.D = a7;
    }
}

