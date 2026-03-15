/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  org.apache.commons.lang3.tuple.Pair
 *  ttw.tradefinder.I
 *  ttw.tradefinder.VA
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.cA
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.zb
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasIcon
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$HorizontalCoordinate
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$PreparedImage
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$RelativeDataVerticalCoordinate
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
import java.util.TreeMap;
import org.apache.commons.lang3.tuple.Pair;
import ttw.tradefinder.I;
import ttw.tradefinder.VA;
import ttw.tradefinder.Y;
import ttw.tradefinder.YD;
import ttw.tradefinder.cA;
import ttw.tradefinder.iH;
import ttw.tradefinder.im;
import ttw.tradefinder.rH;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class zb
implements I {
    private long C;
    private long c;
    private static final int L = 5;
    private boolean E;
    private int b;
    private long l;
    private long j;
    private final cA J;
    private boolean M;
    private iH d;
    private long g;
    private int f;
    private final int a = 10;
    private final YD<VA, im> K;
    private final Object m;
    private final Y F;
    private int e;
    private final TreeMap<Integer, ScreenSpaceCanvas.CanvasIcon> i;
    private double k;
    private final rH I;
    private long G;
    private long D;

    public void dispose() {
        zb a2;
        Object object = a2.m;
        synchronized (object) {
            if (a2.d != null) {
                iH iH2 = a2.d;
                a2.d = null;
                iH2.A();
            }
            return;
        }
    }

    private /* synthetic */ void a() {
        zb a2;
        Object object = a2.m;
        synchronized (object) {
            if (a2.d != null) {
                a2.d.f();
            }
            a2.i.clear();
            return;
        }
    }

    public void A(ScreenSpaceCanvasFactory a2) {
        zb a3;
        Object object = a3.m;
        synchronized (object) {
            if (a3.d != null) {
                a3.d.A();
            }
            if (a2 == null) {
                return;
            }
            a3.d = new iH(a2.createCanvas(ScreenSpaceCanvasFactory.ScreenSpaceCanvasType.RIGHT_OF_TIMELINE));
        }
        a3.f();
    }

    private /* synthetic */ ScreenSpaceCanvas.CanvasIcon A(double a22, double a32) {
        zb a4;
        int n2 = a4.f - 20;
        int n3 = Math.max(40, Math.min(40 + ((VA)a4.K.I).G, n2));
        n2 = Math.max(10, 10 + (int)((double)(n2 - n3) / 100.0 * (double)((VA)a4.K.I).I));
        BufferedImage a22 = a4.J.A(a22, a32, n3, a4.I);
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_PIXEL_ZERO, n2);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate a32 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)ScreenSpaceCanvas.RelativeVerticalCoordinate.VERTICAL_PIXEL_ZERO, a4.b - a22.getHeight() - 5);
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate2 = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, n3);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate relativePixelVerticalCoordinate = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)a32, a22.getHeight());
        return new ScreenSpaceCanvas.CanvasIcon(new ScreenSpaceCanvas.PreparedImage(a22), (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, (ScreenSpaceCanvas.VerticalCoordinate)a32, (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate2, (ScreenSpaceCanvas.VerticalCoordinate)relativePixelVerticalCoordinate);
    }

    public void f() {
        zb a2;
        zb zb2 = a2;
        zb2.a();
        zb2.A();
    }

    public zb(rH a2, Y a3, YD<VA, im> a4) {
        zb a5;
        zb zb2 = a5;
        zb zb3 = a5;
        zb zb4 = a5;
        zb zb5 = a5;
        zb zb6 = a5;
        zb zb7 = a5;
        zb zb8 = a5;
        zb zb9 = a5;
        zb zb10 = a5;
        a5.a = 10;
        a5.m = new Object();
        zb10.d = null;
        zb10.M = false;
        zb9.b = 0;
        zb9.e = 0;
        zb8.f = 0;
        zb8.l = 0L;
        zb7.c = 0L;
        zb7.j = 0L;
        zb6.g = 0L;
        zb6.D = 0L;
        zb5.G = 0L;
        zb5.C = 0L;
        zb4.E = true;
        zb4.k = 0.0;
        zb3.J = new cA();
        zb3.i = new TreeMap();
        zb3.F = a3;
        zb2.K = a4;
        zb2.I = a2;
    }

    public void onHeatmapPriceBottom(long a2) {
        zb a3;
        if (a3.j == a2) {
            return;
        }
        a3.j = a2;
        a3.E = true;
    }

    public void onHeatmapTimeLeft(long a2) {
        a.g = a2;
    }

    public void onRightOfTimelineWidth(int a2) {
        zb a3;
        if (a3.f == a2) {
            return;
        }
        a3.f = a2;
        a3.E = true;
    }

    public void A(boolean a2) {
        zb a3;
        if (a3.M == a2) {
            return;
        }
        a3.M = a2;
        if (!a3.M) {
            a3.a();
            return;
        }
        a3.f();
    }

    private /* synthetic */ ScreenSpaceCanvas.CanvasIcon A(int a22, Pair<Double, Double> a3, double a42) {
        zb a5;
        int n2 = Math.max(1, (int)((long)a5.b / Math.max(1L, a5.c)));
        int n3 = a5.f - 20;
        int n4 = Math.max(40, Math.min(40 + ((VA)a5.K.I).G, n3));
        n3 = Math.max(10, 10 + (int)((double)(n3 - n4) / 100.0 * (double)((VA)a5.K.I).I));
        a3 = a5.J.A(a3, a42, n4, n2, a5.I);
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate a42 = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_PIXEL_ZERO, n3);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate a22 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)new ScreenSpaceCanvas.RelativeDataVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)ScreenSpaceCanvas.RelativeVerticalCoordinate.VERTICAL_DATA_ZERO, (double)(a22 - 1)), n2 / 2 + 1);
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)a42, n4);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate relativePixelVerticalCoordinate = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)a22, n2);
        return new ScreenSpaceCanvas.CanvasIcon(new ScreenSpaceCanvas.PreparedImage(a3), (ScreenSpaceCanvas.HorizontalCoordinate)a42, (ScreenSpaceCanvas.VerticalCoordinate)a22, (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, (ScreenSpaceCanvas.VerticalCoordinate)relativePixelVerticalCoordinate);
    }

    public void A(long a22, int a3) {
        zb a4;
        if (!a4.M) {
            return;
        }
        if (a22 < a4.g || a22 > a4.g + a4.G) {
            return;
        }
        if ((long)a3 < a4.j || (long)a3 > a4.j + a4.c) {
            return;
        }
        Pair a22 = a4.F.A(a3, a4.g, a4.g + a4.G);
        if ((Double)a22.getLeft() > a4.k || (Double)a22.getRight() > a4.k) {
            a4.f();
            return;
        }
        Object object = a4.m;
        synchronized (object) {
            Object object2;
            if (a4.d == null) {
                return;
            }
            ScreenSpaceCanvas.CanvasIcon canvasIcon = (ScreenSpaceCanvas.CanvasIcon)a4.i.get(a3);
            if (canvasIcon != null) {
                object2 = object;
                zb zb2 = a4;
                canvasIcon.setImage(zb2.A(a3, a22, zb2.k).getImage());
            } else {
                zb zb3 = a4;
                zb zb4 = a4;
                canvasIcon = zb4.A(a3, a22, zb4.k);
                zb3.d.A(canvasIcon);
                zb3.i.put(a3, canvasIcon);
                object2 = object;
            }
            // ** MonitorExit[v0] (shouldn't be in output)
            return;
        }
    }

    public void onHeatmapActiveTimeWidth(long a2) {
        a.G = a2;
    }

    public void onHeatmapFullTimeWidth(long a2) {
        zb a3;
        if (a3.l == a2) {
            return;
        }
        a3.l = a2;
        a3.E = true;
    }

    public void onHeatmapFullPixelsWidth(int a2) {
        zb a3;
        if (a3.e == a2) {
            return;
        }
        a3.e = a2;
        a3.E = true;
    }

    public void onMoveEnd() {
        zb a2;
        if (a2.g != a2.D || a2.G != a2.C) {
            zb zb2 = a2;
            zb2.D = zb2.g;
            zb2.C = zb2.G;
            a2.E = true;
        }
        if (a2.E) {
            a2.E = false;
            a2.f();
        }
    }

    public boolean isFEnabled() {
        zb a2;
        return a2.M;
    }

    public void A() {
        zb a2;
        zb zb2 = a2;
        zb2.dispose();
        Object object = zb2.m;
        synchronized (object) {
            a2.i.clear();
            return;
        }
    }

    public void onHeatmapPixelsHeight(int a2) {
        zb a3;
        if (a3.b == a2) {
            return;
        }
        a3.b = a2;
        a3.E = true;
    }

    public boolean A() {
        zb a2;
        if (!a2.M) {
            return false;
        }
        Object object = a2.m;
        synchronized (object) {
            Object object2;
            int n2;
            if (a2.d == null) {
                return false;
            }
            zb zb2 = a2;
            zb2.d.f();
            zb2.i.clear();
            double d2 = 0.0;
            double d3 = 0.0;
            double d4 = 0.0;
            TreeMap<Integer, Object> treeMap = new TreeMap<Integer, Object>();
            int n3 = n2 = (int)a2.j;
            while ((long)n3 <= a2.j + a2.c) {
                object2 = a2.F.A(n2, a2.g, a2.g + a2.G);
                if (object2 != null) {
                    d2 = Math.max((Double)object2.getLeft(), d2);
                    d2 = Math.max((Double)object2.getRight(), d2);
                    treeMap.put(n2, object2);
                    d3 += ((Double)object2.getRight()).doubleValue();
                    d4 += ((Double)object2.getLeft()).doubleValue();
                }
                n3 = ++n2;
            }
            a2.k = d2;
            ScreenSpaceCanvas.CanvasIcon canvasIcon = treeMap.entrySet().iterator();
            block4: while (true) {
                ScreenSpaceCanvas.CanvasIcon canvasIcon2 = canvasIcon;
                while (canvasIcon2.hasNext()) {
                    object2 = canvasIcon.next();
                    ScreenSpaceCanvas.CanvasIcon canvasIcon3 = a2.A(((Integer)object2.getKey()).intValue(), (Pair)object2.getValue(), a2.k);
                    if (canvasIcon3 == null) continue block4;
                    zb zb3 = a2;
                    zb3.d.A(canvasIcon3);
                    zb3.i.put((Integer)object2.getKey(), canvasIcon3);
                    canvasIcon2 = canvasIcon;
                }
                break;
            }
            canvasIcon = a2.A(d3, d4);
            if (canvasIcon != null) {
                a2.d.A(canvasIcon);
            }
        }
        return false;
    }

    public void onHeatmapPriceHeight(long a2) {
        zb a3;
        if (a3.c == a2) {
            return;
        }
        a3.c = a2;
        a3.E = true;
    }
}

