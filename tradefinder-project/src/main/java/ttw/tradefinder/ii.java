/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Wb
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme$ColorDescription
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme$ColorIntervalResponse
 */
package ttw.tradefinder;

import java.awt.Color;
import java.util.Collections;
import ttw.tradefinder.Bc;
import ttw.tradefinder.H;
import ttw.tradefinder.Wb;
import ttw.tradefinder.dE;
import velox.api.layer1.messages.indicators.IndicatorColorScheme;

public class ii
implements IndicatorColorScheme {
    public final /* synthetic */ Wb G;
    public final /* synthetic */ H D;

    public IndicatorColorScheme.ColorDescription[] getColors() {
        ii a2;
        return Collections.singletonList(new IndicatorColorScheme.ColorDescription(a2.D.getClass(), dE.A((Object)"?&=\".//"), Color.WHITE, false)).toArray(new IndicatorColorScheme.ColorDescription[0]);
    }

    public String getColorFor(Double a2) {
        return Bc.A((Object)"6\u001c4\u0018'\u0015&");
    }

    public IndicatorColorScheme.ColorIntervalResponse getColorIntervalsList(double a2, double a3) {
        return new IndicatorColorScheme.ColorIntervalResponse(new String[]{dE.A((Object)"?&=\".//")}, new double[0]);
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ ii(Wb a2, H a3) {
        ii a4;
        a4.G = a2;
        a4.D = a3;
    }
}

