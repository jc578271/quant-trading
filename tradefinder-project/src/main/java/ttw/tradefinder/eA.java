/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AB
 *  ttw.tradefinder.H
 *  ttw.tradefinder.IB
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Zc
 *  ttw.tradefinder.bI
 *  ttw.tradefinder.eA
 *  ttw.tradefinder.rH
 *  velox.api.layer1.messages.indicators.IndicatorLineStyle
 */
package ttw.tradefinder;

import ttw.tradefinder.AB;
import ttw.tradefinder.H;
import ttw.tradefinder.IB;
import ttw.tradefinder.JB;
import ttw.tradefinder.Q;
import ttw.tradefinder.YD;
import ttw.tradefinder.Zc;
import ttw.tradefinder.bI;
import ttw.tradefinder.c;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import velox.api.layer1.messages.indicators.IndicatorLineStyle;

public class eA
extends AB {
    private final rI k;
    private boolean I;
    private final YD<Zc, c> G;
    private final boolean D;

    public boolean isFEnabled() {
        eA a2;
        if (!a2.D) {
            return false;
        }
        return a2.I;
    }

    public IndicatorLineStyle getIndicatorLineStyle() {
        eA a2;
        return JB.A((int)((Zc)a2.G.I).G, (IB)((Zc)a2.G.I).D);
    }

    public void A(rI a2, boolean a3, boolean a4) {
        eA a5;
        if (a2 != a5.k) {
            return;
        }
        a5.A(a3, a4);
    }

    public void A(rI a2, double a3, long a4) {
        eA a5;
        if (a2 != a5.k) {
            return;
        }
        a5.A(a3, a4);
    }

    public eA(H a2, rH a3, YD<Zc, c> a4, rI a5, Q a6, boolean a7) {
        eA a8;
        eA eA2 = a8;
        eA eA3 = a8;
        super(a2, a3, a6);
        eA3.I = true;
        eA3.k = a5;
        eA2.G = a4;
        eA2.D = a7;
    }

    public void A(rI a2, bI a3, boolean a4) {
        eA a5;
        if (a2 != a5.k) {
            return;
        }
        if (a3 == bI.I) {
            a5.f(a4);
            return;
        }
        if (a3 == bI.k) {
            if (!a5.D) {
                return;
            }
            a5.I = a4;
            a5.A(false, true);
            return;
        }
        if (a3 == bI.G && a4) {
            a5.a();
        }
    }
}

