/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.D
 *  ttw.tradefinder.H
 *  ttw.tradefinder.eI
 *  ttw.tradefinder.la
 *  ttw.tradefinder.nb
 *  ttw.tradefinder.sB
 *  ttw.tradefinder.t
 *  ttw.tradefinder.y
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpacePainter
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpacePainterFactory
 *  velox.api.layer1.messages.indicators.AliasFilter
 *  velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyScreenSpacePainter
 *  velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyScreenSpacePainter$Builder
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Set;
import ttw.tradefinder.BI;
import ttw.tradefinder.D;
import ttw.tradefinder.H;
import ttw.tradefinder.bg;
import ttw.tradefinder.eI;
import ttw.tradefinder.kH;
import ttw.tradefinder.la;
import ttw.tradefinder.rg;
import ttw.tradefinder.sB;
import ttw.tradefinder.t;
import ttw.tradefinder.y;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvasFactory;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpacePainter;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpacePainterFactory;
import velox.api.layer1.messages.indicators.AliasFilter;
import velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyScreenSpacePainter;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class nb
extends sB
implements D,
ScreenSpacePainterFactory {
    private final t e;
    private final la i;
    private String k;
    private boolean I;
    private boolean G;
    private Layer1ApiUserMessageModifyScreenSpacePainter.Builder D;

    public void f() {
        nb a2;
        nb nb2 = a2;
        super.f();
        nb2.A();
        nb2.e.A();
    }

    public Set<String> A() {
        nb a2;
        return Collections.singleton(a2.k);
    }

    public la A() {
        nb a2;
        return a2.i;
    }

    public void a() {
        nb a2;
        if (!a2.A().A() && !a2.i.A()) {
            return;
        }
        a2.I = true;
        if (!a2.e.A((bg)bg.G)) {
            return;
        }
        if (a2.G) {
            return;
        }
        nb nb2 = a2;
        List list = nb2.A(nb2.A().A(), a2.a());
        if (list.isEmpty()) {
            return;
        }
        nb nb3 = a2;
        list = nb3.D.setIsAdd(true).setAliasFilter((AliasFilter)new kH(list)).build();
        nb3.k = ((Layer1ApiUserMessageModifyScreenSpacePainter)list).fullName;
        nb3.A().A((Object)list);
        nb3.G = true;
    }

    private /* synthetic */ List<String> A(List<String> a2, List<String> a3) {
        ArrayList<String> arrayList = new ArrayList<String>();
        a3 = a3.iterator();
        while (a3.hasNext()) {
            String string = (String)a3.next();
            if (!a2.contains(string)) continue;
            arrayList.add(string);
        }
        return arrayList;
    }

    public void A(t a2, boolean a3, boolean a4) {
        nb a5;
        if (a3 && a5.I) {
            a5.a();
            return;
        }
        if (!a3 && a5.G) {
            a5.A().A((Object)a5.D.setIsAdd(false).build());
            a5.G = false;
        }
    }

    public boolean A(String a2) {
        nb a3;
        return a2.equals(a3.k);
    }

    public void I(String a2) {
    }

    public void f(String a2, boolean a3) {
        nb a4;
        if (!a4.I) {
            return;
        }
        nb nb2 = a4;
        nb2.A();
        nb2.a();
    }

    public void A() {
        nb a2;
        a2.I = false;
        if (!a2.G) {
            return;
        }
        a2.A().A((Object)a2.D.setIsAdd(false).build());
        a2.G = false;
    }

    public ScreenSpacePainter createScreenSpacePainter(String a2, String a3, ScreenSpaceCanvasFactory a4) {
        nb a5;
        if (!a5.A(a2)) {
            return new BI(a5);
        }
        if (!a5.A().isStrategyEnabled(a3)) {
            return new eI(a5);
        }
        nb nb2 = a5;
        a2 = nb2.A(nb2.A().A(a3), a5.A().A(a3));
        if (!(a2 instanceof y)) {
            return new rg(a5);
        }
        a2 = (y)a2;
        a2.A().A(a4);
        return a2.A();
    }

    public nb(H a2, la a3) {
        nb a4;
        H h2 = a2;
        nb nb2 = a4;
        nb nb3 = a4;
        super(a2, a3.C(), a3.A());
        a4.I = false;
        nb3.G = false;
        nb3.k = "";
        nb2.i = a3;
        nb2.e = a3.f(h2.A()).A((D)a4);
        a4.D = Layer1ApiUserMessageModifyScreenSpacePainter.builder(h2.getClass(), (String)a4.A()).setScreenSpacePainterFactory((ScreenSpacePainterFactory)a4);
    }
}

