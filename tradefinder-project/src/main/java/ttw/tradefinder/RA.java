/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Kf
 *  ttw.tradefinder.RA
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.cd
 *  ttw.tradefinder.ta
 */
package ttw.tradefinder;

import ttw.tradefinder.Kf;
import ttw.tradefinder.Ya;
import ttw.tradefinder.cd;
import ttw.tradefinder.ej;
import ttw.tradefinder.mg;
import ttw.tradefinder.oa;
import ttw.tradefinder.ta;
import ttw.tradefinder.ua;
import ttw.tradefinder.va;

public class RA
extends Ya<ej> {
    public mg k;
    public cd I;
    public Kf G;
    public int D;

    public void A(ej a2) {
        ej ej2 = a2;
        a.G = ej2.IsDefault;
        if (ej2.a != 0) {
            a.D = a2.a;
        }
        if (a2.b != null) {
            a.I = ua.A((va)a2.b);
        }
        if (a2.c != null) {
            a.k = ua.A((ta)a2.c);
        }
        if (a2.d != null) {
            a.G = ua.A((oa)a2.d);
        }
    }

    public RA(cd a2, mg a3, Kf a4, int a5) {
        RA a6;
        RA rA2 = a6;
        RA rA3 = a6;
        RA rA4 = a6;
        super(ej.class);
        a6.I = cd.G;
        rA4.k = mg.D;
        rA4.G = Kf.G;
        rA4.D = 10;
        rA3.I = a2;
        rA3.k = a3;
        rA2.G = a4;
        rA2.D = a5;
    }

    public RA() {
        a2(cd.G, (mg)mg.D, Kf.G, 10);
        RA a2;
    }

    public ej A() {
        RA a2;
        ej ej2 = new ej();
        new ej().IsDefault = a2.G;
        RA rA2 = a2;
        ej2.a = a2.D;
        ej2.b = ua.A((cd)rA2.I);
        ej2.c = ua.A((mg)rA2.k);
        ej2.d = ua.A((Kf)a2.G);
        return ej2;
    }
}

