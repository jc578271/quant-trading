/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AC
 *  ttw.tradefinder.Fg
 *  ttw.tradefinder.IB
 *  ttw.tradefinder.Kd
 *  ttw.tradefinder.Kf
 *  ttw.tradefinder.OC
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.TB
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.Yc
 *  ttw.tradefinder.cd
 *  ttw.tradefinder.ee
 *  ttw.tradefinder.hb
 *  ttw.tradefinder.jA
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.jc
 *  ttw.tradefinder.ra
 *  ttw.tradefinder.sD
 *  ttw.tradefinder.yC
 *  ttw.tradefinder.yc
 */
package ttw.tradefinder;

import java.util.Collection;
import java.util.LinkedHashMap;
import java.util.Map;
import ttw.tradefinder.AC;
import ttw.tradefinder.CB;
import ttw.tradefinder.Fg;
import ttw.tradefinder.IB;
import ttw.tradefinder.JC;
import ttw.tradefinder.Kd;
import ttw.tradefinder.Kf;
import ttw.tradefinder.Ld;
import ttw.tradefinder.OC;
import ttw.tradefinder.SE;
import ttw.tradefinder.TB;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.Yc;
import ttw.tradefinder.cd;
import ttw.tradefinder.ee;
import ttw.tradefinder.fA;
import ttw.tradefinder.gC;
import ttw.tradefinder.gH;
import ttw.tradefinder.hK;
import ttw.tradefinder.hb;
import ttw.tradefinder.jA;
import ttw.tradefinder.jc;
import ttw.tradefinder.mg;
import ttw.tradefinder.ra;
import ttw.tradefinder.sD;
import ttw.tradefinder.yC;
import ttw.tradefinder.yc;

/*
 * Exception performing whole class analysis ignored.
 */
public class jF {
    private static final Map<OC, String> C;
    private static final Map<sD, String> c;
    private static final Map<hb, String> L;
    private static final Map<Kf, String> E;
    private static final Map<gH, String> b;
    private static final Map<jA, String> l;
    private static final Map<JC, String> j;
    private static final Map<cd, String> J;
    private static final Map<yc, String> M;
    private static final Map<gC, String> d;
    private static final Map<TB, String> g;
    private static final Map<ee, String> f;
    private static final Map<Yc, String> a;
    private static final Map<mg, String> K;
    private static final Map<fA, String> m;
    private static final Map<jc, String> F;
    private static final Map<IB, String> e;
    private static final Map<SE, String> i;
    private static final Map<CB, String> k;
    private static final Map<Kd, String> I;
    private static final Map<Ld, String> G;
    private static final Map<yC, String> D;

    public static /* synthetic */ String A(SE a2) {
        if (!i.containsKey(a2)) {
            return Ya.A((Object)"yoy");
        }
        return (String)i.get(a2);
    }

    public static /* synthetic */ Collection<String> A(cd a2) {
        return J.values();
    }

    private static /* synthetic */ Map<hb, String> A(hb a2) {
        a2 = new LinkedHashMap<hb, String>();
        a2.put(hb.m, Ya.A((Object)"\u00150&-#\u0017$"));
        a2.put(hb.k, Fg.A((Object)")\u000e\u001a\u0013\u001f8\u0007\u000b\u0006"));
        a2.put(hb.i, Ya.A((Object)"\u0010- "));
        a2.put(hb.e, Fg.A((Object)"/\u0019\t\t\u000e\r"));
        a2.put(hb.F, Ya.A((Object)"\u0016&+5,3.1\u0017$"));
        a2.put(hb.I, Fg.A((Object)"(\u001a\u0015\t\u0012\u000f\u0010\r8\u0007\u000b\u0006"));
        a2.put(hb.D, Ya.A((Object)"\u000e=,1"));
        return a2;
    }

    public static /* synthetic */ String A(cd a2) {
        if (!J.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)J.get(a2);
    }

    public static /* synthetic */ Collection<String> A(ee a2) {
        return f.values();
    }

