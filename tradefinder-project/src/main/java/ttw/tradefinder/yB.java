/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ub
 *  ttw.tradefinder.bB
 *  ttw.tradefinder.yB
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JButton;
import javax.swing.JOptionPane;
import ttw.tradefinder.T;
import ttw.tradefinder.Ub;
import ttw.tradefinder.bB;

public class yB
implements ActionListener {
    public final /* synthetic */ JButton i;
    public final /* synthetic */ T k;
    public final /* synthetic */ bB I;
    public final /* synthetic */ String G;
    public final /* synthetic */ String D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ yB(bB a2, JButton a3, String a4, String a5, T a6) {
        yB a7;
        yB yB2 = a7;
        a7.I = a2;
        yB2.i = a3;
        yB2.G = a4;
        a7.D = a5;
        a7.k = a6;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        yB a3;
        yB yB2 = a3;
        JOptionPane.showMessageDialog(a3.i, new Ub(yB2.G, yB2.D, a3.k), a3.D + " Marker Settings", -1);
    }
}

