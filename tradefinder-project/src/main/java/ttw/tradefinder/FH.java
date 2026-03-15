/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.ga
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import ttw.tradefinder.ga;
import ttw.tradefinder.xg;

public class FH
implements ItemListener {
    public final /* synthetic */ JButton k;
    public final /* synthetic */ JCheckBox I;
    public final /* synthetic */ ga G;
    public final /* synthetic */ xg D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        FH a3;
        if (a3.I.isSelected() != a3.G.A()) {
            FH fH = a3;
            a3.G.A(fH.I.isSelected());
            fH.k.setEnabled(a3.I.isSelected());
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ FH(xg a2, JCheckBox a3, ga a4, JButton a5) {
        FH a6;
        FH fH = a6;
        fH.D = a2;
        fH.I = a3;
        a6.G = a4;
        a6.k = a5;
    }
}

