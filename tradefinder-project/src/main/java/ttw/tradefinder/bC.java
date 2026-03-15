/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AC
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.bC
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JComboBox;
import javax.swing.JDialog;
import ttw.tradefinder.AC;
import ttw.tradefinder.Nc;
import ttw.tradefinder.SE;
import ttw.tradefinder.YD;
import ttw.tradefinder.jF;
import ttw.tradefinder.rH;
import ttw.tradefinder.tb;
import ttw.tradefinder.yf;

public class bC
implements ActionListener {
    public final /* synthetic */ Nc i;
    public final /* synthetic */ AC k;
    public final /* synthetic */ JComboBox I;
    public final /* synthetic */ YD G;
    public final /* synthetic */ rH D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ bC(AC a2, JComboBox a3, YD a4, rH a5, Nc a6) {
        bC a7;
        bC bC2 = a7;
        a7.k = a2;
        bC2.I = a3;
        bC2.G = a4;
        a7.D = a5;
        a7.i = a6;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        bC a3;
        a2 = jF.A((String)a3.I.getSelectedItem().toString(), (SE)SE.D);
        if (a2 != ((tb)a3.G.I).K) {
            bC bC2;
            boolean bl;
            bC bC3;
            ((tb)a3.G.I).K = a2;
            a3.k.A().A(a3.k.A().g(), a3.D.G, a3.G);
            if (a2 == SE.I) {
                a2 = a3.k.k;
                synchronized (a2) {
                    if (a3.k.I.containsKey(a3.D.G)) {
                        JDialog jDialog = (JDialog)a3.k.I.remove(a3.D.G);
                        jDialog.setVisible(false);
                        jDialog.dispose();
                    }
                    // MONITOREXIT @DISABLED, blocks:[0, 1, 7, 8] lbl13 : MonitorExitStatement: MONITOREXIT : a
                    bC3 = a3;
                }
            } else {
                bC3 = a3;
            }
            bC3.G.A(yf.Ea);
            bC bC4 = a3;
            if (((tb)bC4.G.I).K != SE.I) {
                bl = true;
                bC2 = a3;
            } else {
                bl = false;
                bC2 = a3;
            }
            bC4.i.f(bl, bC2.k.G);
        }
    }
}

