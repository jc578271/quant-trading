/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.LD
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.ga
 *  ttw.tradefinder.la
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.ra
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import ttw.tradefinder.H;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.ga;
import ttw.tradefinder.hK;
import ttw.tradefinder.la;
import ttw.tradefinder.rH;
import ttw.tradefinder.ra;
import ttw.tradefinder.yf;

public class LD
implements ga {
    private final la k;
    private final rH I;
    private final H G;
    private final YD<ra, hK> D;

    public /* synthetic */ void A(boolean a2) {
        LD a3;
        if (a2 != ((ra)a3.D.I).I) {
            ((ra)a3.D.I).I = a2;
            LD lD = a3;
            LD lD2 = a3;
            lD.G.A(lD.k.g(), lD2.I.G, a3.D);
            lD2.D.A(yf.Aa);
        }
    }

    public /* synthetic */ boolean A() {
        LD a2;
        return ((ra)a2.D.I).I;
    }

    public /* synthetic */ LD(H a2, rH a3, la a4) {
        LD a5;
        a5.G = a2;
        a5.k = a4;
        a5.I = a3;
        a5.D = a5.G.A(a5.k.g(), a3.G, (Ya)new ra());
    }
}