    public static /* synthetic */ JC A(String a2, JC a3) {
        for (Map.Entry entry : j.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (JC)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ String A(jA a2) {
        if (!l.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)l.get(a2);
    }

    public static /* synthetic */ IB A(String a2, IB a3) {
        for (Map.Entry entry : e.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (IB)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ ee A(String a2, ee a3) {
        for (Map.Entry entry : f.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (ee)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ jA A(String a2, jA a3) {
        for (Map.Entry entry : l.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (jA)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ String A(Kf a2) {
        if (!E.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)E.get(a2);
    }

    static {
        b = jF.A((gH)gH.b);
        c = jF.A((sD)sD.F);
        f = jF.A((ee)ee.D);
        i = jF.A((SE)SE.I);
        K = jF.A((mg)mg.G);
        E = jF.A((Kf)Kf.D);
        J = jF.A((cd)cd.D);
        F = jF.A((jc)jc.D);
        I = jF.A((Kd)Kd.i);
        L = jF.A((hb)hb.m);
        g = jF.A((TB)TB.G);
        j = jF.A((JC)JC.G);
        d = jF.A((gC)gC.G);
        D = jF.A((yC)yC.G);
        k = jF.A((CB)CB.i);
        a = jF.A((Yc)Yc.e);
        m = jF.A(fA.e);
        M = jF.A((yc)yc.e);
        G = jF.A(Ld.D);
        l = jF.A((jA)jA.k);
        C = jF.A((OC)OC.k);
        e = jF.A((IB)IB.I);
    }

    public static /* synthetic */ Collection<String> A(CB a2) {
        return k.values();
    }

    public static /* synthetic */ String A(CB a2) {
        if (!k.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)k.get(a2);
    }

    private static /* synthetic */ Map<CB, String> A(CB a2) {
        a2 = new LinkedHashMap<CB, String>();
        a2.put(CB.i, Fg.A((Object)")\u000e\u001a\u0013\u001f"));
        a2.put(CB.k, Ya.A((Object)"\u0016&+5,3.1"));
        a2.put(CB.G, Fg.A((Object)":\u0004\u001d\u000f"));
        a2.put(CB.I, Ya.A((Object)"\u0007*;0 \u00048#3"));
        return a2;
    }

    public static /* synthetic */ jc A(String a2, jc a3) {
        for (Map.Entry entry : F.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (jc)entry.getKey();
        }
        return a3;
    }

    private static /* synthetic */ Map<yC, String> A(yC a2) {
        a2 = new LinkedHashMap<yC, String>();
        a2.put(yC.G, Ya.A((Object)"\u0015 ;41"));
        a2.put(yC.I, Fg.A((Object)"*\u0019\u0004\u0013\u001f"));
        return a2;
    }

    private static /* synthetic */ Map<Yc, String> A(Yc a2) {
        a2 = new LinkedHashMap<Yc, String>();
        a2.put(Yc.e, Ya.A((Object)"\u0012#&\u00036-\"'\u00040=!1"));
        a2.put(Yc.I, Fg.A((Object)"=\n\u0013\u001e\u00198\u000e\u0001\u001f\r"));
        a2.put(Yc.G, Ya.A((Object)"\u001b,\u00040=!1"));
        a2.put(Yc.k, Fg.A((Object)">\r\u0010\u0007\u000b8\u000e\u0001\u001f\r"));
        a2.put(Yc.i, Ya.A((Object)"\u0012#&\u00001.;5\u00040=!1"));
        return a2;
    }

    public static /* synthetic */ String A(sD a2) {
        if (!c.containsKey(a2)) {
            return Ya.A((Object)"yoy");
        }
        return (String)c.get(a2);
    }

    private static /* synthetic */ Map<ee, String> A(ee a2) {
        a2 = new LinkedHashMap<ee, String>();
        a2.put(ee.D, Ya.A((Object)"\f56=41"));
        a2.put(ee.k, Fg.A((Object)";\u0005\u0006\b\u0000\u0019\u001c\u0015\u000b"));
        a2.put(ee.I, Ya.A((Object)"\u0001;/6+:'0"));
        return a2;
    }

    public static /* synthetic */ Collection<String> A(gC a2) {
        return d.values();
    }

    private static /* synthetic */ Map<yc, String> A(yc a2) {
        a2 = new LinkedHashMap<yc, String>();
        a2.put(yc.e, Fg.A((Object)")\u000e\u001a\u0013\u001f)\u0018"));
        a2.put(yc.k, Ya.A((Object)"\u00150&-#\u0006;5:"));
        a2.put(yc.D, Fg.A((Object)",\u0013\u001c"));
        a2.put(yc.i, Ya.A((Object)"\u0011%7501"));
        a2.put(yc.m, Fg.A((Object)"(\u001a\u0015\t\u0012\u000f\u0010\r)\u0018"));
        a2.put(yc.G, Ya.A((Object)"\u0016&+5,3.1\u0006;5:"));
        a2.put(yc.I, Fg.A((Object)" Q*\u001d\u001a"));
        a2.put(yc.F, Ya.A((Object)"\u0002o\u0016#&"));
        return a2;
    }

    public static /* synthetic */ CB A(String a2, CB a3) {
        for (Map.Entry entry : k.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (CB)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ Collection<String> A(sD a2) {
        return jF.A((sD)a2, (boolean)false);
    }

    private static /* synthetic */ Map<TB, String> A(TB a2) {
        a2 = new LinkedHashMap<TB, String>();
        a2.put(TB.G, Ya.A((Object)"\u0010#&)\u001301':"));
        a2.put(TB.m, Fg.A((Object)"0\u0001\u001b\u0000\b/\u000e\r\u0019\u0006"));
        a2.put(TB.F, Ya.A((Object)"\u0010#&)\u0006'0"));
        a2.put(TB.k, Fg.A((Object)"0\u0001\u001b\u0000\b:\u0019\f"));
        a2.put(TB.D, Ya.A((Object)"\u0000871"));
        a2.put(TB.i, Fg.A((Object)"3\u001a\u001d\u0006\u001b\r"));
        a2.put(TB.I, Ya.A((Object)"\u001b1.8-#"));
        return a2;
    }

    public static /* synthetic */ String A(fA a2) {
        if (!m.containsKey(a2)) {
            return Ya.A((Object)"yoy");
        }
        return (String)m.get(a2);
    }

    private static /* synthetic */ Map<JC, String> A(JC a2) {
        a2 = new LinkedHashMap<AC, String>();
        a2.put(JC.G, Ya.A((Object)"\n;0=8;, #8"));
        a2.put(JC.D, Fg.A((Object)"*\r\u000e\u001c\u0015\u000b\u001d\u0004"));
        return a2;
    }

    private static /* synthetic */ Map<gH, String> A(gH a2) {
        a2 = new LinkedHashMap<gH, String>();
        a2.put(gH.b, "");
        a2.put(gH.I, new String(Character.toChars(128077)));
        a2.put(gH.M, new String(Character.toChars(128200)));
        a2.put(gH.k, new String(Character.toChars(128201)));
        a2.put(gH.g, new String(Character.toChars(128214)));
        a2.put(gH.L, new String(Character.toChars(128270)));
        a2.put(gH.d, new String(Character.toChars(9878)));
        a2.put(gH.j, new String(Character.toChars(128721)));
        a2.put(gH.J, new String(Character.toChars(10133)));
        a2.put(gH.H, new String(Character.toChars(10134)));
        a2.put(gH.h, new String(Character.toChars(127937)));
        a2.put(gH.m, new String(Character.toChars(128640)));
        a2.put(gH.F, new String(Character.toChars(128308)));
        a2.put(gH.K, new String(Character.toChars(128994)));
        a2.put(gH.l, new String(Character.toChars(127956)));
        a2.put(gH.i, new String(Character.toChars(127939)));
        a2.put(gH.e, new String(Character.toChars(9889)));
        a2.put(gH.f, new String(Character.toChars(10548)));
        a2.put(gH.A, new String(Character.toChars(10549)));
        a2.put(gH.E, new String(Character.toChars(128176)));
        a2.put(gH.C, new String(Character.toChars(129529)));
        a2.put(gH.G, new String(Character.toChars(9876)));
        a2.put(gH.c, new String(Character.toChars(9167)));
        a2.put(gH.D, new String(Character.toChars(127777)));
        return a2;
    }

    public static /* synthetic */ String A(Yc a2) {
        if (!a.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)a.get(a2);
    }

    public static /* synthetic */ String A(gH a2) {
        if (!b.containsKey((Object)a2)) {
            return "";
        }
        return (String)b.get((Object)a2);
    }

    private static /* synthetic */ Map<gC, String> A(gC a2) {
        a2 = new LinkedHashMap<gC, String>();
        a2.put(gC.i, Fg.A((Object)"/\u0011\u0011\n\u0013\u00043\u0006\u0010\u0011"));
        a2.put(gC.G, Ya.A((Object)"\u0017-925! "));
        a2.put(gC.D, Fg.A((Object)"=\f\n\t\u0012\u000b\u0019\f"));
        a2.put(gC.I, Ya.A((Object)"\u0012&-2''1=-:#8"));
        return a2;
    }

    public static /* synthetic */ Kf A(String a2, Kf a3) {
        for (Map.Entry entry : E.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (Kf)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ String A(TB a2) {
        if (!g.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)g.get(a2);
    }

    private static /* synthetic */ Map<Kd, String> A(Kd a2) {
        a2 = new LinkedHashMap<Kd, String>();
        a2.put(Kd.i, Fg.A((Object)";\u0011\t\u0010\u0004"));
        a2.put(Kd.D, Ya.A((Object)"\u000f1&=79"));
        a2.put(Kd.G, Fg.A((Object)"$\u001d\u001a\u001b\r"));
        a2.put(Kd.k, Ya.A((Object)"\u001450=#6.1"));
        return a2;
    }

    public static /* synthetic */ Collection<String> A(JC a2) {
        return j.values();
    }

    private static /* synthetic */ Map<jA, String> A(jA a2) {
        a2 = new LinkedHashMap<jA, String>();
        a2.put(jA.D, Fg.A((Object)",\u0013\u001c"));
        a2.put(jA.i, Ya.A((Object)"\u0011%7501"));
        a2.put(jA.k, Fg.A((Object)"0\u0001\u0012\r"));
        a2.put(jA.G, Ya.A((Object)"\u0016.1,0"));
        return a2;
    }

    public static /* synthetic */ Collection<String> A(yC a2) {
        return D.values();
    }

    public static /* synthetic */ String A(ee a2) {
        if (!f.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)f.get(a2);
    }

    public static /* synthetic */ gC A(String a2, gC a3) {
        for (Map.Entry entry : d.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (gC)((Object)entry.getKey());
        }
        return a3;
    }

    public static /* synthetic */ Collection<String> A(OC a2) {
        return C.values();
    }

    public static /* synthetic */ String A(OC a2) {
        if (!C.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)C.get(a2);
    }

    public static /* synthetic */ Ld A(String a2, Ld a3) {
        for (Map.Entry entry : G.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (Ld)entry.getKey();
        }
        return a3;
    }

    private static /* synthetic */ Map<mg, String> A(mg a2) {
        a2 = new LinkedHashMap<YD, String>();
        a2.put(mg.G, Fg.A((Object)"4\u0007\u000e\u0001\u0006\u0007\u0012\u001c\u001d\u0004"));
        a2.put(mg.D, Ya.A((Object)"\u001410 +7#8"));
        return a2;
    }

    public static /* synthetic */ yC A(String a2, yC a3) {
        for (Map.Entry entry : D.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (yC)entry.getKey();
        }
        return a3;
    }

    private static /* synthetic */ Map<Ld, String> A(Ld a2) {
        a2 = new LinkedHashMap<YD<ra, hK>, String>();
        a2.put(Ld.D, Ya.A((Object)"\u0007'$#&# '0"));
        a2.put(Ld.I, Fg.A((Object)"?\u0007\u0011\n\u0015\u0006\u0019\f"));
        return a2;
    }

    public static /* synthetic */ hb A(String a2, hb a3) {
        for (Map.Entry entry : L.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (hb)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ mg A(String a2, mg a3) {
        for (Map.Entry entry : K.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (mg)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ Yc A(String a2, Yc a3) {
        for (Map.Entry entry : a.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (Yc)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ Collection<String> A(hb a2) {
        return L.values();
    }

    public static /* synthetic */ Collection<String> A(jA a2) {
        return l.values();
    }

    public static /* synthetic */ Collection<String> A(TB a2) {
        return g.values();
    }

    public static /* synthetic */ String A(gC a2) {
        if (!d.containsKey((Object)a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)d.get((Object)a2);
    }

    public static /* synthetic */ Collection<String> A(Kf a2) {
        return E.values();
    }

    private static /* synthetic */ Map<cd, String> A(cd a2) {
        a2 = new LinkedHashMap<cd, String>();
        a2.put(cd.D, Ya.A((Object)"\u0006=15 8'0"));
        a2.put(cd.i, Fg.A((Object)"<\u0013\u00180\r\u001a\u001c"));
        a2.put(cd.k, Ya.A((Object)"\u0016;2\u0006+3* "));
        a2.put(cd.e, Fg.A((Object)">\u0007\b\u001c\u0013\u00050\r\u001a\u001c"));
        a2.put(cd.G, Ya.A((Object)"\u0016- 6;/\u0006+3* "));
        return a2;
    }

    public static /* synthetic */ String A(yc a2) {
        if (!M.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)M.get(a2);
    }

    public static /* synthetic */ Collection<String> A(mg a2) {
        return K.values();
    }

    public static /* synthetic */ String A(mg a2) {
        if (!K.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)K.get(a2);
    }

    public static /* synthetic */ String A(hb a2) {
        if (!L.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)L.get(a2);
    }

    public static /* synthetic */ cd A(String a2, cd a3) {
        for (Map.Entry entry : J.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (cd)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ String A(JC a2) {
        if (!j.containsKey(a2)) {
            return Ya.A((Object)"yoy");
        }
        return (String)j.get(a2);
    }

    private static /* synthetic */ Map<IB, String> A(IB a2) {
        a2 = new LinkedHashMap<IB, String>();
        a2.put(IB.I, Ya.A((Object)"\u0007-8+0"));
        a2.put(IB.e, Fg.A((Object)"/\u0000\u0013\u001a\bH8\t\u000f\u0000"));
        a2.put(IB.k, Ya.A((Object)"\u0018-:%t\u000651<"));
        a2.put(IB.i, Fg.A((Object)"8\t\u000f\u0000Q,\u0013\u001c"));
        a2.put(IB.D, Ya.A((Object)"\u0010- "));
        return a2;
    }

    private static /* synthetic */ Map<fA, String> A(fA a2) {
        a2 = new LinkedHashMap<Map<String, Map<String, Object>>, String>();
        a2.put(fA.e, Ya.A((Object)"\u0002'&;\u0007/5.8"));
        a2.put(fA.D, Fg.A((Object)";\u0011\t\u0010\u0004"));
        a2.put(fA.i, Ya.A((Object)"\u000f1&=79"));
        a2.put(fA.F, Fg.A((Object)"$\u001d\u001a\u001b\r"));
        a2.put(fA.G, Ya.A((Object)"\u0002'&;\u0018#&%1"));
        a2.put(fA.I, Fg.A((Object)"*\t\u000e\u0001\u001d\n\u0010\r"));
        return a2;
    }

    public static /* synthetic */ yc A(String a2, yc a3) {
        for (Map.Entry entry : M.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (yc)entry.getKey();
        }
        return a3;
    }

    private static /* synthetic */ Map<sD, String> A(sD a2) {
        a2 = new LinkedHashMap<sD, String>();
        a2.put(sD.G, Ya.A((Object)"\u00156 ':6=-:"));
        a2.put(sD.i, Fg.A((Object)"2\u0007\b\u0001\u001a\u0011"));
        a2.put(sD.e, Ya.A((Object)"\u0012&+7'\u0018'\"'8"));
        a2.put(sD.I, Fg.A((Object)"/\u0001\u001b\u0006\u001d\u0004"));
        a2.put(sD.D, Ya.A((Object)"\u0017-:$=09"));
        a2.put(sD.F, Fg.A((Object)"2'2-"));
        return a2;
    }

    public static /* synthetic */ Collection<String> A(IB a2) {
        return e.values();
    }

    public static /* synthetic */ String A(IB a2) {
        if (!e.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)e.get(a2);
    }

    public static /* synthetic */ SE A(String a2, SE a3) {
        for (Map.Entry entry : i.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (SE)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ String A(Kd a2) {
        if (!I.containsKey(a2)) {
            return Fg.A((Object)"EQE");
        }
        return (String)I.get(a2);
    }

    public static /* synthetic */ Kd A(String a2, Kd a3) {
        for (Map.Entry entry : I.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (Kd)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ fA A(String a2, fA a3) {
        for (Map.Entry entry : m.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (fA)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ Collection<String> A(Ld a2) {
        return G.values();
    }

    public static /* synthetic */ Collection<String> A(Kd a2) {
        return I.values();
    }

    private static /* synthetic */ Map<OC, String> A(OC a2) {
        a2 = new LinkedHashMap<OC, String>();
        a2.put(OC.k, Ya.A((Object)"\u0017-925! "));
        a2.put(OC.G, Fg.A((Object)"=\f\n\t\u0012\u000b\u0019\f"));
        a2.put(OC.D, Ya.A((Object)"\u0012&-2''1=-:#8"));
        return a2;
    }

    public static /* synthetic */ Collection<String> A(SE a2) {
        return i.values();
    }

    public /* synthetic */ jF() {
        jF a2;
    }

    public static /* synthetic */ Collection<String> A(fA a2) {
        return m.values();
    }

    private static /* synthetic */ Map<jc, String> A(jc a2) {
        a2 = new LinkedHashMap<jc, String>();
        a2.put(jc.D, Ya.A((Object)"\u0004!.8"));
        a2.put(jc.I, Fg.A((Object)"*\t\u000e\u0001\u001d\n\u0010\r"));
        return a2;
    }

    public static /* synthetic */ Collection<String> A(sD a2, boolean a3) {
        if (!a3) {
            return c.values();
        }
        a2 = c.values();
        a2.remove(Fg.A((Object)"2'2-"));
        return a2;
    }

    public static /* synthetic */ String A(jc a2) {
        if (!F.containsKey(a2)) {
            return Ya.A((Object)"yoy");
        }
        return (String)F.get(a2);
    }

    public static /* synthetic */ TB A(String a2, TB a3) {
        for (Map.Entry entry : g.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (TB)entry.getKey();
        }
        return a3;
    }

    private static /* synthetic */ Map<Kf, String> A(Kf a2) {
        a2 = new LinkedHashMap<Kf, String>();
        a2.put(Kf.k, Ya.A((Object)"\u0017-925! "));
        a2.put(Kf.D, Fg.A((Object)"=\f\n\t\u0012\u000b\u0019\f"));
        a2.put(Kf.G, Ya.A((Object)"\u0012&-2''1=-:#8"));
        return a2;
    }

    public static /* synthetic */ sD A(String a2, sD a3) {
        for (Map.Entry entry : c.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (sD)entry.getKey();
        }
        return a3;
    }

    private static /* synthetic */ Map<SE, String> A(SE a2) {
        a2 = new LinkedHashMap<SE, String>();
        a2.put(SE.I, Ya.A((Object)"\u0011,5 8'0"));
        a2.put(SE.D, Fg.A((Object)"8\u0001\u000f\t\u001e\u0004\u0019\f"));
        return a2;
    }

    public static /* synthetic */ String A(yC a2) {
        if (!D.containsKey(a2)) {
            return Ya.A((Object)"yoy");
        }
        return (String)D.get(a2);
    }

    public static /* synthetic */ OC A(String a2, OC a3) {
        for (Map.Entry entry : C.entrySet()) {
            if (!((String)entry.getValue()).equals(a2)) continue;
            return (OC)entry.getKey();
        }
        return a3;
    }

    public static /* synthetic */ String A(Ld a2) {
        if (!G.containsKey(a2)) {
            return Ya.A((Object)"yoy");
        }
        return (String)G.get(a2);
    }

    public static /* synthetic */ Collection<String> A(yc a2) {
        return M.values();
    }

    public static /* synthetic */ Collection<String> A(Yc a2) {
        return a.values();
    }

    public static /* synthetic */ Collection<String> A(jc a2) {
        return F.values();
    }
}

