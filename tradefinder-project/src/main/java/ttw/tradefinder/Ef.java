/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ef
 *  ttw.tradefinder.PF
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import javax.swing.JLabel;
import javax.swing.JSlider;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;
import ttw.tradefinder.PF;
import ttw.tradefinder.YD;
import ttw.tradefinder.el;
import ttw.tradefinder.fe;
import ttw.tradefinder.oc;
import ttw.tradefinder.rH;
import ttw.tradefinder.uC;
import ttw.tradefinder.yf;

public class Ef
implements ChangeListener {
    public final /* synthetic */ rH F;
    public final /* synthetic */ JLabel e;
    public final /* synthetic */ YD i;
    public final /* synthetic */ fe k;
    public final /* synthetic */ JLabel I;
    public final /* synthetic */ JSlider G;
    public final /* synthetic */ int D;

    @Override
    public void stateChanged(ChangeEvent a22) {
        Ef a3;
        int a22 = a3.G.getValue();
        if (a22 != ((oc)a3.i.I).e.I) {
            ((oc)a3.i.I).e.I = (YD<uC, el>)a22;
            Ef ef = a3;
            ef.e.setText(Integer.toString(((oc)ef.i.I).e.I));
            Ef ef2 = a3;
            Ef ef3 = a3;
            ef2.k.A(ef3.i, a3.I);
            ef2.k.A().A(PF.A((Object)"e,fUe\nT\u0016U9_\u0019]\u0001K\u001dC"), a3.F.G, Integer.toString(a3.D), a3.i);
            ef3.i.A(yf.M);
            Ef ef4 = a3;
            ef2.k.A(ef4.i, ef4.D);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Ef(fe a2, JSlider a3, YD a4, JLabel a5, JLabel a6, rH a7, int a8) {
        Ef a9;
        Ef ef = a9;
        Ef ef2 = a9;
        a9.k = a2;
        ef2.G = a3;
        ef2.i = a4;
        ef.e = a5;
        ef.I = a6;
        a9.F = a7;
        a9.D = a8;
    }
}

