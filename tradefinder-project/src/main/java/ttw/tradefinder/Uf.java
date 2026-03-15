/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Uf
 *  ttw.tradefinder.yE
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import javax.swing.JCheckBox;
import ttw.tradefinder.yE;

public class Uf
implements ItemListener {
    public final /* synthetic */ JCheckBox G;
    public final /* synthetic */ yE D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        Uf a3;
        if (a3.G.isSelected() != a3.D.e.A()) {
            Uf uf2 = a3;
            a3.D.e.A(uf2.G.isSelected());
            uf2.D.j();
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Uf(yE a2, JCheckBox a3) {
        Uf a4;
        a4.D = a2;
        a4.G = a3;
    }
}

