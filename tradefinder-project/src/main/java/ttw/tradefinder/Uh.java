/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.JA
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme$ColorDescription
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme$ColorIntervalResponse
 */
package ttw.tradefinder;

import java.awt.Color;
import java.util.ArrayList;
import java.util.Iterator;
import ttw.tradefinder.JA;
import velox.api.layer1.messages.indicators.IndicatorColorScheme;

public class Uh
implements IndicatorColorScheme {
    public final /* synthetic */ JA D;

    public IndicatorColorScheme.ColorIntervalResponse getColorIntervalsList(double a2, double a3) {
        Uh a4;
        Uh uh = a4;
        return uh.D.A(uh.D.D, a2, a3);
    }

    public IndicatorColorScheme.ColorDescription[] getColors() {
        Uh a2;
        Iterator iterator;
        ArrayList<IndicatorColorScheme.ColorDescription> arrayList = new ArrayList<IndicatorColorScheme.ColorDescription>();
        Iterator iterator2 = iterator = a2.D.i.entrySet().iterator();
        while (iterator2.hasNext()) {
            IndicatorColorScheme.ColorDescription colorDescription = iterator.next();
            colorDescription = new IndicatorColorScheme.ColorDescription(a2.D.A().getClass(), (String)colorDescription.getKey(), (Color)colorDescription.getValue(), false);
            iterator2 = iterator;
            arrayList.add(colorDescription);
        }
        return arrayList.toArray(new IndicatorColorScheme.ColorDescription[0]);
    }

    public String getColorFor(Double a2) {
        Uh a3;
        Uh uh = a3;
        return uh.D.A(uh.D.D, a2);
    }

    public /* synthetic */ Uh(JA a2) {
        Uh a3;
        a3.D = a2;
    }
}

