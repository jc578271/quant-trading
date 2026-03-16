/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.w
 *  velox.api.layer1.Layer1ApiAdminAdapter
 *  velox.api.layer1.Layer1ApiAdminListener
 *  velox.api.layer1.Layer1ApiDataAdapter
 *  velox.api.layer1.Layer1ApiDataListener
 *  velox.api.layer1.Layer1ApiInstrumentAdapter
 *  velox.api.layer1.Layer1ApiInstrumentListener
 *  velox.api.layer1.Layer1ApiMboDataAdapter
 *  velox.api.layer1.Layer1ApiMboDataListener
 *  velox.api.layer1.Layer1ApiProvider
 *  velox.api.layer1.Layer1ApiTradingAdapter
 *  velox.api.layer1.Layer1ApiTradingListener
 *  velox.api.layer1.data.TradeInfo
 *  velox.api.layer1.messages.indicators.StrategyUpdateGeneratorFilter$StrategyUpdateGeneratorEventType
 */
package ttw.tradefinder;

import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import ttw.tradefinder.eh;
import ttw.tradefinder.w;
import velox.api.layer1.Layer1ApiAdminAdapter;
import velox.api.layer1.Layer1ApiAdminListener;
import velox.api.layer1.Layer1ApiDataAdapter;
import velox.api.layer1.Layer1ApiDataListener;
import velox.api.layer1.Layer1ApiInstrumentAdapter;
import velox.api.layer1.Layer1ApiInstrumentListener;
import velox.api.layer1.Layer1ApiMboDataAdapter;
import velox.api.layer1.Layer1ApiMboDataListener;
import velox.api.layer1.Layer1ApiProvider;
import velox.api.layer1.Layer1ApiTradingAdapter;
import velox.api.layer1.Layer1ApiTradingListener;
import velox.api.layer1.data.TradeInfo;
import velox.api.layer1.messages.indicators.StrategyUpdateGeneratorFilter;

