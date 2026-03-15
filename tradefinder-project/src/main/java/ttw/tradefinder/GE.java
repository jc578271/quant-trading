/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.GE
 *  ttw.tradefinder.ic
 */
package ttw.tradefinder;

import ttw.tradefinder.ic;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class GE {
    private long[] i;
    private final int k;
    private int I;
    private ic[] G;
    private final boolean D;

    public GE(int a2, int a3, boolean a4) {
        GE a5;
        GE gE = a5;
        GE gE2 = a5;
        gE2.I = 0;
        gE2.D = a4;
        int n2 = a2;
        gE2.k = Math.max(a3, n2);
        gE.i = new long[n2];
        gE.G = new ic[a2];
    }

    public void B() {
        int n2;
        GE a2;
        GE gE = a2;
        long[] lArray = new long[a2.I - gE.I / 4 + 1];
        ic[] icArray = new ic[gE.I - a2.I / 4 + 1];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.I) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3] = a2.i[n2];
                icArray[n3++] = a2.G[n2];
            }
            n4 = ++n2;
        }
        GE gE2 = a2;
        gE2.i = lArray;
        gE2.G = icArray;
        a2.I = n3;
    }

    public int f(long a2) {
        int n2;
        GE a3;
        if (a3.I == 0) {
            return -1;
        }
        if (a2 < a3.i[0]) {
            return -1;
        }
        GE gE = a3;
        if (a2 > gE.i[gE.I - 1]) {
            return a3.I - 1;
        }
        GE gE2 = a3;
        int n3 = n2 = gE2.I / 2;
        int n4 = gE2.I - 1;
        long l2 = a2;
        while (l2 != a3.i[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.i[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 >= a3.i[n3] && n3 < n4) {
            l3 = a2;
            ++n3;
        }
        long l4 = a2;
        while (l4 < a3.i[n3] && n3 > 0) {
            l4 = a2;
            --n3;
        }
        return n3;
    }

    public GE(int a2) {
        a3(a2, Integer.MAX_VALUE, false);
        GE a3;
    }

    public int A(long a2) {
        int n2;
        GE a3;
        if (a3.I == 0) {
            return -1;
        }
        GE gE = a3;
        if (a2 > gE.i[gE.I - 1]) {
            return -1;
        }
        if (a2 < a3.i[0]) {
            return 0;
        }
        GE gE2 = a3;
        int n3 = n2 = gE2.I / 2;
        int n4 = gE2.I - 1;
        long l2 = a2;
        while (l2 != a3.i[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.i[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 <= a3.i[n3] && n3 > 0) {
            l3 = a2;
            --n3;
        }
        long l4 = a2;
        while (l4 > a3.i[n3] && n3 < n4) {
            l4 = a2;
            ++n3;
        }
        return n3;
    }

    public long A(int a2) {
        GE a3;
        if (a2 >= a3.I) {
            return 0L;
        }
        return a3.i[a2];
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 3 << 3 ^ 2;
        int cfr_ignored_0 = (3 ^ 5) << 4 ^ 1;
        int n5 = n3;
        int n6 = 2 << 3 ^ 3;
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

    public int f() {
        GE a2;
        return a2.I;
    }

    public void I() {
        int n2;
        GE a2;
        int n3 = Math.max(a2.i.length, 9);
        long[] lArray = new long[n3];
        ic[] icArray = new ic[n3];
        int n4 = 0;
        int n5 = n2 = 0;
        while (n5 < a2.I) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n4] = a2.i[n2];
                icArray[n4++] = a2.G[n2];
            }
            n5 = ++n2;
        }
        GE gE = a2;
        gE.i = lArray;
        gE.G = icArray;
        a2.I = n4;
    }

    public void a() {
        GE a2;
        int n2;
        int n3 = n2 = 0;
        while (n3 < a2.I) {
            a2.G[n2++] = null;
            n3 = n2;
        }
        a2.I = 0;
    }

    public int A() {
        GE a2;
        return a2.i.length;
    }

    public void f() {
        int n2;
        GE a2;
        GE gE = a2;
        long[] lArray = new long[gE.I];
        ic[] icArray = new ic[gE.I];
        int n3 = n2 = 0;
        while (n3 < a2.I) {
            int n4 = n2;
            lArray[n4] = a2.i[n4];
            int n5 = n2++;
            icArray[n5] = a2.G[n5];
            n3 = n2;
        }
        a2.i = lArray;
        a2.G = icArray;
    }

    public void A() {
        int n2;
        GE a2;
        int n3 = Math.min(a2.i.length + a2.i.length / 2, a2.k);
        long[] lArray = new long[n3];
        ic[] icArray = new ic[n3];
        int n4 = n2 = 0;
        while (n4 < a2.I) {
            int n5 = n2;
            lArray[n5] = a2.i[n5];
            int n6 = n2++;
            icArray[n6] = a2.G[n6];
            n4 = n2;
        }
        a2.i = lArray;
        a2.G = icArray;
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, ic a3) {
        GE gE;
        GE a4;
        GE gE2 = a4;
        if (gE2.I == gE2.i.length) {
            GE gE3 = a4;
            if (gE3.k > gE3.i.length) {
                GE gE4 = a4;
                gE = gE4;
                gE4.A();
            } else {
                if (!a4.D) return;
                GE gE5 = a4;
                gE = gE5;
                gE5.I();
            }
        } else {
            gE = a4;
        }
        gE.i[a4.I] = a2;
        a4.G[a4.I++] = a3;
    }

    public ic A(int a2) {
        GE a3;
        if (a2 >= a3.I) {
            return null;
        }
        return a3.G[a2];
    }
}

