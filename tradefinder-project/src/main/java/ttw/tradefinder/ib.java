/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Cc
 *  ttw.tradefinder.Mc
 *  ttw.tradefinder.ib
 */
package ttw.tradefinder;

import java.util.TimerTask;
import ttw.tradefinder.Cc;
import ttw.tradefinder.Mc;

public class ib
extends TimerTask {
    public final /* synthetic */ Mc D;

    @Override
    public /* synthetic */ void run() {
        ib a2;
        boolean bl = false;
        if (++a2.D.k >= 10) {
            a2.D.k = 0;
            bl = true;
        }
        boolean bl2 = false;
        if (++a2.D.G >= 5) {
            a2.D.G = 0;
            bl2 = true;
        }
        for (Cc cc2 : a2.D.f.values()) {
            if (!bl && (!bl2 || !cc2.A())) continue;
            a2.D.g.A(cc2);
        }
    }

    public /* synthetic */ ib(Mc a2) {
        ib a3;
        a3.D = a2;
    }
}

