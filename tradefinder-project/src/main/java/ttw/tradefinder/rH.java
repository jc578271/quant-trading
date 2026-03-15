/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.DA
 *  ttw.tradefinder.MF
 *  ttw.tradefinder.rH
 *  velox.api.layer1.data.InstrumentInfo
 */
package ttw.tradefinder;

import java.text.DecimalFormat;
import ttw.tradefinder.DA;
import ttw.tradefinder.MF;
import ttw.tradefinder.bg;
import velox.api.layer1.data.InstrumentInfo;

/*
 * Duplicate member names - consider using --renamedupmembers true
 * Exception performing whole class analysis ignored.
 */
public class rH {
    public int f;
    private DecimalFormat a;
    public double K;
    public final String m;
    public double F;
    private DecimalFormat e;
    public double i;
    public boolean k;
    public int I;
    public final String G;
    public bg D;

    public String A(double a2) {
        rH a3;
        return a3.e.format(a2);
    }

    public String a(int a2) {
        rH a3;
        return a3.a.format((double)a2 * a3.K);
    }

    private static /* synthetic */ bg A(String a2, boolean a3) {
        if (a3) {
            return bg.i;
        }
        if (a2 == null || a2.isEmpty()) {
            return bg.e;
        }
        if ((a2 = a2.replace(DA.A((Object)"\u001d"), "").toLowerCase()).contains(MF.A((Object)"tOvRuS"))) {
            return bg.D;
        }
        if (a2.contains(DA.A((Object)">t9c2"))) {
            return bg.k;
        }
        return bg.e;
    }

    public String f(int a2) {
        rH a3;
        return rH.A((int)a3.I, (int)a2, (char)'0');
    }

    private static /* synthetic */ bg A(String a2) {
        if (a2 == null || a2.isEmpty()) {
            return rH.A((String)a2, (boolean)false);
        }
        String[] stringArray = a2.split(MF.A((Object)"w"));
        if (stringArray.length < 2) {
            return rH.A((String)a2, (boolean)false);
        }
        return rH.A((String)stringArray[1], (boolean)false);
    }

    public double f(int a2) {
        rH a3;
        return (double)a2 * a3.K;
    }

    public String toString() {
        rH a2;
        return String.format(DA.A((Object)"xbq1xbq1xbq1-x-bg1xbq10d1e4+}4.=}b4k8|(})xg1xbq19t>r2d3eg1xbq1;d1}9t-e5+}4."), a2.G, a2.m, a2.D.toString(), Double.toString(a2.K), Double.toString(a2.i), Double.toString(a2.F), Integer.toString(a2.I), Boolean.toString(a2.k));
    }

    public void A(InstrumentInfo a2) {
        rH a3;
        if (a2 == null) {
            return;
        }
        InstrumentInfo instrumentInfo = a2;
        a3.D = rH.A((String)a2.type, (boolean)instrumentInfo.isCrypto);
        a3.A(instrumentInfo.multiplier, a2.sizeMultiplier, a2.pips, a2.isFullDepth);
    }

    public String A(int a2) {
        rH a3;
        return a3.e.format((double)a2 / a3.F);
    }

    private static /* synthetic */ String f(String a2) {
        if (a2 == null || a2.isEmpty()) {
            return DA.A((Object)"(\u007f6\u007f2f3");
        }
        String[] stringArray = a2.split(MF.A((Object)"w"));
        if (stringArray.length == 0) {
            return a2;
        }
        return stringArray[0];
    }

    private /* synthetic */ void A(double a2, double a3, double a4, boolean a5) {
        rH a6;
        a6.K = Double.isNaN(a4) || Double.compare(a4, 0.0) <= 0 ? 1.0 : a4;
        a6.i = Double.isNaN(a2) || Double.compare(a2, 0.0) <= 0 ? 1.0 : a2;
        a6.F = Double.isNaN(a3) || Double.compare(a3, 0.0) <= 0 ? 1.0 : a3;
        rH rH2 = a6;
        rH2.k = a5;
        rH2.I = rH.f((double)rH2.F);
        rH2.f = rH.A((double)rH2.K);
        rH2.a = new DecimalFormat(a6.A());
        rH2.e = new DecimalFormat(a6.f(0));
    }

    private static /* synthetic */ int f(double a2) {
        int n2 = 0;
        double d2 = a2;
        while (Double.compare(d2, 1.0) > 0) {
            ++n2;
            d2 = a2 / 10.0;
        }
        return n2;
    }

    private static /* synthetic */ String A(String a2) {
        if ((a2 = rH.f((String)a2)) == null || a2.isEmpty()) {
            return DA.A((Object)"(\u007f6\u007f2f3");
        }
        if (a2.contains(MF.A((Object)"\r"))) {
            String[] stringArray = a2.split(DA.A((Object)"Mg"));
            if (stringArray.length == 0) {
                return a2;
            }
            return stringArray[0];
        }
        String[] stringArray = a2.split(MF.A((Object)"L\u0019"));
        if (stringArray.length == 0) {
            return a2;
        }
        return stringArray[0];
    }

    private static /* synthetic */ String A(int a2, int a3, char a4) {
        if (a2 == 0 && a3 == 0) {
            return DA.A((Object)"~");
        }
        Object object = MF.A((Object)"3\u0019");
        int n2 = a2;
        while (true) {
            --a2;
            if (n2 <= 0) break;
            object = (String)object + a4;
            n2 = a2;
        }
        int n3 = a3;
        while (true) {
            --a3;
            if (n3 <= 0) break;
            object = (String)object + "#";
            n3 = a3;
        }
        return object;
    }

    public static rH A(String a2, InstrumentInfo a3) {
        if (a3 == null) {
            return rH.A((String)a2);
        }
        InstrumentInfo instrumentInfo = a3;
        return new rH(a2, instrumentInfo.symbol, rH.A((String)instrumentInfo.type, (boolean)a3.isCrypto), a3.multiplier, a3.sizeMultiplier, a3.pips, a3.isFullDepth);
    }

    private static /* synthetic */ int A(double a2) {
        boolean bl = false;
        int n2 = 0;
        boolean bl2 = bl;
        while (!bl2 || Double.compare(a2, 1.0) >= 0) {
            if (Double.compare(a2, 1.0) >= 0) {
                bl = true;
            }
            ++n2;
            a2 = a2 % 1.0 * 10.0;
            bl2 = bl;
        }
        return n2 - 1;
    }

    public String A() {
        rH a2;
        return rH.A((int)a2.f, (int)0, (char)'#');
    }

    public static rH A(String a2) {
        String string = a2;
        return new rH(string, rH.A((String)string), rH.A((String)a2), 1.0, 1.0, 1.0, false);
    }

    private /* synthetic */ rH(String a2, String a3, bg a4, double a5, double a6, double a7, boolean a8) {
        rH a9;
        a9.G = a2 == null || a2.isEmpty() ? DA.A((Object)"(\u007f6\u007f2f3") : a2;
        a9.m = a3 == null || a3.isEmpty() ? MF.A((Object)"B~\\~XgY") : a3;
        a9.D = a4;
        a9.A(a5, a6, a7, a8);
    }

    public double A(int a2) {
        rH a3;
        return (double)a2 / a3.F;
    }
}

