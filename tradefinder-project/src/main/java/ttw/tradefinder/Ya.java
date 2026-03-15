/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ya
 */
package ttw.tradefinder;

import ttw.tradefinder.ha;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class Ya<T extends ha> {
    public boolean G;
    private final Class<T> D;

    public abstract void A(T var1);

    public abstract T A();

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 4 << 4 ^ 1 << 1;
        int cfr_ignored_0 = 4 << 3 ^ (3 ^ 5);
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

    public Class<T> A() {
        Ya a2;
        return a2.D;
    }

    public Ya(Class<T> a2) {
        Ya a3;
        Ya ya2 = a3;
        ya2.G = true;
        ya2.D = a2;
    }
}

