/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.BA
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.G
 *  ttw.tradefinder.H
 *  ttw.tradefinder.J
 *  ttw.tradefinder.JA
 *  ttw.tradefinder.JE
 *  ttw.tradefinder.JH
 *  ttw.tradefinder.KF
 *  ttw.tradefinder.La
 *  ttw.tradefinder.Ma
 *  ttw.tradefinder.Mc
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.VE
 *  ttw.tradefinder.Va
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.ch
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.db
 *  ttw.tradefinder.fD
 *  ttw.tradefinder.jB
 *  ttw.tradefinder.la
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.vE
 *  ttw.tradefinder.w
 *  velox.api.layer1.Layer1ApiAdminAdapter
 *  velox.api.layer1.Layer1ApiFinishable
 *  velox.api.layer1.Layer1ApiInstrumentAdapter
 *  velox.api.layer1.Layer1ApiProvider
 *  velox.api.layer1.Layer1CustomPanelsGetter
 *  velox.api.layer1.LayerApiListenable
 *  velox.api.layer1.common.ListenableHelper
 *  velox.api.layer1.common.Log
 *  velox.api.layer1.common.NanoClock
 *  velox.api.layer1.data.InstrumentInfo
 *  velox.api.layer1.layers.strategies.interfaces.Layer1PriceAxisRangeCalculatable
 *  velox.api.layer1.layers.strategies.interfaces.Layer1PriceAxisRangeCalculatable$InputPriceAxisInfo
 *  velox.api.layer1.layers.strategies.interfaces.Layer1PriceAxisRangeCalculatable$ResultPriceAxisInfo
 *  velox.api.layer1.messages.CurrentTimeUserMessage
 *  velox.api.layer1.messages.GeneratedEventInfo
 *  velox.api.layer1.messages.Layer1ApiHistoricalDataLoadedMessage
 *  velox.api.layer1.messages.Layer1ApiRequestCurrentTimeEvents
 *  velox.api.layer1.messages.Layer1ApiSoundAlertMessage
 *  velox.api.layer1.messages.Layer1ApiUserMessageAddStrategyUpdateGenerator
 *  velox.api.layer1.messages.Layer1ApiUserMessageReloadStrategyGui
 *  velox.api.layer1.messages.UserMessageLayersChainCreatedTargeted
 *  velox.api.layer1.messages.UserMessageRewindBase
 *  velox.api.layer1.messages.indicators.SettingsAccess
 *  velox.api.layer1.messages.indicators.StrategyUpdateGenerator
 *  velox.api.layer1.settings.Layer1ConfigSettingsInterface
 *  velox.api.layer1.utils.IdHelper
 *  velox.api.layer1.utils.PriceRangeCalculationHelper
 *  velox.gui.StrategyPanel
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import ttw.tradefinder.BA;
import ttw.tradefinder.Di;
import ttw.tradefinder.Mc;
import ttw.tradefinder.Rh;
import ttw.tradefinder.UC;
import ttw.tradefinder.Va;
import ttw.tradefinder.db;
import ttw.tradefinder.ha;
import ttw.tradefinder.j;
import ttw.tradefinder.la;
import ttw.tradefinder.q;
import ttw.tradefinder.vE;
import velox.api.layer1.Layer1ApiAdminAdapter;
import velox.api.layer1.Layer1ApiFinishable;
import velox.api.layer1.Layer1ApiInstrumentAdapter;
import velox.api.layer1.Layer1ApiProvider;
import velox.api.layer1.Layer1CustomPanelsGetter;
import velox.api.layer1.LayerApiListenable;
import velox.api.layer1.common.ListenableHelper;
import velox.api.layer1.common.Log;
import velox.api.layer1.common.NanoClock;
import velox.api.layer1.data.InstrumentInfo;
import velox.api.layer1.layers.strategies.interfaces.Layer1PriceAxisRangeCalculatable;
import velox.api.layer1.messages.CurrentTimeUserMessage;
import velox.api.layer1.messages.GeneratedEventInfo;
import velox.api.layer1.messages.Layer1ApiHistoricalDataLoadedMessage;
import velox.api.layer1.messages.Layer1ApiRequestCurrentTimeEvents;
import velox.api.layer1.messages.Layer1ApiSoundAlertMessage;
import velox.api.layer1.messages.Layer1ApiUserMessageAddStrategyUpdateGenerator;
import velox.api.layer1.messages.Layer1ApiUserMessageReloadStrategyGui;
import velox.api.layer1.messages.UserMessageLayersChainCreatedTargeted;
import velox.api.layer1.messages.UserMessageRewindBase;
import velox.api.layer1.messages.indicators.SettingsAccess;
import velox.api.layer1.messages.indicators.StrategyUpdateGenerator;
import velox.api.layer1.settings.Layer1ConfigSettingsInterface;
import velox.api.layer1.utils.IdHelper;
import velox.api.layer1.utils.PriceRangeCalculationHelper;
import velox.gui.StrategyPanel;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class Fa
extends La
implements AutoCloseable,
J,
Layer1ApiAdminAdapter,
Layer1ApiFinishable,
Layer1ApiInstrumentAdapter,
Layer1CustomPanelsGetter,
Layer1PriceAxisRangeCalculatable,
Layer1ConfigSettingsInterface {
    private static Object A = new Object();
    private JE H;
    private final aH h;
    private final Object C;
    private final Map<String, ch> c;
    private final Class<?> L;
    private final la E;
    private final String b;
    private final Map<String, rH> l;
    private YD<Va, j> j;
    private boolean J;
    private final ExecutorService M;
    private YD<VE, es> d;
    private boolean g;
    private final G f;
    private boolean a;
    private xg K;
    private final Layer1ApiProvider m;
    private static int F = 0;
    private final Map<String, Map<String, Object>> e;
    private SettingsAccess i;
    private final JH k;
    private final Mc I;
    private final Rh G;
    private final Object D;

    public <T extends Ya<TS>, TS extends ha> YD<T, TS> A(T a2) {
        Fa a3;
        Object object = a3.C;
        synchronized (object) {
            ha ha2 = null;
            T t2 = a2;
            try {
                if (a3.i != null) {
                    ha2 = (ha)a3.i.getSettings(null, "GLOBALSETTINGS." + a2.A().getName(), a2.A());
                }
            }
            catch (Exception exception) {}
            if (ha2 != null && !ha2.IsDefault) {
                t2.A(ha2);
            }
            a2 = new YD(t2);
            return a2;
        }
    }

    public Rh A() {
        Fa a2;
        return a2.G;
    }

    public String A() {
        Fa a2;
        return a2.f.a();
    }

    private /* synthetic */ Layer1ApiUserMessageAddStrategyUpdateGenerator A(boolean a22, ch a3) {
        Fa a4;
        Layer1ApiUserMessageAddStrategyUpdateGenerator a22 = new Layer1ApiUserMessageAddStrategyUpdateGenerator(a4.L, a3.f(), a22, true, true, (StrategyUpdateGenerator)a3, new GeneratedEventInfo[0]);
        return a22;
    }

    public StrategyPanel[] getCustomGuiFor(String a2, String a3) {
        Fa a4;
        a3 = new ArrayList<Nc>();
        a3.addAll(a4.K.A(a4.A((String)a2), (H)a4));
        Fa fa = a4;
        a3.addAll(fa.A(fa.A((String)a2)));
        a3.add(Nc.A((String)a2, (H)a4));
        a2 = new TreeMap();
        a3 = a3.iterator();
        Iterator<Nc> iterator = a3;
        while (iterator.hasNext()) {
            Nc nc2 = (Nc)a3.next();
            int n2 = nc2.A();
            Object object = a2;
            while (object.containsKey(n2)) {
                object = a2;
                ++n2;
            }
            a2.put(n2, new KF(nc2));
            iterator = a3;
        }
        return a2.values().toArray(new StrategyPanel[0]);
    }

    private /* synthetic */ void A(Layer1ApiProvider a2) {
        Fa a3;
        long l2;
        long l3 = (long)((double)a2.getCurrentTime() / 1000000.0);
        a3.a = l3 + 15000L > (l2 = (long)((double)NanoClock.currentTimeNanos() / 1000000.0));
        Log.warn((String)(a3.A() + "-" + a3.f()), (String)("SESSION: " + (String)(a3.a ? fD.A((Object)"i_sS") : "REPLAY, DIFF: " + Long.toString(l2 - l3) + " ms")));
    }

    public <T extends Ya<TS>, TS extends ha> void A(String a2, String a3, String a4, YD<T, TS> a5) {
        Fa a6;
        Object object = a6.C;
        synchronized (object) {
            a5.I.G = false;
            if (!a6.e.containsKey(a3)) {
                a6.e.put(a3, new HashMap());
            }
            if (a6.i != null) {
                a6.i.setSettings((String)a3, a6.A(a2, a5.I.A().getName(), a4), (Object)a5.I.A(), a5.I.A());
            }
            a3 = (Map)a6.e.get(a3);
            a3.put(a6.A(a2, a5.I.getClass().getName(), a4), a5);
            return;
        }
    }

    public void A(Object a2) {
        Fa a3;
        if (!a3.J || a3.g) {
            return;
        }
        a3.m.sendUserMessage(a2);
    }

    public void A(vE a2) {
        Fa a3;
        if (a2.k == dD.I) {
            return;
        }
        a3.H.A(a2);
    }

    public JH A() {
        Fa a2;
        return a2.k;
    }

    public Set<String> A() {
        Fa a2;
        return a2.K.A();
    }

    public ch A(String a2) {
        Fa a3;
        return (ch)a3.c.get(a2);
    }

    public rH A(String a2) {
        Fa a3;
        if (!a3.l.containsKey(a2)) {
            return rH.A((String)a2);
        }
        return (rH)a3.l.get(a2);
    }

    public <T extends Ya<TS>, TS extends ha> YD<T, TS> A(String a2, String a3, String a4, T a5) {
        Fa a6;
        Object object = a6.C;
        synchronized (object) {
            Map map;
            if (!a6.e.containsKey(a3)) {
                a6.e.put(a3, new HashMap());
            }
            if ((map = (Map)a6.e.get(a3)).containsKey(a6.A(a2, a5.getClass().getName(), a4))) {
                return (YD)map.get(a6.A(a2, a5.getClass().getName(), a4));
            }
            ha ha2 = null;
            T t2 = a5;
            try {
                if (a6.i != null) {
                    ha2 = (ha)a6.i.getSettings(a3, a6.A(a2, a5.A().getName(), a4), a5.A());
                }
            }
            catch (Exception exception) {}
            if (ha2 != null && !ha2.IsDefault) {
                t2.A(ha2);
            }
            a3 = new YD(t2);
            map.put(a6.A(a2, a5.getClass().getName(), a4), a3);
            return a3;
        }
    }

    @Override
    public void close() {
        Fa a2;
        a2.finish();
    }

    public Mc A() {
        Fa a2;
        return a2.I;
    }

    public void onInstrumentAdded(String a2, InstrumentInfo a3) {
        Fa fa;
        Fa a4;
        if (a2 == null || a2.isEmpty()) {
            return;
        }
        Fa fa2 = a4;
        fa2.G.A(a2, true);
        Fa fa3 = a4;
        if (fa2.l.containsKey(a2)) {
            ((rH)fa3.l.get(a2)).A(a3);
            fa = a4;
        } else {
            String string = a2;
            fa3.l.put(string, rH.A((String)string, (InstrumentInfo)a3));
            fa = a4;
        }
        if (fa.isStrategyEnabled(a2)) {
            a4.A(a2, true);
        }
    }

    public <T extends Ya<TS>, TS extends ha> YD<T, TS> A(String a2, T a3) {
        Fa a4;
        Object object = a4.C;
        synchronized (object) {
            if (!a4.e.containsKey(a2)) {
                a4.e.put(a2, new HashMap());
            }
            String string = (String)a2 + "." + a3.A().getName();
            if ((a2 = (Map)a4.e.get(a2)).containsKey(string)) {
                return (YD)a2.get(string);
            }
            ha ha2 = null;
            T t2 = a3;
            try {
                if (a4.i != null) {
                    ha2 = (ha)a4.i.getSettings(null, string, a3.A());
                }
            }
            catch (Exception exception) {}
            if (ha2 != null && !ha2.IsDefault) {
                t2.A(ha2);
            }
            a3 = new YD(t2);
            a2.put(string, a3);
            return a3;
        }
    }

    public void f(vE a2) {
        Fa a3;
        if (!a3.a) {
            return;
        }
        if (a3.j == null) {
            return;
        }
        if (((Va)a3.j.I).D != SE.I) {
            return;
        }
        try {
            a3.M.execute(() -> {
                try {
                    Fa a3;
                    a3.H.A(a2);
                    return;
                }
                catch (Exception exception) {
                    return;
                }
            });
            return;
        }
        catch (Exception exception) {
            return;
        }
    }

    public abstract void I();

    public void A(UC a2, BA a3) {
        Fa a4;
        a4.A();
    }

    public static int A() {
        Object object = A;
        synchronized (object) {
            return F++;
        }
    }

    public void f(String a2, String a3) {
        Fa a4;
        if (!a4.a) {
            return;
        }
        if (a3 == null || a3.isEmpty()) {
            return;
        }
        a2 = Layer1ApiSoundAlertMessage.builder().setAlias(a2).setSource(a4.L).setRepeatCount(1L).setRepeatDelay(null).setSound(null).setAlertDeclarationId("").setTextInfo(String.format(jB.A((Object)"2[-\b2["), a2, a3)).build();
        a4.f((Object)a2);
    }

    public String f() {
        Fa a2;
        return a2.b;
    }

    public void finish() {
        Fa a2;
        Object object = a2.D;
        synchronized (object) {
            if (a2.g) {
                return;
            }
            Log.warn((String)(a2.A() + "-" + a2.f()), (String)("FINISH " + a2.f.a() + " " + a2.f.A() + " " + a2.f.f()));
            Fa fa = a2;
            Fa fa2 = a2;
            ListenableHelper.removeListeners((LayerApiListenable)fa.m, (Object)fa2);
            fa.I();
            fa2.a();
            fa.k.A();
            fa.I.f();
            fa.G.A();
            for (Object object2 : fa.e.values()) {
                for (Object e2 : object2.values()) {
                    if (!(e2 instanceof Ma)) continue;
                    ((Ma)e2).A();
                }
            }
            if (a2.J) {
                Fa fa3 = a2;
                fa3.A((Object)new Layer1ApiRequestCurrentTimeEvents(false, 0L, TimeUnit.MILLISECONDS.toNanos(50L)));
                Iterator iterator = fa3.c.values().iterator();
                Iterator iterator2 = iterator;
                while (iterator2.hasNext()) {
                    Object object2;
                    object2 = (ch)iterator.next();
                    object2.A();
                    iterator2 = iterator;
                    Fa fa4 = a2;
                    fa4.A((Object)fa4.A(false, (ch)object2));
                }
                Fa fa5 = a2;
                fa5.c.clear();
                fa5.h.a();
                fa5.K.f();
                fa5.H.A();
            }
            a2.M.shutdown();
            try {
                a2.M.awaitTermination(3L, TimeUnit.SECONDS);
                Fa fa6 = a2;
            }
            catch (InterruptedException interruptedException) {
                throw new RuntimeException(interruptedException);
            }
            fa6.J = false;
            a2.g = true;
            return;
        }
    }

    public abstract Collection<? extends Nc> A(rH var1);

    public void acceptSettingsInterface(SettingsAccess a2) {
        Fa a3;
        Object object = a3.C;
        synchronized (object) {
            a3.i = a2;
        }
        a3.I.A();
        Fa fa = a3;
        super.f();
        Fa fa2 = a3;
        fa.d = fa.A(fa2.A(), (Ya)new VE());
        fa2.j = fa2.A(a3.A(), (Ya)new Va());
    }

    public void A(String a2, String a3, boolean a4) {
        Fa a5;
        if (a5.d == null) {
            return;
        }
        ((VE)a5.d.I).A(a2 + "." + a3, Boolean.valueOf(a4));
        Fa fa = a5;
        fa.A(fa.A(), a5.d);
    }

    public void A(String a2, String a3) {
        Fa a4;
        if (a4.d == null) {
            return;
        }
        ((VE)a4.d.I).A(a2 + "." + a3);
    }

    public void A(String a2, w a3) {
        Fa a4;
        Object object = a4.D;
        synchronized (object) {
            block5: {
                Object object2;
                block4: {
                    block3: {
                        if (!a4.E.f()) break block3;
                        if (!a4.c.containsKey(a2)) break block4;
                        ((ch)a4.c.get(a2)).f(a2, a3);
                        object2 = object;
                        break block5;
                    }
                    a4.h.A(a2, a3);
                }
                object2 = object;
            }
            // ** MonitorExit[v0] (shouldn't be in output)
            return;
        }
    }

    public <T extends Ya<TS>, TS extends ha> void A(YD<T, TS> a2) {
        Fa a3;
        Object object = a3.C;
        synchronized (object) {
            a2.I.G = false;
            if (a3.i != null) {
                a3.i.setSettings(null, "GLOBALSETTINGS." + a2.I.A().getName(), (Object)a2.I.A(), a2.I.A());
            }
            return;
        }
    }

    public boolean A() {
        Fa a2;
        return a2.a;
    }

    public <T extends Ya<TS>, TS extends ha> void A(String a2, YD<T, TS> a3) {
        Fa a4;
        Object object = a4.C;
        synchronized (object) {
            String string = (String)a2 + "." + a3.I.A().getName();
            if (!a4.e.containsKey(a2)) {
                a4.e.put(a2, new HashMap());
            }
            a3.I.G = false;
            if (a4.i != null) {
                a4.i.setSettings(null, string, (Object)a3.I.A(), a3.I.A());
            }
            a2 = (Map)a4.e.get(a2);
            a2.put(string, a3);
            return;
        }
    }

    private /* synthetic */ String A(String a2, String a3, String a4) {
        if (a4.isEmpty()) {
            return a2 + "." + a3;
        }
        return a2 + "." + a3 + "." + a4;
    }

    public abstract List<q> f();

    public Di A(String a2) {
        Fa a3;
        return new Di(a3.k, a2);
    }

    public <T extends Ya<TS>, TS extends ha> void A(String a2, String a3, YD<T, TS> a4) {
        Fa a5;
        a5.A(a2, a3, "", a4);
    }

    public void onInstrumentRemoved(String a2) {
        Fa a3;
        a3.A(a2, false);
        a3.G.A(a2, false);
    }

    public Fa(G a2, Layer1ApiProvider a3, Class<?> a4, la a5) {
        a6(a2, a3, a4, new db(new LinkedHashMap()), a5);
        Fa a6;
    }

    public Map<String, Layer1PriceAxisRangeCalculatable.ResultPriceAxisInfo> getPriceRanges(String a2, double a3, Map<String, Layer1PriceAxisRangeCalculatable.InputPriceAxisInfo> a4) {
        Iterator<Map.Entry<String, Layer1PriceAxisRangeCalculatable.InputPriceAxisInfo>> iterator;
        HashMap<String, Layer1PriceAxisRangeCalculatable.ResultPriceAxisInfo> hashMap = new HashMap<String, Layer1PriceAxisRangeCalculatable.ResultPriceAxisInfo>();
        Iterator<Map.Entry<String, Layer1PriceAxisRangeCalculatable.InputPriceAxisInfo>> iterator2 = iterator = a4.entrySet().iterator();
        while (iterator2.hasNext()) {
            Fa a5;
            Map.Entry<String, Layer1PriceAxisRangeCalculatable.InputPriceAxisInfo> entry = iterator.next();
            String string = entry.getKey();
            Object object = a5.K.A(string);
            if (object == null || !(object instanceof JA)) {
                double d2 = Math.max(Math.abs(entry.getValue().minValue), Math.abs(entry.getValue().maxValue));
                iterator2 = iterator;
                hashMap.put(string, PriceRangeCalculationHelper.getGoodNumbersCalculation((double)(-d2), (double)d2, (double)a3));
                continue;
            }
            JA jA2 = (JA)object;
            Object object22 = jA2.A();
            object = object22 == null ? Collections.singletonList(jA2) : object22.A();
            double d3 = Double.POSITIVE_INFINITY;
            double d4 = Double.NEGATIVE_INFINITY;
            Iterator iterator3 = object.iterator();
            while (iterator3.hasNext()) {
                for (Object object22 : ((q)iterator3.next()).A()) {
                    if ((object22 = a4.get(object22)) == null) continue;
                    d3 = Math.min(d3, ((Layer1PriceAxisRangeCalculatable.InputPriceAxisInfo)object22).minValue);
                    d4 = Math.max(d4, ((Layer1PriceAxisRangeCalculatable.InputPriceAxisInfo)object22).maxValue);
                }
            }
            if (d3 > d4) {
                double d5 = Math.max(Math.abs(entry.getValue().minValue), Math.abs(entry.getValue().maxValue));
                iterator2 = iterator;
                hashMap.put(string, PriceRangeCalculationHelper.getGoodNumbersCalculation((double)(-d5), (double)d5, (double)a3));
                continue;
            }
            if (jA2.f(a2)) {
                double d6 = Math.max(Math.abs(d3), Math.abs(d4)) * 1.1;
                iterator2 = iterator;
                hashMap.put(string, PriceRangeCalculationHelper.getGoodNumbersCalculation((double)(-d6), (double)d6, (double)a3));
                continue;
            }
            hashMap.put(string, PriceRangeCalculationHelper.getGoodNumbersCalculation((double)d3, (double)d4, (double)a3));
            iterator2 = iterator;
        }
        return hashMap;
    }

    public boolean isFEnabled() {
        Fa a2;
        return ((Va)a2.j.I).D == SE.I;
    }

    public void f(String a2, w a3) {
        Fa a4;
        Object object = a4.D;
        synchronized (object) {
            if (a4.c.containsKey(a2)) {
                ((ch)a4.c.get(a2)).A(a2, a3);
            }
            a4.h.f(a2, a3);
            return;
        }
    }

    /*
     * Unable to fully structure code
     */
    public boolean A(String a, boolean a) {
        var4_4 = a.D;
        synchronized (var4_4) {
            block11: {
                block9: {
                    block10: {
                        if (!a.J || a.g) {
                            return false;
                        }
                        v0 = a;
                        v0.K.a();
                        if (!v0.E.f()) break block9;
                        if (!a) break block10;
                        var3_5 = null;
                        if (!a.c.containsKey(a)) {
                            v1 = a;
                            var3_5 = new ch((rH)a.l.get(a), v1.m, v1.E.A());
                            a.c.put(a, var3_5);
                        }
                        a.K.A(a.A(a), (ch)a.c.get(a));
                        if (var3_5 != null) {
                            v2 = a;
                            v2.A((Object)v2.A(true, var3_5));
                        }
                        ** GOTO lbl40
                    }
                    if (a.c.containsKey(a)) {
                        var3_6 = (ch)a.c.remove(a);
                        var3_6.A();
                        v3 = a;
                        v3.A((Object)v3.A(false, var3_6));
                    }
                    v4 = a;
                    v5 = v4;
                    v4.A(a);
                    v4.K.A(a);
                    break block11;
                }
                v6 = a;
                if (a) {
                    v6.K.A(a.A(a), (ch)null);
                    v5 = a;
                } else {
                    v6.A(a);
                    a.K.A(a);
lbl40:
                    // 2 sources

                    v5 = a;
                }
            }
            v5.K.A();
        }
        a.G.f(a, a);
        return true;
    }

    public Fa(G a2, Layer1ApiProvider a3, Class<?> a4, db a5, la a6) {
        Fa a7;
        Fa fa = a7;
        Fa fa2 = a7;
        Fa fa3 = a7;
        Fa fa4 = a7;
        Fa fa5 = a7;
        Fa fa6 = a7;
        Fa fa7 = a7;
        a7.C = new Object();
        a7.D = new Object();
        fa7.g = false;
        fa7.i = null;
        fa6.e = new HashMap();
        fa6.d = null;
        fa6.j = new YD((Ya)new Va());
        fa5.K = new xg();
        fa5.J = false;
        fa5.c = new HashMap();
        fa4.l = Collections.synchronizedMap(new HashMap());
        fa4.a = false;
        fa4.H = new JE();
        fa3.f = a2;
        fa3.g = false;
        fa2.L = a4;
        fa2.m = a3;
        fa.b = IdHelper.generateShortUuid();
        a7.E = a6;
        fa.A(fa.m);
        Log.warn((String)(a7.A() + "-" + a7.f()), (String)("LOAD " + a7.f.a() + " " + a7.f.A() + " " + a7.f.f()));
        Fa fa8 = a7;
        a7.I = new Mc((H)a7, a5, a2);
        Fa fa9 = a7;
        fa9.k = new JH((H)fa9, fa9.E, a7.I);
        a7.M = Executors.newSingleThreadExecutor();
        a7.G = new Rh((H)a7, a2);
        a7.h = new aH(a3, a7.E.A());
        fa8.A(a6, a7.I, a7.a);
        a5.A((J)fa8);
        ListenableHelper.addListeners((LayerApiListenable)fa8.m, (Object)a7);
    }

    public void A() {
        Fa a2;
        Fa fa = a2;
        fa.A(100);
        fa.f((Object)new Layer1ApiUserMessageReloadStrategyGui());
    }

    public void A(String a2) {
        Fa a3;
        Object object = a3.D;
        synchronized (object) {
            if (a3.c.containsKey(a2)) {
                ((ch)a3.c.get(a2)).A(a2);
            }
            a3.h.A(a2);
            return;
        }
    }

    public void A(String a2, String a3, boolean a4) {
        Fa a5;
        if (a5.d == null) {
            return false;
        }
        return ((VE)a5.d.I).A(a2 + "." + a3, a4);
    }

    private /* synthetic */ void A(int a2) {
        Fa a3;
        if (!a3.J || a3.g) {
            return;
        }
        try {
            a3.M.execute(() -> {
                try {
                    Thread.sleep(a2);
                    return;
                }
                catch (Exception exception) {
                    return;
                }
            });
            return;
        }
        catch (Exception exception) {
            return;
        }
    }

    public void onUserMessage(Object a2) {
        String string;
        Fa a3;
        if (a2.getClass() == UserMessageLayersChainCreatedTargeted.class) {
            UserMessageLayersChainCreatedTargeted userMessageLayersChainCreatedTargeted = (UserMessageLayersChainCreatedTargeted)a2;
            if (userMessageLayersChainCreatedTargeted.targetClass == a3.getClass()) {
                Object object = a3.D;
                synchronized (object) {
                    if (a3.J) {
                        return;
                    }
                    a3.J = true;
                    Log.warn((String)(a3.A() + "-" + a3.f()), (String)("START " + a3.f.a() + " " + a3.f.A() + " " + a3.f.f()));
                    Fa fa = a3;
                    Fa fa2 = a3;
                    fa.K.A(fa2.f(), (H)a3);
                    fa.G.f();
                    fa2.k.f();
                    fa.K.A();
                    fa.I.a();
                    Iterator iterator = a2 = fa.c.values().iterator();
                    while (iterator.hasNext()) {
                        userMessageLayersChainCreatedTargeted = (ch)a2.next();
                        iterator = a2;
                        Fa fa3 = a3;
                        fa3.A((Object)fa3.A(true, (ch)userMessageLayersChainCreatedTargeted));
                    }
                    if (!a3.a || !a3.E.f()) {
                        a3.A((Object)new Layer1ApiRequestCurrentTimeEvents(true, 0L, TimeUnit.MILLISECONDS.toNanos(200L)));
                    }
                    return;
                }
            }
            return;
        }
        if (a2 instanceof CurrentTimeUserMessage) {
            long l2 = ((CurrentTimeUserMessage)a2).time;
            if (!a3.a) {
                a3.K.f(l2);
            }
            a3.h.A(l2);
            return;
        }
        if (a2 instanceof UserMessageRewindBase) {
            if (a3.J && !a3.g) {
                Fa fa = a3;
                fa.K.A(fa.m.getCurrentTime());
                return;
            }
        } else if (a2 instanceof Layer1ApiHistoricalDataLoadedMessage && a3.E.a() && a3.isStrategyEnabled(string = ((Layer1ApiHistoricalDataLoadedMessage)a2).alias)) {
            a3.A(string, false);
            a3.A(string, true);
        }
    }

    public void f(Object a2) {
        Fa a3;
        if (!a3.J || a3.g) {
            return;
        }
        try {
            a3.M.execute(() -> {
                Fa a3;
                try {
                    a3.m.sendUserMessage(a2);
                    return;
                }
                catch (Exception a22) {
                    Log.warn((String)(a3.A() + "-" + a3.f()), (String)("EXCEPTION: " + a22.getMessage()));
                    return;
                }
            });
            return;
        }
        catch (Exception exception) {
            return;
        }
    }

    public <T extends Ya<TS>, TS extends ha> YD<T, TS> A(String a2, String a3, T a4) {
        Fa a5;
        return a5.A(a2, a3, "", a4);
    }
}

