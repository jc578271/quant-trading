/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Kg
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.PF
 *  ttw.tradefinder.Wa
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.iD
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import javax.swing.JCheckBox;
import ttw.tradefinder.Nc;
import ttw.tradefinder.PF;
import ttw.tradefinder.Wa;
import ttw.tradefinder.YD;
import ttw.tradefinder.iD;
import ttw.tradefinder.rH;
import ttw.tradefinder.yf;

public class Kg
implements ItemListener {
    public final /* synthetic */ Nc i;
    public final /* synthetic */ YD k;
    public final /* synthetic */ rH I;
    public final /* synthetic */ iD G;
    public final /* synthetic */ JCheckBox D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Kg(iD a2, JCheckBox a3, YD a4, Nc a5, rH a6) {
        Kg a7;
        Kg kg2 = a7;
        a7.G = a2;
        kg2.D = a3;
        kg2.k = a4;
        a7.i = a5;
        a7.I = a6;
    }

    @Override
    public void itemStateChanged(ItemEvent a2) {
        Kg a3;
        if (a3.D.isSelected() != ((Wa)a3.k.I).d) {
            ((Wa)a3.k.I).d = a3.D.isSelected();
            Kg kg2 = a3;
            kg2.i.A(((Wa)kg2.k.I).d);
            Kg kg3 = a3;
            a3.G.A().A(PF.A((Object)",e/\u001c4X\tD\u0011U\u0011E\u0001e\nP\u001bZ\u001dC"), kg3.I.G, a3.k);
            kg3.k.A(yf.H);
        }
    }
}

