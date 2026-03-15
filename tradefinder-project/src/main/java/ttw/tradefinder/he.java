/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ia
 *  ttw.tradefinder.MB
 *  ttw.tradefinder.he
 *  ttw.tradefinder.rH
 *  velox.api.layer1.data.TradeInfo
 */
package ttw.tradefinder;

import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import ttw.tradefinder.Ia;
import ttw.tradefinder.MB;
import ttw.tradefinder.rH;
import ttw.tradefinder.sE;
import ttw.tradefinder.sI;
import velox.api.layer1.data.TradeInfo;

public class he {
    private String K;
    private sE m;
    private final Ia F;
    private sE e;
    private Map<String, sE> i;
    private Map<String, sE> k;
    private boolean I;
    private final boolean G;
    private final Object D;

    public /* synthetic */ boolean A() {
        he a2;
        return a2.I;
    }

    public /* synthetic */ he(Ia a2, rH a3) {
        he a4;
        he he2 = a4;
        he he3 = a4;
        he he4 = a4;
        a4.D = new Object();
        a4.e = null;
        he4.m = null;
        he4.K = "";
        he3.k = new HashMap();
        he3.i = Collections.synchronizedMap(new HashMap());
        he3.F = a2;
        he2.G = a3.k;
        he2.I = a3.G.endsWith(MB.A((Object)"JwD")) || a3.G.endsWith(sI.A("E\u0016L\u0010M\tL\u0007"));
        a4.A();
    }

    /*
     * Enabled aggressive block sorting
     * Enabled unnecessary exception pruning
     * Enabled aggressive exception aggregation
     */
    private /* synthetic */ sE f(long a2, int a3, int a4, TradeInfo a5) {
        he he2;
        he a6;
        if (a6.m != null) {
            if (a6.m.K.equals(a5.aggressorOrderId)) {
                he he3 = a6;
                he3.m.A(a2, (int)a4, (int)a3);
                return he3.m;
            }
            he he4 = a6;
            he4.F.A(he4.m);
            if (he4.G) {
                Object object = a6.D;
                synchronized (object) {
                    he he5 = a6;
                    he5.k.put(he5.m.K, a6.m);
                }
                he2 = a6;
                return he2.A(a2, (int)a3, (int)a4, (TradeInfo)a5);
            }
            he he6 = a6;
            he6.F.f(he6.m);
        }
        he2 = a6;
        return he2.A(a2, (int)a3, (int)a4, (TradeInfo)a5);
    }

    public /* synthetic */ void A(long a22, String a3, int a42, int a5) {
        he a6;
        if (!a6.G) {
            return;
        }
        if (a6.m != null && a6.m.K.equals(a3)) {
            he he2 = a6;
            he2.i.put(he2.m.K, new sE(a6.m));
        }
        if (a6.e != null && a6.e.K.equals(a3)) {
            he he3 = a6;
            he3.i.put(he3.e.K, new sE(a6.e));
        }
        Object a22 = a6.D;
        synchronized (a22) {
            Iterator iterator;
            if (a6.k.size() == 0) {
                return;
            }
            Iterator iterator2 = iterator = a6.k.values().iterator();
            while (iterator2.hasNext()) {
                sE a42 = (sE)((Object)iterator.next());
                if (a42.K.equals(a3)) {
                    iterator2 = iterator;
                    a6.i.put(a42.K, a42);
                    continue;
                }
                a6.F.f(a42);
                iterator2 = iterator;
            }
            a6.k.clear();
            return;
        }
    }

    public /* synthetic */ void A(long a2, int a3, int a4, TradeInfo a5) {
        he he2;
        he a6;
        if (a4 > 0) {
            if (a6.i.containsKey(a5.aggressorOrderId)) {
                ((sE)((Object)a6.i.get(a5.aggressorOrderId))).A(a2, a4, a3);
            }
            if (a6.i.containsKey(a5.passiveOrderId)) {
                ((sE)((Object)a6.i.get(a5.passiveOrderId))).f(a2, a4, a3);
            }
        }
        if (a5.isExecutionStart || a5.isExecutionEnd) {
            if (a5.isExecutionEnd) {
                a6.m = a6.e;
                a6.e = null;
            }
            if (!a5.isExecutionStart || a5.aggressorOrderId == null) {
                return;
            }
            a6.e = a6.f(a2, a3, a4, a5);
            a6.m = null;
            return;
        }
        if (a5.aggressorOrderId == null || a5.aggressorOrderId.isEmpty()) {
            return;
        }
        if (a6.e == null) {
            a6.e = a6.f(a2, a3, a4, a5);
            a6.m = null;
            return;
        }
        if (a6.e.K.equals(a5.aggressorOrderId)) {
            a6.e.A(a2, a4, a3);
            return;
        }
        he he3 = a6;
        he3.F.A(he3.e);
        if (he3.G) {
            Object object = a6.D;
            synchronized (object) {
                he he4 = a6;
                he4.k.put(he4.e.K, a6.e);
                // MONITOREXIT @DISABLED, blocks:[0, 1, 4] lbl33 : MonitorExitStatement: MONITOREXIT : var6_6
                he2 = a6;
            }
        } else {
            he he5 = a6;
            he2 = he5;
            he5.F.f(he5.e);
        }
        he2.e = he2.A(a2, a3, a4, a5);
    }

    private /* synthetic */ sE A(long a22, int a3, int a4, TradeInfo a5) {
        he he2;
        he a6;
        boolean bl;
        String string = a5.aggressorOrderId;
        if (!a5.isBidAggressor) {
            bl = true;
            he2 = a6;
        } else {
            bl = false;
            he2 = a6;
        }
        sE a22 = new sE(string, a4, a3, bl, he2.A(a5.aggressorOrderId), a22);
        if (!a22.e) {
            a6.K = a5.aggressorOrderId;
        }
        return a22;
    }

    public /* synthetic */ void A() {
        he a2;
        a2.e = null;
        a2.m = null;
        a2.i.clear();
        Object object = a2.D;
        synchronized (object) {
            a2.k.clear();
            return;
        }
    }

    private /* synthetic */ boolean A(String a2) {
        he a3;
        if (!a3.I) {
            return false;
        }
        if (a3.K.isEmpty()) {
            return false;
        }
        return a3.K.compareTo(a2) > 0;
    }

    public /* synthetic */ void A(long a22, String a3) {
        he a4;
        if (!a4.G) {
            return;
        }
        sE a22 = (sE)((Object)a4.i.remove(a3));
        if (a22 != null) {
            a4.F.f(a22);
        }
    }
}

