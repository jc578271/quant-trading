/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.MD
 *  ttw.tradefinder.SD
 */
package ttw.tradefinder;

import ttw.tradefinder.SD;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class MD {
    private int k;
    private int I;
    private int[] G;
    private final Object D;

    public int a(int a2) {
        MD a3;
        Object object = a3.D;
        synchronized (object) {
            if (a3.G == null) {
                return 0;
            }
            int n2 = a2 = Math.max(0, Math.min(a2 - a3.I, a3.G.length - 1));
            while (n2 < a3.G.length) {
                if (a3.G[a2] != 0) {
                    return a2 + a3.I;
                }
                n2 = ++a2;
            }
            return 0;
        }
    }

    public int f(int a2) {
        MD a3;
        Object object = a3.D;
        synchronized (object) {
            if (a3.G == null) {
                return 0;
            }
            int n2 = a2 = Math.max(0, Math.min(a2 - a3.I, a3.G.length - 1));
            while (n2 >= 0) {
                if (a3.G[a2] != 0) {
                    return a2 + a3.I;
                }
                n2 = --a2;
            }
            return 0;
        }
    }

    public int A(int a2, int a3) {
        MD a4;
        Object object = a4.D;
        synchronized (object) {
            if (a4.G == null) {
                MD mD = a4;
                a4.G = new int[1];
                mD.I = a2;
                mD.k = a2;
            }
            if (a2 > a4.k) {
                MD mD = a4;
                int n2 = a2;
                mD.k = n2 + Math.min(1000, Integer.MAX_VALUE - n2 - 1);
                mD.A(mD.k - a4.I + 1, 0);
            }
            if (a2 < a4.I) {
                MD mD = a4;
                mD.I = Math.max(a2 - 1000, 0);
                MD mD2 = a4;
                mD.A(mD.k - a4.I + 1, mD2.k - mD2.I + 1 - a4.G.length);
            }
            if (a2 - a4.I <= 0 || a2 - a4.I > a4.G.length) {
                return 0;
            }
            MD mD = a4;
            int n3 = mD.G[a2 - a4.I];
            mD.G[a2 - a4.I] = a3;
            return n3;
        }
    }

    private /* synthetic */ void A(int a2, int a3) {
        int[] nArray = new int[a2];
        int n2 = 0;
        int n3 = 0;
        int n4 = n2;
        while (n4 < a2) {
            MD a4;
            if (n2 >= a3 && n3 < a4.G.length) {
                int n5 = a4.G[n3];
                ++n3;
                nArray[n2] = n5;
            } else {
                nArray[n2] = 0;
            }
            n4 = ++n2;
        }
        a4.G = nArray;
    }

    public void A() {
        MD a2;
        Object object = a2.D;
        synchronized (object) {
            MD mD = a2;
            mD.G = null;
            mD.I = -1;
            a2.k = -1;
            return;
        }
    }

    public double A(int a2, int a3) {
        MD a4;
        Object object = a4.D;
        synchronized (object) {
            if (a4.G == null) {
                return 0.0;
            }
            if (a2 > a4.k || a3 < a4.I) {
                return 0.0;
            }
            long l2 = 0L;
            int n2 = 0;
            MD mD = a4;
            a2 = Math.min(Math.max(a4.I, a2), a4.k) - mD.I;
            a3 = Math.min(Math.max(mD.I, a3), a4.k) - a4.I;
            int n3 = a2 = a2;
            while (n3 <= a3) {
                ++n2;
                l2 += (long)a4.G[a2];
                n3 = ++a2;
            }
            if (n2 == 0) {
                return 0.0;
            }
            return (double)l2 / (double)n2;
        }
    }

    public void A(SD a2, int a3, int a4) {
        MD a5;
        Object object = a5.D;
        synchronized (object) {
            if (a5.G == null) {
                return;
            }
            if (a3 > a5.k || a4 < a5.I) {
                return;
            }
            MD mD = a5;
            a3 = Math.min(Math.max(mD.I, a3), a5.k) - a5.I;
            a4 = Math.min(Math.max(mD.I, a4), a5.k) - a5.I;
            int n2 = a3 = a3;
            while (n2 <= a4) {
                a2.A(a5.G[a3++]);
                n2 = a3;
            }
            return;
        }
    }

    public int A(int a2) {
        MD a3;
        Object object = a3.D;
        synchronized (object) {
            if (a3.G == null || a2 < a3.I || a2 > a3.k) {
                return 0;
            }
            return a3.G[a2 - a3.I];
        }
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 2 << 3 ^ 1;
        int cfr_ignored_0 = 5 << 4 ^ 4 << 1;
        int n5 = n3;
        int n6 = 5 << 3;
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

    public MD() {
        MD a2;
        MD mD = a2;
        a2.D = new Object();
        a2.G = null;
        mD.I = -1;
        mD.k = -1;
    }
}

