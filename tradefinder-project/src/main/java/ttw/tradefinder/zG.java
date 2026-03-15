/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nh
 *  ttw.tradefinder.dH
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.xe
 *  ttw.tradefinder.zG
 */
package ttw.tradefinder;

import java.util.HashMap;
import java.util.Map;
import java.util.StringJoiner;
import ttw.tradefinder.Nh;
import ttw.tradefinder.dH;
import ttw.tradefinder.gH;
import ttw.tradefinder.go;
import ttw.tradefinder.jF;
import ttw.tradefinder.og;
import ttw.tradefinder.tg;
import ttw.tradefinder.xe;

public class zG {
    private static final String G = go.A("t(t(t");
    private dH D = new dH();

    public void f(String a2, String a3, String a4) {
        zG a5;
        Object object = new HashMap<String, String>();
        object.put(go.A("\u0004z=k1"), xe.A((Object)"f\fj\b|\u0004"));
        object.put(go.A("^;d!e1"), xe.A((Object)"k\u0004k\u0004|\u0004"));
        StringJoiner stringJoiner = new StringJoiner(System.lineSeparator());
        zG zG2 = a5;
        zG2.A(og.D, stringJoiner);
        zG2.A(og.J, stringJoiner);
        zG2.A(og.a, stringJoiner);
        zG2.A(og.I, stringJoiner);
        zG2.A(og.g, stringJoiner);
        zG2.A(og.k, stringJoiner);
        zG2.A(og.i, stringJoiner);
        zG2.A(og.e, stringJoiner);
        zG2.A(og.E, stringJoiner);
        zG2.A(og.l, stringJoiner);
        zG2.A(og.G, stringJoiner);
        zG2.A(og.d, stringJoiner);
        zG2.A(og.b, stringJoiner);
        zG2.A(og.j, stringJoiner);
        zG2.A(og.m, stringJoiner);
        zG2.A(og.K, stringJoiner);
        zG2.A(og.F, stringJoiner);
        zG2.A(og.M, stringJoiner);
        zG2.A(og.L, stringJoiner);
        stringJoiner.add(String.format(go.A("-'\"q{~2t-'"), xe.A((Object)"\u001dr\u001dr\u001d"), go.A("A:{ z!e1f "), ((String)a2).split(xe.A((Object)"}"))[0]));
        a2 = object.entrySet().iterator();
        Object object2 = a2;
        while (object2.hasNext()) {
            object = (Map.Entry)a2.next();
            stringJoiner.add(String.format(go.A("-'\"q{~2t-'"), xe.A((Object)"\u001dr\u001dr\u001d"), object.getKey(), object.getValue()));
            object2 = a2;
        }
        a5.D.f(new Nh(a3, a4, stringJoiner.toString()));
    }

    private /* synthetic */ void A(og a2, StringJoiner a3) {
        zG a4;
        StringJoiner stringJoiner = a3;
        stringJoiner.add(a4.A(a2, tg.D));
        stringJoiner.add(String.format(go.A("-'\"q{~2t-'"), xe.A((Object)"\u001dr\u001dr\u001d"), a4.A(a2), a4.f(a2, tg.D)));
        a3.add(a4.A(a2, tg.I));
        a3.add(String.format(go.A("-'\"q{~2t-'"), xe.A((Object)"\u001dr\u001dr\u001d"), a4.A(a2), a4.f(a2, tg.I)));
    }

    /*
     * Enabled aggressive block sorting
     */
    private /* synthetic */ String f(og a2, tg a3) {
        switch (a2) {
            case I: {
                return jF.A((gH)gH.f);
            }
            case g: {
                return jF.A((gH)gH.A);
            }
            case i: 
            case m: 
            case K: 
            case F: 
            case M: 
            case L: {
                switch (a3) {
                    case D: {
                        return go.A("\u0016]\r");
                    }
                }
                return xe.A((Object)"\u0001x\u001eq");
            }
        }
        switch (a3) {
            case D: {
                return go.A("\u0015[\u001f");
            }
        }
        return xe.A((Object)"\u007f\u001by");
    }

