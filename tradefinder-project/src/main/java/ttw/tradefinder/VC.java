/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.MB
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.VC
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.Xc
 *  ttw.tradefinder.YA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.ab
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.mc
 *  ttw.tradefinder.p
 *  ttw.tradefinder.qc
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.ra
 *  ttw.tradefinder.t
 *  ttw.tradefinder.vD
 *  ttw.tradefinder.vE
 *  ttw.tradefinder.w
 *  ttw.tradefinder.x
 *  ttw.tradefinder.y
 *  ttw.tradefinder.yf
 *  ttw.tradefinder.z
 *  velox.api.layer1.data.TradeInfo
 */
package ttw.tradefinder;

import java.util.Arrays;
import java.util.HashMap;
import java.util.Map;
import java.util.function.BiConsumer;
import ttw.tradefinder.Di;
import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.MB;
import ttw.tradefinder.Q;
import ttw.tradefinder.SE;
import ttw.tradefinder.UC;
import ttw.tradefinder.Xb;
import ttw.tradefinder.Xc;
import ttw.tradefinder.YA;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.ab;
import ttw.tradefinder.dD;
import ttw.tradefinder.hK;
import ttw.tradefinder.mc;
import ttw.tradefinder.og;
import ttw.tradefinder.p;
import ttw.tradefinder.qc;
import ttw.tradefinder.rH;
import ttw.tradefinder.ra;
import ttw.tradefinder.re;
import ttw.tradefinder.t;
import ttw.tradefinder.tg;
import ttw.tradefinder.vD;
import ttw.tradefinder.vE;
import ttw.tradefinder.w;
import ttw.tradefinder.x;
import ttw.tradefinder.y;
import ttw.tradefinder.yf;
import ttw.tradefinder.z;
import velox.api.layer1.data.TradeInfo;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class VC
extends Xb
implements p,
w,
y,
z,
x {
    private boolean f;
    private final YA a;
    private int K;
    private boolean m;
    private int F;
    private final String e;
    private final Xc i;
    private final t k;
    private final ab I;
    private final YD<ra, hK> G;
    private final Di D;

    public boolean A() {
        VC a2;
        return a2.G.A() && ((ra)a2.G.I).I;
    }

    public static /* synthetic */ rH f(VC a2) {
        return a2.e;
    }

    private /* synthetic */ vE A(MB a2) {
        VC a3;
        String string = a2.D ? vD.A((Object)"\u0005g#") : re.A((Object)":\u0001\u0010");
        String string2 = a3.e.a(a2.G.I);
        return a2.G.F.A(a3.e.m, string + " Price reached", "at " + string2, string2, a2.D);
    }

    public void A(yf a2) {
        if (a2 == yf.ma || a2 == yf.Aa) {
            VC a3;
            a3.I();
        }
    }

    private /* synthetic */ void I() {
        VC a2;
        VC vC2 = a2;
        vC2.i.f();
        if (!vC2.A()) {
            return;
        }
        ((ra)a2.G.I).A((BiConsumer)new mc(a2));
    }

    public static /* synthetic */ rH A(VC a2) {
        return a2.e;
    }

    public void A() {
        VC a2;
        VC vC2 = a2;
        super.A();
        vC2.k.A();
        vC2.D.A();
        vC2.G.f((z)a2);
        vC2.i.A();
        vC2.a.A();
    }

    private /* synthetic */ String A(MB a2) {
        VC a3;
        String string = a2.D ? vD.A((Object)"\u0017\\\u000eM\u0002.\u0006B\u0002\\\u0013.\u0005G\u0003") : re.A((Object)"+ 21>R:>> /R:!0");
        return String.format(vD.A((Object)"+4.e+4,k.&zg+4"), string, a2.G.e, a3.e.a(a2.G.I));
    }

    public void A(long a2, int a3, int a4, TradeInfo a5) {
        VC vC2;
        VC a6;
        if (!a6.A()) {
            return;
        }
        if (a5.isBidAggressor) {
            if (a6.K == a3) {
                return;
            }
            vC2 = a6;
            a6.K = a3;
        } else {
            if (a6.F == a3) {
                return;
            }
            vC2 = a6;
            a6.F = a3;
        }
        if (vC2.K == 0 || a6.F == 0) {
            return;
        }
        if (!a6.f || !a6.m) {
            return;
        }
        VC vC3 = a6;
        vC3.a.A(vC3.F);
        VC vC4 = a6;
        vC3.i.A(vC4.K, vC4.F);
    }

    public void A(boolean a2, boolean a3) {
        VC a4;
        a4.a.a();
    }

    public void a() {
        VC a2;
        if (a2.m) {
            return;
        }
        a2.m = true;
        a2.A(false, true);
    }

    private /* synthetic */ Map<String, String> A(MB a2) {
        VC a3;
        HashMap<String, String> hashMap = new HashMap<String, String>();
        hashMap.put(re.A((Object)"<\u001a\u001f\u001e"), a2.G.e);
        hashMap.put(vD.A((Object)"\u0017|.m\""), a3.e.a(a2.G.I));
        return hashMap;
    }

    public void f() {
        VC a2;
        super.f();
        a2.m = false;
    }

    public I A() {
        VC a2;
        return a2.a;
    }

    public static /* synthetic */ H A(VC a2) {
        return a2.G;
    }

    public VC(String a2, H a3, rH a4, ab a5, Q a6) {
        VC a7;
        VC vC2 = a7;
        VC vC3 = a7;
        VC vC4 = a7;
        VC vC5 = a7;
        super(a3, a4, a6);
        vC5.f = false;
        vC5.m = false;
        vC4.K = 0;
        vC4.F = 0;
        vC3.e = a2;
        vC3.I = a5;
        vC2.D = vC2.A();
        vC2.G = a3.A(vC2.e, a4.G, (Ya)new ra());
        a7.G.A((z)a7);
        a7.a = new YA(a7.G);
        a7.i = new Xc((x)a7);
        a2 = new qc(re.A((Object)"\"\t\u001b\u0018\u0017:\u001e\u001e\u0000\u000f\u00016\u0017\b\u0001\u001e\u001c\u001c\u0017\t"), Arrays.asList(UC.i, UC.F, UC.G), a3.A());
        a7.k = a2.A(null);
        a7.f = a3.A();
        a7.m = false;
        a7.f();
        a7.I();
    }

    public void A(MB a2) {
        VC a3;
        if (!(a3.f && a3.m && a2.G.m)) {
            return;
        }
        ((ra)a3.G.I).A(a2.I);
        VC vC2 = a3;
        VC vC3 = a3;
        VC vC4 = a3;
        vC3.G.A(vC3.e, vC4.e.G, a3.G);
        vC4.G.A(yf.ma);
        vC2.I.B(a3.e.G);
        String string = vC2.A(a2);
        vE vE2 = vC2.A(a2);
        if (!string.isEmpty() && a2.G.i == SE.I) {
            VC vC5 = a3;
            vC5.A(vC5.e.G, string);
        }
        if (vE2.k != dD.I) {
            a3.A(vE2);
        }
        if (a2.G.G == SE.I) {
            VC vC6 = a3;
            if (vC6.k.A(vC6.e.D)) {
                VC vC7;
                tg tg2;
                Di di2 = a3.D;
                if (a2.D) {
                    tg2 = tg.I;
                    vC7 = a3;
                } else {
                    tg2 = tg.D;
                    vC7 = a3;
                }
                di2.A(og.d, tg2, vC7.A(a2));
            }
        }
    }
}

