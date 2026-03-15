/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.RB
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.ab
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.ra
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import ttw.tradefinder.Nc;
import ttw.tradefinder.YD;
import ttw.tradefinder.ab;
import ttw.tradefinder.rH;
import ttw.tradefinder.ra;
import ttw.tradefinder.yf;

public class RB
implements ActionListener {
    public final /* synthetic */ ab e;
    public final /* synthetic */ YD i;
    public final /* synthetic */ Nc k;
    public final /* synthetic */ int I;
    public final /* synthetic */ Nc G;
    public final /* synthetic */ rH D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ RB(ab a2, YD a3, int a4, rH a5, Nc a6, Nc a7) {
        RB a8;
        RB rB = a8;
        RB rB2 = a8;
        rB2.e = a2;
        rB2.i = a3;
        rB.I = a4;
        rB.D = a5;
        a8.k = a6;
        a8.G = a7;
    }

    /*
     * Unable to fully structure code
     */
    @Override
    public void actionPerformed(ActionEvent a) {
        a = false;
        if (((ra)a.i.I).A() >= ab.I) {
            a = true;
        }
        ((ra)a.i.I).f(a.I);
        v0 = a;
        a.e.A().A(a.e.A().g(), v0.D.G, a.i);
        v0.i.A(yf.ma);
        if (a) ** GOTO lbl-1000
        v1 = a;
        if (v1.k.A(v1.G)) {
            v2 = a;
            v3 = v2;
            a.e.j(v2.D.G);
        } else lbl-1000:
        // 2 sources

        {
            v4 = a;
            v3 = v4;
            v4.e.B(v4.D.G);
        }
        v3.e.A().A(a.D.G, a.e.A() + a.I);
    }
}

