/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.CF
 *  ttw.tradefinder.RF
 */
package ttw.tradefinder;

import ttw.tradefinder.CF;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class RF {
    private int i;
    private long[] k;
    private final int I;
    private final boolean G;
    private CF[] D;

    public RF(int a2, int a3, boolean a4) {
        RF a5;
        RF rF = a5;
        RF rF2 = a5;
        rF2.i = 0;
        rF2.G = a4;
        int n2 = a2;
        rF2.I = Math.max(a3, n2);
        rF.k = new long[n2];
        rF.D = new CF[a2];
    }

    public void B() {
        int n2;
        RF a2;
        RF rF = a2;
        long[] lArray = new long[rF.i];
        CF[] cFArray = new CF[rF.i];
        int n3 = n2 = 0;
        while (n3 < a2.i) {
            int n4 = n2;
            lArray[n4] = a2.k[n4];
            int n5 = n2++;
            cFArray[n5] = a2.D[n5];
            n3 = n2;
        }
        a2.k = lArray;
        a2.D = cFArray;
    }

    public void I() {
        int n2;
        RF a2;
        long[] lArray = new long[a2.k.length];
        CF[] cFArray = new CF[a2.k.length];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.i) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3] = a2.k[n2];
                cFArray[n3++] = a2.D[n2];
            }
            n4 = ++n2;
        }
        RF rF = a2;
        rF.k = lArray;
        rF.D = cFArray;
        a2.i = n3;
    }

    public int f() {
        RF a2;
        return a2.k.length;
    }

    public RF(int a2) {
        a3(a2, Integer.MAX_VALUE, false);
        RF a3;
    }

    public int f(long a2) {
        int n2;
        RF a3;
        if (a3.i == 0) {
            return -1;
        }
        if (a2 < a3.k[0]) {
            return -1;
        }
        RF rF = a3;
        if (a2 > rF.k[rF.i - 1]) {
            return a3.i - 1;
        }
        RF rF2 = a3;
        int n3 = n2 = rF2.i / 2;
        int n4 = rF2.i - 1;
        long l2 = a2;
        while (l2 != a3.k[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.k[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 >= a3.k[n3] && n3 < n4) {
            l3 = a2;
            ++n3;
        }
        long l4 = a2;
        while (l4 < a3.k[n3] && n3 > 0) {
            l4 = a2;
            --n3;
        }
        return n3;
    }

    public int A(long a2) {
        int n2;
        RF a3;
        if (a3.i == 0) {
            return -1;
        }
        RF rF = a3;
        if (a2 > rF.k[rF.i - 1]) {
            return -1;
        }
        if (a2 < a3.k[0]) {
            return 0;
        }
        RF rF2 = a3;
        int n3 = n2 = rF2.i / 2;
        int n4 = rF2.i - 1;
        long l2 = a2;
        while (l2 != a3.k[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.k[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 <= a3.k[n3] && n3 > 0) {
            l3 = a2;
            --n3;
        }
        long l4 = a2;
        while (l4 > a3.k[n3] && n3 < n4) {
            l4 = a2;
            ++n3;
        }
        return n3;
    }

    public void a() {
        int n2;
        RF a2;
        int n3 = Math.min(a2.k.length + a2.k.length / 2, a2.I);
        long[] lArray = new long[n3];
        CF[] cFArray = new CF[n3];
        int n4 = n2 = 0;
        while (n4 < a2.i) {
            int n5 = n2;
            lArray[n5] = a2.k[n5];
            int n6 = n2++;
            cFArray[n6] = a2.D[n6];
            n4 = n2;
        }
        a2.k = lArray;
        a2.D = cFArray;
    }

    public CF A(int a2) {
        RF a3;
        if (a2 >= a3.i) {
            return null;
        }
        return a3.D[a2];
    }

    public void f() {
        int n2;
        RF a2;
        RF rF = a2;
        long[] lArray = new long[a2.i - rF.i / 4 + 1];
        CF[] cFArray = new CF[rF.i - a2.i / 4 + 1];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.i) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3] = a2.k[n2];
                cFArray[n3++] = a2.D[n2];
            }
            n4 = ++n2;
        }
        RF rF2 = a2;
        rF2.k = lArray;
        rF2.D = cFArray;
        a2.i = n3;
    }

    public long A(int a2) {
        RF a3;
        if (a2 >= a3.i) {
            return 0L;
        }
        return a3.k[a2];
    }

    public int A() {
        RF a2;
        return a2.i;
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, CF a3) {
        RF rF;
        RF a4;
        RF rF2 = a4;
        if (rF2.i == rF2.k.length) {
            RF rF3 = a4;
            if (rF3.I > rF3.k.length) {
                RF rF4 = a4;
                rF = rF4;
                rF4.a();
            } else {
                if (!a4.G) return;
                RF rF5 = a4;
                rF = rF5;
                rF5.I();
            }
        } else {
            rF = a4;
        }
        rF.k[a4.i] = a2;
        a4.D[a4.i++] = a3;
    }

    public void A() {
        RF a2;
        int n2;
        int n3 = n2 = 0;
        while (n3 < a2.i) {
            a2.D[n2++] = null;
            n3 = n2;
        }
        a2.i = 0;
    }
}

