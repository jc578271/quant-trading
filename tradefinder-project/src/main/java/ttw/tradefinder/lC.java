/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.lC
 *  ttw.tradefinder.m
 *  ttw.tradefinder.mf
 *  ttw.tradefinder.rH
 *  velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener
 *  velox.api.layer1.layers.strategies.interfaces.InvalidateInterface
 *  velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable$Marker
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.function.Consumer;
import ttw.tradefinder.H;
import ttw.tradefinder.Q;
import ttw.tradefinder.Xb;
import ttw.tradefinder.m;
import ttw.tradefinder.mf;
import ttw.tradefinder.n;
import ttw.tradefinder.rH;
import velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener;
import velox.api.layer1.layers.strategies.interfaces.InvalidateInterface;
import velox.api.layer1.layers.strategies.interfaces.OnlineCalculatable;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class lC
extends Xb
implements n {
    private static int K = 60000;
    private boolean m;
    private long F;
    private mf e;
    private final Object i;
    private Consumer<Object> k;
    private InvalidateInterface I;
    private Boolean G;
    private final boolean D;

    public abstract OnlineCalculatable.Marker A(m var1);

    public Consumer<Object> A() {
        lC a2;
        Object object = a2.i;
        synchronized (object) {
            return a2.k;
        }
    }

    public void A(long a22, long a3, int a4, CalculatedResultListener a5) {
        lC a6;
        if (!a6.G.booleanValue()) {
            a5.setCompleted();
            return;
        }
        long l2 = a22 + (long)a4 * a3 + 1L;
        int n2 = 0;
        if (a22 >= l2) {
            a5.setCompleted();
            return;
        }
        a6.F = 0L;
        long l3 = a22;
        ArrayList<OnlineCalculatable.Marker> arrayList = new ArrayList<OnlineCalculatable.Marker>();
        mf mf2 = a6.e;
        synchronized (mf2) {
            long l4;
            int a22 = a6.e.f(a22);
            if (a22 == -1) {
                a5.setCompleted();
                return;
            }
            int n3 = a22 = a22;
            while (n3 < a6.e.f() && !a5.isCancelled() && (l4 = a6.e.A(a22)) <= l2) {
                lC lC2 = a6;
                a6.F = l4;
                while (lC2.F > l3) {
                    ++n2;
                    l3 += a3;
                    a5.provideResponse(arrayList.size() > 0 ? arrayList : null);
                    arrayList = new ArrayList();
                    lC2 = a6;
                }
                lC lC3 = a6;
                arrayList.add(lC3.A(lC3.e.A(a22++)));
                n3 = a22;
            }
            ++n2;
            a5.provideResponse(arrayList.size() > 0 ? arrayList : null);
        }
        if (a6.F == 0L) {
            a6.F = l2;
        }
        int n4 = n2;
        while (true) {
            ++n2;
            if (n4 >= a4) break;
            n4 = n2;
            a5.provideResponse(null);
        }
        a5.setCompleted();
    }

    public void A(Consumer<Object> a2, InvalidateInterface a3) {
        lC a4;
        Object object = a4.i;
        synchronized (object) {
            a4.k = a2;
            a4.I = a3;
            return;
        }
    }

    public lC(H a2, rH a3, Q a4) {
        lC a5;
        lC lC2 = a5;
        lC lC3 = a5;
        lC lC4 = a5;
        super(a2, a3, a4);
        a5.i = new Object();
        a5.e = new mf(K);
        lC4.k = null;
        lC4.I = null;
        lC4.G = Boolean.FALSE;
        lC3.F = 0L;
        lC3.m = false;
        lC2.D = a2.A();
        lC2.m = false;
    }

    public void A(m a2, long a3) {
        lC a4;
        Object object = a4.e;
        synchronized (object) {
            a4.e.A(a3, a2);
        }
        if (!a4.D) {
            return;
        }
        if (!a4.m) {
            return;
        }
        if (!a4.G.booleanValue()) {
            return;
        }
        object = a4.A();
        if (object == null) {
            return;
        }
        object.accept(a4.A(a2));
    }

    public void A(boolean a222, boolean a3) {
        InvalidateInterface a222;
        boolean bl;
        lC a4;
        if (a222) {
            mf a222 = a4.e;
            synchronized (a222) {
                a4.e.A();
                // MONITOREXIT @DISABLED, blocks:[0, 1, 5] lbl6 : MonitorExitStatement: MONITOREXIT : a
                bl = a3;
            }
        } else {
            bl = a3;
        }
        if (bl && (a222 = a4.A()) != null) {
            a222.invalidate();
        }
    }

    public void a() {
        lC a2;
        a2.m = true;
        a2.A(false, true);
    }

    public InvalidateInterface A() {
        lC a2;
        Object object = a2.i;
        synchronized (object) {
            return a2.I;
        }
    }

    public void A() {
        lC a2;
        lC lC2 = a2;
        super.A();
        lC2.A(true, true);
    }

    public boolean A() {
        lC a2;
        return a2.G;
    }

    public void f(boolean a2) {
        lC a3;
        if (a3.G == a2) {
            return;
        }
        boolean bl = a2;
        a3.G = bl;
        a3.A(bl);
    }

    public void A(long a2) {
        a.F = 0L;
    }

    public void f(long a2) {
        lC a3;
        if (!a3.G.booleanValue() || a3.D) {
            return;
        }
        Consumer consumer = a3.A();
        if (consumer == null) {
            return;
        }
        if (a3.F == 0L) {
            a3.F = a2 - 1L;
        }
        ArrayList<OnlineCalculatable.Marker> arrayList = new ArrayList<OnlineCalculatable.Marker>();
        if (a3.F >= a2) {
            return;
        }
        mf mf2 = a3.e;
        synchronized (mf2) {
            long l2;
            lC lC2 = a3;
            int n2 = lC2.e.f(lC2.F + 1L);
            if (n2 == -1) {
                return;
            }
            int n3 = n2 = n2;
            while (n3 < a3.e.f() && (l2 = a3.e.A(n2)) <= a2) {
                lC lC3 = a3;
                lC3.F = l2;
                arrayList.add(lC3.A(lC3.e.A(n2++)));
                n3 = n2;
            }
        }
        if (arrayList.size() > 0) {
            consumer.accept(arrayList);
        }
    }

    public void f() {
        lC a2;
        super.f();
        a2.F = 0L;
    }
}

