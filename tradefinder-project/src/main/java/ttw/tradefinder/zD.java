/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.TB
 *  ttw.tradefinder.iE
 *  ttw.tradefinder.jB
 *  ttw.tradefinder.tF
 *  ttw.tradefinder.zD
 *  velox.gui.colors.Colors
 */
package ttw.tradefinder;

import java.awt.Color;
import ttw.tradefinder.TB;
import ttw.tradefinder.iE;
import ttw.tradefinder.jB;
import ttw.tradefinder.tF;
import velox.gui.colors.Colors;

/*
 * Exception performing whole class analysis ignored.
 */
public class zD {
    public static Color J;
    public static Color M;
    public static Color d;
    public static Color g;
    public static Color f;
    public static Color a;
    public static Color K;
    public static Color m;
    public static Color F;
    public static Color e;
    public static Color i;
    public static Color k;
    public static Color I;
    public static Color G;
    public static Color D;

    /*
     * Enabled aggressive block sorting
     */
    public static /* synthetic */ Color f(TB a2, int a3) {
        switch (iE.D[a2.ordinal()]) {
            case 1: {
                return zD.f((Color)m, (int)a3);
            }
            case 2: {
                return zD.f((Color)I, (int)a3);
            }
            case 3: {
                return zD.f((Color)g, (int)a3);
            }
            case 4: {
                return zD.f((Color)e, (int)a3);
            }
            case 5: {
                return zD.f((Color)f, (int)a3);
            }
            case 6: {
                return zD.f((Color)F, (int)a3);
            }
            case 7: {
                return zD.f((Color)d, (int)a3);
            }
        }
        return zD.f((Color)G, (int)a3);
    }

    public /* synthetic */ zD() {
        zD a2;
    }

    public static /* synthetic */ Color f(Color a2, int a3) {
        return new Color(a2.getRed(), a2.getGreen(), a2.getBlue(), a3);
    }

    public static /* synthetic */ Color A(Color a2, int a3) {
        return new Color(a2.getRed(), a2.getGreen(), a2.getBlue(), a3);
    }

    static {
        K = Color.decode(jB.A((Object)"\u000bQnQnQn"));
        G = Color.decode(tF.A((Object)" e1e3e2"));
        D = Color.decode(jB.A((Object)"\u000bUnUnUn"));
        k = Color.decode(tF.A((Object)" \u0013F\u0013F\u0013F"));
        m = Color.decode(jB.A((Object)"\u000b#\u001bVi/j"));
        I = Color.decode(tF.A((Object)" o4\u00135\u0015@"));
        g = Color.decode(jB.A((Object)"\u000bQ\u0011#\u0019#\u001c"));
        e = Color.decode(tF.A((Object)" \u0010G\u00176\u0017;"));
        i = Color.decode(jB.A((Object)"\u000b\"lV\u001bRi"));
        f = Color.decode(tF.A((Object)" a4nG\u0017:"));
        M = Color.decode(jB.A((Object)"\u000b'\u0018$\u001bRm"));
        F = Color.decode(tF.A((Object)" \u0010EoEg@"));
        d = Color.decode(jB.A((Object)"\u000bQjS\u0010 n"));
        a = Color.decode(tF.A((Object)" \u0010E\u00124f3"));
        J = Colors.TRANSPARENT;
    }

    public static /* synthetic */ Color A(TB a2, int a3) {
        switch (iE.D[a2.ordinal()]) {
            case 1: 
            case 3: 
            case 5: 
            case 6: {
                while (false) {
                }
                return zD.f((Color)K, (int)a3);
            }
            case 2: 
            case 4: 
            case 7: {
                return zD.f((Color)G, (int)a3);
            }
        }
        return zD.f((Color)K, (int)a3);
    }

    public static /* synthetic */ Color A(Color a2, Color a3) {
        return new Color(a2.getRed(), a2.getGreen(), a2.getBlue(), a3.getAlpha());
    }
}

