/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.O
 *  ttw.tradefinder.vA
 */
package ttw.tradefinder;

import javax.swing.JSlider;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;
import ttw.tradefinder.DC;
import ttw.tradefinder.O;

public class vA
implements ChangeListener {
    public final /* synthetic */ DC G;
    public final /* synthetic */ O D;

    @Override
    public void stateChanged(ChangeEvent a22) {
        vA a3;
        int a22 = ((JSlider)(a22 = (JSlider)a22.getSource())).getValue();
        if (a22 != a3.D.f()) {
            vA vA2 = a3;
            vA2.D.f(a22);
            vA2.G.D.repaint();
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ vA(DC a2, O a3) {
        vA a4;
        a4.G = a2;
        a4.D = a3;
    }
}

