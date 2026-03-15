/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.wD
 */
package ttw.tradefinder;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class wD {
    private final boolean e;
    private long[] i;
    private int[] k;
    private int I;
    private final int G;
    private boolean[] D;

    public void a() {
        int n2;
        wD a2;
        int n3 = Math.min(a2.i.length + a2.i.length / 2, a2.G);
        long[] lArray = new long[n3];
        int[] nArray = new int[n3];
        boolean[] blArray = new boolean[n3];
        int n4 = n2 = 0;
        while (n4 < a2.I) {
            int n5 = n2;
            lArray[n5] = a2.i[n5];
            int n6 = n2;
            nArray[n6] = a2.k[n6];
            int n7 = n2++;
            blArray[n7] = a2.D[n7];
            n4 = n2;
        }
        wD wD2 = a2;
        wD2.i = lArray;
        wD2.k = nArray;
        a2.D = blArray;
    }

    public int f(long a2) {
        int n2;
        wD a3;
        if (a3.I == 0) {
            return -1;
        }
        wD wD2 = a3;
        if (a2 > wD2.i[wD2.I - 1]) {
            return -1;
        }
        if (a2 < a3.i[0]) {
            return 0;
        }
        wD wD3 = a3;
        int n3 = n2 = wD3.I / 2;
        int n4 = wD3.I - 1;
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

    public boolean A(int a2) {
        wD a3;
        if (a2 >= a3.I) {
            return false;
        }
        return a3.D[a2];
    }

    public void f() {
        int n2;
        wD a2;
        int n3 = Math.max(a2.i.length, 9);
        long[] lArray = new long[n3];
        int[] nArray = new int[n3];
        boolean[] blArray = new boolean[n3];
        int n4 = 0;
        int n5 = 0;
        int n6 = 0;
        int n7 = n2 = 0;
        while (n7 < a2.I) {
            wD wD2 = a2;
            boolean bl = wD2.D[n2];
            int n8 = wD2.k[n2];
            if (n2 % 4 == 0 && n2 != 0) {
                if (bl) {
                    n5 += a2.k[n2];
                } else {
                    n6 += a2.k[n2];
                }
            } else {
                long[] lArray2;
                if (n5 != 0 && bl) {
                    n8 += n5;
                    n5 = 0;
                    lArray2 = lArray;
                } else {
                    if (n6 != 0 && !bl) {
                        n8 += n6;
                        n6 = 0;
                    }
                    lArray2 = lArray;
                }
                lArray2[n4] = a2.i[n2];
                int n9 = n4++;
                nArray[n9] = n8;
                blArray[n9] = bl;
            }
            n7 = ++n2;
        }
        if (n5 != 0) {
            int n10 = n4;
            lArray[n4] = lArray[n4 - 1] + 1L;
            ++n4;
            nArray[n10] = n5;
            blArray[n10] = true;
        }
        if (n6 != 0) {
            int n11 = n4;
            lArray[n4] = lArray[n4 - 1] + 1L;
            ++n4;
            nArray[n11] = n6;
            blArray[n11] = false;
        }
        wD wD3 = a2;
        a2.i = lArray;
        wD3.k = nArray;
        wD3.D = blArray;
        a2.I = n4;
    }

    public int f() {
        wD a2;
        return a2.I;
    }

    public int A() {
        wD a2;
        return a2.i.length;
    }

    public wD(int a2, int a3, boolean a4) {
        wD a5;
        wD wD2 = a5;
        int n2 = a2;
        wD wD3 = a5;
        a5.I = 0;
        wD3.e = a4;
        wD3.G = Math.max(a3, a2);
        a5.i = new long[n2];
        wD2.k = new int[n2];
        wD2.D = new boolean[a2];
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, int a3, boolean a4) {
        wD wD2;
        wD a5;
        wD wD3 = a5;
        if (wD3.I == wD3.i.length) {
            wD wD4 = a5;
            if (wD4.G > wD4.i.length) {
                wD wD5 = a5;
                wD2 = wD5;
                wD5.a();
            } else {
                if (!a5.e) return;
                wD wD6 = a5;
                wD2 = wD6;
                wD6.f();
            }
        } else {
            wD2 = a5;
        }
        wD2.i[a5.I] = a2;
        wD wD7 = a5;
        a5.k[wD7.I] = a3;
        wD7.D[a5.I++] = a4;
    }

    public wD(int a2) {
        a3(a2, Integer.MAX_VALUE, false);
        wD a3;
    }

    public int A(int a2) {
        wD a3;
        if (a2 >= a3.I) {
            return 0;
        }
        return a3.k[a2];
    }

    public long A(int a2) {
        wD a3;
        if (a2 >= a3.I) {
            return 0L;
        }
        return a3.i[a2];
    }

    public void A() {
        int n2;
        wD a2;
        wD wD2 = a2;
        if (wD2.I >= wD2.i.length) {
            return;
        }
        wD wD3 = a2;
        long[] lArray = new long[wD3.I];
        int[] nArray = new int[wD3.I];
        boolean[] blArray = new boolean[wD3.I];
        int n3 = n2 = 0;
        while (n3 < a2.I) {
            int n4 = n2;
            lArray[n4] = a2.i[n4];
            int n5 = n2;
            nArray[n5] = a2.k[n5];
            int n6 = n2++;
            blArray[n6] = a2.D[n6];
            n3 = n2;
        }
        wD wD4 = a2;
        wD4.i = lArray;
        wD4.k = nArray;
        a2.D = blArray;
    }

    public int A(long a2) {
        int n2;
        wD a3;
        if (a3.I == 0) {
            return -1;
        }
        if (a2 < a3.i[0]) {
            return -1;
        }
        wD wD2 = a3;
        if (a2 > wD2.i[wD2.I - 1]) {
            return a3.I - 1;
        }
        wD wD3 = a3;
        int n3 = n2 = wD3.I / 2;
        int n4 = wD3.I - 1;
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
}

