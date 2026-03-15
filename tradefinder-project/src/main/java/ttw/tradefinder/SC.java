/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AC
 *  ttw.tradefinder.SC
 *  ttw.tradefinder.Td
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.r
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.xf
 */
package ttw.tradefinder;

import ttw.tradefinder.AC;
import ttw.tradefinder.Td;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.r;
import ttw.tradefinder.rH;
import ttw.tradefinder.tb;
import ttw.tradefinder.xf;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class SC
implements r {
    public final /* synthetic */ rH I;
    public final /* synthetic */ AC G;
    public final /* synthetic */ YD D;

    public String A(boolean a2) {
        SC a3;
        if (a2) {
            return ((tb)a3.D.I).m.A(a3.I.m, xf.H, xf.A, xf.j, Td.A((Object)"\u000e"));
        }
        return ((tb)a3.D.I).m.f(a3.I.m, xf.H, xf.j, Ya.A((Object)"t"));
    }

    public boolean isFEnabled() {
        SC a2;
        return a2.G.A().isFEnabled();
    }

    public void f() {
        SC a2;
        a2.G.A().A(a2.G.A().g(), a2.I.G, a2.D);
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ SC(AC a2, rH a3, YD a4) {
        SC a5;
        a5.G = a2;
        a5.I = a3;
        a5.D = a4;
    }

    public void A() {
        SC a2;
        a2.G.A().A(((tb)a2.D.I).m.A(a2.I.m, xf.H, xf.A, xf.j, false));
    }

    public boolean A() {
        return false;
    }
}

