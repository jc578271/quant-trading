/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.KD
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.yE
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import ttw.tradefinder.dD;
import ttw.tradefinder.yE;

public class KD
implements ItemListener {
    public final /* synthetic */ yE D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        KD a3;
        if (a3.D.i.isSelected()) {
            a3.D.e.A(dD.I);
        }
    }

    public /* synthetic */ KD(yE a2) {
        KD a3;
        a3.D = a2;
    }
}