    public void A(String a2, String a3) {
        zG a4;
        StringJoiner stringJoiner = new StringJoiner(xe.A((Object)"\u001d"));
        stringJoiner.add(go.A("~K;f3z5|'$tq;}tl=lta )~"));
        stringJoiner.add(jF.A((gH)gH.I));
        stringJoiner.add(xe.A((Object)"\u000bR'Or\u0017\u0006i\u0005\u001d\u0013Q7O&Nr\u007f=Ix\u001d;NrH\"\u001d3S6\u001d H<S;S5\u001d\u007f\u001d%XrJ;N:\u001d+R'\u001d\"O=[;I3_>XrI \\6X!\u001c"));
        stringJoiner.add(jF.A((gH)gH.m));
        a4.D.f(new Nh(a2, a3, stringJoiner.toString()));
    }

    public void f() {
        zG a2;
        a2.D.f();
    }

    public void A() {
        zG a2;
        a2.D.A();
    }

    public void A(og a2, tg a3, String a4, String a5, String a6, Map<String, String> a7) {
        zG a8;
        StringJoiner stringJoiner = new StringJoiner(System.lineSeparator());
        stringJoiner.add(a8.A((og)((Object)a2), (tg)((Object)a3)));
        stringJoiner.add(String.format(go.A("-'\"q{~2t-'"), xe.A((Object)"\u001dr\u001dr\u001d"), go.A("A:{ z!e1f "), a4.split(xe.A((Object)"}"))[0]));
        stringJoiner.add(String.format(go.A("-'\"q{~2t-'"), xe.A((Object)"\u001dr\u001dr\u001d"), a8.A((og)((Object)a2)), a8.f((og)((Object)a2), (tg)((Object)a3))));
        a2 = a7.entrySet().iterator();
        Object object = a2;
        while (object.hasNext()) {
            a3 = (Map.Entry)a2.next();
            stringJoiner.add(String.format(go.A("-'\"q{~2t-'"), xe.A((Object)"\u001dr\u001dr\u001d"), a3.getKey(), a3.getValue()));
            object = a2;
        }
        a8.D.f(new Nh(a5, a6, stringJoiner.toString()));
    }

    public void A(String a2, String a3, String a4) {
        zG a5;
        StringJoiner stringJoiner = new StringJoiner(go.A("t"));
        stringJoiner.add(jF.A((gH)gH.e));
        stringJoiner.add(a2);
        stringJoiner.add(xe.A((Object)"T!\u001d H<S;S5\u001d3S6\u001d X3Y+\u001d&RrN7S6\u001d!T5S3Q!\u0013"));
        a5.D.f(new Nh(a3, a4, stringJoiner.toString()));
    }

