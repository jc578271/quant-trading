/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AE
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import javax.swing.JCheckBox;
import ttw.tradefinder.Nc;
import ttw.tradefinder.YD;
import ttw.tradefinder.fe;
import ttw.tradefinder.ha;
import ttw.tradefinder.oc;
import ttw.tradefinder.rH;
import ttw.tradefinder.yf;

public class AE
implements ItemListener {
    public final /* synthetic */ rH e;
    public final /* synthetic */ fe i;
    public final /* synthetic */ int k;
    public final /* synthetic */ JCheckBox I;
    public final /* synthetic */ Nc G;
    public final /* synthetic */ YD D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ AE(fe a2, JCheckBox a3, YD a4, Nc a5, rH a6, int a7) {
        AE a8;
        AE aE = a8;
        AE aE2 = a8;
        aE2.i = a2;
        aE2.I = a3;
        aE.D = a4;
        aE.G = a5;
        a8.e = a6;
        a8.k = a7;
    }

    @Override
    public void itemStateChanged(ItemEvent a2) {
        AE a3;
        if (a3.I.isSelected() != ((oc)a3.D.I).i) {
            ((oc)a3.D.I).i = a3.I.isSelected();
            AE aE = a3;
            aE.G.A(((oc)aE.D.I).i);
            AE aE2 = a3;
            a3.i.A().A(ha.A((Object)"132J1\u0015\u0000\t\u0001&\u000b\u0006\t\u001e\u001f\u0002\u0017"), aE2.e.G, Integer.toString(a3.k), a3.D);
            aE2.D.A(yf.V);
        }
    }
}

