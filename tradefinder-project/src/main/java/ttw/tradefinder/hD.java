/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.Va
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.YF
 *  ttw.tradefinder.hD
 *  ttw.tradefinder.jF
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JComboBox;
import ttw.tradefinder.SE;
import ttw.tradefinder.Va;
import ttw.tradefinder.YD;
import ttw.tradefinder.YF;
import ttw.tradefinder.jF;

public class hD
implements ActionListener {
    public final /* synthetic */ YF I;
    public final /* synthetic */ JComboBox G;
    public final /* synthetic */ YD D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ hD(YF a2, JComboBox a3, YD a4) {
        hD a5;
        a5.I = a2;
        a5.G = a3;
        a5.D = a4;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        hD a3;
        a2 = jF.A((String)a3.G.getSelectedItem().toString(), (SE)SE.D);
        if (a2 != ((Va)a3.D.I).D) {
            ((Va)a3.D.I).D = a2;
            hD hD2 = a3;
            a3.I.k.A(a3.I.k.A(), hD2.D);
            hD2.I.k.A();
        }
    }
}

