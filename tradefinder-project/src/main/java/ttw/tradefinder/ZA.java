/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.ZA
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.wc
 */
package ttw.tradefinder;

import java.awt.Dialog;
import java.awt.Window;
import java.awt.event.ComponentAdapter;
import java.awt.event.ComponentEvent;
import javax.swing.JDialog;
import javax.swing.SwingUtilities;
import ttw.tradefinder.rH;
import ttw.tradefinder.wc;

public class ZA
extends ComponentAdapter {
    public final /* synthetic */ wc D;

    @Override
    public void componentHidden(ComponentEvent a2) {
        ZA a3;
        ZA zA = a3;
        SwingUtilities.invokeLater(() -> {
            ZA a3;
            Object object = a3.D.I.G;
            synchronized (object) {
                if (a3.D.I.D.containsKey(((rH)a2).G)) {
                    zA.D.k = (JDialog)a3.D.I.D.remove(((rH)a2).G);
                    ((Dialog)zA.D.k).setVisible(false);
                    ((Window)zA.D.k).dispose();
                }
                return;
            }
        });
    }

    public /* synthetic */ ZA(wc a2) {
        ZA a3;
        a3.D = a2;
    }
}

