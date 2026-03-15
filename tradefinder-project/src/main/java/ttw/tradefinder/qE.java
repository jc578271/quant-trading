/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.MF
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.ga
 *  ttw.tradefinder.qE
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import ttw.tradefinder.Bc;
import ttw.tradefinder.H;
import ttw.tradefinder.MF;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.el;
import ttw.tradefinder.ga;
import ttw.tradefinder.rH;
import ttw.tradefinder.uC;
import ttw.tradefinder.yf;

public class qE
implements ga {
    private final rH I;
    private final YD<uC, el> G;
    private final H D;

    public /* synthetic */ boolean A() {
        qE a2;
        return ((uC)a2.G.I).m;
    }

    public /* synthetic */ qE(H a2, rH a3) {
        qE a4;
        a4.D = a2;
        a4.I = a3;
        a4.G = a4.D.A(Bc.A((Object)"-\u0006.\u007f1;\u001d6\u001c<6 \u001d7\u000b!"), a3.G, (Ya)new uC(a3.F));
    }

    public /* synthetic */ void A(boolean a2) {
        qE a3;
        if (a2 != ((uC)a3.G.I).m) {
            ((uC)a3.G.I).m = a2;
            qE qE2 = a3;
            a3.D.A(MF.A((Object)"DcG\u001aX^tSuY_EtRbD"), qE2.I.G, a3.G);
            qE2.G.A(yf.Aa);
        }
    }
}

