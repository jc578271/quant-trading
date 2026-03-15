/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.OC
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.YF
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.cE
 *  ttw.tradefinder.ga
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.xA
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import ttw.tradefinder.H;
import ttw.tradefinder.OC;
import ttw.tradefinder.YD;
import ttw.tradefinder.YF;
import ttw.tradefinder.Ya;
import ttw.tradefinder.ga;
import ttw.tradefinder.l;
import ttw.tradefinder.oc;
import ttw.tradefinder.rH;
import ttw.tradefinder.xA;
import ttw.tradefinder.yf;

public class cE
implements ga {
    private final H k;
    private final int I;
    private final YD<oc, l> G;
    private final rH D;

    public /* synthetic */ cE(int a2, H a3, rH a4) {
        cE a5;
        a5.I = a2;
        a5.k = a3;
        a5.D = a4;
        a5.G = a5.k.A(xA.A((Object)"1]2$1{\u0000g\u0001H\u000bh\tp\u001fl\u0017"), a5.D.G, Integer.toString(a5.I), (Ya)new oc());
    }

    public /* synthetic */ boolean A() {
        cE a2;
        return (boolean)((oc)a2.G.I).D;
    }

    public /* synthetic */ void A(boolean a2) {
        cE a3;
        if (a2 != ((oc)a3.G.I).D) {
            ((oc)a3.G.I).D = (OC)a2;
            cE cE2 = a3;
            a3.k.A(YF.A((Object)"/\u007f,\u0006/Y\u001eE\u001fj\u0015J\u0017R\u0001N\t"), cE2.D.G, Integer.toString(a3.I), a3.G);
            cE2.G.A(yf.Aa);
        }
    }
}

