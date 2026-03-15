/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.JA
 *  velox.api.layer1.messages.indicators.HorizontalValueLinesInfo
 */
package ttw.tradefinder;

import java.util.Map;
import ttw.tradefinder.JA;
import velox.api.layer1.messages.indicators.HorizontalValueLinesInfo;

public class VG
implements HorizontalValueLinesInfo {
    public final /* synthetic */ JA D;

    public /* synthetic */ VG(JA a2) {
        VG a3;
        a3.D = a2;
    }

    public Map<Double, String> getHorizontalLines(String a2) {
        VG a3;
        VG vG = a3;
        return vG.D.A(vG.D.D, a2);
    }
}

