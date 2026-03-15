/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Bi
 *  velox.api.layer1.messages.indicators.AliasFilter
 */
package ttw.tradefinder;

import velox.api.layer1.messages.indicators.AliasFilter;

public class Bi
implements AliasFilter {
    private final String D;

    public boolean isDisplayedForAlias(String a2) {
        Bi a3;
        return a3.D.equals(a2);
    }

    public Bi(String a2) {
        Bi a3;
        a3.D = a2;
    }

    public Bi() {
        a2("");
        Bi a2;
    }
}

