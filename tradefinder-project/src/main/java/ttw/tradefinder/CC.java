/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.CC
 *  ttw.tradefinder.Hd
 *  ttw.tradefinder.RA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JComboBox;
import ttw.tradefinder.Hd;
import ttw.tradefinder.RA;
import ttw.tradefinder.YD;
import ttw.tradefinder.jF;
import ttw.tradefinder.mg;
import ttw.tradefinder.yf;

public class CC
implements ActionListener {
    public final /* synthetic */ Hd k;
    public final /* synthetic */ JComboBox I;
    public final /* synthetic */ String G;
    public final /* synthetic */ YD D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        CC a3;
        a2 = jF.A((String)a3.I.getSelectedItem().toString(), (mg)mg.G);
        if (a2 != ((RA)a3.D.I).k) {
            ((RA)a3.D.I).k = a2;
            CC cC = a3;
            a3.k.A().A(a3.k.A().g(), cC.G, a3.D);
            cC.D.A(yf.Q);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ CC(Hd a2, JComboBox a3, YD a4, String a5) {
        CC a6;
        CC cC = a6;
        cC.k = a2;
        cC.I = a3;
        a6.D = a4;
        a6.G = a5;
    }
}

