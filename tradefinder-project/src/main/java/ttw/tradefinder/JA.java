/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AB
 *  ttw.tradefinder.Bi
 *  ttw.tradefinder.D
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Hf
 *  ttw.tradefinder.JA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.fC
 *  ttw.tradefinder.la
 *  ttw.tradefinder.sB
 *  ttw.tradefinder.t
 *  ttw.tradefinder.ti
 *  velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener
 *  velox.api.layer1.layers.strategies.interfaces.InvalidateInterface
 *  velox.api.layer1.layers.strategies.interfaces.Layer1IndicatorColorInterface
 *  velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable
 *  velox.api.layer1.layers.strategies.interfaces.OnlineValueCalculatorAdapter
 *  velox.api.layer1.messages.indicators.AliasFilter
 *  velox.api.layer1.messages.indicators.IndicatorColorInterface
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme$ColorIntervalResponse
 *  velox.api.layer1.messages.indicators.IndicatorDisplayLogic
 *  velox.api.layer1.messages.indicators.IndicatorLineStyle
 *  velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator
 *  velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator$GraphType
 *  velox.api.layer1.messages.indicators.ValuesFormatter
 *  velox.api.layer1.messages.indicators.WidgetDisplayInfo
 *  velox.colors.ColorsChangedListener
 *  velox.gui.colors.ColorsConfigItem
 */
package ttw.tradefinder;

