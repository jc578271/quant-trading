/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.ab
 *  ttw.tradefinder.cb
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.Color;
import java.util.function.Consumer;
import ttw.tradefinder.Na;
import ttw.tradefinder.YD;
import ttw.tradefinder.ab;
import ttw.tradefinder.rH;
import ttw.tradefinder.yf;

public class cb
implements Consumer<Color> {
    public final /* synthetic */ Na k;
    public final /* synthetic */ ab I;
    public final /* synthetic */ rH G;
    public final /* synthetic */ YD D;

    public void A(Color a2) {
        cb a3;
        if (a3.k.D == a2) {
            return;
        }
        cb cb2 = a3;
        cb2.k.D = a2;
        cb cb3 = a3;
        cb2.I.A().A(a3.I.A().g(), cb3.G.G, a3.D);
        cb3.D.A(yf.ma);
        cb2.I.j(a3.G.G);
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ cb(ab a2, Na a3, rH a4, YD a5) {
        cb a6;
        cb cb2 = a6;
        cb2.I = a2;
        cb2.k = a3;
        a6.G = a4;
        a6.D = a5;
    }
}

