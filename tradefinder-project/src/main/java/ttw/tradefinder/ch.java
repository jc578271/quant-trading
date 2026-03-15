/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ja
 *  ttw.tradefinder.Me
 *  ttw.tradefinder.XF
 *  ttw.tradefinder.ch
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.w
 *  velox.api.layer1.Layer1ApiProvider
 *  velox.api.layer1.data.TradeInfo
 *  velox.api.layer1.layers.strategies.interfaces.CustomGeneratedEventAliased
 *  velox.api.layer1.messages.indicators.StrategyUpdateGenerator
 *  velox.api.layer1.messages.indicators.StrategyUpdateGeneratorFilter
 *  velox.api.layer1.messages.indicators.StrategyUpdateGeneratorFilter$StrategyUpdateGeneratorEventType
 *  velox.api.layer1.utils.IdHelper
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.Set;
import java.util.function.Consumer;
import ttw.tradefinder.Ja;
import ttw.tradefinder.Me;
import ttw.tradefinder.XF;
import ttw.tradefinder.bg;
import ttw.tradefinder.cH;
import ttw.tradefinder.eh;
import ttw.tradefinder.rH;
import ttw.tradefinder.w;
import velox.api.layer1.Layer1ApiProvider;
import velox.api.layer1.data.TradeInfo;
import velox.api.layer1.layers.strategies.interfaces.CustomGeneratedEventAliased;
import velox.api.layer1.messages.indicators.StrategyUpdateGenerator;
import velox.api.layer1.messages.indicators.StrategyUpdateGeneratorFilter;
import velox.api.layer1.utils.IdHelper;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class ch
implements StrategyUpdateGenerator,
StrategyUpdateGeneratorFilter {
    private final Layer1ApiProvider d;
    private Consumer<CustomGeneratedEventAliased> g;
    private long f;
    private final Object a;
    private boolean K;
    private final String m;
    private final Set<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> F;
    private boolean e;
    private long i;
    private final String k;
    private boolean I;
    private final Ja G;
    private List<cH> D;

    public String f() {
        ch a2;
        return a2.k;
    }

    public boolean A() {
        ch a2;
        Object object = a2.a;
        synchronized (object) {
            Object object2;
            boolean bl;
            Iterator iterator = a2.D.iterator();
            while (iterator.hasNext()) {
                cH cH2 = (cH)iterator.next();
                if (!((ch)cH2).A()) continue;
                ((ch)cH2).A();
                iterator.remove();
            }
            if (a2.D.size() > 0) {
                bl = true;
                object2 = object;
            } else {
                bl = false;
                object2 = object;
            }
            // ** MonitorExit[v1] (shouldn't be in output)
            return bl;
        }
    }

    public void setTime(long a2) {
        Iterator iterator;
        ch a3;
        if (!a3.K) {
            return;
        }
        a3.f = a2;
        if (a2 < a3.i) {
            return;
        }
        a3.i = a2 + eh.e;
        Object object = a3.a;
        synchronized (object) {
            Iterator iterator2 = iterator = a3.D.iterator();
            while (iterator2.hasNext()) {
                ((cH)iterator.next()).A(a2);
                iterator2 = iterator;
            }
        }
        if (!a3.I) {
            ch ch2;
            if (a3.e) {
                a3.I = true;
                object = a3.a;
                synchronized (object) {
                    iterator = a3.D.iterator();
                    Iterator iterator3 = iterator;
                    while (iterator3.hasNext()) {
                        ((ch)((cH)iterator.next())).f();
                        iterator3 = iterator;
                    }
                    // MONITOREXIT @DISABLED, blocks:[1, 3, 10, 11] lbl34 : MonitorExitStatement: MONITOREXIT : var3_4
                    ch2 = a3;
                }
            } else {
                ch2 = a3;
            }
            if (!ch2.e && a2 >= a3.d.getCurrentTime()) {
                a3.e = true;
            }
        }
    }

    public void onDepth(String a2, boolean a3, int a4, int a5) {
        ch a6;
        if (!a6.A((String)a2)) {
            return;
        }
        if (a4 <= 0) {
            return;
        }
        ch ch2 = a6;
        ch2.G.A(a3, a4, a5);
        a2 = ch2.a;
        synchronized (a2) {
            Iterator iterator;
            Iterator iterator2 = iterator = a6.D.iterator();
            while (iterator2.hasNext()) {
                ((cH)iterator.next()).A(a6.f, a4, a5, a3);
                iterator2 = iterator;
            }
            return;
        }
    }

    public void f(String a2, w a3) {
        ch a4;
        if (!((String)a2).equals(a4.m)) {
            return;
        }
        a2 = new cH(a3);
        Object object = a4.a;
        synchronized (object) {
            a4.D.add(a2);
        }
        if (a4.I) {
            a3.a();
        }
    }

    public void onUserMessage(Object a2) {
    }

    public String A() {
        ch a2;
        return a2.m;
    }

    public void onMboSend(String a2, String a3, boolean a4, int a5, int a6) {
        ch a7;
        if (!a7.A((String)a2)) {
            return;
        }
        if (a5 <= 0) {
            return;
        }
        a2 = a7.a;
        synchronized (a2) {
            Iterator iterator;
            Iterator iterator2 = iterator = a7.D.iterator();
            while (iterator2.hasNext()) {
                ((cH)iterator.next()).A(a7.f, a3, a5, a6, a4);
                iterator2 = iterator;
            }
            return;
        }
    }

    public Consumer<CustomGeneratedEventAliased> getGeneratedEventsConsumer() {
        ch a2;
        return a2.g;
    }

    private /* synthetic */ boolean A(String a2) {
        ch a3;
        if (!a3.K) {
            return false;
        }
        return a3.m.equals(a2);
    }

    public void f() {
        ch a2;
        Object object = a2.a;
        synchronized (object) {
            Iterator iterator;
            Iterator iterator2 = iterator = a2.D.iterator();
            while (iterator2.hasNext()) {
                ((ch)((cH)iterator.next())).A();
                iterator2 = iterator;
            }
            a2.D.clear();
            return;
        }
    }

    public void setGeneratedEventsConsumer(Consumer<CustomGeneratedEventAliased> a2) {
        a.g = a2;
    }

    public void onMboCancel(String a2, String a3) {
        ch a4;
        if (!a4.A((String)a2)) {
            return;
        }
        a2 = a4.a;
        synchronized (a2) {
            Iterator iterator;
            Iterator iterator2 = iterator = a4.D.iterator();
            while (iterator2.hasNext()) {
                ((cH)iterator.next()).A(a4.f, a3);
                iterator2 = iterator;
            }
            return;
        }
    }

    public Set<String> getGeneratorAliases() {
        ch a2;
        return Collections.singleton(a2.A());
    }

    public ch(rH a2, Layer1ApiProvider a3, Set<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> a4) {
        ch ch2;
        ch a5;
        ch ch3 = a5;
        ch ch4 = a5;
        ch ch5 = a5;
        ch ch6 = a5;
        ch ch7 = a5;
        a5.a = new Object();
        ch7.f = 0L;
        ch7.i = 0L;
        ch6.I = false;
        ch6.e = false;
        ch5.g = null;
        ch5.K = true;
        ch4.D = new ArrayList();
        ch4.k = IdHelper.generateShortUuid();
        ch4.F = a4;
        ch3.m = a2.G;
        ch3.d = a3;
        if (a2.D == bg.i) {
            ch2 = a5;
            a5.G = new Me(a2);
        } else {
            ch2 = a5;
            a5.G = new XF(a2);
        }
        ch2.K = true;
    }

    public void A() {
        ch a2;
        ch ch2 = a2;
        a2.K = false;
        ch2.f();
        ch2.g = null;
        ch2.G.clear();
    }

    public void A(String a2, w a3) {
        ch a4;
        if (!((String)a2).equals(a4.m)) {
            return;
        }
        a2 = a4.a;
        synchronized (a2) {
            Iterator iterator = a4.D.iterator();
            while (iterator.hasNext()) {
                cH cH2 = (cH)iterator.next();
                if (!cH2.A(a3)) continue;
                ((ch)cH2).A();
                iterator.remove();
            }
            return;
        }
    }

    public void A(String a2) {
        ch a3;
        if (!((String)a2).equals(a3.m)) {
            return;
        }
        a2 = a3.a;
        synchronized (a2) {
            Iterator iterator;
            Iterator iterator2 = iterator = a3.D.iterator();
            while (iterator2.hasNext()) {
                ((ch)((cH)iterator.next())).A();
                iterator2 = iterator;
            }
            a3.D.clear();
            return;
        }
    }

    public Ja A() {
        ch a2;
        return a2.G;
    }

    public void onMboReplace(String a2, String a3, int a4, int a5) {
        ch a6;
        if (!a6.A((String)a2)) {
            return;
        }
        if (a4 <= 0) {
            return;
        }
        a2 = a6.a;
        synchronized (a2) {
            Iterator iterator;
            Iterator iterator2 = iterator = a6.D.iterator();
            while (iterator2.hasNext()) {
                ((cH)iterator.next()).A(a6.f, a3, a4, a5);
                iterator2 = iterator;
            }
            return;
        }
    }

    public void onTrade(String a22, double a32, int a4, TradeInfo a5) {
        ch a6;
        if (!a6.A(a22)) {
            return;
        }
        int a22 = (int)(a32 + 0.5);
        if (a22 <= 0) {
            return;
        }
        Object a32 = a6.a;
        synchronized (a32) {
            Iterator iterator;
            Iterator iterator2 = iterator = a6.D.iterator();
            while (iterator2.hasNext()) {
                ((cH)iterator.next()).A(a6.f, a22, a4, a5);
                iterator2 = iterator;
            }
            return;
        }
    }

    public Set<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> getGeneratorUpdateTypes() {
        ch a2;
        return a2.F;
    }
}

