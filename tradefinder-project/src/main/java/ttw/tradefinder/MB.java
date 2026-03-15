/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.MB
 *  ttw.tradefinder.Na
 */
package ttw.tradefinder;

import ttw.tradefinder.Na;

public class MB {
    public boolean k;
    public final int I;
    public final Na G;
    public boolean D;

    public MB(int a2, Na a3, int a4, int a5) {
        MB a6;
        MB mB2 = a6;
        MB mB3 = a6;
        mB3.D = false;
        mB3.k = false;
        mB2.G = a3;
        mB2.I = a2;
        if (a5 == 0 || a4 == 0) {
            a6.k = false;
            return;
        }
        a6.D = a6.G.I < a5;
        a6.k = true;
    }

    public boolean A(int a2, int a3) {
        MB a4;
        if (!a4.k) {
            if (a3 == 0 || a2 == 0) {
                return false;
            }
            a4.D = a4.G.I < a3;
            a4.k = true;
        }
        if (a4.D && a3 <= a4.G.I) {
            return true;
        }
        return !a4.D && a2 >= a4.G.I;
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = (3 ^ 5) << 3 ^ 5;
        int cfr_ignored_0 = (2 ^ 5) << 4;
        int n5 = n3;
        int n6 = 1 << 3 ^ 2;
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

