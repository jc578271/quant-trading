/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Mg
 *  ttw.tradefinder.Sa
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Zd
 *  ttw.tradefinder.lb
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import javax.swing.JSpinner;
import javax.swing.SpinnerNumberModel;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;
import ttw.tradefinder.Sa;
import ttw.tradefinder.YD;
import ttw.tradefinder.Zd;
import ttw.tradefinder.lb;
import ttw.tradefinder.rH;
import ttw.tradefinder.yf;

public class Mg
implements ChangeListener {
    public final /* synthetic */ rH k;
    public final /* synthetic */ JSpinner I;
    public final /* synthetic */ YD G;
    public final /* synthetic */ Zd D;

    @Override
    public void stateChanged(ChangeEvent a22) {
        Mg a3;
        a22 = a3.I.getModel();
        int a22 = ((SpinnerNumberModel)a22).getNumber().intValue();
        if (a22 != ((Sa)a3.G.I).e) {
            ((Sa)a3.G.I).e = a22;
            Mg mg2 = a3;
            a3.D.A().A(lb.A((Object)"AFB?XsgypfPje~z`p`"), mg2.k.G, a3.G);
            mg2.G.A(yf.La);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Mg(Zd a2, JSpinner a3, YD a4, rH a5) {
        Mg a6;
        Mg mg2 = a6;
        mg2.D = a2;
        mg2.I = a3;
        a6.G = a4;
        a6.k = a5;
    }
}

