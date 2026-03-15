/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Zd
 *  ttw.tradefinder.gg
 *  ttw.tradefinder.yF
 *  ttw.tradefinder.ye
 */
package ttw.tradefinder;

import ttw.tradefinder.Zd;
import ttw.tradefinder.rI;
import ttw.tradefinder.yF;
import ttw.tradefinder.ye;

public class gg {
    private final Zd D;

    public static /* synthetic */ String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 5 << 3 ^ 3;
        int cfr_ignored_0 = 1 << 3 ^ (3 ^ 5);
        int n5 = n3;
        int n6 = 4 << 4 ^ (2 << 2 ^ 3);
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

    public /* synthetic */ ye A(rI a2, yF a3) {
        gg a4;
        return new ye(a4.D, a2, a3);
    }

    public static /* synthetic */ ye A() {
        return new ye(null, rI.C, yF.p);
    }

    public /* synthetic */ gg(Zd a2) {
        gg a3;
        a3.D = a2;
    }

    public /* synthetic */ ye A(yF a2) {
        gg a3;
        return new ye(a3.D, (rI)rI.m, a2);
    }

    public /* synthetic */ ye A(rI a2) {
        gg a3;
        return new ye(a3.D, a2, yF.M);
    }
}

