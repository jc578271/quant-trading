/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.jg
 *  ttw.tradefinder.yE
 */
package ttw.tradefinder;

import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import ttw.tradefinder.dD;
import ttw.tradefinder.yE;

public class jg
implements ItemListener {
    public final /* synthetic */ yE D;

    @Override
    public void itemStateChanged(ItemEvent a2) {
        jg a3;
        if (a3.D.m.isSelected()) {
            a3.D.e.A(dD.D);
        }
    }

    public /* synthetic */ jg(yE a2) {
        jg a3;
        a3.D = a2;
    }
}

