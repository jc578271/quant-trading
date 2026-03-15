/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.ID
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.yE
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import ttw.tradefinder.dD;
import ttw.tradefinder.yE;

public class ID
implements ItemListener {
    public final /* synthetic */ yE D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        ID a3;
        if (a3.D.G.isSelected()) {
            a3.D.e.A(dD.i);
        }
    }

    public /* synthetic */ ID(yE a2) {
        ID a3;
        a3.D = a2;
    }
}

