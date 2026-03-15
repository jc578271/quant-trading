/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.MB
 *  ttw.tradefinder.Na
 *  ttw.tradefinder.Xc
 *  ttw.tradefinder.x
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import ttw.tradefinder.MB;
import ttw.tradefinder.Na;
import ttw.tradefinder.x;

public class Xc {
    private final Object I;
    private final List<MB> G;
    private final x D;

    public void f() {
        Xc a2;
        Object object = a2.I;
        synchronized (object) {
            a2.G.clear();
            return;
        }
    }

    public Xc(x a2) {
        Xc a3;
        Xc xc2 = a3;
        xc2.I = new Object();
        xc2.G = new ArrayList();
        xc2.D = a2;
    }

    public void A() {
        Xc a2;
        a2.f();
    }

    public void A(int a2, int a3) {
        MB mB2;
        Xc a4;
        ArrayList<MB> arrayList = new ArrayList<MB>();
        Iterator iterator = a4.I;
        synchronized (iterator) {
            mB2 = a4.G.iterator();
            while (mB2.hasNext()) {
                MB mB3 = (MB)mB2.next();
                if (!mB3.A(a2, a3)) continue;
                arrayList.add(mB3);
                mB2.remove();
            }
        }
        Iterator iterator2 = iterator = arrayList.iterator();
        while (iterator2.hasNext()) {
            mB2 = (MB)iterator.next();
            iterator2 = iterator;
            a4.D.A(mB2);
        }
    }

    public void A(int a2, Na a3, int a4, int a5) {
        Xc a6;
        if (!a3.m || a3.I <= a4 && a3.I >= a5) {
            return;
        }
        Object object = a6.I;
        synchronized (object) {
            a6.G.add(new MB(a2, a3, a4, a5));
            return;
        }
    }
}

