/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ac
 *  ttw.tradefinder.Kd
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.jA
 *  ttw.tradefinder.jc
 *  ttw.tradefinder.kC
 *  ttw.tradefinder.nA
 *  ttw.tradefinder.p
 *  ttw.tradefinder.sA
 *  ttw.tradefinder.xC
 */
package ttw.tradefinder;

import java.awt.Color;
import javax.swing.ImageIcon;
import javax.swing.JLabel;
import ttw.tradefinder.Ac;
import ttw.tradefinder.Kd;
import ttw.tradefinder.Ld;
import ttw.tradefinder.U;
import ttw.tradefinder.YD;
import ttw.tradefinder.jA;
import ttw.tradefinder.jc;
import ttw.tradefinder.kC;
import ttw.tradefinder.p;
import ttw.tradefinder.sA;
import ttw.tradefinder.xC;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class nA
implements U {
    public final /* synthetic */ String i;
    public final /* synthetic */ xC k;
    public final /* synthetic */ JLabel I;
    public final /* synthetic */ String G;
    public final /* synthetic */ YD D;

    public void A(Ld a2) {
        nA a3;
        if (((Ac)a3.D.I).I.equals(a2)) {
            return;
        }
        ((Ac)a3.D.I).I = a2;
        a3.A();
    }

    public void f(Color a2) {
        nA a3;
        if (((Ac)a3.D.I).G.equals(a2)) {
            return;
        }
        ((Ac)a3.D.I).G = a2;
        a3.A();
    }

    public boolean A() {
        nA a2;
        return a2.k.G;
    }

    public void a(int a2) {
        nA a3;
        if (((Ac)a3.D.I).e == a2) {
            return;
        }
        ((Ac)a3.D.I).e = a2;
        a3.A();
    }

    public void A(jc a2) {
        nA a3;
        if (((Ac)a3.D.I).m.equals((Object)a2)) {
            return;
        }
        ((Ac)a3.D.I).m = a2;
        a3.A();
    }

    public Color A() {
        nA a2;
        return ((Ac)a2.D.I).F;
    }

    public int f() {
        nA a2;
        return ((Ac)a2.D.I).i;
    }

    public void a(Color a2) {
        nA a3;
        if (((Ac)a3.D.I).F.equals(a2)) {
            return;
        }
        ((Ac)a3.D.I).F = a2;
        a3.A();
    }

    public Color a() {
        nA a2;
        return ((Ac)a2.D.I).a;
    }

    public jc A() {
        nA a2;
        return ((Ac)a2.D.I).m;
    }

    public jA A() {
        nA a2;
        return ((Ac)a2.D.I).k;
    }

    public void A(jA a2) {
        nA a3;
        if (((Ac)a3.D.I).k.equals((Object)a2)) {
            return;
        }
        ((Ac)a3.D.I).k = a2;
        a3.A();
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ nA(xC a2, String a3, String a4, YD a5, JLabel a6) {
        nA a7;
        nA nA2 = a7;
        a7.k = a2;
        nA2.G = a3;
        nA2.i = a4;
        a7.D = a5;
        a7.I = a6;
    }

    public int a() {
        nA a2;
        return ((Ac)a2.D.I).e;
    }

    public void A(Kd a2) {
        nA a3;
        if (((Ac)a3.D.I).D.equals((Object)a2)) {
            return;
        }
        ((Ac)a3.D.I).D = a2;
        a3.A();
    }

    public Kd A() {
        nA a2;
        return ((Ac)a2.D.I).D;
    }

    public int A() {
        nA a2;
        return ((Ac)a2.D.I).K;
    }

    public void A(int a2) {
        nA a3;
        if (((Ac)a3.D.I).K == a2) {
            return;
        }
        ((Ac)a3.D.I).K = a2;
        a3.A();
    }

    public Ld A() {
        nA a2;
        return ((Ac)a2.D.I).I;
    }

    private /* synthetic */ void A() {
        nA a2;
        nA nA2 = a2;
        nA nA3 = a2;
        a2.k.A().A(a2.k.A().g(), nA2.G, nA3.i, nA3.D);
        nA2.I.setIcon(new ImageIcon(kC.A((Ac)((Ac)a2.D.I))));
        nA nA4 = a2;
        p p2 = nA4.k.A(nA4.G);
        if (p2 == null || !(p2 instanceof sA)) {
            return;
        }
        ((sA)p2).A(false, true);
    }

    public void A(Color a2) {
        nA a3;
        if (((Ac)a3.D.I).a.equals(a2)) {
            return;
        }
        ((Ac)a3.D.I).a = a2;
        a3.A();
    }

    public void f(int a2) {
        nA a3;
        if (((Ac)a3.D.I).i == a2) {
            return;
        }
        ((Ac)a3.D.I).i = a2;
        a3.A();
    }

    public boolean isFEnabled() {
        nA a2;
        return a2.k.D;
    }

    public Color f() {
        nA a2;
        return ((Ac)a2.D.I).G;
    }
}

