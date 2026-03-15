/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.ee
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.lf
 *  ttw.tradefinder.me
 *  ttw.tradefinder.rD
 *  ttw.tradefinder.rH
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JComboBox;
import ttw.tradefinder.Nc;
import ttw.tradefinder.YD;
import ttw.tradefinder.ee;
import ttw.tradefinder.jF;
import ttw.tradefinder.me;
import ttw.tradefinder.rD;
import ttw.tradefinder.rH;
import ttw.tradefinder.uC;

public class lf
implements ActionListener {
    public final /* synthetic */ rH i;
    public final /* synthetic */ rD k;
    public final /* synthetic */ JComboBox I;
    public final /* synthetic */ Nc G;
    public final /* synthetic */ YD D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ lf(rD a2, JComboBox a3, YD a4, rH a5, Nc a6) {
        lf a7;
        lf lf2 = a7;
        a7.k = a2;
        lf2.I = a3;
        lf2.D = a4;
        a7.i = a5;
        a7.G = a6;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        lf a3;
        a2 = jF.A((String)a3.I.getSelectedItem().toString(), (ee)ee.D);
        if (a2 != ((uC)a3.D.I).F) {
            lf lf2;
            boolean bl;
            ((uC)a3.D.I).F = a2;
            lf lf3 = a3;
            a3.k.A().A(me.A((Object)"\u0013\u000f\u0010v\u000f2#?\"5\b)#>5("), lf3.i.G, a3.D);
            Nc nc2 = lf3.G;
            if (((uC)a3.D.I).F != ee.D) {
                bl = true;
                lf2 = a3;
            } else {
                bl = false;
                lf2 = a3;
            }
            nc2.f(bl, lf2.k.G);
        }
    }
}

