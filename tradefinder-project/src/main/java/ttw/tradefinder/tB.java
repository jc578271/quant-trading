/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  org.apache.commons.lang3.tuple.Pair
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.N
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.jd
 *  ttw.tradefinder.p
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.tB
 *  ttw.tradefinder.vD
 *  ttw.tradefinder.y
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasIcon
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$HorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$PreparedImage
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativeDataHorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativeHorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativePixelHorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativePixelVerticalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativeVerticalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$VerticalCoordinate
 */
package ttw.tradefinder;

import java.awt.image.BufferedImage;
import org.apache.commons.lang3.tuple.Pair;
import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.N;
import ttw.tradefinder.Q;
import ttw.tradefinder.Sh;
import ttw.tradefinder.Xb;
import ttw.tradefinder.jd;
import ttw.tradefinder.p;
import ttw.tradefinder.rH;
import ttw.tradefinder.vD;
import ttw.tradefinder.y;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class tB
extends Xb
implements N,
p,
y {
    private vD F;
    private long e;
    private static int i = 2000000;
    private final boolean k;
    private final Sh I;
    private static int G = 600000;
    private boolean D;

    public boolean A() {
        tB a2;
        return a2.I.A();
    }

    public boolean isFEnabled() {
        return false;
    }

    public void f(boolean a2) {
        tB a3;
        if (a3.I.A() == a2) {
            return;
        }
        tB tB2 = a3;
        boolean bl = a2;
        tB2.I.A(bl);
        tB2.A(bl);
    }

    public void A(long a2) {
        tB a3;
        a3.I.A(a2);
        a3.e = 0L;
    }

    public void f(long a22) {
        tB a3;
        if (!a3.I.A() || a3.k) {
            return;
        }
        Pair a22 = a3.A(a22);
        if (a22 != null && a3.e != (Long)a22.getKey() && (Long)a22.getKey() > a3.I.A()) {
            a3.f((jd)a22.getValue(), ((Long)a22.getKey()).longValue());
        }
    }

    private /* synthetic */ void f(jd a2, long a32) {
        tB a4;
        a4.e = a32;
        a2 = a4.A(a2);
        BufferedImage a32 = (BufferedImage)a2.getLeft();
        ScreenSpaceCanvas.PreparedImage preparedImage = new ScreenSpaceCanvas.PreparedImage(a32);
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)new ScreenSpaceCanvas.RelativeDataHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_DATA_ZERO, a4.e), -(a32.getWidth() / 2));
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate2 = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, a32.getWidth());
        a2 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)ScreenSpaceCanvas.RelativeVerticalCoordinate.VERTICAL_PIXEL_ZERO, ((Integer)a2.getRight()).intValue());
        a32 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)a2, a32.getHeight());
        a2 = new ScreenSpaceCanvas.CanvasIcon(preparedImage, (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, (ScreenSpaceCanvas.VerticalCoordinate)a2, (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate2, (ScreenSpaceCanvas.VerticalCoordinate)a32);
        a4.I.A((ScreenSpaceCanvas.CanvasIcon)a2);
    }

    public void f() {
        tB a2;
        super.f();
        a2.e = 0L;
    }

    public void A(jd a2, long a3) {
        tB a4;
        vD vD2 = a4.F;
        synchronized (vD2) {
            a4.F.A(a3, a2);
        }
        if (!a4.k) {
            return;
        }
        if (!a4.D) {
            return;
        }
        if (!a4.I.A()) {
            return;
        }
        a4.f(a2, a3);
    }

    public void f(long a22, long a3) {
        tB a4;
        vD vD2 = a4.F;
        synchronized (vD2) {
            int a22 = a4.F.f(a22);
            if (a22 == -1) {
                return;
            }
            int n2 = a22 = a22;
            while (n2 < a4.F.A()) {
                long l2 = a4.F.A(a22);
                if (l2 > a3) {
                    return;
                }
                tB tB2 = a4;
                tB2.f(tB2.F.A(a22++), l2);
                n2 = a22;
            }
            return;
        }
    }

    public tB(H a2, rH a3, Q a4) {
        tB a5;
        tB tB2 = a5;
        tB tB3 = a5;
        super(a2, a3, a4);
        a5.F = new vD(G, i, true);
        tB3.e = 0L;
        tB3.D = false;
        tB3.I = new Sh((N)a5);
        tB2.k = a2.A();
        tB2.D = false;
    }

    public void A(boolean a22, boolean a3) {
        boolean bl;
        tB a4;
        if (a22) {
            vD a22 = a4.F;
            synchronized (a22) {
                a4.F.A();
                // MONITOREXIT @DISABLED, blocks:[0, 1, 5] lbl6 : MonitorExitStatement: MONITOREXIT : a
                bl = a3;
            }
        } else {
            bl = a3;
        }
        if (bl) {
            a4.I.a();
        }
    }

    public abstract Pair<BufferedImage, Integer> A(jd var1);

    private /* synthetic */ Pair<Long, jd> A(long a22) {
        tB a3;
        vD vD2 = a3.F;
        synchronized (vD2) {
            int a22 = a3.F.A(a22);
            if (a22 == -1) {
                return null;
            }
            return Pair.of((Object)a3.F.A(a22), (Object)a3.F.A(a22));
        }
    }

    public I A() {
        tB a2;
        return a2.I;
    }

    public void A() {
        tB a2;
        tB tB2 = a2;
        super.A();
        tB2.A(true, true);
        tB2.I.A();
    }
}

