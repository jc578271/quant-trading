/*
 * Decompiled with CFR 0.152.
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JOptionPane;
import ttw.tradefinder.xg;

public class UI
implements ActionListener {
    public final /* synthetic */ JButton i;
    public final /* synthetic */ String k;
    public final /* synthetic */ String I;
    public final /* synthetic */ xg G;
    public final /* synthetic */ String D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        UI a3;
        UI uI = a3;
        UI uI2 = a3;
        JOptionPane.showMessageDialog(uI.i, uI.G.D.get(a3.k), uI2.D + ", " + uI2.I, -1, new ImageIcon(xg.k));
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ UI(xg a2, JButton a3, String a4, String a5, String a6) {
        UI a7;
        UI uI = a7;
        a7.G = a2;
        uI.i = a3;
        uI.k = a4;
        a7.D = a5;
        a7.I = a6;
    }
}

