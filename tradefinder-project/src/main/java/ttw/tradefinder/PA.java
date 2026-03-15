/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Hd
 *  ttw.tradefinder.Kf
 *  ttw.tradefinder.PA
 *  ttw.tradefinder.RA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JComboBox;
import ttw.tradefinder.Hd;
import ttw.tradefinder.Kf;
import ttw.tradefinder.RA;
import ttw.tradefinder.YD;
import ttw.tradefinder.jF;
import ttw.tradefinder.rH;
import ttw.tradefinder.yf;

public class PA
implements ActionListener {
    public final /* synthetic */ rH k;
    public final /* synthetic */ Hd I;
    public final /* synthetic */ JComboBox G;
    public final /* synthetic */ YD D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        PA a3;
        a2 = jF.A((String)a3.G.getSelectedItem().toString(), (Kf)Kf.D);
        if (a2 != ((RA)a3.D.I).G) {
            ((RA)a3.D.I).G = a2;
            PA pA = a3;
            a3.I.A().A(a3.I.A().g(), pA.k.G, a3.D);
            pA.D.A(yf.Q);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ PA(Hd a2, JComboBox a3, YD a4, rH a5) {
        PA a6;
        PA pA = a6;
        pA.I = a2;
        pA.G = a3;
        a6.D = a4;
        a6.k = a5;
    }
}

