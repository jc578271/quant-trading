/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.ND
 *  ttw.tradefinder.gf
 */
package ttw.tradefinder;

import ttw.tradefinder.ND;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class gf {
    private long[] k;
    private ND[] I;
    private final int G;
    private int D;

    public gf(int a2) {
        a3(a2, Integer.MAX_VALUE);
        gf a3;
    }

    public void a() {
        gf a2;
        int n2;
        int n3 = n2 = 0;
        while (n3 < a2.D) {
            a2.I[n2++] = null;
            n3 = n2;
        }
        a2.D = 0;
    }

    public ND A(int a2) {
        gf a3;
        if (a2 >= a3.D) {
            return null;
        }
        return a3.I[a2];
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 4 << 4 ^ 3;
        int cfr_ignored_0 = 5 << 3 ^ (3 ^ 5);
        int n5 = n3;
        int n6 = 2 ^ 5;
        while (n5 >= 0) {
            int n7 = n3--;
            a2[n7] = (char)(((String)object2).charAt(n7) ^ n6);
            if (n3 < 0) break;
            int n8 = n3--;
            a2[n8] = (char)(((String)object2).charAt(n8) ^ n4);
            n5 = n3;
        }
        return new String((char[])a2);
    }

    public int f(long a2) {
        int n2;
        gf a3;
        if (a3.D == 0) {
            return -1;
        }
        gf gf2 = a3;
        if (a2 > gf2.k[gf2.D - 1]) {
            return -1;
        }
        if (a2 < a3.k[0]) {
            return 0;
        }
        gf gf3 = a3;
        int n3 = n2 = gf3.D / 2;
        int n4 = gf3.D - 1;
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

    public void f() {
        int n2;
        gf a2;
        int n3 = Math.min(a2.k.length + a2.k.length / 2, a2.G);
        long[] lArray = new long[n3];
        ND[] nDArray = new ND[n3];
        int n4 = n2 = 0;
        while (n4 < a2.D) {
            int n5 = n2;
            lArray[n5] = a2.k[n5];
            int n6 = n2++;
            nDArray[n6] = a2.I[n6];
            n4 = n2;
        }
        a2.k = lArray;
        a2.I = nDArray;
    }

    public long A(int a2) {
        gf a3;
        if (a2 >= a3.D) {
            return 0L;
        }
        return a3.k[a2];
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, ND a3) {
        gf gf2;
        gf a4;
        gf gf3 = a4;
        if (gf3.D == gf3.k.length) {
            gf gf4 = a4;
            if (gf4.G <= gf4.k.length) return;
            gf gf5 = a4;
            gf2 = gf5;
            gf5.f();
        } else {
            gf2 = a4;
        }
        gf2.k[a4.D] = a2;
        a4.I[a4.D++] = a3;
    }

    public gf(int a2, int a3) {
        gf a4;
        gf gf2 = a4;
        a4.D = 0;
        int n2 = a2;
        a4.G = Math.max(a3, n2);
        gf2.k = new long[n2];
        gf2.I = new ND[a2];
    }

    public int f() {
        gf a2;
        return a2.D;
    }

    public void A() {
        int n2;
        gf a2;
        gf gf2 = a2;
        if (gf2.D >= gf2.k.length) {
            return;
        }
        gf gf3 = a2;
        long[] lArray = new long[gf3.D];
        ND[] nDArray = new ND[gf3.D];
        int n3 = n2 = 0;
        while (n3 < a2.D) {
            int n4 = n2;
            lArray[n4] = a2.k[n4];
            int n5 = n2++;
            nDArray[n5] = a2.I[n5];
            n3 = n2;
        }
        a2.k = lArray;
        a2.I = nDArray;
    }

    public int A(long a2) {
        int n2;
        gf a3;
        if (a3.D == 0) {
            return -1;
        }
        if (a2 < a3.k[0]) {
            return -1;
        }
        gf gf2 = a3;
        if (a2 > gf2.k[gf2.D - 1]) {
            return a3.D - 1;
        }
        gf gf3 = a3;
        int n3 = n2 = gf3.D / 2;
        int n4 = gf3.D - 1;
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

    public int A() {
        gf a2;
        return a2.k.length;
    }
}

