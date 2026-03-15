/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.CF
 *  ttw.tradefinder.lD
 */
package ttw.tradefinder;

import ttw.tradefinder.lD;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class CF {
    public int k;
    public int I;
    public int G;
    public int D;

    public void A(CF a2) {
        CF a3;
        if (!a2.A()) {
            return;
        }
        if (!a3.A()) {
            CF cF2 = a3;
            CF cF3 = a2;
            a3.k = a2.k;
            a3.D = cF3.D;
            cF2.I = cF3.I;
            cF2.G = a2.G;
            return;
        }
        CF cF4 = a3;
        cF4.I = a2.I;
        cF4.k = Math.max(cF4.k, a2.k);
        cF4.D = Math.min(cF4.D, a2.D);
    }

    public CF() {
        CF a2;
        CF cF2 = a2;
        CF cF3 = a2;
        cF3.G = 0;
        cF3.I = 0;
        cF2.k = 0;
        cF2.D = Integer.MAX_VALUE;
    }

    public void f() {
        CF a2;
        CF cF2 = a2;
        CF cF3 = a2;
        cF3.G = 0;
        cF3.I = 0;
        cF2.k = 0;
        cF2.D = Integer.MAX_VALUE;
    }

    public CF(CF a2) {
        CF a3;
        CF cF2 = a3;
        CF cF3 = a3;
        cF3.G = 0;
        cF3.I = 0;
        cF2.k = 0;
        cF2.D = Integer.MAX_VALUE;
        if (!a2.A()) {
            return;
        }
        CF cF4 = a3;
        CF cF5 = a2;
        CF cF6 = a3;
        cF6.k = a2.k;
        cF6.D = a2.D;
        cF4.I = cF5.I;
        cF4.G = cF5.G;
    }

    public void A(int a2) {
        CF a3;
        if (a3.G == 0) {
            a3.G = a2;
        }
        CF cF2 = a3;
        cF2.I = a2;
        cF2.k = Math.max(cF2.k, a2);
        cF2.D = Math.min(cF2.D, a2);
    }

    public CF(int a2) {
        CF a3;
        CF cF2 = a3;
        CF cF3 = a3;
        CF cF4 = a3;
        a3.G = 0;
        cF4.I = 0;
        cF4.k = 0;
        cF3.D = Integer.MAX_VALUE;
        cF3.k = a2;
        cF2.I = a3.D = a2;
        cF2.G = a2;
    }

    public lD A() {
        CF a2;
        if (!a2.A()) {
            return lD.G;
        }
        CF cF2 = a2;
        if (cF2.I > cF2.G) {
            return lD.k;
        }
        return lD.D;
    }

    public void A() {
        CF a2;
        if (a2.A()) {
            CF cF2 = a2;
            cF2.k = cF2.I;
            cF2.D = cF2.I;
            cF2.G = cF2.I;
            return;
        }
        a2.f();
    }

    public boolean A() {
        CF a2;
        return a2.I != 0;
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 3 << 3 ^ (3 ^ 5);
        int cfr_ignored_0 = (3 ^ 5) << 4 ^ 3;
        int n5 = n3;
        int n6 = 3 << 3 ^ 2;
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
}

