/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.D
 *  ttw.tradefinder.H
 *  ttw.tradefinder.JH
 *  ttw.tradefinder.t
 */
package ttw.tradefinder;

import ttw.tradefinder.D;
import ttw.tradefinder.H;
import ttw.tradefinder.JH;
import ttw.tradefinder.Jd;
import ttw.tradefinder.t;

public class qh
implements D {
    public final /* synthetic */ H G;
    public final /* synthetic */ JH D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ qh(JH a2, H a3) {
        qh a4;
        a4.D = a2;
        a4.G = a3;
    }

    public void A(t a2, boolean a3, boolean a4) {
        qh a5;
        if (a3 && a4 && a5.D.I && a5.G.A()) {
            qh qh2 = a5;
            qh2.D.I = false;
            a2 = qh2.D.A();
            qh2.D.A(a5.D.i.A(), (String)((Jd)a2.I).k, a5.D.F.A((String)((Jd)a2.I).I));
        }
    }
}

