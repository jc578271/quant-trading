/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Cg
 *  ttw.tradefinder.D
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.bI
 *  ttw.tradefinder.dA
 *  ttw.tradefinder.hF
 *  ttw.tradefinder.kB
 *  ttw.tradefinder.ld
 *  ttw.tradefinder.p
 *  ttw.tradefinder.qc
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.t
 *  ttw.tradefinder.xe
 *  ttw.tradefinder.y
 *  ttw.tradefinder.yf
 *  ttw.tradefinder.z
 */
package ttw.tradefinder;

import java.util.Arrays;
import java.util.Collections;
import java.util.function.Consumer;
import ttw.tradefinder.Cg;
import ttw.tradefinder.D;
import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.Q;
import ttw.tradefinder.R;
import ttw.tradefinder.SE;
import ttw.tradefinder.UC;
import ttw.tradefinder.Xb;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.bI;
import ttw.tradefinder.bg;
import ttw.tradefinder.dA;
import ttw.tradefinder.el;
import ttw.tradefinder.hF;
import ttw.tradefinder.in;
import ttw.tradefinder.ld;
import ttw.tradefinder.lg;
import ttw.tradefinder.mE;
import ttw.tradefinder.p;
import ttw.tradefinder.qc;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import ttw.tradefinder.t;
import ttw.tradefinder.tb;
import ttw.tradefinder.uC;
import ttw.tradefinder.xe;
import ttw.tradefinder.y;
import ttw.tradefinder.yf;
import ttw.tradefinder.z;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class kB
extends Xb
implements p,
y,
z,
R {
    private hF f;
    private Cg a;
    private final YD<tb, in> K;
    private boolean m;
    private static int F = 20000;
    private boolean e;
    private final t i;
    private final ld k;
    private final YD<uC, el> I;
    private final Object G;
    private boolean D;

    public void A(rI a2, boolean a3, boolean a4) {
        kB a5;
        if (a2 == rI.u) {
            a5.A(a3, a4);
            return;
        }
        if (a2 == rI.Ia) {
            if (!a5.m) {
                kB kB2 = a5;
                kB2.B(kB2.k.A());
                return;
            }
            kB kB3 = a5;
            kB3.k.I();
            if (!kB3.k.A()) {
                return;
            }
            if (a5.a != null && a5.D) {
                kB kB4 = a5;
                kB4.k.f((mE)kB4.a);
            }
        }
    }

    public void I() {
        kB a2;
        if (!a2.D) {
            return;
        }
        if (!a2.k.A()) {
            return;
        }
        kB kB2 = a2;
        kB kB3 = a2;
        Cg cg2 = kB2.f(kB2.a == null ? kB3.k.A() : kB3.a.i);
        if (cg2 != null && a2.k.A(cg2.i, cg2.i)) {
            if (a2.a != null) {
                kB kB4 = a2;
                kB4.k.A((mE)kB4.a);
            }
            a2.a = cg2;
            a2.k.f((mE)a2.a);
        }
    }

    public I A() {
        kB a2;
        return a2.k;
    }

    private /* synthetic */ Cg a(long a22) {
        kB a3;
        Object object = a3.G;
        synchronized (object) {
            int a22 = a3.f.A(a22 + 1L);
            if (a22 == -1) {
                return null;
            }
            return a3.f.A(a22);
        }
    }

    public void a(boolean a22) {
        kB a3;
        a3.D = a22;
        if (!a3.k.A()) {
            return;
        }
        if (a3.a != null) {
            kB kB2 = a3;
            kB2.k.A((mE)kB2.a);
        }
        if (!a22) {
            a3.a = null;
            return;
        }
        kB kB3 = a3;
        Cg a22 = kB3.A(kB3.k.A());
        if (a22 != null) {
            a3.a = a22;
            a3.k.f((mE)a3.a);
        }
    }

    public void B(long a2) {
        int n2;
        kB a3;
        if (a3.m) {
            return;
        }
        if (!a3.k.A()) {
            return;
        }
        kB kB2 = a3;
        kB2.k.I();
        if (kB2.a != null) {
            kB kB3 = a3;
            kB3.k.f((mE)kB3.a);
        }
        if ((n2 = a3.f.A(a2 + 1L)) == -1) {
            return;
        }
        int n3 = n2 = n2;
        while (n3 < a3.f.f()) {
            Cg cg2 = a3.f.A(n2);
            if (cg2.f < a2) {
                kB kB4 = a3;
                if (cg2.A(a2) >= ((tb)a3.K.I).A(kB4.e, ((uC)kB4.I.I).K)) {
                    a3.k.f((mE)cg2);
                }
            }
            n3 = ++n2;
        }
        a3.k.A(true);
    }

    public void f(long a22, long a3, Consumer<mE> a4) {
        kB a5;
        if (a22 >= a3) {
            return;
        }
        Object object = a5.G;
        synchronized (object) {
            int a22 = a5.f.A(a22);
            if (a22 == -1) {
                return;
            }
            int n2 = a22 = a22;
            while (n2 < a5.f.f()) {
                if (a5.f.A(a22) > a3) {
                    return;
                }
                a4.accept((mE)a5.f.A(a22++));
                n2 = a22;
            }
            return;
        }
    }

    public void A(boolean a22, boolean a3) {
        kB a4;
        if (a22) {
            Object a22 = a4.G;
            synchronized (a22) {
                a4.f.a();
            }
            if (a4.a != null) {
                a4.k.A((mE)a4.a);
                a4.a = null;
            }
        }
        if (a3) {
            a4.k.A(true);
        }
    }

    private /* synthetic */ void f(boolean a2) {
        kB a3;
        if (a2 == a3.k.A()) {
            return;
        }
        kB kB2 = a3;
        boolean bl = a2;
        kB2.k.f(bl);
        kB2.A(bl);
    }

    public void A(rI a2, bI a3, boolean a4) {
        if (a2 != rI.u) {
            return;
        }
        if (a3 != bI.G) {
            return;
        }
        a.e = a4;
    }

    public void A(rI a2, lg a3) {
        kB a4;
        if (a2 == rI.u) {
            a2 = a4.G;
            synchronized (a2) {
                a4.f.A(a3.i, Cg.A((lg)a3));
            }
            if (a4.m && a4.e && ((tb)a4.K.I).K == SE.I) {
                a4.k.A(true);
                return;
            }
        } else if (a4.m && a4.k.A()) {
            if (a2 == rI.Ia) {
                a4.k.f((mE)((Object)a3));
                return;
            }
            if (a2 == rI.K) {
                a4.k.A((mE)((Object)a3));
            }
        }
    }

    public void A(yf a2) {
        block8: {
            kB a3;
            block6: {
                block7: {
                    if (a2 != yf.J && a2 != yf.Aa) break block6;
                    if (!((tb)a3.K.I).a || !((uC)a3.I.I).m) break block7;
                    kB kB2 = a3;
                    if (!kB2.i.A(kB2.e.D)) break block8;
                }
                kB kB3 = a3;
                kB3.f(((tb)kB3.K.I).a && ((uC)a3.I.I).m);
                if (((tb)a3.K.I).a && ((uC)a3.I.I).m) {
                    a3.k.A(true);
                    return;
                }
                break block8;
            }
            if (a2 == yf.Ja || a2 == yf.Ea) {
                a3.k.A(true);
                return;
            }
            if (a2 == yf.v || a2 == yf.S) {
                if (a3.m || !a3.k.A()) {
                    return;
                }
                kB kB4 = a3;
                kB4.B(kB4.k.A());
            }
        }
    }

    @Override
    public boolean A() {
        kB a2;
        return a2.k.A();
    }

    private /* synthetic */ Cg f(long a22) {
        kB a3;
        Object object = a3.G;
        synchronized (object) {
            int a22 = a3.f.f(a22 - 1L);
            if (a22 == -1) {
                return null;
            }
            return a3.f.A(a22);
        }
    }

    @Override
    public void f() {
        kB a2;
        super.f();
    }

    public void A(long a2) {
        kB a3;
        if (!a3.m) {
            return;
        }
        if (!a3.k.A()) {
            return;
        }
        a3.k.A(true);
    }

    private /* synthetic */ Cg A(long a22) {
        kB a3;
        Object object = a3.G;
        synchronized (object) {
            int a22 = a3.f.f(a22);
            if (a22 == -1) {
                return null;
            }
            return a3.f.A(a22);
        }
    }

    public kB(String a2, H a3, rH a4, Q a5) {
        kB a6;
        kB kB2 = a6;
        kB kB3 = a6;
        kB kB4 = a6;
        super(a3, a4, a5);
        a6.G = new Object();
        a6.f = new hF(F);
        kB4.D = false;
        kB4.a = null;
        kB3.m = false;
        kB3.e = false;
        kB2.K = a3.A(a2, a4.G, (Ya)new tb());
        kB2.I = a3.A(a2, a4.G, (Ya)new uC(a4.F));
        kB kB5 = a6;
        a6.K.A((z)kB5);
        kB5.I.A((z)a6);
        a6.k = new ld(a4, a6.K, (R)a6, a3.A());
        a5 = new qc(xe.A((Object)"\u001aT6Y7S\u001dO6X i \\1X"), Collections.singletonList(UC.i), Arrays.asList(bg.e), a3.A());
        a6.i = a5.A((D)new dA(a6, a4, a3, a2));
        a6.m = a3.A();
        a6.k.f(((tb)a6.K.I).a && ((uC)a6.I.I).m);
    }

    @Override
    public void A() {
        kB a2;
        kB kB2 = a2;
        kB kB3 = a2;
        super.A();
        kB2.A(true, true);
        kB3.k.A();
        kB2.i.A();
        kB2.K.f((z)a2);
        kB2.I.f((z)a2);
    }

    public void a() {
        kB a2;
        if (!a2.D) {
            return;
        }
        if (!a2.k.A()) {
            return;
        }
        kB kB2 = a2;
        kB kB3 = a2;
        Cg cg2 = kB2.a(kB2.a == null ? kB3.k.A() : kB3.a.i);
        if (cg2 != null && a2.k.A(cg2.i, cg2.i)) {
            if (a2.a != null) {
                kB kB4 = a2;
                kB4.k.A((mE)kB4.a);
            }
            a2.a = cg2;
            a2.k.f((mE)a2.a);
        }
    }
}

