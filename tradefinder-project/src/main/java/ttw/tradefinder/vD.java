/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.jd
 *  ttw.tradefinder.vD
 */
package ttw.tradefinder;

import ttw.tradefinder.jd;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class vD {
    private final int i;
    private jd[] k;
    private final boolean I;
    private long[] G;
    private int D;

    public void I() {
        int n2;
        vD a2;
        vD vD2 = a2;
        long[] lArray = new long[vD2.D];
        jd[] jdArray = new jd[vD2.D];
        int n3 = n2 = 0;
        while (n3 < a2.D) {
            int n4 = n2;
            lArray[n4] = a2.G[n4];
            int n5 = n2++;
            jdArray[n5] = a2.k[n5];
            n3 = n2;
        }
        a2.G = lArray;
        a2.k = jdArray;
    }

    public int f() {
        vD a2;
        return a2.G.length;
    }

    public vD(int a2, int a3, boolean a4) {
        vD a5;
        vD vD2 = a5;
        vD vD3 = a5;
        vD3.D = 0;
        vD3.I = a4;
        int n2 = a2;
        vD3.i = Math.max(a3, n2);
        vD2.G = new long[n2];
        vD2.k = new jd[a2];
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, jd a3) {
        vD vD2;
        vD a4;
        vD vD3 = a4;
        if (vD3.D == vD3.G.length) {
            vD vD4 = a4;
            if (vD4.i > vD4.G.length) {
                vD vD5 = a4;
                vD2 = vD5;
                vD5.a();
            } else {
                if (!a4.I) return;
                vD vD6 = a4;
                vD2 = vD6;
                vD6.f();
            }
        } else {
            vD2 = a4;
        }
        vD2.G[a4.D] = a2;
        a4.k[a4.D++] = a3;
    }

    public int f(long a2) {
        int n2;
        vD a3;
        if (a3.D == 0) {
            return -1;
        }
        vD vD2 = a3;
        if (a2 > vD2.G[vD2.D - 1]) {
            return -1;
        }
        if (a2 < a3.G[0]) {
            return 0;
        }
        vD vD3 = a3;
        int n3 = n2 = vD3.D / 2;
        int n4 = vD3.D - 1;
        long l2 = a2;
        while (l2 != a3.G[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.G[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 <= a3.G[n3] && n3 > 0) {
            l3 = a2;
            --n3;
        }
        long l4 = a2;
        while (l4 > a3.G[n3] && n3 < n4) {
            l4 = a2;
            ++n3;
        }
        return n3;
    }

    public jd A(int a2) {
        vD a3;
        if (a2 >= a3.D) {
            return null;
        }
        return a3.k[a2];
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 1 << 3 ^ (3 ^ 5);
        int cfr_ignored_0 = (3 ^ 5) << 3 ^ (2 ^ 5);
        int n5 = n3;
        int n6 = 4 << 4 ^ (2 ^ 5);
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

    public void a() {
        int n2;
        vD a2;
        int n3 = Math.min(a2.G.length + a2.G.length / 2, a2.i);
        long[] lArray = new long[n3];
        jd[] jdArray = new jd[n3];
        int n4 = n2 = 0;
        while (n4 < a2.D) {
            int n5 = n2;
            lArray[n5] = a2.G[n5];
            int n6 = n2++;
            jdArray[n6] = a2.k[n6];
            n4 = n2;
        }
        a2.G = lArray;
        a2.k = jdArray;
    }

    public vD(int a2) {
        a3(a2, Integer.MAX_VALUE, false);
        vD a3;
    }

    public long A(int a2) {
        vD a3;
        if (a2 >= a3.D) {
            return 0L;
        }
        return a3.G[a2];
    }

    public void f() {
        int n2;
        vD a2;
        int n3 = Math.max(a2.G.length, 9);
        long[] lArray = new long[n3];
        jd[] jdArray = new jd[n3];
        int n4 = 0;
        jd jd2 = null;
        jd jd3 = null;
        int n5 = n2 = 0;
        while (n5 < a2.D) {
            jd jd4 = a2.k[n2];
            if (n2 % 4 == 0 && n2 != 0) {
                if (jd4.A()) {
                    if (jd3 != null) {
                        jd3.A(jd4);
                    } else {
                        jd3 = jd4;
                    }
                } else if (jd2 != null) {
                    jd2.A(jd4);
                } else {
                    jd2 = jd4;
                }
            } else {
                long[] lArray2;
                if (jd3 != null && jd4.A()) {
                    jd4.A(jd3);
                    jd3 = null;
                    lArray2 = lArray;
                } else {
                    if (jd2 != null && !jd4.A()) {
                        jd4.A(jd2);
                        jd2 = null;
                    }
                    lArray2 = lArray;
                }
                lArray2[n4] = a2.G[n2];
                jdArray[n4++] = jd4;
            }
            n5 = ++n2;
        }
        if (jd3 != null) {
            int n6 = n4;
            long l2 = lArray[n4 - 1] + 1L;
            ++n4;
            lArray[n6] = l2;
            jdArray[n6] = jd3;
        }
        if (jd2 != null) {
            int n7 = n4;
            long l3 = lArray[n4 - 1] + 1L;
            ++n4;
            lArray[n7] = l3;
            jdArray[n7] = jd2;
        }
        vD vD2 = a2;
        vD2.G = lArray;
        vD2.k = jdArray;
        a2.D = n4;
    }

    public int A() {
        vD a2;
        return a2.D;
    }

    public int A(long a2) {
        int n2;
        vD a3;
        if (a3.D == 0) {
            return -1;
        }
        if (a2 < a3.G[0]) {
            return -1;
        }
        vD vD2 = a3;
        if (a2 > vD2.G[vD2.D - 1]) {
            return a3.D - 1;
        }
        vD vD3 = a3;
        int n3 = n2 = vD3.D / 2;
        int n4 = vD3.D - 1;
        long l2 = a2;
        while (l2 != a3.G[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.G[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 >= a3.G[n3] && n3 < n4) {
            l3 = a2;
            ++n3;
        }
        long l4 = a2;
        while (l4 < a3.G[n3] && n3 > 0) {
            l4 = a2;
            --n3;
        }
        return n3;
    }

    public void A() {
        vD a2;
        int n2;
        int n3 = n2 = 0;
        while (n3 < a2.D) {
            a2.k[n2++] = null;
            n3 = n2;
        }
        a2.D = 0;
    }
}

