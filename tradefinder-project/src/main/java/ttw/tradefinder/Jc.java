/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AC
 *  ttw.tradefinder.Jc
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import javax.swing.JCheckBox;
import javax.swing.JDialog;
import ttw.tradefinder.AC;
import ttw.tradefinder.Nc;
import ttw.tradefinder.YD;
import ttw.tradefinder.rH;
import ttw.tradefinder.tb;
import ttw.tradefinder.yf;

public class Jc
implements ItemListener {
    public final /* synthetic */ Nc i;
    public final /* synthetic */ YD k;
    public final /* synthetic */ JCheckBox I;
    public final /* synthetic */ AC G;
    public final /* synthetic */ rH D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        Jc a3;
        if (a3.I.isSelected() != ((tb)a3.k.I).a) {
            ((tb)a3.k.I).a = a3.I.isSelected();
            Jc jc2 = a3;
            jc2.i.A(((tb)jc2.k.I).a);
            Jc jc3 = a3;
            Jc jc4 = a3;
            jc3.G.A().A(jc4.G.A().g(), a3.D.G, a3.k);
            jc3.k.A(yf.J);
            if (!((tb)jc4.k.I).a) {
                a2 = a3.G.k;
                synchronized (a2) {
                    if (a3.G.I.containsKey(a3.D.G)) {
                        JDialog jDialog = (JDialog)a3.G.I.get(a3.D.G);
                        jDialog.setVisible(false);
                    }
                    return;
                }
            }
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Jc(AC a2, JCheckBox a3, YD a4, Nc a5, rH a6) {
        Jc a7;
        Jc jc2 = a7;
        a7.G = a2;
        jc2.I = a3;
        jc2.k = a4;
        a7.i = a5;
        a7.D = a6;
    }
}

