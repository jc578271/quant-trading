/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.ze
 */
package ttw.tradefinder;

public class ze {
    private long I;
    private double G;
    private double D;

    public /* synthetic */ void A(int a2) {
        ze a3;
        ze ze2 = a3;
        a3.G = 0.0;
        ze2.I = 0L;
        ze2.D = (double)a2 / Math.log(2.0);
    }

    public /* synthetic */ double A(long a2, int a3) {
        ze a4;
        ze ze2 = a4;
        ze2.G = ze2.A(a2) + (double)a3;
        return ze2.G;
    }

    public /* synthetic */ double A(long a2) {
        ze a3;
        ze ze2 = a3;
        ze2.G *= Math.exp((double)(a3.I - a2) / a3.D);
        ze2.I = a2;
        return ze2.G;
    }

    public static /* synthetic */ String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = (3 ^ 5) << 4 ^ 1;
        int cfr_ignored_0 = 3 << 3 ^ (3 ^ 5);
        int n5 = n3;
        int n6 = 4 << 3;
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

    public /* synthetic */ ze() {
        ze a2;
        ze ze2 = a2;
        a2.G = 0.0;
        ze2.I = 0L;
        ze2.D = 1.0;
    }

    public /* synthetic */ double A(long a2, double a3) {
        ze a4;
        ze ze2 = a4;
        ze2.G = ze2.A(a2) + a3;
        return ze2.G;
    }
}

