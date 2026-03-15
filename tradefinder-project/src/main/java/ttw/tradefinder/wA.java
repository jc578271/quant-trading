/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  org.apache.commons.lang3.time.DurationFormatUtils
 *  ttw.tradefinder.Bb
 *  ttw.tradefinder.MB
 *  ttw.tradefinder.OC
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.wA
 *  ttw.tradefinder.wC
 *  ttw.tradefinder.zD
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Font;
import java.awt.Graphics2D;
import java.time.Duration;
import java.util.Iterator;
import org.apache.commons.lang3.time.DurationFormatUtils;
import ttw.tradefinder.Bb;
import ttw.tradefinder.Ie;
import ttw.tradefinder.MB;
import ttw.tradefinder.OC;
import ttw.tradefinder.YB;
import ttw.tradefinder.mE;
import ttw.tradefinder.rH;
import ttw.tradefinder.wC;
import ttw.tradefinder.zD;

public class wA
extends YB {
    private static Font m;
    private static int F;
    private static int e;
    private static Font i;
    private int k;
    private int I;
    private static int G;
    private Bb D;

    public void a(int a2, int a3, Graphics2D a4) {
        wA a5;
        Iterator iterator;
        a3 += F;
        Iterator iterator2 = iterator = a5.D.iterator();
        while (iterator2.hasNext()) {
            wC wC2 = (wC)iterator.next();
            wC2.A(a2 + e, a3, a4);
            a3 += G + wC2.A();
            iterator2 = iterator;
        }
    }

    public int f() {
        wA a2;
        return a2.k;
    }

    static {
        F = 4;
        G = 3;
        e = 7;
        i = new Font(Ie.A((Object)"^CvPs"), 1, 14);
        m = new Font(MB.A((Object)"KGcTf"), 1, 11);
    }

    public void f(int a2, int a3, Graphics2D a4) {
        wA a5;
        Iterator iterator;
        a3 += F;
        Iterator iterator2 = iterator = a5.D.iterator();
        while (iterator2.hasNext()) {
            wC wC2 = (wC)iterator.next();
            wC2.A(a2, a3, a5.I - e, a4);
            a3 += G + wC2.A();
            iterator2 = iterator;
        }
    }

    public int A() {
        wA a2;
        return a2.I;
    }

    public wA(mE a2, rH a3, int a2222, int a4, boolean a5, long a6, OC a7, int a82) {
        wA wA2;
        OC oC2;
        Font font;
        Font a82;
        float f2;
        wA a9;
        wA wA3 = a9;
        a9.D = new Bb();
        wA3.I = 0;
        wA3.k = 0;
        Color color = a2.K ? zD.m : zD.g;
        float f3 = (float)Math.min(Math.max(a82, 8), 18) / 10.0f;
        if (f2 <= 1.0f) {
            a82 = i.deriveFont(14.0f * f3);
            font = m;
            oC2 = a7;
        } else if (f3 <= 1.2f) {
            a82 = i;
            font = m.deriveFont(11.0f * f3);
            oC2 = a7;
        } else if (f3 <= 1.4f) {
            a82 = i.deriveFont(12.0f * f3);
            font = m.deriveFont(11.0f * f3);
            oC2 = a7;
        } else {
            a82 = i.deriveFont(0, 12.0f * f3);
            font = m.deriveFont(0, 11.0f * f3);
            oC2 = a7;
        }
        if (oC2 == OC.k) {
            wA wA4 = a9;
            wA2 = wA4;
            wA4.D.A(new wC(a3.A(a2222), a82, color));
        } else if (a7 == OC.G) {
            Object a2222 = a3.A(a2222);
            if (a5) {
                a2222 = (String)a2222 + " / " + a3.A((int)a2.I);
            }
            wA wA5 = a9;
            wA2 = wA5;
            wA5.D.A(new wC((String)a2222, a82, color));
            a9.D.A(new wC(a9.A(Duration.ofNanos(a6 - a2.F)), font, color));
        } else {
            int a2222;
            Object a2222 = a3.A(a2222);
            if (a5) {
                a2222 = (String)a2222 + " / " + a3.A((int)a2.I);
            }
            if (a2.m != 0) {
                a2222 = (String)a2222 + " - " + a3.A(a2.m);
            }
            wA wA6 = a9;
            wA6.D.A(new wC((String)a2222, a82, color));
            a9.D.A(new wC(a9.A(Duration.ofNanos(a6 - a2.F)), font, color));
            wA6.D.A(new wC("at " + a3.a(a4), a82, color));
            wA2 = a9;
        }
        wA2.I = wA2.D.f() + 2 * e;
        a9.k = a9.D.A() + 2 * F + (a9.D.size() - 1) * G;
    }

    private /* synthetic */ String A(Duration a2) {
        if (a2.toHours() > 0L) {
            return DurationFormatUtils.formatDuration((long)a2.toMillis(), (String)MB.A((Object)"}-]-\u0015g\u0012g\u0012*F-F-"));
        }
        if (a2.toMinutes() > 0L) {
            return DurationFormatUtils.formatDuration((long)a2.toMillis(), (String)Ie.A((Object)"r\u0016r\u0016?B8B8"));
        }
        return DurationFormatUtils.formatDuration((long)a2.toMillis(), (String)MB.A((Object)"F-F-"));
    }

    public void A(int a2, int a3, Graphics2D a4) {
        wA a5;
        Iterator iterator;
        a3 += F;
        Iterator iterator2 = iterator = a5.D.iterator();
        while (iterator2.hasNext()) {
            wC wC2 = (wC)iterator.next();
            wC2.f(a2, a3, a5.I, a4);
            a3 += G + wC2.A();
            iterator2 = iterator;
        }
    }
}

