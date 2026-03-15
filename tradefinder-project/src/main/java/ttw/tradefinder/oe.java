/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.oe
 */
package ttw.tradefinder;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class oe {
    private int I = 0;
    private int G;
    private int[] D;

    public oe(int a2) {
        oe a3;
        a3.G = a2;
        a3.D = new int[a2];
        a3.A();
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = (3 ^ 5) << 3;
        int cfr_ignored_0 = 4 << 4 ^ (3 ^ 5) << 1;
        int n5 = n3;
        int n6 = (3 ^ 5) << 4 ^ 1;
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

    public void A() {
        oe a2;
        int n2;
        int n3 = n2 = 0;
        while (n3 < a2.G) {
            a2.D[n2++] = 0;
            n3 = n2;
        }
        a2.I = 0;
    }

    public int A() {
        oe a2;
        int n2;
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.G) {
            n3 += a2.D[n2++];
            n4 = n2;
        }
        return n3;
    }

    public void f(int a2) {
        oe a3;
        a3.D[a3.I++] = a2;
        oe oe2 = a3;
        if (oe2.I >= oe2.G) {
            a3.I = 0;
        }
    }

    public double A() {
        oe a2;
        int n2;
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a2.G) {
            n3 += a2.D[n2++];
            n4 = n2;
        }
        return (double)n3 / (double)a2.G;
    }

    public void A(int a2) {
        int n2;
        oe a3;
        if (a2 == a3.G) {
            return;
        }
        int[] nArray = new int[a2];
        int n3 = 0;
        int n4 = n2 = 0;
        while (n4 < a3.G) {
            nArray[n3++] = a3.D[a3.I++];
            oe oe2 = a3;
            if (oe2.I >= oe2.G) {
                a3.I = 0;
            }
            if (n3 >= a2) {
                n3 = 0;
            }
            n4 = ++n2;
        }
        oe oe3 = a3;
        oe3.D = nArray;
        oe3.G = a2;
        a3.I = n3;
    }
}

