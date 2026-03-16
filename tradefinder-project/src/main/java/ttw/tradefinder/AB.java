/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  org.apache.commons.lang3.tuple.Pair
 *  ttw.tradefinder.AB
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.bD
 *  ttw.tradefinder.rH
 *  velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener
 *  velox.api.layer1.layers.strategies.interfaces.InvalidateInterface
 *  velox.api.layer1.messages.indicators.IndicatorLineStyle
 */
package ttw.tradefinder;
 
import java.util.function.Consumer;
import org.apache.commons.lang3.tuple.Pair;
import ttw.tradefinder.H;
import ttw.tradefinder.Q;
import ttw.tradefinder.Xb;
import ttw.tradefinder.bD;
import ttw.tradefinder.n;
import ttw.tradefinder.rH;
import velox.api.layer1.layers.strategies.interfaces.CalculatedResultListener;
import velox.api.layer1.layers.strategies.interfaces.InvalidateInterface;
import velox.api.layer1.messages.indicators.IndicatorLineStyle;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class AB
extends Xb
implements n {
    private bD f;
    private Boolean a;
    private final boolean K;
    private static int m = 2000000;
    private Double F;
    private long e;
    private static int i = 600000;
    private InvalidateInterface k;
    private final Object I;
    private Consumer<Object> G;
    private boolean D;

    public void A(long a2, long a3, int a4, CalculatedResultListener a5) {
        int n2;
        if (!this.a.booleanValue()) {
            a5.setCompleted();
            return;
        }
        long l2 = 0L;
        int n3 = n2 = 1;
        while (n3 <= a4) {
            long l3 = a2 + a3 * (long)n2;
            if (a5.isCancelled()) break;
            Pair pair = this.getPairAtTimestamp(l3);
            if (pair != null) {
                l2 = (Long)pair.getKey();
                a5.provideResponse(pair.getValue());
            } else {
                a5.provideResponse(null);
            }
            n3 = ++n2;
        }
        this.e = l2;
        a5.setCompleted();
    }

    public void f(boolean a2) {
        if (this.a == a2) {
            return;
        }
        boolean bl = a2;
        this.a = bl;
        this.A(bl);
    }

    public void f() {
        super.f();
        synchronized (this.f) {
            this.f.clear();
            this.F = Double.NaN;
        }
        this.e = 0L;
    }

    public boolean isFEnabled() {
        return false;
    }

    public void I() {
        synchronized (this.f) {
            this.f.clear();
            this.F = Double.NaN;
            return;
        }
    }

    public AB(H a2, rH a3, Q a4) {
        super(a2, a3, a4);
        this.I = new Object();
        this.f = new bD(i, m, true);
        this.G = null;
        this.k = null;
        this.F = Double.NaN;
        this.a = Boolean.FALSE;
        this.e = 0L;
        this.D = false;
        this.K = a2.A();
        this.D = false;
    }

    public IndicatorLineStyle getIndicatorLineStyle() {
        return IndicatorLineStyle.DEFAULT;
    }

    public void A(long a2) {
        this.F = Double.NaN;
        this.e = 0L;
    }

    private Pair<Long, Double> getPairAtTimestamp(long a22) {
        synchronized (this.f) {
            int n = this.f.f(a22);
            if (n == -1) {
                return null;
            }
            return Pair.of((Object)this.f.getTimestamp(n), (Object)this.f.getValue(n));
        }
    }

    public void A(boolean bl, boolean bl2) {
        InvalidateInterface invalidateInterface;
        boolean bl3;
        if (bl) {
            synchronized (this.f) {
                this.f.clear();
                this.F = Double.NaN;
                bl3 = bl2;
            }
        } else {
            bl3 = bl2;
        }
        if (bl3 && (invalidateInterface = this.getInvalidateInterface()) != null) {
            invalidateInterface.invalidate();
        }
    }

    public InvalidateInterface getInvalidateInterface() {
        synchronized (this.I) {
            return this.k;
        }
    }

    public void A(Consumer<Object> a2, InvalidateInterface a3) {
        synchronized (this.I) {
            this.G = a2;
            this.k = a3;
            return;
        }
    }

    public Consumer<Object> getConsumer() {
        synchronized (this.I) {
            return this.G;
        }
    }

    public void a() {
        this.D = true;
        this.A(false, true);
    }

    public void f(long l) {
        if (!this.a.booleanValue() || this.K) {
            return;
        }
        Pair pair = this.getPairAtTimestamp(l);
        if (pair != null && this.e != (Long)pair.getKey()) {
            this.e = (Long)pair.getKey();
            Object object = pair.getValue();
            Consumer consumer = this.getConsumer();
            if (consumer == null) {
                return;
            }
            consumer.accept(object);
        }
    }

    public void clear() {
        super.A();
        this.A(true, true);
    }

    public boolean isEnabled() {
        return this.a;
    }

    public void A(double d, long l) {
        if (Double.isNaN(this.F)) {
            this.F = d;
        } else {
            if (this.F == d) {
                return;
            }
            this.F = d;
        }
        synchronized (this.f) {
            this.f.A(l, d);
        }
        if (!this.K) {
            return;
        }
        if (!this.D) {
            return;
        }
        if (!this.a.booleanValue()) {
            return;
        }
        Consumer consumer = this.getConsumer();
        if (consumer == null) {
            return;
        }
        consumer.accept(d);
    }
}

