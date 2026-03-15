/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.df
 *  ttw.tradefinder.fD
 *  ttw.tradefinder.xe
 */
package ttw.tradefinder;

import ttw.tradefinder.fD;
import ttw.tradefinder.xe;

public class df
extends xe {
    private long i;
    private fD k;
    private final long I;
    private long G;
    private fD D;

    public void A(long a2, boolean a3, int a4) {
        df a5;
        if (a3) {
            if (a2 > a5.G) {
                df df2 = a5;
                a5.G = a2 + a5.I;
                df2.D = new fD(a4, a3);
                df2.A(df2.G, a5.D);
                return;
            }
            a5.D.A(a4);
            return;
        }
        if (a2 > a5.i) {
            df df3 = a5;
            a5.i = a2 + a5.I;
            df3.k = new fD(a4, a3);
            df3.A(df3.i, a5.k);
            return;
        }
        a5.k.A(a4);
    }

    public df(long a2, int a3, int a4, boolean a5) {
        df a6;
        df df2 = a6;
        df df3 = a6;
        super(a3, a4, a5);
        df3.G = 0L;
        df3.i = 0L;
        df3.D = new fD();
        df2.k = new fD();
        df2.I = a2;
    }
}

