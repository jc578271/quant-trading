/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Kd
 *  ttw.tradefinder.Wc
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.bB
 *  ttw.tradefinder.eB
 *  ttw.tradefinder.hb
 *  ttw.tradefinder.jb
 *  ttw.tradefinder.jc
 *  ttw.tradefinder.p
 */
package ttw.tradefinder;

import java.awt.Color;
import javax.swing.ImageIcon;
import javax.swing.JLabel;
import ttw.tradefinder.Hc;
import ttw.tradefinder.Kd;
import ttw.tradefinder.T;
import ttw.tradefinder.Wc;
import ttw.tradefinder.YD;
import ttw.tradefinder.bB;
import ttw.tradefinder.hb;
import ttw.tradefinder.jb;
import ttw.tradefinder.jc;
import ttw.tradefinder.p;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class eB
implements T {
    public final /* synthetic */ JLabel i;
    public final /* synthetic */ bB k;
    public final /* synthetic */ String I;
    public final /* synthetic */ YD G;
    public final /* synthetic */ String D;

    public Color A() {
        eB a2;
        return ((Wc)a2.G.I).I;
    }

    public void A(Color a2) {
        eB a3;
        if (!((Wc)a3.G.I).I.equals(a2)) {
            ((Wc)a3.G.I).I = a2;
            eB eB2 = a3;
            eB eB3 = a3;
            a3.k.A().A(a3.k.A().g(), eB2.D, eB3.I, eB3.G);
            eB2.i.setIcon(new ImageIcon(jb.A((Wc)((Wc)a3.G.I), (int)10)));
            eB eB4 = a3;
            a2 = eB4.k.A(eB4.D);
            if (a2 == null || !(a2 instanceof Hc)) {
                return;
            }
            ((Hc)((Object)a2)).A(false, true);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ eB(bB a2, YD a3, String a4, String a5, JLabel a6) {
        eB a7;
        eB eB2 = a7;
        a7.k = a2;
        eB2.G = a3;
        eB2.D = a4;
        a7.I = a5;
        a7.i = a6;
    }

    public int A() {
        eB a2;
        return ((Wc)a2.G.I).G;
    }

    public jc A() {
        eB a2;
        return ((Wc)a2.G.I).i;
    }

    public void A(int a22) {
        eB a3;
        ((Wc)a3.G.I).G = a22;
        eB eB2 = a3;
        eB eB3 = a3;
        a3.k.A().A(a3.k.A().g(), eB2.D, eB3.I, eB3.G);
        eB2.i.setIcon(new ImageIcon(jb.A((Wc)((Wc)a3.G.I), (int)10)));
        eB eB4 = a3;
        p a22 = eB4.k.A(eB4.D);
        if (a22 == null || !(a22 instanceof Hc)) {
            return;
        }
        ((Hc)a22).A(false, true);
    }

    public Kd A() {
        eB a2;
        return ((Wc)a2.G.I).D;
    }

    public hb A() {
        eB a2;
        return ((Wc)a2.G.I).k;
    }

    public void A(hb a2) {
        eB a3;
        ((Wc)a3.G.I).k = a2;
        eB eB2 = a3;
        eB eB3 = a3;
        a3.k.A().A(a3.k.A().g(), eB2.D, eB3.I, eB3.G);
        eB2.i.setIcon(new ImageIcon(jb.A((Wc)((Wc)a3.G.I), (int)10)));
        eB eB4 = a3;
        a2 = eB4.k.A(eB4.D);
        if (a2 == null || !(a2 instanceof Hc)) {
            return;
        }
        ((Hc)a2).A(false, true);
    }

    public void A(Kd a2) {
        eB a3;
        ((Wc)a3.G.I).D = a2;
        eB eB2 = a3;
        eB eB3 = a3;
        a3.k.A().A(a3.k.A().g(), eB2.D, eB3.I, eB3.G);
        eB2.i.setIcon(new ImageIcon(jb.A((Wc)((Wc)a3.G.I), (int)10)));
        eB eB4 = a3;
        a2 = eB4.k.A(eB4.D);
        if (a2 == null || !(a2 instanceof Hc)) {
            return;
        }
        ((Hc)a2).A(false, true);
    }

    public void A(jc a2) {
        eB a3;
        ((Wc)a3.G.I).i = a2;
        eB eB2 = a3;
        eB eB3 = a3;
        a3.k.A().A(a3.k.A().g(), eB2.D, eB3.I, eB3.G);
        eB2.i.setIcon(new ImageIcon(jb.A((Wc)((Wc)a3.G.I), (int)10)));
        eB eB4 = a3;
        a2 = eB4.k.A(eB4.D);
        if (a2 == null || !(a2 instanceof Hc)) {
            return;
        }
        ((Hc)a2).A(false, true);
    }
}

