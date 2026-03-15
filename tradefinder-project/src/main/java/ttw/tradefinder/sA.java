/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ab
 *  ttw.tradefinder.Ac
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.SA
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.bI
 *  ttw.tradefinder.df
 *  ttw.tradefinder.fD
 *  ttw.tradefinder.jA
 *  ttw.tradefinder.kC
 *  ttw.tradefinder.la
 *  ttw.tradefinder.p
 *  ttw.tradefinder.pe
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.sA
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

import java.awt.Color;
import java.awt.image.BufferedImage;
import ttw.tradefinder.Ab;
import ttw.tradefinder.Ac;
import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.Ld;
import ttw.tradefinder.Q;
import ttw.tradefinder.SA;
import ttw.tradefinder.Xb;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.Z;
import ttw.tradefinder.bI;
import ttw.tradefinder.df;
import ttw.tradefinder.eh;
import ttw.tradefinder.fD;
import ttw.tradefinder.hJ;
import ttw.tradefinder.jA;
import ttw.tradefinder.kC;
import ttw.tradefinder.la;
import ttw.tradefinder.p;
import ttw.tradefinder.pe;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import ttw.tradefinder.y;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class sA
extends Xb
implements p,
y,
Z {
    private final YD<Ac, hJ> d;
    private final Ab g;
    private final Ab f;
    private df a;
    private static final long K = eh.e;
    private static final int m = 40000;
    private static final int F = 2000000;
    private final rI e;
    private final Ab i;
    private final pe k;
    private final SA I;
    private final Object G;
    private final pe D;

    public void A(long a222, long a3) {
        sA a4;
        Object object = a4.G;
        synchronized (object) {
            Object object2;
            int n2;
            int n3 = ((Ac)a4.d.I).I == Ld.I ? ((Ac)a4.d.I).e : ((Ac)a4.d.I).i;
            int n4 = n2 = ((Ac)a4.d.I).I == Ld.I ? ((Ac)a4.d.I).e : ((Ac)a4.d.I).K;
            if (a222 >= a3) {
                return;
            }
            if (((Ac)a4.d.I).k == jA.G) {
                sA sA2 = a4;
                a4.D.A();
                sA2.k.A();
                long l2 = Math.max(1L, (a3 - a222) / (long)kC.G);
                long l3 = a222 / l2 * l2;
                double d2 = (double)(a222 - l3) / (double)l2;
                long l4 = l3 + l2;
                if (((Ac)sA2.d.I).I == Ld.D) {
                    long l5 = 1L;
                    long l6 = -1L;
                    long l7 = 0L;
                    long l8 = 0L;
                    int a222 = a4.a.A(l3);
                    if (a222 != -1) {
                        long l9;
                        int n5 = a222 = a222;
                        while (n5 < a4.a.A() && (l9 = a4.a.A(a222)) <= a3) {
                            fD fD2 = a4.a.A(a222);
                            long l10 = l9;
                            while (l10 > l4) {
                                l4 += l2;
                                l5 = Math.max(l5, l7);
                                l6 = Math.min(l6, l8);
                                sA sA3 = a4;
                                sA3.D.A(l7);
                                sA3.k.A(l8);
                                l7 = 0L;
                                l8 = 0L;
                                l10 = l9;
                            }
                            if (fD2.D) {
                                l8 -= (long)fD2.G;
                            } else {
                                l7 += (long)fD2.G;
                            }
                            n5 = ++a222;
                        }
                    }
                    long l11 = l4;
                    while (l11 <= a3 + l2) {
                        l4 += l2;
                        sA sA4 = a4;
                        sA4.D.A(l7);
                        sA4.k.A(l8);
                        l7 = 0L;
                        l8 = 0L;
                        l11 = l4;
                    }
                    BufferedImage a222 = kC.A((Ac)((Ac)a4.d.I), (long)l5, (pe)a4.D, (double)d2);
                    sA sA5 = a4;
                    a4.I.A(sA5.A(a222, ((Ac)sA5.d.I).i, l3, a3));
                    sA sA6 = a4;
                    a4.I.A(sA6.A(((Ac)sA6.d.I).i, a4.g));
                    BufferedImage bufferedImage = kC.A((Ac)((Ac)a4.d.I), (long)l6, (pe)a4.k, (double)d2);
                    sA sA7 = a4;
                    a4.I.A(sA7.A(bufferedImage, ((Ac)sA7.d.I).K, l3, a3));
                    sA sA8 = a4;
                    a4.I.A(sA8.A(((Ac)sA8.d.I).K, a4.i));
                    object2 = object;
                } else {
                    long l12 = 1L;
                    long l13 = 0L;
                    int n6 = a4.a.A(l3);
                    if (n6 != -1) {
                        long l14;
                        int n7;
                        int n8 = n7 = n6;
                        while (n8 < a4.a.A() && (l14 = a4.a.A(n7)) <= a3) {
                            fD a222 = a4.a.A(n7);
                            long l15 = l14;
                            while (l15 > l4) {
                                l4 += l2;
                                l12 = Math.max(l12, Math.abs(l13));
                                a4.D.A(l13);
                                l13 = 0L;
                                l15 = l14;
                            }
                            fD fD3 = a222;
                            l13 += (long)(a222.D ? -fD3.G : fD3.G);
                            n8 = ++n7;
                        }
                    }
                    long l16 = l4;
                    while (l16 <= a3 + l2) {
                        l12 = Math.max(l12, Math.abs(l13));
                        a4.D.A(l13);
                        l13 = 0L;
                        l16 = l4 += l2;
                    }
                    BufferedImage bufferedImage = kC.A((Ac)((Ac)a4.d.I), (long)l12, (pe)a4.D, (double)d2);
                    sA sA9 = a4;
                    a4.I.A(sA9.A(bufferedImage, ((Ac)sA9.d.I).e, l3, a3));
                    sA sA10 = a4;
                    a4.I.A(sA10.A(((Ac)sA10.d.I).e, a4.f));
                    object2 = object;
                }
            } else {
                int n9;
                int n10;
                int n11;
                int n12;
                int n13;
                int n14;
                block28: {
                    long a222;
                    n14 = a4.a.A(a222);
                    if (n14 == -1) {
                        return;
                    }
                    n13 = 1;
                    n12 = 1;
                    int n15 = n11 = n14;
                    while (n15 < a4.a.A()) {
                        if (a4.a.A(n11) > a3) {
                            n10 = n13;
                            --n11;
                            break block28;
                        }
                        fD fD4 = a4.a.A(n11);
                        if (fD4.D) {
                            n12 = Math.max(n12, fD4.G);
                        } else {
                            n13 = Math.max(n13, fD4.G);
                        }
                        n15 = ++n11;
                    }
                    n10 = n13;
                }
                n13 = (int)((double)n10 * 0.7);
                n12 = (int)((double)n12 * 0.7);
                int n16 = n9 = n14;
                while (n16 < n11) {
                    fD fD5 = a4.a.A(n9);
                    sA sA11 = a4;
                    if (fD5.D) {
                        sA11.I.A(a4.A(fD5, n12, n2, ((Ac)a4.d.I).F, a4.a.A(n9)));
                    } else {
                        sA11.I.A(a4.A(fD5, n13, n3, ((Ac)a4.d.I).a, a4.a.A(n9)));
                    }
                    n16 = ++n9;
                }
                if (n3 == n2) {
                    object2 = object;
                    sA sA12 = a4;
                    a4.I.A(sA12.A(n3, sA12.f));
                } else {
                    sA sA13 = a4;
                    sA sA14 = a4;
                    sA13.I.A(sA14.A(n3, sA14.g));
                    sA sA15 = a4;
                    sA13.I.A(sA15.A(n2, sA15.i));
                    object2 = object;
                }
            }
            // ** MonitorExit[v11] (shouldn't be in output)
            return;
        }
    }

    public sA(H a2, rH a3, la a4, Q a5) {
        sA a6;
        sA sA2 = a6;
        sA sA3 = a6;
        super(a2, a3, a5);
        a6.G = new Object();
        a6.D = new pe(kC.G << 1);
        a6.k = new pe(kC.G << 1);
        a6.a = new df(K, 40000, 2000000, true);
        a6.I = new SA((Z)a6);
        sA3.e = a4.A();
        sA3.d = a2.A(a4.g(), a3.G, a4.C(), (Ya)a4.A());
        sA2.f = new Ab(a4.C());
        sA2.g = new Ab(a4.C() + " ASK");
        a6.i = new Ab(a4.C() + " BID");
    }

    public void A(rI a2, int a3, boolean a4, long a5) {
        sA a6;
        if (a2 != a6.e) {
            return;
        }
        a2 = a6.G;
        synchronized (a2) {
            a6.a.A(a5, a4, Math.abs(a3));
            return;
        }
    }

    public ScreenSpaceCanvas.CanvasIcon A(int a22, Ab a3) {
        sA a4;
        a3 = kC.A((Ab)a3);
        a22 = kC.A((int)a22, (int)((BufferedImage)a3).getHeight(), (int)a4.I.A());
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_PIXEL_ZERO, 10);
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate relativePixelHorizontalCoordinate2 = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, ((BufferedImage)a3).getWidth());
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate a22 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)ScreenSpaceCanvas.RelativeVerticalCoordinate.VERTICAL_PIXEL_ZERO, a22);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate relativePixelVerticalCoordinate = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)a22, ((BufferedImage)a3).getHeight());
        return new ScreenSpaceCanvas.CanvasIcon(new ScreenSpaceCanvas.PreparedImage((BufferedImage)a3), (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate, (ScreenSpaceCanvas.VerticalCoordinate)a22, (ScreenSpaceCanvas.HorizontalCoordinate)relativePixelHorizontalCoordinate2, (ScreenSpaceCanvas.VerticalCoordinate)relativePixelVerticalCoordinate);
    }

    public void f(boolean a2) {
        sA a3;
        if (a3.I.A() == a2) {
            return;
        }
        sA sA2 = a3;
        boolean bl = a2;
        sA2.I.A(bl);
        sA2.A(bl);
    }

    public ScreenSpaceCanvas.CanvasIcon A(fD a222, int a32, int a42, Color a5, long a62) {
        sA a7;
        int a222 = (int)Math.min(10.0 / (double)a32 * (double)a222.G, 10.0);
        BufferedImage a222 = kC.A((Ac)((Ac)a7.d.I), (int)a222, (Color)a5);
        a32 = kC.A((int)a42, (int)a222.getHeight(), (int)a7.I.A());
        ScreenSpaceCanvas.RelativePixelHorizontalCoordinate a42 = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)new ScreenSpaceCanvas.RelativeDataHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_DATA_ZERO, a62), -(a222.getWidth() / 2));
        a5 = new ScreenSpaceCanvas.RelativePixelHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)a42, a222.getWidth());
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate a32 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)ScreenSpaceCanvas.RelativeVerticalCoordinate.VERTICAL_PIXEL_ZERO, a32);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate a62 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)a32, a222.getHeight());
        return new ScreenSpaceCanvas.CanvasIcon(new ScreenSpaceCanvas.PreparedImage(a222), (ScreenSpaceCanvas.HorizontalCoordinate)a42, (ScreenSpaceCanvas.VerticalCoordinate)a32, (ScreenSpaceCanvas.HorizontalCoordinate)a5, (ScreenSpaceCanvas.VerticalCoordinate)a62);
    }

    public void A(rI a2, bI a3, boolean a4) {
        sA a5;
        if (a2 != a5.e) {
            return;
        }
        if (a3 == bI.I) {
            a5.f(a4);
            return;
        }
        if (a3 == bI.G && a4) {
            a5.A(false, true);
        }
    }

    public void A() {
        sA a2;
        sA sA2 = a2;
        super.A();
        sA2.A(true, true);
        sA2.I.A();
    }

    public boolean A() {
        sA a2;
        return a2.I.A();
    }

    public void A(rI a2, boolean a3, boolean a4) {
        sA a5;
        if (a2 != a5.e) {
            return;
        }
        a5.A(a3, a4);
    }

    public void A(long a2) {
        sA a3;
        a3.I.a();
    }

    public ScreenSpaceCanvas.CanvasIcon A(BufferedImage a2, int a32, long a42, long a52) {
        sA a6;
        a32 = kC.A((int)a32, (int)a2.getHeight(), (int)a6.I.A());
        ScreenSpaceCanvas.RelativeDataHorizontalCoordinate a42 = new ScreenSpaceCanvas.RelativeDataHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_DATA_ZERO, a42);
        ScreenSpaceCanvas.RelativeDataHorizontalCoordinate relativeDataHorizontalCoordinate = new ScreenSpaceCanvas.RelativeDataHorizontalCoordinate((ScreenSpaceCanvas.HorizontalCoordinate)ScreenSpaceCanvas.RelativeHorizontalCoordinate.HORIZONTAL_DATA_ZERO, a52);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate a32 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)ScreenSpaceCanvas.RelativeVerticalCoordinate.VERTICAL_PIXEL_ZERO, a32);
        ScreenSpaceCanvas.RelativePixelVerticalCoordinate a52 = new ScreenSpaceCanvas.RelativePixelVerticalCoordinate((ScreenSpaceCanvas.VerticalCoordinate)a32, a2.getHeight());
        return new ScreenSpaceCanvas.CanvasIcon(new ScreenSpaceCanvas.PreparedImage(a2), (ScreenSpaceCanvas.HorizontalCoordinate)a42, (ScreenSpaceCanvas.VerticalCoordinate)a32, (ScreenSpaceCanvas.HorizontalCoordinate)relativeDataHorizontalCoordinate, (ScreenSpaceCanvas.VerticalCoordinate)a52);
    }

    public void A(boolean a22, boolean a3) {
        boolean bl;
        sA a4;
        if (a22) {
            Object a22 = a4.G;
            synchronized (a22) {
                a4.a.A();
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

    public I A() {
        sA a2;
        return a2.I;
    }
}

