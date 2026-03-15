/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.VA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.hc
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.xb
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import javax.swing.JCheckBox;
import ttw.tradefinder.Nc;
import ttw.tradefinder.VA;
import ttw.tradefinder.YD;
import ttw.tradefinder.rH;
import ttw.tradefinder.xb;
import ttw.tradefinder.yf;

public class hc
implements ItemListener {
    public final /* synthetic */ Nc i;
    public final /* synthetic */ rH k;
    public final /* synthetic */ JCheckBox I;
    public final /* synthetic */ YD G;
    public final /* synthetic */ xb D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        hc a3;
        if (a3.I.isSelected() != ((VA)a3.G.I).D) {
            ((VA)a3.G.I).D = a3.I.isSelected();
            hc hc2 = a3;
            hc2.i.A(((VA)hc2.G.I).D);
            hc hc3 = a3;
            a3.D.A().A(a3.D.A().g(), hc3.k.G, a3.G);
            hc3.G.A(yf.f);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ hc(xb a2, JCheckBox a3, YD a4, Nc a5, rH a6) {
        hc a7;
        hc hc2 = a7;
        a7.D = a2;
        hc2.I = a3;
        hc2.G = a4;
        a7.i = a5;
        a7.k = a6;
    }
}

