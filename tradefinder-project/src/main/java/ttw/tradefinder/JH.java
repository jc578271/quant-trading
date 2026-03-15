/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.JH
 *  ttw.tradefinder.Mc
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.di
 *  ttw.tradefinder.la
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.t
 *  ttw.tradefinder.zG
 */
package ttw.tradefinder;

import java.util.Collection;
import java.util.Collections;
import java.util.Map;
import ttw.tradefinder.H;
import ttw.tradefinder.Jd;
import ttw.tradefinder.Mc;
import ttw.tradefinder.Nc;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.bg;
import ttw.tradefinder.di;
import ttw.tradefinder.ei;
import ttw.tradefinder.la;
import ttw.tradefinder.og;
import ttw.tradefinder.qh;
import ttw.tradefinder.rH;
import ttw.tradefinder.t;
import ttw.tradefinder.tg;
import ttw.tradefinder.zG;

public class JH {
    private di F;
    private final la e;
    private final H i;
    private zG k;
    private boolean I;
    private boolean G;
    private final t D;

    public String f(String a2) {
        JH a3;
        return a3.F.f(a2);
    }

    public String A(String a2) {
        JH a3;
        return a3.F.A(a2);
    }

    public void A(String a2, String a3) {
        JH a4;
        if (a4.D.A((bg)bg.G)) {
            a4.k.A(a2, a3);
        }
    }

    public Collection<? extends Nc> A(rH a2) {
        JH a3;
        if (!a3.i.A()) {
            return Collections.emptyList();
        }
        return a3.F.A(a3.A(), a2, a3.i);
    }

    public void f(String a2, String a3, String a4) {
        JH a5;
        if (a5.D.A((bg)bg.G)) {
            a5.k.f(a2, a3, a4);
        }
    }

    public void f() {
        JH a2;
        if (a2.G) {
            return;
        }
        if (!a2.i.A()) {
            return;
        }
        a2.k.A();
        a2.G = true;
        if (a2.I && a2.D.A((bg)bg.G)) {
            JH jH = a2;
            jH.I = false;
            YD yD2 = jH.A();
            jH.A(jH.i.A(), (String)((Jd)yD2.I).k, a2.F.A((String)((Jd)yD2.I).I));
        }
    }

    public void A(String a2, String a3, String a4) {
        JH a5;
        if (a5.D.A((bg)bg.G)) {
            a5.k.A(a2, a3, a4);
        }
    }

    public JH(H a2, la a3, Mc a4) {
        JH a5;
        JH jH = a5;
        JH jH2 = a5;
        JH jH3 = a5;
        JH jH4 = a5;
        a5.k = new zG();
        jH4.F = null;
        jH4.G = false;
        jH3.I = true;
        jH3.i = a2;
        jH2.e = a3;
        jH2.F = new di(a2, a5, a3);
        jH2.G = false;
        jH.I = true;
        jH.D = a3.f(a4).A(new qh(a5, a2));
    }

    public boolean A(bg a2) {
        JH a3;
        if (!a3.i.A()) {
            return false;
        }
        return a3.F.A(a2);
    }

    public void A() {
        JH a2;
        JH jH = a2;
        jH.k.f();
        jH.F.A();
        jH.D.A();
    }

    public YD<Jd, ei> A() {
        JH a2;
        JH jH = a2;
        return jH.i.A(jH.e.g(), (Ya)new Jd());
    }

    public void A(YD<Jd, ei> a2) {
        JH a3;
        JH jH = a3;
        jH.i.A(jH.e.g(), a2);
    }

    public void A(og a2, tg a3, String a4, String a5, String a6, Map<String, String> a7) {
        JH a8;
        if (a8.D.A((bg)bg.G)) {
            a8.k.A(a2, a3, a4, a5, a6, a7);
        }
    }
}

