/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.bD
 */
package ttw.tradefinder;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class bD {
    private double[] i;
    private final int k;
    private int I;
    private final boolean G;
    private long[] D;

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 4 << 4 ^ (3 ^ 5) << 1;
        int cfr_ignored_0 = 3 << 3 ^ 4;
        int n5 = n3;
        int n6 = 5 << 4 ^ 3;
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

    public void A(int a22) {
        int n2;
        bD a3;
        if ((a22 = Math.max(a22, Math.max(a3.I, 1))) >= a3.D.length) {
            return;
        }
        long[] lArray = new long[a22];
        double[] a22 = new double[a22];
        int n3 = n2 = 0;
        while (n3 < a3.I) {
            int n4 = n2;
            lArray[n4] = a3.D[n4];
            int n5 = n2++;
            a22[n5] = a3.i[n5];
            n3 = n2;
        }
        a3.D = lArray;
        a3.i = a22;
    }

    public int f(long a2) {
        int n2;
        bD a3;
        if (a3.I == 0) {
            return -1;
        }
        if (a2 < a3.D[0]) {
            return -1;
        }
        bD bD2 = a3;
        if (a2 > bD2.D[bD2.I - 1]) {
            return a3.I - 1;
        }
        bD bD3 = a3;
        int n3 = n2 = bD3.I / 2;
        int n4 = bD3.I - 1;
        long l2 = a2;
        while (l2 != a3.D[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.D[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 >= a3.D[n3] && n3 < n4) {
            l3 = a2;
            ++n3;
        }
        long l4 = a2;
        while (l4 < a3.D[n3] && n3 > 0) {
            l4 = a2;
            --n3;
        }
        return n3;
    }

    public double getValue(int a2) {
        bD a3;
        if (a2 >= a3.I) {
            return 0.0;
        }
        return a3.i[a2];
    }

    public bD(int a2, int a3, boolean a4) {
        bD a5;
        bD bD2 = a5;
        bD bD3 = a5;
        bD3.I = 0;
        bD3.G = a4;
        int n2 = a2;
        bD3.k = Math.max(a3, n2);
        bD2.D = new long[n2];
        bD2.i = new double[a2];
    }

    public long getTimestamp(int a2) {
        bD a3;
        if (a2 >= a3.I) {
            return 0L;
        }
        return a3.D[a2];
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2, double a3) {
        bD bD2;
        bD a4;
        bD bD3 = a4;
        if (bD3.I == bD3.D.length) {
            bD bD4 = a4;
            if (bD4.k > bD4.D.length) {
                bD bD5 = a4;
                bD2 = bD5;
                bD5.B();
            } else {
                if (!a4.G) return;
                bD bD6 = a4;
                bD2 = bD6;
                bD6.A();
            }
        } else {
            bD2 = a4;
        }
        bD2.D[a4.I] = a2;
        a4.i[a4.I++] = a3;
    }

    public void B() {
        int n2;
        bD a2;
        int n3 = Math.min(a2.D.length + a2.D.length / 2, a2.k);
        long[] lArray = new long[n3];
        double[] dArray = new double[n3];
        int n4 = n2 = 0;
        while (n4 < a2.I) {
            int n5 = n2;
            lArray[n5] = a2.D[n5];
            int n6 = n2++;
            dArray[n6] = a2.i[n6];
            n4 = n2;
        }
        a2.D = lArray;
        a2.i = dArray;
    }

    public void I() {
        bD a2;
        a2.A(0);
    }

    public int getSize() {
        bD a2;
        return a2.I;
    }

    public int A(long a2) {
        int n2;
        bD a3;
        if (a3.I == 0) {
            return -1;
        }
        bD bD2 = a3;
        if (a2 > bD2.D[bD2.I - 1]) {
            return -1;
        }
        if (a2 < a3.D[0]) {
            return 0;
        }
        bD bD3 = a3;
        int n3 = n2 = bD3.I / 2;
        int n4 = bD3.I - 1;
        long l2 = a2;
        while (l2 != a3.D[n3] && (n2 /= 2) != 0) {
            if (a2 < a3.D[n3]) {
                n3 -= n2;
                l2 = a2;
                continue;
            }
            n3 += n2;
            l2 = a2;
        }
        long l3 = a2;
        while (l3 <= a3.D[n3] && n3 > 0) {
            l3 = a2;
            --n3;
        }
        long l4 = a2;
        while (l4 > a3.D[n3] && n3 < n4) {
            l4 = a2;
            ++n3;
        }
        return n3;
    }

    public bD(int a2) {
        a3(a2, Integer.MAX_VALUE, false);
        bD a3;
    }

    public void a() {
        int n2;
        bD a2;
        bD bD2 = a2;
        long[] lArray = new long[a2.I - bD2.I / 4 + 1];
        double[] dArray = new double[bD2.I - a2.I / 4 + 1];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.I) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3] = a2.D[n2];
                dArray[n3++] = a2.i[n2];
            }
            n4 = ++n2;
        }
        bD bD3 = a2;
        bD3.D = lArray;
        bD3.i = dArray;
        a2.I = n3;
    }

    public int getCapacity() {
        bD a2;
        return a2.D.length;
    }

    public void clear() {
        this.I = 0;
    }

    public void optimize() {
        int n2;
        bD a2;
        long[] lArray = new long[a2.D.length];
        double[] dArray = new double[a2.D.length];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.I) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3] = a2.D[n2];
                dArray[n3++] = a2.i[n2];
            }
            n4 = ++n2;
        }
        bD bD2 = a2;
        bD2.D = lArray;
        bD2.i = dArray;
        a2.I = n3;
    }
}