public class aH
implements Layer1ApiAdminAdapter,
Layer1ApiDataAdapter,
Layer1ApiInstrumentAdapter,
Layer1ApiMboDataAdapter,
Layer1ApiTradingAdapter {
    private final Object m;
    private long F;
    private boolean e;
    private final Set<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> i;
    private final Layer1ApiProvider k;
    private Map<String, List<WeakReference<w>>> I;
    private boolean G;
    private long D;

    public void onMboSend(String a2, String a3, boolean a4, int a5, int a6) {
        aH a7;
        if (!a7.e) {
            return;
        }
        Object object = a7.m;
        synchronized (object) {
            a2 = a7.I.get(a2);
            if (a2 == null) {
                return;
            }
            a2 = a2.iterator();
            while (a2.hasNext()) {
                WeakReference weakReference = (WeakReference)a2.next();
                if (weakReference.get() == null) continue;
                ((w)weakReference.get()).A(a7.F, a3, a5, a6, a4);
            }
            return;
        }
    }

    private /* synthetic */ void I() {
        aH a2;
        if (a2.G || a2.e) {
            return;
        }
        Iterator<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> iterator = a2.i.iterator();
        block0: while (true) {
            Iterator<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> iterator2 = iterator;
            while (iterator2.hasNext()) {
                StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType strategyUpdateGeneratorEventType = iterator.next();
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.DEPTH_MBP || strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.TRADES) {
                    a2.k.addListener((Layer1ApiDataListener)a2);
                    iterator2 = iterator;
                    continue;
                }
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.DEPTH_MBO) {
                    iterator2 = iterator;
                    a2.k.addListener((Layer1ApiMboDataListener)a2);
                    continue;
                }
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.INSTRUMENTS) {
                    iterator2 = iterator;
                    a2.k.addListener((Layer1ApiInstrumentListener)a2);
                    continue;
                }
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.TRADES) {
                    iterator2 = iterator;
                    a2.k.addListener((Layer1ApiTradingListener)a2);
                    continue;
                }
                if (strategyUpdateGeneratorEventType != StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.OTHER) continue block0;
                a2.k.addListener((Layer1ApiAdminListener)a2);
                continue block0;
            }
            break;
        }
        a2.e = true;
    }

    public void a() {
        aH a2;
        a2.G = true;
        a2.e = false;
        a2.unregisterListeners();
        a2.f();
    }

    public void onMboCancel(String a2, String a3) {
        aH a4;
        if (!a4.e) {
            return;
        }
        Object object = a4.m;
        synchronized (object) {
            a2 = a4.I.get(a2);
            if (a2 == null) {
                return;
            }
            a2 = a2.iterator();
            while (a2.hasNext()) {
                WeakReference weakReference = (WeakReference)a2.next();
                if (weakReference.get() == null) continue;
                ((w)weakReference.get()).A(a4.F, a3);
            }
            return;
        }
    }

    public aH(Layer1ApiProvider a2, Set<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> a3) {
        aH a4;
        aH aH2 = a4;
        aH aH3 = a4;
        aH aH4 = a4;
        a4.m = new Object();
        aH4.F = Long.MIN_VALUE;
        aH4.D = 0L;
        aH3.e = false;
        aH3.G = false;
        aH3.I = Collections.synchronizedMap(new HashMap());
        aH2.k = a2;
        aH2.i = a3;
    }

    public void onTrade(String a2, double a3, int a4, TradeInfo a5) {
        aH a6;
        if (!a6.e) {
            return;
        }
        Object object = a6.m;
        synchronized (object) {
            a2 = a6.I.get(a2);
            if (a2 == null) {
                return;
            }
            a2 = a2.iterator();
            while (a2.hasNext()) {
                WeakReference weakReference = (WeakReference)a2.next();
                if (weakReference.get() == null) continue;
                ((w)weakReference.get()).A(a6.F, (int)(a3 + 0.5), a4, a5);
            }
            return;
        }
    }

    public void f() {
        aH a2;
        Object object = a2.m;
        synchronized (object) {
            a2.I.clear();
            return;
        }
    }

    public void onUserMessage(Object a2) {
    }

    public void f(String a2, w a3) {
        aH a4;
        Object object = a4.m;
        synchronized (object) {
            if (a4.I.containsKey(a2)) {
                a2 = a4.I.get(a2).iterator();
                block3: while (true) {
                    Object object2 = a2;
                    while (object2.hasNext()) {
                        WeakReference weakReference = (WeakReference)a2.next();
                        if (weakReference.get() == null) {
                            Object object3 = a2;
                            object2 = object3;
                            object3.remove();
                            continue;
                        }
                        if (weakReference.get() != a3) continue block3;
                        weakReference.clear();
                        a2.remove();
                        continue block3;
                    }
                    break;
                }
            }
        }
        if (!a4.hasActiveListeners()) {
            a4.unregisterListeners();
        }
    }

    private /* synthetic */ void unregisterListeners() {
        aH a2;
        if (!a2.e) {
            return;
        }
        a2.e = false;
        Iterator<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> iterator = a2.i.iterator();
        block0: while (true) {
            Iterator<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> iterator2 = iterator;
            while (iterator2.hasNext()) {
                StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType strategyUpdateGeneratorEventType = iterator.next();
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.DEPTH_MBP || strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.TRADES) {
                    a2.k.removeListener((Layer1ApiDataListener)a2);
                    iterator2 = iterator;
                    continue;
                }
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.DEPTH_MBO) {
                    iterator2 = iterator;
                    a2.k.removeListener((Layer1ApiMboDataListener)a2);
                    continue;
                }
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.INSTRUMENTS) {
                    iterator2 = iterator;
                    a2.k.removeListener((Layer1ApiInstrumentListener)a2);
                    continue;
                }
                if (strategyUpdateGeneratorEventType == StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.TRADES) {
                    iterator2 = iterator;
                    a2.k.removeListener((Layer1ApiTradingListener)a2);
                    continue;
                }
                if (strategyUpdateGeneratorEventType != StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.OTHER) continue block0;
                a2.k.removeListener((Layer1ApiAdminListener)a2);
                continue block0;
            }
            break;
        }
    }

    private /* synthetic */ boolean hasActiveListeners() {
        aH a2;
        Object object = a2.m;
        synchronized (object) {
            Object object2;
            boolean bl;
            Iterator<List<WeakReference<w>>> iterator;
            Object object3;
            for (List<WeakReference<w>> list : a2.I.values()) {
                object3 = list.iterator();
                while (object3.hasNext()) {
                    WeakReference<w> weakReference = object3.next();
                    if (weakReference.get() != null) continue;
                    object3.remove();
                }
            }
            int n2 = 0;
            Iterator<List<WeakReference<w>>> iterator2 = iterator = a2.I.values().iterator();
            while (iterator2.hasNext()) {
                object3 = iterator.next();
                n2 += object3.size();
                iterator2 = iterator;
            }
            if (n2 > 0) {
                bl = true;
                object2 = object;
            } else {
                bl = false;
                object2 = object;
            }
            // ** MonitorExit[v2] (shouldn't be in output)
            return bl;
        }
    }

    public void A(String a2, w a3) {
        aH a4;
        if (!a4.hasActiveListeners()) {
            a4.I();
        }
        if (!a4.e) {
            return;
        }
        Object object = a4.m;
        synchronized (object) {
            if (!a4.I.containsKey(a2)) {
                a4.I.put(a2, new ArrayList());
            }
            a4.I.get(a2).add(new WeakReference<w>(a3));
        }
        a3.a();
    }

    public void onDepth(String a2, boolean a3, int a4, int a5) {
        aH a6;
        if (!a6.e) {
            return;
        }
        Object object = a6.m;
        synchronized (object) {
            a2 = a6.I.get(a2);
            if (a2 == null) {
                return;
            }
            a2 = a2.iterator();
            while (a2.hasNext()) {
                WeakReference weakReference = (WeakReference)a2.next();
                if (weakReference.get() == null) continue;
                ((w)weakReference.get()).A(a6.F, a4, a5, a3);
            }
            return;
        }
    }

    public void onMboReplace(String a2, String a3, int a4, int a5) {
        aH a6;
        if (!a6.e) {
            return;
        }
        Object object = a6.m;
        synchronized (object) {
            a2 = a6.I.get(a2);
            if (a2 == null) {
                return;
            }
            a2 = a2.iterator();
            while (a2.hasNext()) {
                WeakReference weakReference = (WeakReference)a2.next();
                if (weakReference.get() == null) continue;
                ((w)weakReference.get()).A(a6.F, a3, a4, a5);
            }
            return;
        }
    }

    public void A(long a2) {
        aH a3;
        if (!a3.e) {
            return;
        }
        a3.F = a2;
        if (a2 < a3.D) {
            return;
        }
        a3.D = a2 + eh.e;
        Object object = a3.m;
        synchronized (object) {
            for (List<WeakReference<w>> list : a3.I.values()) {
                for (WeakReference<w> weakReference : list) {
                    if (weakReference.get() == null) continue;
                    ((w)weakReference.get()).I(a2);
                }
            }
            return;
        }
    }

    public void A(String a2) {
        aH a3;
        Object object = a3.m;
        synchronized (object) {
            if (a3.I.containsKey(a2)) {
                Iterator<WeakReference<w>> iterator;
                Iterator<WeakReference<w>> iterator2 = iterator = a3.I.get(a2).iterator();
                while (iterator2.hasNext()) {
                    iterator.next().clear();
                    iterator2 = iterator;
                }
                a3.I.get(a2).clear();
            }
        }
        if (!a3.hasActiveListeners()) {
            a3.unregisterListeners();
        }
    }
}

