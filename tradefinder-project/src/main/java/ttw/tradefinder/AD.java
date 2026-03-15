/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AD
 *  ttw.tradefinder.ED
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.rH
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.JComboBox;
import ttw.tradefinder.ED;
import ttw.tradefinder.YD;
import ttw.tradefinder.fe;
import ttw.tradefinder.go;
import ttw.tradefinder.oc;
import ttw.tradefinder.rH;

public class AD
implements ActionListener {
    public final /* synthetic */ int e;
    public final /* synthetic */ rH i;
    public final /* synthetic */ fe k;
    public final /* synthetic */ String I;
    public final /* synthetic */ YD G;
    public final /* synthetic */ JComboBox D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ AD(fe a2, JComboBox a3, String a4, YD a5, rH a6, int a7) {
        AD a8;
        AD aD = a8;
        AD aD2 = a8;
        aD2.k = a2;
        aD2.D = a3;
        aD.I = a4;
        aD.G = a5;
        a8.i = a6;
        a8.e = a7;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        AD a3;
        if (!a3.D.getSelectedItem().toString().equals(a3.I)) {
            AD aD;
            AD aD2 = a3;
            if (a3.D.getSelectedItem().toString().equals(go.A("\u0015d8"))) {
                ((oc)aD2.G.I).e.G = false;
                aD = a3;
            } else {
                ((oc)aD2.G.I).e.G = Integer.parseInt(a3.D.getSelectedItem().toString());
                aD = a3;
            }
            aD.k.A().A(ED.A((Object)"x\\{%xzIfHIBi@qVm^"), a3.i.G, Integer.toString(a3.e), a3.G);
        }
    }
}

