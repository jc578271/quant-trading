/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.KC
 *  ttw.tradefinder.xC
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JButton;
import javax.swing.JOptionPane;
import ttw.tradefinder.U;
import ttw.tradefinder.xC;
import ttw.tradefinder.xc;

public class KC
implements ActionListener {
    public final /* synthetic */ JButton i;
    public final /* synthetic */ U k;
    public final /* synthetic */ String I;
    public final /* synthetic */ xC G;
    public final /* synthetic */ String D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        KC a3;
        KC kC = a3;
        JOptionPane.showMessageDialog(a3.i, new xc(kC.D, kC.I, a3.k), a3.I + " Settings", -1);
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ KC(xC a2, JButton a3, String a4, String a5, U a6) {
        KC a7;
        KC kC = a7;
        a7.G = a2;
        kC.i = a3;
        kC.D = a4;
        a7.I = a5;
        a7.k = a6;
    }
}

