/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.D
 *  ttw.tradefinder.H
 *  ttw.tradefinder.dA
 *  ttw.tradefinder.kB
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.t
 */
package ttw.tradefinder;

import ttw.tradefinder.D;
import ttw.tradefinder.H;
import ttw.tradefinder.kB;
import ttw.tradefinder.rH;
import ttw.tradefinder.t;
import ttw.tradefinder.tb;
import ttw.tradefinder.uC;

public class dA
implements D {
    public final /* synthetic */ H k;
    public final /* synthetic */ String I;
    public final /* synthetic */ kB G;
    public final /* synthetic */ rH D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ dA(kB a2, rH a3, H a4, String a5) {
        dA a6;
        dA dA2 = a6;
        dA2.G = a2;
        dA2.D = a3;
        a6.k = a4;
        a6.I = a5;
    }

    public void A(t a2, boolean a3, boolean a4) {
        dA a5;
        if (a4 && !a2.A(a5.D.D) && ((tb)a5.G.K.I).a) {
            ((tb)a5.G.K.I).a = false;
            dA dA2 = a5;
            dA dA3 = a5;
            dA2.k.A(dA2.I, dA3.D.G, a5.G.K);
            dA3.G.f(((tb)a5.G.K.I).a && ((uC)a5.G.I.I).m);
        }
    }
}

