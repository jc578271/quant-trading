/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.G
 *  ttw.tradefinder.Gf
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Mf
 *  ttw.tradefinder.NC
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Te
 *  ttw.tradefinder.WD
 *  ttw.tradefinder.We
 *  ttw.tradefinder.YF
 *  ttw.tradefinder.kA
 *  ttw.tradefinder.la
 *  ttw.tradefinder.ma
 *  ttw.tradefinder.rE
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.sC
 *  ttw.tradefinder.uB
 *  ttw.tradefinder.yF
 *  velox.api.layer1.Layer1ApiProvider
 *  velox.api.layer1.annotations.Layer1ApiVersion
 *  velox.api.layer1.annotations.Layer1ApiVersionValue
 *  velox.api.layer1.annotations.Layer1Attachable
 *  velox.api.layer1.annotations.Layer1StrategyDateLicensed
 *  velox.api.layer1.annotations.Layer1StrategyName
 *  velox.api.layer1.annotations.UnrestrictedData
 */
package ttw.strategies;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import ttw.tradefinder.Fa;
import ttw.tradefinder.G;
import ttw.tradefinder.Gf;
import ttw.tradefinder.H;
import ttw.tradefinder.Mf;
import ttw.tradefinder.NC;
import ttw.tradefinder.Nc;
import ttw.tradefinder.Te;
import ttw.tradefinder.WD;
import ttw.tradefinder.We;
import ttw.tradefinder.YF;
import ttw.tradefinder.fe;
import ttw.tradefinder.kA;
import ttw.tradefinder.la;
import ttw.tradefinder.ma;
import ttw.tradefinder.q;
import ttw.tradefinder.rE;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import ttw.tradefinder.sC;
import ttw.tradefinder.uB;
import ttw.tradefinder.yF;
import velox.api.layer1.Layer1ApiProvider;
import velox.api.layer1.annotations.Layer1ApiVersion;
import velox.api.layer1.annotations.Layer1ApiVersionValue;
import velox.api.layer1.annotations.Layer1Attachable;
import velox.api.layer1.annotations.Layer1StrategyDateLicensed;
import velox.api.layer1.annotations.Layer1StrategyName;
import velox.api.layer1.annotations.UnrestrictedData;

