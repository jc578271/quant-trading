/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.UB
 */
package ttw.tradefinder;

public class UB {
    public double i;
    public double k;
    public double I;
    public long G;
    public double D;

    public void A(UB a2) {
        UB uB2;
        UB uB3;
        UB uB4;
        UB a3;
        if (Double.isNaN(a3.I)) {
            uB4 = a3;
            a3.I = a2.I;
        } else {
            if (!Double.isNaN(a2.I)) {
                a3.I = (a3.I + a2.I) / 2.0;
            }
            uB4 = a3;
        }
        if (Double.isNaN(uB4.D)) {
            uB3 = a3;
            a3.D = a2.D;
        } else {
            if (!Double.isNaN(a2.D)) {
                a3.D = (a3.D + a2.D) / 2.0;
            }
            uB3 = a3;
        }
        if (Double.isNaN(uB3.i)) {
            uB2 = a3;
            a3.i = a2.i;
        } else {
            if (!Double.isNaN(a2.i)) {
                a3.i = (a3.i + a2.i) / 2.0;
            }
            uB2 = a3;
        }
        if (Double.isNaN(uB2.k)) {
            a3.k = a2.k;
            return;
        }
        if (!Double.isNaN(a2.k)) {
            a3.k = (a3.k + a2.k) / 2.0;
        }
    }

    public UB() {
        UB a2;
        UB uB2 = a2;
        UB uB3 = a2;
        a2.I = Double.NaN;
        uB3.D = Double.NaN;
        uB3.i = Double.NaN;
        uB2.k = Double.NaN;
        uB2.G = 0L;
    }

    public UB(double a2, double a3, double a4, double a5, long a6) {
        UB a7;
        UB uB2 = a7;
        UB uB3 = a7;
        UB uB4 = a7;
        UB uB5 = a7;
        UB uB6 = a7;
        uB6.I = Double.NaN;
        uB6.D = Double.NaN;
        uB5.i = Double.NaN;
        uB5.k = Double.NaN;
        uB4.G = 0L;
        uB4.I = a2;
        uB3.D = a3;
        uB3.i = a4;
        uB2.k = a5;
        uB2.G = a6;
    }
}

