/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.VC
 *  ttw.tradefinder.mc
 *  ttw.tradefinder.ra
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.util.function.BiConsumer;
import ttw.tradefinder.Na;
import ttw.tradefinder.VC;
import ttw.tradefinder.ra;
import ttw.tradefinder.yf;

public class mc
implements BiConsumer<Integer, Na> {
    public final /* synthetic */ VC D;

    public /* synthetic */ mc(VC a2) {
        mc a3;
        a3.D = a2;
    }

    public void A(Integer a2, Na a3) {
        mc a4;
        if (!a3.m) {
            return;
        }
        if (a3.I <= a4.D.K && a3.I >= a4.D.F) {
            ((ra)a4.D.G.I).A(a2.intValue());
            mc mc2 = a4;
            mc mc3 = a4;
            VC.A((VC)mc2.D).A(mc3.D.e, VC.f((VC)a4.D).G, a4.D.G);
            mc2.D.G.A(yf.ma);
            mc3.D.I.B(VC.A((VC)a4.D).G);
            return;
        }
        a4.D.i.A(a2.intValue(), a3, a4.D.K, a4.D.F);
    }
}

