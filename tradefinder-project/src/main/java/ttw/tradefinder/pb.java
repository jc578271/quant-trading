/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ce
 *  ttw.tradefinder.D
 *  ttw.tradefinder.Gd
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.jB
 *  ttw.tradefinder.pb
 *  ttw.tradefinder.qc
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.t
 *  ttw.tradefinder.vC
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasIcon
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$HorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$PreparedImage
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativeHorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativePixelHorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativePixelVerticalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativeVerticalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$VerticalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory$ScreenSpaceCanvasType
 */
package ttw.tradefinder;

import java.awt.image.BufferedImage;
import java.util.Arrays;
import ttw.tradefinder.Ce;
import ttw.tradefinder.D;
import ttw.tradefinder.Gd;
import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.UC;
import ttw.tradefinder.iH;
import ttw.tradefinder.jB;
import ttw.tradefinder.qc;
import ttw.tradefinder.rH;
import ttw.tradefinder.t;
import ttw.tradefinder.vC;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class pb
implements I {
    private ScreenSpaceCanvas.CanvasIcon g;
    private final t f;
    private final jB a;
    private int K;
    private static final int m = 300;
    private static final int F = 10;
    private final Object e;
    private boolean i;
    private static final int k = 300;
    private int I;
    private iH G;
    private static final int D = 200;

    private /* synthetic */ void a() {
        pb a2;
        Object object = a2.e;
        synchronized (object) {
            if (a2.G != null) {
                a2.G.f();
            }
            return;
        }
    }

    public void A() {
        pb a2;
        pb pb2 = a2;
        pb2.dispose();
        pb2.f.A();
    }

    private /* synthetic */ ScreenSpaceCanvas.CanvasIcon A() {
        pb a2;
        int n2 = Math.max(200, a2.I - 300 - 300);
        BufferedImage bufferedImage = a2.a.A(Ce.A((Object)"J1N<MpN9G2GpA<M%FpL?V5Q"), n2);
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_PIXEL_ZERO, a2.I - n2 / 2);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate relativePixelVerticalCoordinate = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)ScreenSpaceCanvas.RelativeVerticalCoordinate.VERTICAL_PIXEL_ZERO, a2.K - 10 - bufferedImage.getHeight());
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate2 = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, bufferedImage.getWidth());
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate relativePixelVerticalCoordinate2 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)relativePixelVerticalCoordinate, bufferedImage.getHeight());
        return new ScreenSpaceCanvas.CanvasIcon(new ScreenSpaceCanvas.PreparedImage(bufferedImage), (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, (ScreenSpaceCanvas.VerticalCoordinate)relativePixelVerticalCoordinate, (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate2, (ScreenSpaceCanvas.VerticalCoordinate)relativePixelVerticalCoordinate2);
    }

    public void A(ScreenSpaceCanvasFactory a2) {
        pb a3;
        Object object = a3.e;
        synchronized (object) {
            if (a3.G != null) {
                a3.G.A();
            }
            if (a2 == null) {
                return;
            }
            a3.G = new iH(a2.createCanvas(ScreenSpaceCanvasFactory.ScreenSpaceCanvasType.HEATMAP));
        }
        a3.f();
    }

    public boolean A() {
        pb a2;
        return a2.i;
    }

    public void dispose() {
        pb a2;
        Object object = a2.e;
        synchronized (object) {
            if (a2.G != null) {
                iH iH2 = a2.G;
                a2.G = null;
                iH2.A();
            }
            return;
        }
    }

    public void onHeatmapFullPixelsWidth(int a2) {
        pb a3;
        Object object = a3.e;
        synchronized (object) {
            if (a3.I == a2) {
                return;
            }
            a3.I = a2;
            a3.f();
            return;
        }
    }

    private /* synthetic */ void f() {
        pb a2;
        pb pb2 = a2;
        pb2.a();
        Object object = pb2.e;
        synchronized (object) {
            if (!a2.i || a2.G == null) {
                return;
            }
            pb pb3 = a2;
            pb3.g = pb3.A();
            pb3.G.A(a2.g);
            return;
        }
    }

    public pb(H a2, rH a3) {
        pb a4;
        pb pb2 = a4;
        pb pb3 = a4;
        a4.e = new Object();
        a4.G = null;
        pb3.g = null;
        pb3.i = false;
        pb2.K = 0;
        pb2.I = 0;
        pb2.a = new jB();
        a2 = new qc(vC.A((Object)"!~\rg\u0006\\\rf\u0007a4s\u000e{\u0006{\u0016k"), Arrays.asList(UC.i, UC.F, UC.G), a2.A());
        a4.f = a2.A((D)new Gd(a4));
        a4.i = true;
        a4.g = a4.A();
    }

    public void A(boolean a2) {
        pb a3;
        if (a3.i == a2) {
            return;
        }
        a3.i = a2;
        if (!a3.i) {
            a3.a();
            return;
        }
        a3.f();
    }

    public void onHeatmapPixelsHeight(int a2) {
        pb a3;
        Object object = a3.e;
        synchronized (object) {
            if (a3.K == a2) {
                return;
            }
            a3.K = a2;
            a3.f();
            return;
        }
    }
}

