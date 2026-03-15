/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.TA
 *  ttw.tradefinder.YA
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasIcon
 */
package ttw.tradefinder;

import java.util.function.BiConsumer;
import ttw.tradefinder.Na;
import ttw.tradefinder.YA;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;

public class TA
implements BiConsumer<Integer, Na> {
    public final /* synthetic */ YA D;

    public void A(Integer a2, Na a3) {
        TA a4;
        a2 = a4.D.F;
        synchronized (a2) {
            if (a4.D.G == null) {
                return;
            }
            TA tA = a4;
            a3 = tA.D.A(a3);
            tA.D.G.A((ScreenSpaceCanvas.CanvasIcon)a3);
            return;
        }
    }

    public /* synthetic */ TA(YA a2) {
        TA a3;
        a3.D = a2;
    }
}

