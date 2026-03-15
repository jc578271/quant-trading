/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Bg
 *  ttw.tradefinder.Se
 *  ttw.tradefinder.if
 */
package ttw.tradefinder;

import java.util.function.Consumer;
import ttw.tradefinder.Se;
import ttw.tradefinder.if;

public class Bg
implements Consumer<String> {
    public final /* synthetic */ Se G;
    public final /* synthetic */ if D;

    public void A(String a2) {
        Bg a3;
        Bg bg2 = a3;
        bg2.D.A(a2);
        bg2.G.repaint();
        bg2.G.revalidate();
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Bg(Se a2, if a3) {
        Bg a4;
        a4.G = a2;
        a4.D = a3;
    }
}

