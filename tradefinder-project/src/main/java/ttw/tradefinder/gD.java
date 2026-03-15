/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.de
 *  ttw.tradefinder.gD
 *  ttw.tradefinder.jd
 *  ttw.tradefinder.sD
 *  ttw.tradefinder.vE
 */
package ttw.tradefinder;

import ttw.tradefinder.Ga;
import ttw.tradefinder.dD;
import ttw.tradefinder.de;
import ttw.tradefinder.go;
import ttw.tradefinder.ja;
import ttw.tradefinder.jd;
import ttw.tradefinder.sD;
import ttw.tradefinder.ua;
import ttw.tradefinder.vE;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class gD {
    public boolean i;
    public String k;
    public dD I;
    public String G;
    public sD D;

    public gD A() {
        gD a2;
        gD gD2 = new gD();
        new gD().I = a2.I;
        gD gD3 = a2;
        gD2.D = a2.D;
        gD2.k = gD3.k;
        gD2.G = gD3.G;
        gD2.i = a2.i;
        return gD2;
    }

    public go A() {
        gD a2;
        go go2 = new go();
        new go().a = ua.A((dD)a2.I);
        go go3 = go2;
        go3.b = ua.A((sD)a2.D);
        go3.c = a2.k != null ? a2.k : "";
        go2.e = a2.G != null ? a2.G : "";
        go2.d = a2.i;
        return go2;
    }

    public gD(dD a2, sD a3, boolean a4) {
        gD a5;
        gD gD2 = a5;
        gD gD3 = a5;
        gD gD4 = a5;
        a5.I = dD.I;
        a5.D = sD.i;
        gD4.k = "";
        gD4.G = "";
        gD3.i = false;
        gD3.I = a2;
        gD2.D = a3;
        gD2.i = a4;
    }

    public String A(String a2, String a3, String a4) {
        gD a5;
        return a5.f(a2, a3, a4, jd.A((Object)"O~"));
    }

    public String f(String a2, String a3, String a4, String a5) {
        gD a6;
        Object object = "";
        if (a6.i && a4 != null && !a4.isEmpty()) {
            String string = a5;
            object = string + "at" + string + a4;
        }
        return new String(a2 + a5 + a3 + (String)object).trim();
    }

    public vE A() {
        gD a2;
        return a2.A("", "", "", "", false);
    }

    public boolean A() {
        gD a2;
        return a2.I != dD.I;
    }

    public gD() {
        a2(dD.I, sD.i, false);
        gD a2;
    }

    public void A(go a2) {
        gD a3;
        if (a2.a != null) {
            a3.I = ua.A((Ga)a2.a);
        }
        if (a2.b != null) {
            a3.D = ua.A((ja)a2.b);
        }
        if (a2.c != null) {
            a3.k = a2.c;
        }
        if (a2.e != null) {
            a3.G = a2.e;
        }
        a3.i = a2.d;
        if (a3.k != null && !a3.k.isEmpty() && a3.G.isEmpty()) {
            a3.G = a3.k;
        }
    }

    public vE A(String a2, String a3, String a4, String a5, boolean a6) {
        gD a7;
        if (a7.I == dD.k) {
            gD gD2 = a7;
            return new vE(gD2.I, gD2.D);
        }
        if (a7.I == dD.G) {
            if (a6) {
                gD gD3 = a7;
                return new vE(gD3.I, gD3.G);
            }
            gD gD4 = a7;
            return new vE(gD4.I, gD4.k);
        }
        if (a7.I == dD.D) {
            return new vE(a7.I, a7.A(a2, a3, a4, a5, de.A((Object)" u")));
        }
        if (a7.I == dD.i) {
            return new vE(a7.I, a7.f(a2, a3, a5, jd.A((Object)"O~")));
        }
        return new vE(dD.I, "");
    }

    public String A(String a2, String a3, String a4, String a5, String a6) {
        gD a7;
        Object object = "";
        Object object2 = "";
        if (a7.i && a5 != null && !a5.isEmpty()) {
            String string = a6;
            object = string + "at" + string + a5;
        }
        if (a4 != null && !a4.isEmpty()) {
            object2 = a6 + a4;
        }
        return new String(a2 + a6 + a3 + (String)object2 + (String)object).trim();
    }

    public String A(String a2, String a3, String a4, String a5) {
        gD a6;
        return a6.A(a2, a3, a4, a5, de.A((Object)" u"));
    }
}

