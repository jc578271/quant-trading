/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Cg
 *  ttw.tradefinder.SF
 */
package ttw.tradefinder;

import ttw.tradefinder.SF;
import ttw.tradefinder.lg;
import ttw.tradefinder.mE;

public class Cg
extends mE {
    private /* synthetic */ Cg(String a2, boolean a3, SF a4) {
        Cg a5;
        Cg cg2 = a5;
        super(a2, a3, a4);
        cg2.M.a();
    }

    public static /* synthetic */ Cg A(lg a2) {
        lg lg2 = a2;
        Cg cg2 = new Cg(lg2.a, lg2.K, a2.M);
        new Cg(lg2.a, lg2.K, a2.M).I = a2.I;
        lg lg3 = a2;
        Cg cg3 = cg2;
        lg lg4 = a2;
        Cg cg4 = cg2;
        lg lg5 = a2;
        cg2.k = lg5.k;
        cg4.G = (int)lg5.G;
        cg4.D = (int)a2.D;
        cg2.m = lg4.m;
        cg3.g = lg4.g;
        cg3.d = a2.d;
        cg2.f = lg3.f;
        cg2.F = lg3.F;
        cg2.i = a2.i;
        return cg2;
    }
}

