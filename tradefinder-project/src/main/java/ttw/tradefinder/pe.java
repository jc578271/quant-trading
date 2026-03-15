/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.pe
 */
package ttw.tradefinder;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class pe {
    private final int k;
    private long[] I;
    private final boolean G;
    private int D;

    public void B() {
        int n2;
        pe a2;
        long[] lArray = new long[a2.I.length];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.D) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3++] = a2.I[n2];
            }
            n4 = ++n2;
        }
        a2.I = lArray;
        a2.D = n3;
    }

    public void A(int a22) {
        int n2;
        pe a3;
        if ((a22 = Math.max(a22, Math.max(a3.D, 1))) >= a3.I.length) {
            return;
        }
        long[] a22 = new long[a22];
        int n3 = n2 = 0;
        while (n3 < a3.D) {
            int n4 = n2++;
            a22[n4] = a3.I[n4];
            n3 = n2;
        }
        a3.I = a22;
    }

    public int f() {
        pe a2;
        return a2.I.length;
    }

    public void I() {
        pe a2;
        a2.A(0);
    }

    public int A() {
        pe a2;
        return a2.D;
    }

    public void a() {
        int n2;
        pe a2;
        pe pe2 = a2;
        long[] lArray = new long[pe2.D - pe2.D / 4 + 1];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.D) {
            if (n2 % 4 != 0 || n2 == 0) {
                lArray[n3++] = a2.I[n2];
            }
            n4 = ++n2;
        }
        a2.I = lArray;
        a2.D = n3;
    }

    public pe(int a2, int a3, boolean a4) {
        pe a5;
        pe pe2 = a5;
        pe pe3 = a5;
        pe3.D = 0;
        pe3.G = a4;
        pe2.k = Math.max(a3, a2);
        pe2.I = new long[a2];
    }

    public pe(int a2) {
        a3(a2, Integer.MAX_VALUE, false);
        pe a3;
    }

    public void f() {
        int n2;
        pe a2;
        int n3 = Math.min(a2.I.length + a2.I.length / 2, a2.k);
        long[] lArray = new long[n3];
        int n4 = n2 = 0;
        while (n4 < a2.D) {
            int n5 = n2++;
            lArray[n5] = a2.I[n5];
            n4 = n2;
        }
        a2.I = lArray;
    }

    public long A(int a2) {
        pe a3;
        if (a2 >= a3.D) {
            return 0L;
        }
        return a3.I[a2];
    }

    public void A() {
        a.D = 0;
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(long a2) {
        pe pe2;
        pe a3;
        pe pe3 = a3;
        if (pe3.D == pe3.I.length) {
            pe pe4 = a3;
            if (pe4.k > pe4.I.length) {
                pe pe5 = a3;
                pe2 = pe5;
                pe5.f();
            } else {
                if (!a3.G) return;
                pe pe6 = a3;
                pe2 = pe6;
                pe6.B();
            }
        } else {
            pe2 = a3;
        }
        pe2.I[a3.D++] = a2;
    }
}

