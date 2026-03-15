/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ja
 *  ttw.tradefinder.Me
 *  ttw.tradefinder.SD
 *  ttw.tradefinder.rH
 *  velox.api.layer1.layers.utils.OrderBook
 */
package ttw.tradefinder;

import java.util.Iterator;
import ttw.tradefinder.Ja;
import ttw.tradefinder.SD;
import ttw.tradefinder.rH;
import velox.api.layer1.layers.utils.OrderBook;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class Me
extends OrderBook
implements Ja {
    private long e;
    private int i;
    private int k;
    private final rH I;
    private int G;

    public Me(rH a2) {
        Me a3;
        Me me2 = a3;
        Me me3 = a3;
        a3.e = 0L;
        me3.G = 0;
        me3.k = Integer.MAX_VALUE;
        me2.i = 0;
        me2.I = a2;
    }

    public double A(int a2, int a32) {
        Me a4;
        if (a4.k == Integer.MAX_VALUE) {
            return 0.0;
        }
        long l2 = 0L;
        int n2 = 0;
        Me me2 = a4;
        Iterator iterator = a2 = super.levels(false, a4.k + a2, me2.k + a32).values().iterator();
        while (iterator.hasNext()) {
            Long a32 = (Long)a2.next();
            ++n2;
            l2 += a32.longValue();
            iterator = a2;
        }
        if (n2 == 0) {
            return 0.0;
        }
        return (double)l2 / (double)n2 / a4.I.F;
    }

    public int A(int a2) {
        Me a3;
        if (a3.k == Integer.MAX_VALUE || a3.i == Integer.MAX_VALUE) {
            return 0;
        }
        if (a2 >= a3.k) {
            return (int)super.getSizeFor(false, a2, 0L);
        }
        if (a2 <= a3.i) {
            return (int)super.getSizeFor(true, a2, 0L);
        }
        return 0;
    }

    public int a() {
        Me a2;
        if (a2.i == 0) {
            return Integer.MAX_VALUE;
        }
        return a2.i;
    }

    public void A(boolean a2, int a3, int a2222) {
        int a2222;
        Me a4;
        if ((long)a2222 <= 0L) {
            Me me2;
            Me me3 = a4;
            Long a2222 = a2 ? (Long)me3.bidMap.remove(a3) : (Long)me3.askMap.remove(a3);
            if (a2222 == null) {
                a4.G = 0;
                return;
            }
            if (!a2 && a3 == a4.k) {
                Me me4 = a4;
                me2 = me4;
                me4.k = super.getBestAskPriceOrNone();
            } else {
                if (a2 && a3 == a4.i) {
                    Me me5 = a4;
                    me5.i = super.getBestBidPriceOrNone();
                    if (me5.i == Integer.MAX_VALUE) {
                        a4.i = 0;
                    }
                }
                me2 = a4;
            }
            me2.G = a2222.intValue();
            return;
        }
        Me me6 = a4;
        Long a2222 = a2 ? me6.bidMap.put(a3, Long.valueOf(a2222)) : me6.askMap.put(a3, Long.valueOf(a2222));
        if (a2222 == null) {
            Me me7;
            if (a2 && a3 > a4.i) {
                me7 = a4;
                a4.i = a3;
            } else {
                if (!a2 && a3 < a4.k) {
                    a4.k = a3;
                }
                me7 = a4;
            }
            me7.G = 0;
            return;
        }
        a4.G = a2222.intValue();
    }

    public void A(SD a2, int a3, int a42) {
        Me a5;
        if (a5.i == 0) {
            return;
        }
        Iterator iterator = a3 = super.levels(true, a5.i - a42, a5.i - a3).values().iterator();
        while (iterator.hasNext()) {
            Long a42 = (Long)a3.next();
            iterator = a3;
            a2.A(a42.intValue());
        }
    }

    public double f(int a2, int a32) {
        Me a4;
        if (a4.i == Integer.MAX_VALUE) {
            return 0.0;
        }
        long l2 = 0L;
        int n2 = 0;
        Me me2 = a4;
        Iterator iterator = a2 = super.levels(true, a4.i - a32, me2.i - a2).values().iterator();
        while (iterator.hasNext()) {
            Long a32 = (Long)a2.next();
            ++n2;
            l2 += a32.longValue();
            iterator = a2;
        }
        if (n2 == 0) {
            return 0.0;
        }
        return (double)l2 / (double)n2 / a4.I.F;
    }

    public double A(int a2) {
        Me a3;
        return (double)a2 / a3.I.F;
    }

    public int f() {
        Me a2;
        return a2.G;
    }

    public int A() {
        Me a2;
        return a2.k;
    }

    public double A(boolean a2, int a3) {
        Me a4;
        long l2 = a4.getSizeFor(a2, a3, 0L);
        if (l2 == 0L) {
            return 0.0;
        }
        return (double)l2 / a4.I.F;
    }

    public void clear() {
        Me a2;
        super.clear();
    }

    public long A() {
        Me a2;
        Me me2 = a2;
        ++me2.e;
        return me2.e;
    }

    public void f(SD a2, int a3, int a42) {
        Me a5;
        if (a5.k == Integer.MAX_VALUE) {
            return;
        }
        Iterator iterator = a3 = super.levels(false, a5.k + a3, a5.k + a42).values().iterator();
        while (iterator.hasNext()) {
            Long a42 = (Long)a3.next();
            iterator = a3;
            a2.A(a42.intValue());
        }
    }
}

