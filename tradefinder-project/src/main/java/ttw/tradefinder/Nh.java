/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nh
 */
package ttw.tradefinder;

public class Nh {
    public String I;
    public String G;
    public String D;

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 2 << 3 ^ 4;
        int cfr_ignored_0 = 2 ^ 5;
        int n5 = n3;
        int n6 = (2 ^ 5) << 3 ^ 2;
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

    public Nh(String a2, String a3, String a4) {
        Nh a5;
        Nh nh2 = a5;
        Nh nh3 = a5;
        Nh nh4 = a5;
        nh4.I = "";
        nh4.D = "";
        nh3.G = "";
        nh3.D = a2;
        nh2.I = a4;
        nh2.G = a3;
    }
}

