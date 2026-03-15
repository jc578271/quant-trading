/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.SF
 */
package ttw.tradefinder;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class SF {
    private int[] F;
    private long[] e;
    private final int i;
    private final boolean k;
    private final int I;
    private int G;
    private int[] D;

    public int a(long a2) {
        int n2;
        SF a3;
        if (a3.G == 0) {
            return -1;
        }
        SF sF2 = a3;
        if (a2 > sF2.e[sF2.G - 1]) {
            return -1;
        }
        if (a2 < a3.e[0]) {
            return 0;
        }
        SF sF3 = a3;
        int n3 = n2 = sF3.G / 2;
        int n4 = sF3.G - 1;
        long l2 = a2;
        while (l2 != a3.e[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.e[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 <= a3.e[n3] && n3 > 0) {
            l3 = a2;
            --n3;
        }
        long l4 = a2;
        while (l4 > a3.e[n3] && n3 < n4) {
            l4 = a2;
            ++n3;
        }
        return n3;
    }

    public long A(int a2) {
        SF a3;
        if (a2 >= a3.G) {
            return 0L;
        }
        return a3.e[a2];
    }

    public int f(long a2) {
        int n2;
        SF a3;
        if (a3.G == 0) {
            return -1;
        }
        if (a2 < a3.e[0]) {
            return -1;
        }
        SF sF2 = a3;
        if (a2 > sF2.e[sF2.G - 1]) {
            return a3.G - 1;
        }
        SF sF3 = a3;
        int n3 = n2 = sF3.G / 2;
        int n4 = sF3.G - 1;
        long l2 = a2;
        while (l2 != a3.e[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.e[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 >= a3.e[n3] && n3 < n4) {
            l3 = a2;
            ++n3;
        }
        long l4 = a2;
        while (l4 < a3.e[n3] && n3 > 0) {
            l4 = a2;
            --n3;
        }
        return n3;
    }

    public int f(int a2) {
        SF a3;
        if (a2 >= a3.G) {
            return 0;
        }
        return a3.F[a2];
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 5 << 4 ^ 1;
        int cfr_ignored_0 = (3 ^ 5) << 4 ^ 5;
        int n5 = n3;
        int n6 = 4 << 4 ^ 1;
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

    public SF(int a2, int a3, int a4, boolean a5) {
        SF a6;
        SF sF2 = a6;
        int n2 = a2;
        SF sF3 = a6;
        SF sF4 = a6;
        sF4.G = 0;
        sF4.k = a5;
        sF3.i = Math.max(a3, a2);
        sF3.I = a4;
        a6.e = new long[n2];
        sF2.D = new int[n2];
        sF2.F = new int[a2];
    }

    public SF(int a2, int a3) {
        a4(a2, Integer.MAX_VALUE, a3, false);
        SF a4;
    }

    public void I() {
        int n2;
        SF a2;
        SF sF2 = a2;
        int n3 = sF2.G - sF2.G / 4 + 1;
        long[] lArray = new long[n3];
        int[] nArray = new int[n3];
        int[] nArray2 = new int[n3];
        int n4 = 0;
        int n5 = n2 = 0;
        while (n5 < a2.G) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n4] = a2.e[n2];
                int n6 = n4++;
                nArray[n6] = a2.D[n2];
                nArray2[n6] = a2.F[n2];
            }
            n5 = ++n2;
        }
        SF sF3 = a2;
        a2.e = lArray;
        sF3.D = nArray;
        sF3.F = nArray2;
        a2.G = n4;
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, int a3, int a4) {
        SF sF2;
        SF a5;
        SF sF3 = a5;
        if (sF3.G == sF3.e.length) {
            SF sF4 = a5;
            if (sF4.i > sF4.e.length) {
                SF sF5 = a5;
                sF2 = sF5;
                sF5.f();
            } else {
                if (!a5.k) return;
                SF sF6 = a5;
                sF2 = sF6;
                sF6.A();
            }
        } else {
            sF2 = a5;
        }
        sF2.e[a5.G] = a2;
        SF sF7 = a5;
        a5.D[sF7.G] = a3;
        sF7.F[a5.G++] = a4;
    }

    public int a() {
        SF a2;
        return a2.G;
    }

    public int A(long a2) {
        SF a3;
        int n2;
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a3.G) {
            if (a3.e[n2] > a2) {
                return n3;
            }
            int n5 = a3.F[n2];
            n3 += Math.max(0, n5);
            n4 = ++n2;
        }
        return n3;
    }

    public int f() {
        SF a2;
        int n2;
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.G) {
            int n5 = a2.F[n2];
            if (n5 > 0) {
                n3 += a2.D[n2] * n5;
            }
            n4 = ++n2;
        }
        return n3;
    }

    public int A() {
        SF a2;
        return a2.e.length;
    }

    public void a() {
        int n2;
        SF a2;
        SF sF2 = a2;
        if (sF2.G >= sF2.e.length) {
            return;
        }
        SF sF3 = a2;
        long[] lArray = new long[sF3.G];
        int[] nArray = new int[sF3.G];
        int[] nArray2 = new int[sF3.G];
        int n3 = n2 = 0;
        while (n3 < a2.G) {
            int n4 = n2;
            lArray[n4] = a2.e[n4];
            int n5 = n2;
            nArray[n5] = a2.D[n5];
            int n6 = n2++;
            nArray2[n6] = a2.F[n6];
            n3 = n2;
        }
        SF sF4 = a2;
        sF4.e = lArray;
        sF4.D = nArray;
        a2.F = nArray2;
    }

    public void f() {
        int n2;
        SF a2;
        int n3 = Math.min(a2.e.length + a2.e.length / 2, a2.i);
        n3 = Math.max(n3, a2.I);
        long[] lArray = new long[n3];
        int[] nArray = new int[n3];
        int[] nArray2 = new int[n3];
        int n4 = n2 = 0;
        while (n4 < a2.G) {
            int n5 = n2;
            lArray[n5] = a2.e[n5];
            int n6 = n2;
            nArray[n6] = a2.D[n6];
            int n7 = n2++;
            nArray2[n7] = a2.F[n7];
            n4 = n2;
        }
        SF sF2 = a2;
        sF2.e = lArray;
        sF2.D = nArray;
        a2.F = nArray2;
    }

    public void A() {
        int n2;
        SF a2;
        long[] lArray = new long[a2.e.length];
        int[] nArray = new int[a2.e.length];
        int[] nArray2 = new int[a2.e.length];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.G) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3] = a2.e[n2];
                int n5 = n3++;
                nArray[n5] = a2.D[n2];
                nArray2[n5] = a2.F[n2];
            }
            n4 = ++n2;
        }
        SF sF2 = a2;
        a2.e = lArray;
        sF2.D = nArray;
        sF2.F = nArray2;
        a2.G = n3;
    }

    public SF(int a2) {
        SF a3;
        int n2 = a2;
        a3(n2, Integer.MAX_VALUE, n2, false);
    }

    public int A(int a2) {
        SF a3;
        if (a2 >= a3.G) {
            return 0;
        }
        return a3.D[a2];
    }
}

