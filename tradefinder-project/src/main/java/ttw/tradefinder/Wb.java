/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.D
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Wb
 *  ttw.tradefinder.la
 *  ttw.tradefinder.sB
 *  ttw.tradefinder.t
 *  velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener
 *  velox.api.layer1.layers.strategies.interfaces.InvalidateInterface
 *  velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable
 *  velox.api.layer1.layers.strategies.interfaces.OnlineValueCalculatorAdapter
 *  velox.api.layer1.messages.indicators.AliasFilter
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme
 *  velox.api.layer1.messages.indicators.IndicatorLineStyle
 *  velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator
 *  velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator$GraphType
 */
package ttw.tradefinder;

import java.awt.Color;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.function.Consumer;
import ttw.tradefinder.Bj;
import ttw.tradefinder.D;
import ttw.tradefinder.H;
import ttw.tradefinder.Ji;
import ttw.tradefinder.TI;
import ttw.tradefinder.bg;
import ttw.tradefinder.ii;
import ttw.tradefinder.kH;
import ttw.tradefinder.la;
import ttw.tradefinder.n;
import ttw.tradefinder.ng;
import ttw.tradefinder.sB;
import ttw.tradefinder.t;
import velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener;
import velox.api.layer1.layers.strategies.interfaces.InvalidateInterface;
import velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable;
import velox.api.layer1.layers.strategies.interfaces.OnlineValueCalculatorAdapter;
import velox.api.layer1.messages.indicators.AliasFilter;
import velox.api.layer1.messages.indicators.IndicatorColorScheme;
import velox.api.layer1.messages.indicators.IndicatorLineStyle;
import velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class Wb
extends sB
implements D,
OnlineCalculatable {
    private String m;
    private final t F;
    private Layer1ApiUserMessageModifyIndicator e;
    private Map<String, InvalidateInterface> i;
    private boolean k;
    private final Object I;
    public final la G;
    private boolean D;

    public void f() {
        Wb a2;
        Wb wb = a2;
        super.f();
        wb.A();
        wb.F.A();
        wb.i.clear();
    }

    public void f(String a2, boolean a3) {
        Wb a4;
        a2 = a4.I;
        synchronized (a2) {
            if (!a4.k) {
                return;
            }
            Wb wb = a4;
            wb.B();
            wb.I();
            return;
        }
    }

    public void calculateValuesInRange(String a2, String a3, long a4, long a5, int a6, CalculatedResultListener a7) {
        Wb a8;
        if (!a8.A(a2)) {
            a7.setCompleted();
        }
        if ((a2 = a8.A(a3)) == null || !(a2 instanceof n)) {
            a7.setCompleted();
            return;
        }
        ((n)((Object)a2)).A(a4, a5, a6, a7);
    }

    public void I(String a2) {
        Wb a3;
        if ((a2 = (InvalidateInterface)a3.i.get(a2)) != null) {
            a2.invalidate();
        }
    }

    private /* synthetic */ void B() {
        Wb a2;
        a2.k = false;
        if (!a2.D) {
            return;
        }
        a2.A().A((Object)new Layer1ApiUserMessageModifyIndicator(a2.e, false));
        a2.D = false;
    }

    public int A() {
        return -1;
    }

    private /* synthetic */ void I() {
        Wb a2;
        if (!a2.A().A() && !a2.G.A()) {
            return;
        }
        a2.k = true;
        if (!a2.F.A((bg)bg.G)) {
            return;
        }
        if (a2.D) {
            return;
        }
        Wb wb = a2;
        List list = wb.A(wb.A().A(), a2.a());
        if (list.isEmpty()) {
            return;
        }
        Wb wb2 = a2;
        Wb wb3 = a2;
        wb2.e = wb2.A(wb3.G, a2.A(), (AliasFilter)new kH(list), true);
        wb3.m = wb3.e.fullName;
        wb2.A().A((Object)a2.e);
        a2.D = true;
    }

    public Set<String> A() {
        Wb a2;
        return Collections.singleton(a2.m);
    }

    public void A() {
        Wb a2;
        Object object = a2.I;
        synchronized (object) {
            a2.B();
            return;
        }
    }

    public boolean A(String a2) {
        Wb a3;
        return a2.equals(a3.m);
    }

    private /* synthetic */ List<String> A(List<String> a2, List<String> a3) {
        ArrayList<String> arrayList = new ArrayList<String>();
        a3 = a3.iterator();
        while (a3.hasNext()) {
            String string = (String)a3.next();
            if (!a2.contains(string)) continue;
            arrayList.add(string);
        }
        return arrayList;
    }

    public Wb(H a2, la a3) {
        Wb a4;
        Wb wb = a4;
        Wb wb2 = a4;
        Wb wb3 = a4;
        super(a2, a3.C(), a3.A());
        wb3.I = new Object();
        wb3.i = Collections.synchronizedMap(new HashMap());
        wb3.k = false;
        wb2.D = false;
        wb2.m = "";
        wb.F = a3.f(a2.A()).A((D)a4);
        wb.G = a3;
    }

    public void a() {
        Wb a2;
        Object object = a2.I;
        synchronized (object) {
            a2.I();
            return;
        }
    }

    public Layer1ApiUserMessageModifyIndicator A(la a2, H a32, AliasFilter a4, boolean a5) {
        Wb a6;
        a2 = new Layer1ApiUserMessageModifyIndicator(a32.getClass(), a6.A(), a5, (IndicatorColorScheme)new ii(a6, a32), null, IndicatorLineStyle.NONE, Color.white, Color.black, null, null, null, null, null, Layer1ApiUserMessageModifyIndicator.GraphType.PRIMARY, Boolean.FALSE, Boolean.FALSE, Boolean.TRUE, (OnlineCalculatable)a6, a4);
        int a32 = a6.A();
        if (a32 != -1) {
            a2.graphLayerRenderPriority = a32;
        }
        return a2;
    }

    public void A(String a2) {
        Wb a3;
        Wb wb = a3;
        super.A(a2);
        wb.I(a2);
        wb.i.remove(a2);
    }

    public OnlineValueCalculatorAdapter createOnlineValueCalculator(String a2, String a3, long a4, Consumer<Object> a5, InvalidateInterface a6) {
        Wb a7;
        if (!a7.A((String)a2)) {
            return new Bj(a7);
        }
        if (!a7.A().isStrategyEnabled(a3)) {
            return new Ji(a7);
        }
        Wb wb = a7;
        a2 = wb.A(wb.A().A(a3), a7.A().A(a3));
        if (!(a2 instanceof n)) {
            return new TI(a7);
        }
        a2 = (n)a2;
        a7.i.put(a3, a6);
        a2.A(a5, a6);
        return new ng(a7);
    }

    public void A(t a2, boolean a3, boolean a4) {
        Wb a5;
        a2 = a5.I;
        synchronized (a2) {
            Object object;
            if (a3 && a5.k) {
                object = a2;
                a5.I();
            } else {
                if (!a3 && a5.D) {
                    a5.A().A((Object)new Layer1ApiUserMessageModifyIndicator(a5.e, false));
                    a5.D = false;
                }
                object = a2;
            }
            // ** MonitorExit[v0 /* !! */ ] (shouldn't be in output)
            return;
        }
    }
}