    /*
     * Enabled aggressive block sorting
     */
    private /* synthetic */ String A(og a2, tg a3) {
        StringJoiner stringJoiner;
        StringJoiner stringJoiner2 = new StringJoiner(go.A("t"));
        switch (a2) {
            case D: {
                stringJoiner2.add(jF.A((gH)gH.E));
                stringJoiner2.add(xe.A((Object)"i\u0006jrq\u001bl\u0007t\u0016t\u0006d"));
                stringJoiner2.add(jF.A((gH)gH.E));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.F : gH.K)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case J: {
                stringJoiner2.add(jF.A((gH)gH.E));
                stringJoiner2.add(go.A("\u0000\\\u0003(\u0018A\u0005]\u001dL\u001d\\\r"));
                stringJoiner2.add(jF.A((gH)gH.E));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.F : gH.K)));
                stringJoiner2.add(jF.A((gH)gH.J));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case a: {
                stringJoiner2.add(jF.A((gH)gH.E));
                stringJoiner2.add(xe.A((Object)"i\u0006jrq\u001bl\u0007t\u0016t\u0006d"));
                stringJoiner2.add(jF.A((gH)gH.E));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.F : gH.K)));
                stringJoiner2.add(jF.A((gH)gH.H));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case I: {
                stringJoiner2.add(jF.A((gH)gH.d));
                stringJoiner2.add(go.A("\u0000\\\u0003(\u001dE\u0016I\u0018I\u001aK\u0011"));
                stringJoiner2.add(jF.A((gH)gH.d));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner2.add(jF.A((gH)gH.f));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case g: {
                stringJoiner2.add(jF.A((gH)gH.d));
                stringJoiner2.add(xe.A((Object)"i\u0006jrt\u001f\u007f\u0013q\u0013s\u0011x"));
                stringJoiner2.add(jF.A((gH)gH.d));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner2.add(jF.A((gH)gH.A));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case k: {
                stringJoiner2.add(jF.A((gH)gH.d));
                stringJoiner2.add(go.A("\\\u0000_tA\u0019J\u0015D\u0015F\u0017MtR\u0011Z\u001b%\u0017Z\u001b[\u0007"));
                stringJoiner2.add(jF.A((gH)gH.d));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner2.add(jF.A((gH)gH.G));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case i: {
                stringJoiner2.add(jF.A((gH)gH.i));
                stringJoiner2.add(xe.A((Object)"\u0006i\u0005\u001d\u0004r\u001eh\u001fxrn\u0002t\u0019x"));
                stringJoiner2.add(jF.A((gH)gH.i));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case e: {
                stringJoiner2.add(jF.A((gH)gH.l));
                stringJoiner2.add(go.A("\\\u0000_t@\u001dL\u0010M\u001a(\u001bZ\u0010M\u0006"));
                stringJoiner2.add(jF.A((gH)gH.l));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.F : gH.K)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case E: {
                stringJoiner2.add(jF.A((gH)gH.l));
                stringJoiner2.add(xe.A((Object)"i\u0006jrt\u0011x\u0010x\u0000zry\u0017k\u0017q\u001dm\u001fx\u001ci"));
                stringJoiner2.add(jF.A((gH)gH.l));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.F : gH.K)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case l: {
                stringJoiner2.add(jF.A((gH)gH.C));
                stringJoiner2.add(go.A("\u0000\\\u0003(\u0007_\u0011M\u0004"));
                stringJoiner2.add(jF.A((gH)gH.C));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case G: {
                stringJoiner2.add(jF.A((gH)gH.h));
                stringJoiner2.add(xe.A((Object)"\u0006i\u0005\u001d\u0013\u007f\u0001r\u0000m\u0006t\u001ds"));
                stringJoiner2.add(jF.A((gH)gH.h));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case d: {
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.M : gH.k)));
                stringJoiner2.add(go.A("\u0000\\\u0003(\u0004Z\u001dK\u0011(\u0015D\u0011Z\u0000"));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.M : gH.k)));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case b: {
                stringJoiner2.add(jF.A((gH)gH.c));
                stringJoiner2.add(xe.A((Object)"i\u0006jrk\u007fy\u0017q\u0006|"));
                stringJoiner2.add(jF.A((gH)gH.c));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case j: {
                stringJoiner2.add(jF.A((gH)gH.G));
                stringJoiner2.add(go.A("\\\u0000_t^yL\u0011D\u0000ItR\u0011Z\u001b%\u0017Z\u001b[\u0007"));
                stringJoiner2.add(jF.A((gH)gH.G));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case m: {
                stringJoiner2.add(jF.A((gH)gH.c));
                stringJoiner2.add(xe.A((Object)"\u0006i\u0005\u001d\u001f|\u0000v\u0017irk\u001dq\u0007p\u0017\u001d\u0001i\u001dm"));
                stringJoiner2.add(jF.A((gH)gH.c));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
            }
            case K: {
                stringJoiner2.add(jF.A((gH)gH.j));
                stringJoiner2.add(go.A("\u0000\\\u0003(\u0002G\u0018]\u0019Mt[\u0000G\u0004"));
                stringJoiner2.add(jF.A((gH)gH.j));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case F: {
                stringJoiner2.add(jF.A((gH)gH.j));
                stringJoiner2.add(xe.A((Object)"\u0006i\u0005\u001d\u0001i\u001dmro\u0007s"));
                stringJoiner2.add(jF.A((gH)gH.j));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
                stringJoiner = stringJoiner2;
                return stringJoiner.toString();
            }
            case M: {
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.M : gH.k)));
                stringJoiner2.add(go.A("\\\u0000_t\\\u0006M\u001aLt+e"));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.M : gH.k)));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
            }
            case L: {
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.M : gH.k)));
                stringJoiner2.add(xe.A((Object)"\u0006i\u0005\u001d\u0006o\u0017s\u0016\u001dq\u000f"));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.M : gH.k)));
                stringJoiner2.add(jF.A((gH)(a3 == tg.D ? gH.K : gH.F)));
            }
        }
        stringJoiner = stringJoiner2;
        return stringJoiner.toString();
    }

    /*
     * Enabled aggressive block sorting
     */
    private /* synthetic */ String A(og a2) {
        switch (a2) {
            case I: 
            case g: 
            case i: 
            case M: 
            case L: {
                return go.A("\u0010a&m7|=g:");
            }
        }
        return xe.A((Object)"\u0001T6X");
    }

    public zG() {
        zG a2;
    }
}

