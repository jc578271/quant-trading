/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ce
 *  ttw.tradefinder.JA
 *  ttw.tradefinder.PF
 *  velox.api.layer1.messages.indicators.ValuesFormatter
 */
package ttw.tradefinder;

import ttw.tradefinder.Ce;
import ttw.tradefinder.JA;
import ttw.tradefinder.PF;
import velox.api.layer1.messages.indicators.ValuesFormatter;

public class xi
implements ValuesFormatter {
    public final /* synthetic */ JA D;

    public String formatWidget(double a2) {
        xi a3;
        if (Double.isNaN(a2)) {
            return PF.A((Object)"\u001cU\u001c");
        }
        return a3.D.G.format(a2);
    }

    public /* synthetic */ xi(JA a2) {
        xi a3;
        a3.D = a2;
    }

    public String formatTooltip(double a2, double a3, double a4, int a5) {
        xi a6;
        if (Double.isNaN(a2)) {
            return Ce.A((Object)"\u000f}\u000f");
        }
        return a6.D.G.format(a2);
    }
}

