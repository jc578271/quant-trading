/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.ND
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.bI
 *  ttw.tradefinder.gf
 *  ttw.tradefinder.mb
 *  ttw.tradefinder.p
 *  ttw.tradefinder.qC
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.y
 */
package ttw.tradefinder;

import java.util.function.Consumer;
import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.ND;
import ttw.tradefinder.Q;
import ttw.tradefinder.V;
import ttw.tradefinder.Xb;
import ttw.tradefinder.bI;
import ttw.tradefinder.gf;
import ttw.tradefinder.mb;
import ttw.tradefinder.p;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import ttw.tradefinder.y;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class qC
extends Xb
implements p,
y,
V {
    private final mb e;
    private final Object i;
    private boolean k;
    private gf I;
    private static int G = 2000;
    private boolean D;

    public I A() {
        qC a2;
        return a2.e;
    }

    public void A() {
        qC a2;
        qC qC2 = a2;
        super.A();
        qC2.A(true, true);
        qC2.e.A();
    }

    public void f() {
        qC a2;
        super.f();
    }

    private /* synthetic */ void f(boolean a2) {
        qC a3;
        if (a2 == a3.e.A()) {
            return;
        }
        qC qC2 = a3;
        boolean bl = a2;
        qC2.e.A(bl);
        qC2.A(bl);
    }

    public void a(long a2) {
        qC a3;
        if (a3.k) {
            return;
        }
        if (!a3.e.A()) {
            return;
        }
        a3.e.f(true);
    }

    public qC(String a2, H a3, rH a4, Q a5) {
        qC a6;
        qC qC2 = a6;
        qC qC3 = a6;
        super(a3, a4, a5);
        qC3.i = new Object();
        qC3.I = new gf(G);
        qC3.k = false;
        qC2.D = false;
        qC2.e = new mb((V)a6);
        qC2.k = a3.A();
    }

    public void A(boolean a22, boolean a3) {
        boolean bl;
        qC a4;
        if (a22) {
            Object a22 = a4.i;
            synchronized (a22) {
                a4.I.a();
                // MONITOREXIT @DISABLED, blocks:[0, 1, 5] lbl6 : MonitorExitStatement: MONITOREXIT : a
                bl = a3;
            }
        } else {
            bl = a3;
        }
        if (bl) {
            a4.e.f(true);
        }
    }

    public void A(long a22, long a3, Consumer<ND> a4) {
        qC a5;
        if (a22 >= a3) {
            return;
        }
        Object object = a5.i;
        synchronized (object) {
            int a22 = a5.I.f(a22);
            if (a22 == -1) {
                return;
            }
            int n2 = a22 = a22;
            while (n2 < a5.I.f()) {
                if (a5.I.A(a22) > a3) {
                    return;
                }
                a4.accept(a5.I.A(a22++));
                n2 = a22;
            }
            return;
        }
    }

    public boolean A() {
        qC a2;
        return a2.e.A();
    }

    public void A(long a2) {
        qC a3;
        if (!a3.k) {
            return;
        }
        if (!a3.e.A()) {
            return;
        }
        a3.e.f(true);
    }

    public void A(rI a2, boolean a3, boolean a4) {
        if (a2 == rI.v) {
            qC a5;
            a5.A(a3, a4);
        }
    }

    public void A(rI a2, bI a3, boolean a4) {
        if (a2 != rI.v) {
            return;
        }
        if (a3 == bI.I) {
            qC a5;
            a5.f(a4);
            return;
        }
        if (a3 != bI.G) {
            a5.D = a4;
        }
    }

    public void A(rI a2, ND a3) {
        if (a2 == rI.v) {
            qC a4;
            a2 = a4.i;
            synchronized (a2) {
                a4.I.A(a3.k, a3);
            }
            if (a4.k && a4.D) {
                a4.e.f(true);
            }
        }
    }
}