@Layer1StrategyDateLicensed(value="MP.Indicators.495197.66.TTW-TrendAnalyzer")
@Layer1Attachable
@Layer1StrategyName(value="TTW-Trend Analyzer")
@Layer1ApiVersion(value=Layer1ApiVersionValue.VERSION2)
@UnrestrictedData
public class TrendAnalysis
extends Fa {
    public List<q> f() {
        TrendAnalysis a2;
        fe fe2 = new fe((H)a2);
        Object object = new WD(fe2);
        NC nC = new NC((H)a2, (la)object.A(rI.n));
        sC sC2 = new sC((H)a2, (la)object.A(rI.Ka), true);
        sC sC3 = new sC((H)a2, (la)object.A(rI.j), true);
        uB uB2 = new uB((H)a2, (la)object.A(rI.X), true);
        uB uB3 = new uB((H)a2, (la)object.A((rI)rI.i), true);
        uB2.A(true, false);
        uB3.A(true, false);
        uB uB4 = new uB((H)a2, (la)object.A(rI.d), true);
        uB uB5 = new uB((H)a2, (la)object.A((rI)rI.G), true);
        sC sC4 = new sC((H)a2, (la)object.A(rI.c), true);
        sC sC5 = new sC((H)a2, (la)object.A(rI.ha), true);
        uB uB6 = new uB((H)a2, (la)object.A((rI)rI.D), true);
        uB uB7 = new uB((H)a2, (la)object.A(rI.ka), true);
        uB6.A(true, false);
        uB7.A(true, false);
        uB uB8 = new uB((H)a2, (la)object.A(rI.Fa), true);
        uB uB9 = new uB((H)a2, (la)object.A(rI.Ha), true);
        We we = new We((H)a2, (ma)object.A(yF.c), 1 != 0, 1);
        we.A((q)((Object)fe2), Mf.d);
        we = new We((H)a2, (ma)object.A(yF.g), false, 1);
        we.A((q)((Object)fe2), Mf.x);
        We we2 = new We((H)a2, (ma)object.A(yF.I), true, 2);
        we2.A((q)((Object)fe2), Mf.Ba);
        We we3 = new We((H)a2, (ma)object.A(yF.Y), true, 3);
        we3.A((q)((Object)fe2), Mf.z);
        We we4 = we3;
        we4.A((q)sC3, Mf.k);
        we4.A((q)sC2, Mf.k);
        We we5 = new We((H)a2, (ma)object.A(yF.K), true, 4);
        we5.A((q)((Object)fe2), Mf.F);
        We we6 = we5;
        we6.A((q)uB3);
        we6.A((q)uB2);
        We we7 = new We((H)a2, (ma)object.A(yF.m), true, 5);
        we7.A((q)((Object)fe2), Mf.t);
        We we8 = we7;
        we7.A((q)uB5);
        we8.A((q)uB4);
        we8.A((q)((Object)fe2), Mf.p);
        We we9 = new We((H)a2, (ma)new Te(yF.b), 2, rE.k, Gf.J, fe2.A(1));
        we9.A(we);
        We we10 = we9;
        We we11 = we9;
        we11.A(we2);
        we11.A(we3);
        we10.A(we5);
        we10.A(we7);
        we = new We((H)a2, (ma)object.A(yF.S), false, 1);
        we.A((q)((Object)fe2), Mf.h);
        we2 = new We((H)a2, (ma)object.A(yF.U), true, 2);
        we2.A((q)((Object)fe2), Mf.Aa);
        we3 = new We((H)a2, (ma)object.A(yF.O), true, 3);
        we3.A((q)((Object)fe2), Mf.y);
        We we12 = we3;
        we12.A((q)sC5, Mf.k);
        we12.A((q)sC4, Mf.k);
        we5 = new We((H)a2, (ma)object.A(yF.e), true, 4);
        we5.A((q)((Object)fe2), Mf.e);
        We we13 = we5;
        we13.A((q)uB7);
        we13.A((q)uB6);
        object = new We((H)a2, (ma)object.A(yF.B), true, 5);
        object.A((q)((Object)fe2), Mf.w);
        WD wD = object;
        object.A((q)uB9);
        wD.A((q)uB8);
        wD.A((q)((Object)fe2), Mf.s);
        we7 = new We((H)a2, (ma)new Te(yF.D), 3, rE.k, Gf.J, fe2.A(2));
        we7.A(we);
        We we14 = we7;
        We we15 = we7;
        we15.A(we2);
        we15.A(we3);
        we14.A(we5);
        we14.A((We)object);
        object = new We((H)a2, (ma)new Te(yF.W), 2);
        object.A(we9, false);
        object.A(we7, false);
        object = new ArrayList<fe>();
        object.add(fe2);
        object.add(sC3);
        object.add(sC2);
        object.add(uB3);
        object.add(uB2);
        object.add(uB5);
        object.add(uB4);
        object.add(sC5);
        object.add(sC4);
        object.add(uB7);
        object.add(uB6);
        object.add(uB9);
        object.add(uB8);
        object.add(nC);
        return object;
    }

    public TrendAnalysis(Layer1ApiProvider a2) {
        super((G)new kA(), a2, TrendAnalysis.class, (la)WD.A());
        TrendAnalysis a3;
    }

    public void I() {
    }

    public Collection<? extends Nc> A(rH a2) {
        TrendAnalysis a3;
        ArrayList arrayList = new ArrayList();
        arrayList.addAll(a3.A().A(a2));
        arrayList.addAll(new YF((H)a3, a2).A(999));
        return arrayList;
    }
}

