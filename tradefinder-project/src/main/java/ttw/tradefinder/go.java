/*
 * Decompiled with CFR 0.152.
 */
package ttw.tradefinder;

import ttw.tradefinder.Ga;
import ttw.tradefinder.ja;

public class go {
    public String e;
    public Ga a;
    public boolean d;
    public String c;
    public ja b;

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 1 << 3;
        int cfr_ignored_0 = (3 ^ 5) << 4 ^ (3 << 2 ^ 1);
        int n5 = n3;
        int n6 = 5 << 4 ^ 2 << 1;
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

    public go() {
        go a2;
        go go2 = a2;
        go go3 = a2;
        go3.a = Ga.e;
        go3.b = ja.a;
        go3.c = "";
        go2.e = "";
        go2.d = false;
    }
}

