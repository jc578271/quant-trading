/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Hd
 *  ttw.tradefinder.RA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.ZA
 *  ttw.tradefinder.nC
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.wc
 */
package ttw.tradefinder;

import java.awt.Container;
import java.awt.Dialog;
import java.awt.Dimension;
import java.awt.Window;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.ComponentListener;
import javax.swing.JDialog;
import javax.swing.JFrame;
import javax.swing.JOptionPane;
import ttw.tradefinder.Hd;
import ttw.tradefinder.RA;
import ttw.tradefinder.YD;
import ttw.tradefinder.ZA;
import ttw.tradefinder.cF;
import ttw.tradefinder.nC;
import ttw.tradefinder.rH;

public class wc
implements ActionListener {
    public final /* synthetic */ rH k;
    public final /* synthetic */ Hd I;
    public final /* synthetic */ nC G;
    public final /* synthetic */ YD D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        wc a3;
        a2 = a3.I.G;
        synchronized (a2) {
            if (a3.I.D.containsKey(a3.k.G)) {
                JDialog jDialog = (JDialog)a3.I.D.get(a3.k.G);
                jDialog.setVisible(true);
                jDialog.toFront();
                return;
            }
        }
        a2 = new JOptionPane();
        Window window = new JFrame();
        window.setType(Window.Type.UTILITY);
        JFrame jFrame = window;
        jFrame.setUndecorated(true);
        window = ((JOptionPane)a2).createDialog(jFrame, a3.k.G + " - Live View");
        wc wc2 = a3;
        a2 = new cF(wc2.k, wc2.G, (float)((RA)a3.D.I).D / 10.0f);
        Object object = a3.I.G;
        synchronized (object) {
            a3.I.D.put(a3.k.G, window);
            Window window2 = window;
            Window window3 = window;
            ((JDialog)window).setContentPane((Container)a2);
            window3.addComponentListener((ComponentListener)new ZA(a3));
            a2 = a2.getLayout().preferredLayoutSize(window);
            window2.setSize(new Dimension(Math.max(((Dimension)a2).width, 260), ((Dimension)a2).height));
            ((Dialog)window3).setModalityType(Dialog.ModalityType.MODELESS);
            window2.setModalExclusionType(Dialog.ModalExclusionType.APPLICATION_EXCLUDE);
            ((JDialog)window2).setDefaultCloseOperation(1);
            window2.setAlwaysOnTop(true);
            ((Dialog)window2).setVisible(true);
            window2.toFront();
            return;
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ wc(Hd a2, rH a3, nC a4, YD a5) {
        wc a6;
        wc wc2 = a6;
        wc2.I = a2;
        wc2.k = a3;
        a6.G = a4;
        a6.D = a5;
    }
}

