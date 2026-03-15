/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.IE
 *  ttw.tradefinder.UB
 */
package ttw.tradefinder;

import ttw.tradefinder.UB;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class IE {
    private long[] i;
    private final int k;
    private UB[] I;
    private int G;
    private final boolean D;

    public void B() {
        int n2;
        IE a2;
        IE iE = a2;
        long[] lArray = new long[iE.G];
        UB[] uBArray = new UB[iE.G];
        int n3 = n2 = 0;
        while (n3 < a2.G) {
            int n4 = n2;
            lArray[n4] = a2.i[n4];
            int n5 = n2++;
            uBArray[n5] = a2.I[n5];
            n3 = n2;
        }
        a2.i = lArray;
        a2.I = uBArray;
    }

    public void I() {
        int n2;
        IE a2;
        IE iE = a2;
        long[] lArray = new long[a2.G - iE.G / 4 + 1];
        UB[] uBArray = new UB[iE.G - a2.G / 4 + 1];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.G) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3] = a2.i[n2];
                uBArray[n3++] = a2.I[n2];
            }
            n4 = ++n2;
        }
        IE iE2 = a2;
        iE2.i = lArray;
        iE2.I = uBArray;
        a2.G = n3;
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, UB a3) {
        IE iE;
        IE a4;
        IE iE2 = a4;
        if (iE2.G == iE2.i.length) {
            IE iE3 = a4;
            if (iE3.k > iE3.i.length) {
                IE iE4 = a4;
                iE = iE4;
                iE4.a();
            } else {
                if (!a4.D) return;
                IE iE5 = a4;
                iE = iE5;
                iE5.f();
            }
        } else {
            iE = a4;
        }
        iE.i[a4.G] = a2;
        a4.I[a4.G++] = a3;
    }

    public UB A(int a2) {
        IE a3;
        if (a2 >= a3.G) {
            return null;
        }
        return a3.I[a2];
    }

    public long A(int a2) {
        IE a3;
        if (a2 >= a3.G) {
            return 0L;
        }
        return a3.i[a2];
    }

    public void a() {
        int n2;
        IE a2;
        int n3 = Math.min(a2.i.length + a2.i.length / 2, a2.k);
        long[] lArray = new long[n3];
        UB[] uBArray = new UB[n3];
        int n4 = n2 = 0;
        while (n4 < a2.G) {
            int n5 = n2;
            lArray[n5] = a2.i[n5];
            int n6 = n2++;
            uBArray[n6] = a2.I[n6];
            n4 = n2;
        }
        a2.i = lArray;
        a2.I = uBArray;
    }

    public int f(long a2) {
        int n2;
        IE a3;
        if (a3.G == 0) {
            return -1;
        }
        if (a2 < a3.i[0]) {
            return -1;
        }
        IE iE = a3;
        if (a2 > iE.i[iE.G - 1]) {
            return a3.G - 1;
        }
        IE iE2 = a3;
        int n3 = n2 = iE2.G / 2;
        int n4 = iE2.G - 1;
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

    public IE(int a2, int a3, boolean a4) {
        IE a5;
        IE iE = a5;
        IE iE2 = a5;
        iE2.G = 0;
        iE2.D = a4;
        int n2 = a2;
        iE2.k = Math.max(a3, n2);
        iE.i = new long[n2];
        iE.I = new UB[a2];
    }

    public void f() {
        int n2;
        IE a2;
        int n3 = Math.max(a2.i.length, 5);
        long[] lArray = new long[n3];
        UB[] uBArray = new UB[n3];
        int n4 = 0;
        UB uB2 = null;
        int n5 = n2 = 0;
        while (n5 < a2.G) {
            UB uB3 = a2.I[n2];
            if (n2 % 4 == 0 && n2 != 0) {
                uB2 = uB3;
            } else {
                if (uB2 != null) {
                    uB3.A(uB2);
                    uB2 = null;
                }
                lArray[n4] = a2.i[n2];
                uBArray[n4++] = uB3;
            }
            n5 = ++n2;
        }
        if (uB2 != null) {
            int n6 = n4;
            long l2 = lArray[n4 - 1] + 1L;
            ++n4;
            lArray[n6] = l2;
            uBArray[n6] = uB2;
        }
        IE iE = a2;
        iE.i = lArray;
        iE.I = uBArray;
        a2.G = n4;
    }

    public int f() {
        IE a2;
        return a2.i.length;
    }

    public int A() {
        IE a2;
        return a2.G;
    }

    public IE(int a2) {
        a3(a2, Integer.MAX_VALUE, false);
        IE a3;
    }

    public int A(long a2) {
        int n2;
        IE a3;
        if (a3.G == 0) {
            return -1;
        }
        IE iE = a3;
        if (a2 > iE.i[iE.G - 1]) {
            return -1;
        }
        if (a2 < a3.i[0]) {
            return 0;
        }
        IE iE2 = a3;
        int n3 = n2 = iE2.G / 2;
        int n4 = iE2.G - 1;
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

    public void A() {
        IE a2;
        int n2;
        int n3 = n2 = 0;
        while (n3 < a2.G) {
            a2.I[n2++] = null;
            n3 = n2;
        }
        a2.G = 0;
    }
}

