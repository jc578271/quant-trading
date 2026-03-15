/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AC
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.qA
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import javax.swing.JSlider;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;
import ttw.tradefinder.AC;
import ttw.tradefinder.YD;
import ttw.tradefinder.rH;
import ttw.tradefinder.tb;
import ttw.tradefinder.yf;

public class qA
implements ChangeListener {
    public final /* synthetic */ YD I;
    public final /* synthetic */ AC G;
    public final /* synthetic */ rH D;

    @Override
    public void stateChanged(ChangeEvent a22) {
        qA a3;
        int a22 = ((JSlider)(a22 = (JSlider)a22.getSource())).getValue();
        if (a22 != ((tb)a3.I.I).G) {
            ((tb)a3.I.I).G = a22;
            qA qA2 = a3;
            a3.G.A().A(a3.G.A().g(), qA2.D.G, a3.I);
            qA2.I.A(yf.Ja);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ qA(AC a2, YD a3, rH a4) {
        qA a5;
        a5.G = a2;
        a5.I = a3;
        a5.D = a4;
    }
}

