/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.CF
 *  ttw.tradefinder.Ka
 *  ttw.tradefinder.ke
 *  ttw.tradefinder.lD
 */
package ttw.tradefinder;

import ttw.tradefinder.CF;
import ttw.tradefinder.Ka;
import ttw.tradefinder.lD;
import ttw.tradefinder.oc;

public class ke {
    private int m;
    private final oc F;
    private double e;
    private final Ka i;
    private final int k;
    private double I;
    private lD G;
    private double D;

    public /* synthetic */ void A(long a2, int a3) {
        ke a4;
        if (a3 % a4.F.e.k != 0 || a3 == 0) {
            return;
        }
        if (Double.isNaN(a4.D)) {
            ke ke2 = a4;
            ke2.i.f(ke2.k, a2, false, Double.NaN);
            a4.i.f(a4.k, a2, true, Double.NaN);
            a4.D = 0.0;
        }
        if (a4.G == lD.G) {
            a4.G = a4.i.A(a3);
        }
        ke ke3 = a4;
        ++ke3.m;
        ke ke4 = a4;
        double d2 = ke3.i.A(a3, (int)ke4.F.e.k) * a4.F.e.D;
        int n2 = Math.min(ke4.m, a4.F.e.I);
        ke3.D = (ke3.D * (double)(n2 - 1) + d2) / (double)n2;
        ke3.D = Math.round(ke3.D);
        ke3.A(a2, new CF(a4.i.A((int)a3).I));
        if (ke3.G == lD.k && !Double.isNaN(a4.e)) {
            ke ke5 = a4;
            ke5.i.A(ke5.k, a2, false, a4.e);
            return;
        }
        if (a4.G == lD.D && !Double.isNaN(a4.I)) {
            ke ke6 = a4;
            ke6.i.A(ke6.k, a2, true, a4.I);
        }
    }

    public /* synthetic */ void A() {
        ke a2;
        ke ke2 = a2;
        ke ke3 = a2;
        ke3.D = Double.NaN;
        ke3.e = Double.NaN;
        ke2.I = Double.NaN;
        ke2.G = lD.G;
        ke2.m = 0;
    }

    public /* synthetic */ lD A() {
        ke a2;
        return a2.G;
    }

    public /* synthetic */ ke(int a2, oc a3, Ka a4) {
        ke a5;
        ke ke2 = a5;
        ke ke3 = a5;
        a5.D = Double.NaN;
        ke3.e = Double.NaN;
        ke3.I = Double.NaN;
        ke3.G = lD.G;
        ke2.m = 0;
        ke2.F = a3;
        a5.i = a4;
        a5.k = a2;
        a5.A();
    }

    public /* synthetic */ void A(long a2, CF a3) {
        ke a4;
        if (Double.isNaN(a4.D) || !a3.A()) {
            return;
        }
        if (a4.G == lD.k) {
            if (Double.isNaN(a4.e)) {
                ke ke2 = a4;
                ke2.e = (double)a3.I - ke2.D;
                ke2.i.A(a4.k, a2, false, a4.e, a3.I, a4.F);
                return;
            }
            if (a4.e < (double)a3.k - a4.D) {
                ke ke3 = a4;
                ke3.e = (double)a3.k - ke3.D;
                ke3.i.f(a4.k, a2, false, a4.e);
            }
            if (a4.e >= (double)(a3.D + a4.F.e.i)) {
                ke ke4 = a4;
                ke4.i.A(a4.k, a2, false, a4.e);
                ke4.i.f(a4.k, a2, false, Double.NaN);
                a4.G = lD.D;
                a4.e = Double.NaN;
                a4.I = (double)a3.I + a4.D;
                a4.i.A(a4.k, a2, true, a4.I, a3.I, a4.F);
                return;
            }
        } else if (a4.G == lD.D) {
            if (Double.isNaN(a4.I)) {
                ke ke5 = a4;
                ke5.I = (double)a3.I + ke5.D;
                ke5.i.A(a4.k, a2, true, a4.I, a3.I, a4.F);
                return;
            }
            if (a4.I > (double)a3.D + a4.D) {
                ke ke6 = a4;
                ke6.I = (double)a3.D + ke6.D;
                ke6.i.f(a4.k, a2, true, a4.I);
            }
            if (a4.I <= (double)(a3.k - a4.F.e.i)) {
                ke ke7 = a4;
                ke7.i.A(a4.k, a2, true, a4.I);
                ke7.i.f(a4.k, a2, true, Double.NaN);
                a4.G = lD.k;
                a4.I = Double.NaN;
                a4.e = (double)a3.I - a4.D;
                a4.i.A(a4.k, a2, false, a4.e, a3.I, a4.F);
            }
        }
    }
}

