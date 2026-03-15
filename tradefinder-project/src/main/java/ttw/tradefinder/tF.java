/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Gf
 *  ttw.tradefinder.K
 *  ttw.tradefinder.MD
 *  ttw.tradefinder.MF
 *  ttw.tradefinder.gD
 *  ttw.tradefinder.ig
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.tF
 *  ttw.tradefinder.vE
 */
package ttw.tradefinder;

import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import ttw.tradefinder.Gf;
import ttw.tradefinder.K;
import ttw.tradefinder.MD;
import ttw.tradefinder.MF;
import ttw.tradefinder.cc;
import ttw.tradefinder.ed;
import ttw.tradefinder.gD;
import ttw.tradefinder.go;
import ttw.tradefinder.ig;
import ttw.tradefinder.rH;
import ttw.tradefinder.vE;

public class tF {
    private MF m;
    public long F;
    private final int e;
    public int i;
    private boolean k;
    private final boolean I;
    private final Object G;
    public double D;

    public static /* synthetic */ String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 5 << 4 ^ 3 << 1;
        int cfr_ignored_0 = (3 ^ 5) << 3 ^ 3;
        int n5 = n3;
        int n6 = 3;
        while (n5 >= 0) {
            int n7 = n3--;
            a2[n7] = (char)(((String)object2).charAt(n7) ^ n6);
            if (n3 < 0) break;
            int n8 = n3--;
            a2[n8] = (char)(((String)object2).charAt(n8) ^ n4);
            n5 = n3;
        }
        return new String((char[])a2);
    }

    public /* synthetic */ tF(boolean a2, int a3) {
        tF a4;
        tF tF2 = a4;
        tF tF3 = a4;
        tF tF4 = a4;
        a4.G = new Object();
        a4.m = new MF();
        tF4.k = false;
        tF4.i = 0;
        tF3.F = 0L;
        tF3.D = 0.0;
        tF2.I = a2;
        tF2.e = a3;
    }

    public /* synthetic */ boolean A(long a22, double a3) {
        Map.Entry a22;
        tF a4;
        double d2 = 0.0;
        int n2 = Integer.MAX_VALUE;
        int n3 = Integer.MIN_VALUE;
        Object object = a4.G;
        synchronized (object) {
            Iterator iterator;
            if (a4.m.isEmpty()) {
                return false;
            }
            tF tF2 = a4;
            tF2.m.headMap((Object)a22).clear();
            a22 = tF2.m.lastEntry();
            Iterator iterator2 = iterator = tF2.m.values().iterator();
            while (iterator2.hasNext()) {
                ig ig2 = (ig)iterator.next();
                d2 += ig2.D;
                n2 = Math.min(n2, ig2.G);
                n3 = Math.max(n3, ig2.G);
                iterator2 = iterator;
            }
        }
        int n4 = Math.abs(n3 - n2);
        if (a4.k) {
            if (d2 > a4.D && n4 < a4.e) {
                tF tF3 = a4;
                tF3.D = d2;
                tF3.F = (Long)a22.getKey();
                a4.i = ((ig)a22.getValue()).G;
            }
            if (d2 < 0.9 * a4.D || n4 >= a4.e) {
                return true;
            }
        } else if (d2 >= a3 && n4 < a4.e) {
            tF tF4 = a4;
            tF4.k = true;
            tF4.i = ((ig)a22.getValue()).G;
            a4.F = (Long)a22.getKey();
            a4.D = d2;
        }
        return false;
    }

    public /* synthetic */ void A() {
        tF a2;
        Object object = a2.G;
        synchronized (object) {
            a2.m.clear();
        }
        a2.k = false;
        tF tF2 = a2;
        a2.i = 0;
        tF2.F = 0L;
        tF2.D = 0.0;
    }

    public /* synthetic */ vE A(rH a2, gD a3) {
        tF a4;
        String string = a4.I ? go.A("\u0007m8dtI6{;z$|=g:") : MD.A((Object)"S]h\bPJbGcXeA~F");
        return a3.A(a2.m, string, a2.A(a4.D), a2.a(a4.i), a4.I);
    }

    public /* synthetic */ String A(rH a2) {
        tF a3;
        String string = a3.I ? MD.A((Object)"Bm]d") : go.A("\u0016]\r");
        return String.format(MD.A((Object)"PjBgCxEa^f1\rb\u00041~+\b4[1Ie\b4["), string, a2.A(a3.D), a2.a(a3.i));
    }

    public /* synthetic */ Map<String, String> A(rH a2) {
        tF a3;
        HashMap<String, String> hashMap = new HashMap<String, String>();
        hashMap.put(MD.A((Object)"GG}]|M"), a2.A(a3.D));
        hashMap.put(go.A("\u0004z=k1"), a2.a(a3.i));
        return hashMap;
    }

    public /* synthetic */ cc A(rH a2) {
        rH rH2;
        double d2;
        tF a3;
        String string = a3.I ? MD.A((Object)"{tD}\bPJbGcXeA~F") : go.A("J!qtI6{;z$|=g:");
        String string2 = a2.a(a3.i);
        tF tF2 = a3;
        if (a3.I) {
            d2 = -tF2.D;
            rH2 = a2;
        } else {
            d2 = tF2.D;
            rH2 = a2;
        }
        return new cc(string, string2, Gf.i, (K)new ed(d2, rH2.A(a3.D)), Collections.emptyList(), null);
    }

    public /* synthetic */ void A(long a2, int a3, double a4) {
        tF a5;
        Object object = a5.G;
        synchronized (object) {
            a5.m.A(a2, a3, a4);
            return;
        }
    }
}

