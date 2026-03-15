/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.mf
 *  ttw.tradefinder.ne
 */
package ttw.tradefinder;

import java.text.DecimalFormat;
import ttw.tradefinder.mf;

public class ne {
    public boolean I;
    public int G;
    public int D;

    public /* synthetic */ ne() {
        ne a2;
        ne ne2 = a2;
        ne ne3 = a2;
        ne ne4 = a2;
        ne4.I = false;
        ne4.D = 0;
        ne3.G = 0;
        ne3.I = false;
        ne2.D = 0;
        ne2.G = 0;
    }

    public /* synthetic */ int A(ne a2) {
        ne a3;
        if (a3.D > a2.D) {
            return 1;
        }
        if (a3.D < a2.D) {
            return -1;
        }
        if (a3.G > a2.G) {
            return 1;
        }
        if (a3.G < a2.G) {
            return -1;
        }
        return 0;
    }

    public /* synthetic */ ne(int a2, int a3) {
        ne a4;
        ne ne2 = a4;
        ne ne3 = a4;
        ne ne4 = a4;
        ne4.I = false;
        ne4.D = 0;
        ne3.G = 0;
        ne3.I = true;
        ne2.D = a2;
        ne2.G = a3;
    }

    public /* synthetic */ String toString() {
        ne a2;
        DecimalFormat decimalFormat = new DecimalFormat(mf.A((Object)"\u0004W"));
        return decimalFormat.format(a2.D) + ":" + decimalFormat.format(a2.G);
    }

    public static /* synthetic */ ne A() {
        return new ne();
    }
}

