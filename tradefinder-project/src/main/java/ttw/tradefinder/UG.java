/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.di
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import ttw.tradefinder.Jd;
import ttw.tradefinder.YD;
import ttw.tradefinder.di;

public class UG
implements ActionListener {
    public final /* synthetic */ di G;
    public final /* synthetic */ YD D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ UG(di a2, YD a3) {
        UG a4;
        a4.G = a2;
        a4.D = a3;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        UG a3;
        UG uG = a3;
        a3.G.K.A((String)((Jd)a3.D.I).k, uG.G.A((String)((Jd)uG.D.I).I));
    }
}

