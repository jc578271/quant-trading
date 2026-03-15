/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Pc
 *  ttw.tradefinder.m
 *  velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable$Marker
 */
package ttw.tradefinder;

import ttw.tradefinder.GB;
import ttw.tradefinder.Pc;
import velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable;

public interface m {
    default public OnlineCalculatable.Marker A(GB a2) {
        return null;
    }

    default public OnlineCalculatable.Marker A(Pc a2) {
        return null;
    }
}

