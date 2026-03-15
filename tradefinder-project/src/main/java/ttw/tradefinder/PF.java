/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Mf
 *  ttw.tradefinder.PF
 */
package ttw.tradefinder;

import ttw.tradefinder.Mf;
import ttw.tradefinder.q;

public class PF {
    public final Mf G;
    public final q D;

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = (2 ^ 5) << 4 ^ 4 << 1;
        int cfr_ignored_0 = (2 ^ 5) << 4 ^ 3 << 1;
        int n5 = n3;
        int n6 = (3 ^ 5) << 3 ^ 1;
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

    public PF(q a2, Mf a3) {
        PF a4;
        PF pF2 = a4;
        pF2.D = a2;
        pF2.G = a3;
    }
}

