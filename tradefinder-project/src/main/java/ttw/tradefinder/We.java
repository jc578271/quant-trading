/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ca
 *  ttw.tradefinder.Gf
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Mf
 *  ttw.tradefinder.PF
 *  ttw.tradefinder.We
 *  ttw.tradefinder.ma
 *  ttw.tradefinder.rE
 *  ttw.tradefinder.t
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.List;
import ttw.tradefinder.Ca;
import ttw.tradefinder.Gf;
import ttw.tradefinder.H;
import ttw.tradefinder.Mf;
import ttw.tradefinder.PF;
import ttw.tradefinder.bg;
import ttw.tradefinder.ma;
import ttw.tradefinder.q;
import ttw.tradefinder.rE;
import ttw.tradefinder.t;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class We {
    private boolean d;
    private final int g;
    private final String f;
    private final Gf a;
    private final String K;
    private List<We> m;
    private List<PF> F;
    private final Ca e;
    private List<We> i;
    private final rE k;
    private final t I;
    private boolean G;
    private final boolean D;

    public boolean isFEnabled() {
        We a2;
        return a2.d;
    }

    public We(H a2, ma a3, boolean a4, int a5, rE a6, Gf a7) {
        a8(a2, a3, a4, a5, a6, a7, null);
        We a8;
    }

    public List<PF> a() {
        We a2;
        return a2.F;
    }

    public We(H a2, ma a3, int a4, rE a5, Gf a6, Ca a7) {
        a8(a2, a3, false, a4, a5, a6, a7);
        We a8;
    }

    private /* synthetic */ void f(We a2) {
        We a3;
        a3.i.add(a2);
    }

    public boolean A(bg a2, boolean a3) {
        We a4;
        if (!a4.I.A(a2)) {
            return false;
        }
        return a3 || a4.D;
    }

    public String f() {
        We a2;
        return a2.K;
    }

    public void A(q a2, Mf a3) {
        We a4;
        a4.F.add(new PF(a2, a3));
        a2.A(a4);
    }

    public Ca A() {
        We a2;
        return a2.e;
    }

    public Gf A() {
        We a2;
        return a2.a;
    }

    public We(H a2, ma a3) {
        a4(a2, a3, -1);
        We a4;
    }

    public List<We> f() {
        We a2;
        return a2.m;
    }

    public void A() {
        We a2;
        a2.I.A();
    }

    public rE A() {
        We a2;
        return a2.k;
    }

    public We(H a2, ma a3, boolean a4, int a5) {
        a6(a2, a3, a4, a5, rE.G, Gf.d);
        We a6;
    }

    public We(H a2, ma a3, int a4) {
        a5(a2, a3, false, a4);
        We a5;
    }

    public We(H a2, ma a3, boolean a4, int a5, rE a6, Gf a7, Ca a8) {
        We a9;
        We we = a9;
        We we2 = a9;
        We we3 = a9;
        We we4 = a9;
        We we5 = a9;
        We we6 = a9;
        We we7 = a9;
        we7.d = true;
        we6.G = false;
        we7.F = new ArrayList();
        we6.i = new ArrayList();
        we6.m = new ArrayList();
        we5.f = a3.A();
        we5.K = a3.f();
        we4.g = a5;
        we4.D = a3.A();
        we3.a = a7;
        we3.I = a3.A(a2.A()).A(null);
        we2.k = a6;
        we2.e = a8;
        we.d = true;
        we.G = a4;
    }

    public void A(boolean a2) {
        a.d = a2;
    }

    public String A() {
        We a2;
        return a2.f;
    }

    public void A(We a2) {
        We a3;
        a3.A(a2, true);
    }

    public void A(We a2, boolean a3) {
        We a4;
        We we = a4;
        We we2 = a2;
        we.m.add(we2);
        a2.f(a4);
        we2.A(we.k == rE.G ? a3 : false);
    }

    public List<We> A() {
        We a2;
        return a2.i;
    }

    public void A(q a2) {
        We a3;
        a3.A(a2, Mf.U);
    }

    public int A() {
        We a2;
        return a2.g;
    }

    public boolean A() {
        We a2;
        return a2.G;
    }
}

