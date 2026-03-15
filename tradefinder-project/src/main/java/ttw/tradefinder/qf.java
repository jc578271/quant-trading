/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.JD
 *  ttw.tradefinder.qf
 */
package ttw.tradefinder;

import ttw.tradefinder.JD;

public class qf {
    private double e;
    private int i;
    private long k;
    private double I;
    private boolean G;
    private boolean D;

    public /* synthetic */ void A(double a2) {
        qf a3;
        qf qf2;
        a3.e = a2;
        if (qf2.e < a3.I * 0.8) {
            a3.D = true;
        }
        a3.I = Math.max(a3.I, a3.e);
        a3.G = false;
    }

    public /* synthetic */ void A() {
        qf a2;
        qf qf2 = a2;
        qf qf3 = a2;
        qf3.G = false;
        qf3.D = true;
        qf2.I = 0.0;
        qf2.k = 0L;
    }

    public /* synthetic */ JD A(long a2, double a3, int a4) {
        qf a5;
        double d2;
        JD jD = null;
        double d3 = Math.abs(a3);
        if (d2 > a5.I) {
            a5.I = d3;
            a5.k = a2;
            a5.i = a4;
            if (a5.D) {
                qf qf2 = a5;
                qf2.D = false;
                qf2.G = true;
            }
        }
        if (d3 < a5.I * 0.9 && a5.G) {
            qf qf3;
            boolean bl;
            a5.G = false;
            if (a3 <= 0.0) {
                bl = true;
                qf3 = a5;
            } else {
                bl = false;
                qf3 = a5;
            }
            qf qf4 = a5;
            jD = new JD(a2, bl, qf3.e, a5.I, qf4.i, qf4.k);
        }
        if (d3 < Math.max(a5.e * 0.8, a5.I * 0.7)) {
            qf qf5 = a5;
            qf qf6 = a5;
            qf6.I = Math.max(d3 * 1.3, a5.e);
            qf6.k = a2;
            qf5.i = a4;
            qf5.D = true;
        }
        return jD;
    }

    public /* synthetic */ qf(double a2) {
        qf a3;
        qf qf2 = a3;
        qf qf3 = a3;
        a3.e = 0.0;
        qf3.I = 0.0;
        qf3.k = 0L;
        qf2.i = 0;
        qf2.G = false;
        a3.D = true;
        a3.I = a3.e = a2;
    }
}

