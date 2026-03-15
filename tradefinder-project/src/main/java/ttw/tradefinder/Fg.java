/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Fg
 *  ttw.tradefinder.gD
 *  ttw.tradefinder.lb
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.vE
 */
package ttw.tradefinder;

import java.util.Collections;
import java.util.Map;
import ttw.tradefinder.gD;
import ttw.tradefinder.lb;
import ttw.tradefinder.lc;
import ttw.tradefinder.rH;
import ttw.tradefinder.vE;

public class Fg {
    public static /* synthetic */ String A(int a2, rH a3, boolean a4, int a5) {
        return String.format(lb.A((Object)"7f2A`p|q20a5aasgfpv5sa20a"), a4 ? lc.A((Object)"7Z(S") : lb.A((Object)"P@K"), Integer.toString(a2), a3.a(a5));
    }

    public /* synthetic */ Fg() {
        Fg a2;
    }

    public static /* synthetic */ Map<String, String> A(int a2, rH a3, boolean a4, int a5) {
        return Collections.singletonMap(lc.A((Object)"O\u0016v\u0007z"), a3.a(a5));
    }

    public static /* synthetic */ vE A(int a22, rH a3, gD a4, boolean a5, int a6) {
        int n2;
        String string;
        if (a5) {
            string = lc.A((Object)"7z\bs");
            n2 = a22;
        } else {
            string = lb.A((Object)"P`k");
            n2 = a22;
        }
        String a22 = string + " Trend " + Integer.toString(n2) + " Started";
        return a4.A(a3.m, a22, "", a3.a(a6), a5);
    }

    public static /* synthetic */ String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = (2 ^ 5) << 4 ^ (3 ^ 5) << 1;
        int cfr_ignored_0 = (2 ^ 5) << 3 ^ 5;
        int n5 = n3;
        int n6 = (3 ^ 5) << 4 ^ 4 << 1;
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

