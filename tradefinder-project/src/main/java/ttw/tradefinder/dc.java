/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Hd
 *  ttw.tradefinder.RA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.dc
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import javax.swing.JDialog;
import javax.swing.JSlider;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;
import ttw.tradefinder.Hd;
import ttw.tradefinder.RA;
import ttw.tradefinder.YD;
import ttw.tradefinder.rH;
import ttw.tradefinder.yf;

public class dc
implements ChangeListener {
    public final /* synthetic */ YD I;
    public final /* synthetic */ Hd G;
    public final /* synthetic */ rH D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ dc(Hd a2, YD a3, rH a4) {
        dc a5;
        a5.G = a2;
        a5.I = a3;
        a5.D = a4;
    }

    @Override
    public void stateChanged(ChangeEvent a222) {
        dc a3;
        int a222 = ((JSlider)(a222 = (JSlider)a222.getSource())).getValue();
        if (a222 != ((RA)a3.I.I).D) {
            ((RA)a3.I.I).D = a222;
            dc dc2 = a3;
            dc dc3 = a3;
            dc2.G.A().A(dc3.G.A().g(), a3.D.G, a3.I);
            dc2.I.A(yf.A);
            Object a222 = dc3.G.G;
            synchronized (a222) {
                if (a3.G.D.containsKey(a3.D.G)) {
                    ((JDialog)a3.G.D.get(a3.D.G)).pack();
                }
                return;
            }
        }
    }
}