import java.awt.Color;
import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.function.Consumer;
import ttw.tradefinder.AB;
import ttw.tradefinder.Bi;
import ttw.tradefinder.D;
import ttw.tradefinder.H;
import ttw.tradefinder.Hf;
import ttw.tradefinder.Mh;
import ttw.tradefinder.NI;
import ttw.tradefinder.Uh;
import ttw.tradefinder.VG;
import ttw.tradefinder.WG;
import ttw.tradefinder.WH;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.bg;
import ttw.tradefinder.fC;
import ttw.tradefinder.gj;
import ttw.tradefinder.jh;
import ttw.tradefinder.la;
import ttw.tradefinder.n;
import ttw.tradefinder.nh;
import ttw.tradefinder.sB;
import ttw.tradefinder.t;
import ttw.tradefinder.ti;
import ttw.tradefinder.xi;
import velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener;
import velox.api.layer1.layers.strategies.interfaces.InvalidateInterface;
import velox.api.layer1.layers.strategies.interfaces.Layer1IndicatorColorInterface;
import velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable;
import velox.api.layer1.layers.strategies.interfaces.OnlineValueCalculatorAdapter;
import velox.api.layer1.messages.indicators.AliasFilter;
import velox.api.layer1.messages.indicators.IndicatorColorInterface;
import velox.api.layer1.messages.indicators.IndicatorColorScheme;
import velox.api.layer1.messages.indicators.IndicatorDisplayLogic;
import velox.api.layer1.messages.indicators.IndicatorLineStyle;
import velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator;
import velox.api.layer1.messages.indicators.ValuesFormatter;
import velox.api.layer1.messages.indicators.WidgetDisplayInfo;
import velox.colors.ColorsChangedListener;
import velox.gui.colors.ColorsConfigItem;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class JA
extends sB
implements D,
Layer1IndicatorColorInterface,
OnlineCalculatable {
    private final t a;
    private nh K;
    private final Map<String, Layer1ApiUserMessageModifyIndicator> m;
    private Set<String> F;
    private Map<String, InvalidateInterface> e;
    private Map<String, Color> i;
    private boolean k;
    private final Object I;
    private DecimalFormat G;
    public final la D;

    public Set<String> A() {
        JA a2;
        return a2.F;
    }

    public void A() {
        JA a2;
        Object object = a2.I;
        synchronized (object) {
            Iterator iterator;
            a2.k = false;
            Iterator iterator2 = iterator = a2.m.values().iterator();
            while (iterator2.hasNext()) {
                Layer1ApiUserMessageModifyIndicator layer1ApiUserMessageModifyIndicator = (Layer1ApiUserMessageModifyIndicator)iterator.next();
                iterator2 = iterator;
                a2.A().A((Object)new Layer1ApiUserMessageModifyIndicator(layer1ApiUserMessageModifyIndicator, false));
            }
            JA jA2 = a2;
            jA2.m.clear();
            jA2.F.clear();
            return;
        }
    }

    public void a() {
        JA a2;
        Object object = a2.I;
        synchronized (object) {
            if (!a2.A().A() && !a2.D.A()) {
                return;
            }
            a2.k = true;
            if (!a2.a.A((bg)bg.G)) {
                return;
            }
            if (a2.m.size() > 0) {
                return;
            }
            JA jA2 = a2;
            jA2.F.clear();
            Object object2 = jA2.A(jA2.A().A(), a2.a());
            if (object2.isEmpty()) {
                return;
            }
            Object object3 = object2 = object2.iterator();
            while (object3.hasNext()) {
                String string = (String)object2.next();
                object3 = object2;
                a2.I(string);
            }
            return;
        }
    }

    public ColorsConfigItem A(String a2, String a3) {
        JA a4;
        return a4.A(a2, a3, (ColorsChangedListener)new jh(a4));
    }

    public Layer1ApiUserMessageModifyIndicator A(String a2, boolean a32) {
        int n2;
        JA a4;
        JA jA2 = a4;
        Layer1ApiUserMessageModifyIndicator a32 = new Layer1ApiUserMessageModifyIndicator(a4.A().getClass(), a4.A(), a32, (IndicatorColorScheme)new Uh(a4), (Layer1IndicatorColorInterface)jA2, jA2.A(a2), Color.white, Color.black, new IndicatorDisplayLogic().setValuesFormatter((ValuesFormatter)new xi(a4)), null, null, null, null, a4.A(), Boolean.valueOf(a4.A()), Boolean.FALSE, Boolean.TRUE, (OnlineCalculatable)a4, (AliasFilter)new Bi(a2));
        new Layer1ApiUserMessageModifyIndicator(a4.A().getClass(), a4.A(), a32, (IndicatorColorScheme)new Uh(a4), (Layer1IndicatorColorInterface)jA2, jA2.A(a2), Color.white, Color.black, new IndicatorDisplayLogic().setValuesFormatter((ValuesFormatter)new xi(a4)), null, null, null, null, a4.A(), Boolean.valueOf(a4.A()), Boolean.FALSE, Boolean.TRUE, (OnlineCalculatable)a4, (AliasFilter)new Bi(a2)).horizontalValueLinesInfo = new VG(a4);
        WidgetDisplayInfo widgetDisplayInfo = a4.A();
        if (widgetDisplayInfo != null) {
            a32.widgetDisplayInfo = widgetDisplayInfo;
        }
        if ((n2 = a4.A()) != -1) {
            a32.graphLayerRenderPriority = n2;
        }
        a32.isWidgetEnabledByDefault = true;
        a32.applyNameModifier(a2);
        return a32;
    }

    public void calculateValuesInRange(String a2, String a3, long a4, long a5, int a6, CalculatedResultListener a7) {
        JA a8;
        if (!a8.A(a2)) {
            a7.setCompleted();
        }
        if ((a2 = a8.A(a3)) == null || !(a2 instanceof n)) {
            a7.setCompleted();
            return;
        }
        ((n)((Object)a2)).A(a4, a5, a6, a7);
    }

    public void setColor(String a2, String a3, Color a4) {
        JA a5;
        YD yD2 = a5.A().A(a5.D.g(), a2, (Ya)new Hf());
        ((Hf)yD2.I).A(a3, a4);
        a5.A().A(a5.D.g(), a2, yD2);
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

    public abstract WidgetDisplayInfo A();

    public boolean A(String a2) {
        JA a3;
        return a3.F.contains(a2);
    }

    public abstract IndicatorColorScheme.ColorIntervalResponse A(la var1, double var2, double var4);

    private /* synthetic */ void k(String a2) {
        JA a3;
        if (!a3.m.containsKey(a2)) {
            return;
        }
        a2 = (Layer1ApiUserMessageModifyIndicator)a3.m.remove(a2);
        a3.F.remove(((Layer1ApiUserMessageModifyIndicator)a2).fullName);
        a3.A().A((Object)new Layer1ApiUserMessageModifyIndicator((Layer1ApiUserMessageModifyIndicator)a2, false));
    }

    public void j(String a2) {
        JA a3;
        if ((a2 = (InvalidateInterface)a3.e.get(a2)) != null) {
            a2.invalidate();
        }
    }

    public ColorsConfigItem A(String a2, String a3, ColorsChangedListener a4) {
        JA a5;
        Color color = Color.WHITE;
        if (a5.i.containsKey(a3)) {
            color = (Color)a5.i.get(a3);
        }
        ti ti2 = new ti(a5, a2);
        ti2.addColorChangeListener(a4);
        String string = a3;
        return new ColorsConfigItem(string, string, false, color, (IndicatorColorInterface)ti2, (ColorsChangedListener)new gj(a5, a2));
    }

    public abstract Layer1ApiUserMessageModifyIndicator.GraphType A();

    public Color getColor(String a2, String a3) {
        JA a4;
        a2 = ((Hf)a4.A().A((String)a4.D.g(), (String)a2, (Ya)new Hf()).I).A(a3);
        if (a2 != null) {
            return a2;
        }
        a2 = (Color)a4.i.get(a3);
        if (a2 != null) {
            return a2;
        }
        return Color.WHITE;
    }

    public abstract boolean A();

    public void f(String a2, boolean a3) {
        JA a4;
        Object object = a4.I;
        synchronized (object) {
            Object object2;
            if (!a4.k) {
                return;
            }
            JA jA2 = a4;
            if (a3) {
                jA2.I(a2);
                object2 = object;
            } else {
                jA2.k(a2);
                object2 = object;
            }
            // ** MonitorExit[v1] (shouldn't be in output)
            return;
        }
    }

    public abstract List<fC> A(la var1);

    public int A() {
        return -1;
    }

    public abstract String A(la var1, Double var2);

    public JA(H a2, la a3) {
        JA a4;
        JA jA2 = a4;
        JA jA3 = a4;
        JA jA4 = a4;
        JA jA5 = a4;
        super((H)a2, a3.C(), a3.A());
        a4.I = new Object();
        jA5.e = Collections.synchronizedMap(new HashMap());
        jA5.m = new HashMap();
        jA5.k = false;
        jA4.F = Collections.synchronizedSet(new HashSet());
        jA3.K = null;
        jA4.i = new HashMap();
        jA3.G = new DecimalFormat(Ya.A((Object)"azaw"));
        jA2.a = a3.f(a2.A()).A((D)a4);
        jA2.D = a3;
        a2 = jA2.A(jA2.D);
        Object object = a2 = a2.iterator();
        while (object.hasNext()) {
            a3 = (fC)a2.next();
            object = a2;
            la la2 = a3;
            a4.i.put(la2.G, la2.D);
        }
    }

    public void A(String a2) {
        JA a3;
        JA jA2 = a3;
        super.A(a2);
        jA2.j(a2);
        jA2.e.remove(a2);
    }

    public nh A() {
        JA a2;
        return a2.K;
    }

    public void A(nh a2) {
        a.K = a2;
    }

    public Map<Double, String> A(la a2, String a3) {
        return new HashMap<Double, String>();
    }

    public boolean f(String a2) {
        JA a3;
        if ((a2 = a3.A(a2)) == null || !(a2 instanceof AB)) {
            return false;
        }
        return ((AB)a2).isFEnabled();
    }

    public void B(String a2) {
        JA a3;
        Object object = a3.I;
        synchronized (object) {
            if (!a3.m.containsKey(a2)) {
                return;
            }
            JA jA2 = a3;
            String string = a2;
            jA2.k(string);
            jA2.I(string);
            return;
        }
    }

    public void addColorChangeListener(ColorsChangedListener a2) {
    }

    public IndicatorLineStyle A(String a2) {
        JA a3;
        if ((a2 = a3.A(a2)) == null || !(a2 instanceof AB)) {
            return IndicatorLineStyle.DEFAULT;
        }
        return ((AB)a2).getIndicatorLineStyle();
    }

    public void A(t a2, boolean a3, boolean a4) {
        JA a5;
        if (a3 && a5.k) {
            a5.a();
            return;
        }
        if (!a3 && a5.m.size() > 0) {
            a5.A();
        }
    }

    private /* synthetic */ void I(String a2) {
        JA a3;
        if (a3.m.containsKey(a2)) {
            return;
        }
        JA jA2 = a3;
        Layer1ApiUserMessageModifyIndicator layer1ApiUserMessageModifyIndicator = jA2.A(a2, true);
        jA2.F.add(layer1ApiUserMessageModifyIndicator.fullName);
        a3.m.put(a2, layer1ApiUserMessageModifyIndicator);
        a3.A().A((Object)layer1ApiUserMessageModifyIndicator);
    }

    public OnlineValueCalculatorAdapter createOnlineValueCalculator(String a2, String a3, long a4, Consumer<Object> a5, InvalidateInterface a6) {
        JA a7;
        if (!a7.A((String)a2)) {
            return new WG(a7);
        }
        if (!a7.A().isStrategyEnabled(a3)) {
            return new WH(a7);
        }
        JA jA2 = a7;
        a2 = jA2.A(jA2.A().A(a3), a7.A().A(a3));
        if (!(a2 instanceof n)) {
            return new Mh(a7);
        }
        a2 = (n)a2;
        a7.e.put(a3, a6);
        a2.A(a5, a6);
        return new NI(a7);
    }

    public void f() {
        JA a2;
        JA jA2 = a2;
        super.f();
        jA2.A();
        jA2.a.A();
        jA2.e.clear();
    }
}

