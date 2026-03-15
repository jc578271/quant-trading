/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.fD
 */
package ttw.tradefinder;

public class fD {
    public int G;
    public boolean D;

    public void A(int a2) {
        a.G += a2;
    }

    public fD(int a2, boolean a3) {
        fD a4;
        fD fD2 = a4;
        fD2.G = a2;
        fD2.D = a3;
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 4 << 3 ^ 5;
        int cfr_ignored_0 = 4 << 4 ^ (2 ^ 5) << 1;
        int n5 = n3;
        int n6 = 2 << 3 ^ (3 ^ 5);
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

    public fD() {
        a2(0, false);
        fD a2;
    }
}

