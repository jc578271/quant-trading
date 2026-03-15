/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.PC
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.ab
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.rH
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JComboBox;
import ttw.tradefinder.Na;
import ttw.tradefinder.SE;
import ttw.tradefinder.YD;
import ttw.tradefinder.ab;
import ttw.tradefinder.jF;
import ttw.tradefinder.rH;

public class PC
implements ActionListener {
    public final /* synthetic */ JComboBox i;
    public final /* synthetic */ ab k;
    public final /* synthetic */ Na I;
    public final /* synthetic */ rH G;
    public final /* synthetic */ YD D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        PC a3;
        a2 = jF.A((String)a3.i.getSelectedItem().toString(), (SE)SE.D);
        if (a2 != a3.I.G) {
            a3.I.G = a2;
            a3.k.A().A(a3.k.A().g(), a3.G.G, a3.D);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ PC(ab a2, JComboBox a3, Na a4, rH a5, YD a6) {
        PC a7;
        PC pC2 = a7;
        a7.k = a2;
        pC2.i = a3;
        pC2.I = a4;
        a7.G = a5;
        a7.D = a6;
    }
}

