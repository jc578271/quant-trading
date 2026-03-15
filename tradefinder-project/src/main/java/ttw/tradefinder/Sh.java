/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.I
 *  ttw.tradefinder.N
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasIcon
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory$ScreenSpaceCanvasType
 */
package ttw.tradefinder;

import ttw.tradefinder.I;
import ttw.tradefinder.N;
import ttw.tradefinder.iH;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class Sh
implements I {
    private boolean g;
    private long f;
    private final Object a;
    private final N K;
    private iH m;
    private boolean F;
    private long e;
    private boolean i;
    private long k;
    private long I;
    private long G;
    private long D;

    public void a() {
        Sh a2;
        Sh sh = a2;
        sh.f();
        sh.A(sh.k, a2.k + a2.G);
    }

    public long A() {
        Sh a2;
        return a2.k;
    }

    public void A() {
        Sh a2;
        Object object = a2.a;
        synchronized (object) {
            if (a2.m != null) {
                iH iH2 = a2.m;
                a2.m = null;
                iH2.A();
            }
            return;
        }
    }

    public Sh(N a2) {
        Sh a3;
        Sh sh = a3;
        Sh sh2 = a3;
        Sh sh3 = a3;
        Sh sh4 = a3;
        Sh sh5 = a3;
        a3.a = new Object();
        a3.m = null;
        sh5.g = false;
        sh5.k = 0L;
        sh4.e = 0L;
        sh4.G = 0L;
        sh3.f = 0L;
        sh3.D = 0L;
        sh2.I = 0L;
        sh2.F = false;
        sh.i = false;
        sh.K = a2;
    }

    public void onMoveStart() {
        a.i = true;
    }

    public void A(ScreenSpaceCanvasFactory a2) {
        Sh a3;
        Object object = a3.a;
        synchronized (object) {
            if (a3.m != null) {
                a3.m.A();
            }
            if (a2 == null) {
                return;
            }
            a3.m = new iH(a2.createCanvas(ScreenSpaceCanvasFactory.ScreenSpaceCanvasType.HEATMAP));
        }
        a3.a();
    }

    public void onHeatmapFullTimeWidth(long a2) {
        a.D = a2;
    }

    private /* synthetic */ void A(long a2, long a3) {
        Sh a4;
        if (!a4.g) {
            return;
        }
        a4.K.f(a2, a3);
    }

    public void A(boolean a2) {
        Sh a3;
        if (a3.g == a2) {
            return;
        }
        a3.g = a2;
        if (!a3.g) {
            a3.f();
            return;
        }
        Sh sh = a3;
        sh.A(sh.k, a3.k + a3.G);
    }

    public void onMoveEnd() {
        Sh a2;
        a2.i = false;
        if (a2.e == 0L || a2.f == 0L || a2.I == 0L) {
            Sh sh = a2;
            sh.e = sh.k;
            sh.f = sh.G;
            sh.I = sh.D;
            return;
        }
        boolean bl = false;
        if (a2.F) {
            bl = true;
        }
        if (a2.D != a2.I) {
            bl = true;
        }
        if (a2.k < a2.e) {
            bl = true;
        }
        if (a2.k > a2.e && a2.G >= a2.D) {
            bl = true;
        }
        if (a2.f >= a2.I && a2.G < a2.D) {
            bl = true;
        }
        Sh sh = a2;
        sh.F = false;
        sh.e = sh.k;
        sh.f = sh.G;
        sh.I = sh.D;
        if (bl) {
            Sh sh2 = a2;
            sh2.f();
            sh2.A(sh2.k, a2.k + a2.G);
        }
    }

    public void A(ScreenSpaceCanvas.CanvasIcon a2) {
        Sh a3;
        Object object = a3.a;
        synchronized (object) {
            if (!a3.g || a3.m == null) {
                return;
            }
            a3.m.A(a2);
            return;
        }
    }

    public boolean A() {
        Sh a2;
        return a2.g;
    }

    public void onHeatmapActiveTimeWidth(long a2) {
        a.G = a2;
    }

    public void dispose() {
        Sh a2;
        a2.A();
    }

    public void A(long a2) {
        Sh a3;
        if (a3.i) {
            a3.F = true;
        }
    }

    public void onHeatmapTimeLeft(long a2) {
        a.k = a2;
    }

    private /* synthetic */ void f() {
        Sh a2;
        Object object = a2.a;
        synchronized (object) {
            if (a2.m != null) {
                a2.m.f();
            }
            return;
        }
    }
}

