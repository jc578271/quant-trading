/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.O
 *  ttw.tradefinder.ob
 *  ttw.tradefinder.qd
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JButton;
import javax.swing.JOptionPane;
import ttw.tradefinder.DC;
import ttw.tradefinder.O;
import ttw.tradefinder.qd;

public class ob
implements ActionListener {
    public final /* synthetic */ qd i;
    public final /* synthetic */ JButton k;
    public final /* synthetic */ O I;
    public final /* synthetic */ String G;
    public final /* synthetic */ String D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ ob(qd a2, JButton a3, String a4, String a5, O a6) {
        ob a7;
        ob ob2 = a7;
        a7.i = a2;
        ob2.k = a3;
        ob2.G = a4;
        a7.D = a5;
        a7.I = a6;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        ob a3;
        ob ob2 = a3;
        ob ob3 = a3;
        JOptionPane.showMessageDialog(a3.k, new DC(ob2.G, ob2.D, ob3.I, ob3.i.G.A()), a3.D + " Marker Settings", -1);
    }
}

