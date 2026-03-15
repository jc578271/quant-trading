/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ec
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.Vc
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import javax.swing.JCheckBox;
import ttw.tradefinder.Na;
import ttw.tradefinder.Vc;
import ttw.tradefinder.yf;

public class Ec
implements ItemListener {
    public final /* synthetic */ Na I;
    public final /* synthetic */ JCheckBox G;
    public final /* synthetic */ Vc D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        Ec a3;
        if (a3.G.isSelected() != a3.I.m) {
            Ec ec = a3;
            Ec ec2 = a3;
            ec.I.m = ec2.G.isSelected();
            ec.D.G.A(a3.D.I, a3.D.D.G, a3.D.k);
            ec2.D.k.A(yf.ma);
            ec.D.G.A();
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Ec(Vc a2, JCheckBox a3, Na a4) {
        Ec a5;
        a5.D = a2;
        a5.G = a3;
        a5.I = a4;
    }
}

