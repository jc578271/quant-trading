/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  velox.api.layer1.messages.indicators.AliasFilter
 */
package ttw.tradefinder;

import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import velox.api.layer1.messages.indicators.AliasFilter;

public class kH
implements AliasFilter {
    private final Set<String> D;

    public kH(List<String> a2) {
        kH a3;
        kH kH2 = a3;
        kH2.D = new HashSet<String>();
        kH2.D.clear();
        kH2.D.addAll(a2);
    }

    public kH() {
        a2(Collections.emptyList());
        kH a2;
    }

    public boolean isDisplayedForAlias(String a2) {
        kH a3;
        return a3.D.contains(a2);
    }
}

