/*
 * Decompiled with CFR 0.152.
 */
package ttw.tradefinder;

import java.util.EventObject;

public class hI
extends EventObject {
    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = (2 ^ 5) << 3 ^ 5;
        int cfr_ignored_0 = 3 << 3 ^ 2;
        int n5 = n3;
        char c2 = '\u0001';
        while (n5 >= 0) {
            int n6 = n3--;
            a2[n6] = (char)(((String)object2).charAt(n6) ^ c2);
            if (n3 < 0) break;
            int n7 = n3--;
            a2[n7] = (char)(((String)object2).charAt(n7) ^ n4);
            n5 = n3;
        }
        return new String((char[])a2);
    }
}

