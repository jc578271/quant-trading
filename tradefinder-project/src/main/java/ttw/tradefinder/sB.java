/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.We
 *  ttw.tradefinder.ch
 *  ttw.tradefinder.p
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.sB
 *  ttw.tradefinder.v
 *  ttw.tradefinder.w
 */
package ttw.tradefinder;

import java.lang.ref.Reference;
import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import ttw.tradefinder.H;
import ttw.tradefinder.P;
import ttw.tradefinder.Q;
import ttw.tradefinder.We;
import ttw.tradefinder.ch;
import ttw.tradefinder.p;
import ttw.tradefinder.q;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import ttw.tradefinder.v;
import ttw.tradefinder.w;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class sB
implements Q,
q {
    private List<We> F;
    private String e;
    private final Map<String, p> i;
    private v k;
    private final H I;
    private final Object G;
    private final Map<String, Map<rI, List<WeakReference<P>>>> D;

    public abstract /* synthetic */ p A(H var1, rH var2, ch var3);

    public List<String> a() {
        sB a2;
        ArrayList<String> arrayList = new ArrayList<String>();
        Object object = a2.G;
        synchronized (object) {
            for (Map.Entry entry : a2.i.entrySet()) {
                if (!((p)entry.getValue()).A()) continue;
                arrayList.add((String)entry.getKey());
            }
            return arrayList;
        }
    }

    /*
     * Enabled aggressive block sorting
     * Enabled unnecessary exception pruning
     * Enabled aggressive exception aggregation
     */
    public void A(String a2, rI a3, P a4) {
        sB a5;
        Object object = a5.G;
        synchronized (object) {
            WeakReference weakReference;
            if (a5.i.containsKey(a2)) {
                ((p)a5.i.get(a2)).f((rI)((Object)a3), (P)a4);
            }
            if (!a5.D.containsKey(a2)) {
                a5.D.put(a2, new HashMap());
            }
            if (!(a2 = (Map)a5.D.get(a2)).containsKey(a3)) {
                a2.put(a3, new ArrayList());
            }
            a2 = (List)a2.get(a3);
            a3 = a2.iterator();
            block3: do {
                Iterator iterator = a3;
                while (true) {
                    if (!iterator.hasNext()) {
                        a2.add(new WeakReference<void>(a4));
                        return;
                    }
                    weakReference = (WeakReference)a3.next();
                    if (weakReference.get() != null) continue block3;
                    Iterator iterator2 = a3;
                    iterator = iterator2;
                    iterator2.remove();
                }
            } while (weakReference.get() != a4);
            return;
        }
    }

    public List<We> A() {
        sB a2;
        return a2.F;
    }

    public void A(P a2) {
        sB a3;
        Object object = a3.G;
        synchronized (object) {
            Iterator iterator;
            Iterator iterator2 = iterator = a3.i.values().iterator();
            while (iterator2.hasNext()) {
                Object object2 = (p)iterator.next();
                object2.A(a2);
                iterator2 = iterator;
            }
            for (Object object2 : a3.D.values()) {
                for (Object object3 : object2.values()) {
                    object3 = object3.iterator();
                    while (object3.hasNext()) {
                        WeakReference weakReference = (WeakReference)object3.next();
                        if (weakReference.get() != null && weakReference.get() != a2) continue;
                        object3.remove();
                    }
                }
            }
            return;
        }
    }

    @Override
    public void A(String a2, boolean a3) {
        sB a4;
        if (!a4.I.A().contains(a2)) {
            return;
        }
        a4.f(a2, a3);
    }

    public void f() {
        Iterator<Object> iterator2;
        sB a2;
        Object object = a2.D.values().iterator();
        while (object.hasNext()) {
            iterator2 = (Map)object.next();
            for (Iterator<Object> iterator2 : iterator2.values()) {
                iterator2 = iterator2.iterator();
                Iterator<Object> iterator3 = iterator2;
                while (iterator3.hasNext()) {
                    ((WeakReference)iterator2.next()).clear();
                    iterator3 = iterator2;
                }
            }
        }
        sB sB2 = a2;
        sB2.D.clear();
        Object object2 = object = sB2.F.iterator();
        while (object2.hasNext()) {
            iterator2 = (We)object.next();
            iterator2.A();
            object2 = object;
        }
        object = a2.G;
        synchronized (object) {
            iterator2 = a2.i.values().iterator();
            Iterator<Object> iterator4 = iterator2;
            while (iterator4.hasNext()) {
                p p2 = (p)iterator2.next();
                p2.A();
                iterator4 = iterator2;
            }
        }
        a2.i.clear();
    }

    public void A(String a2, p a3) {
        sB a4;
        Object object = a4.G;
        synchronized (object) {
            block7: {
                if (!a4.D.containsKey(a2)) break block7;
                for (Map.Entry entry : ((Map)a4.D.get(a2)).entrySet()) {
                    Iterator iterator = ((List)entry.getValue()).iterator();
                    while (iterator.hasNext()) {
                        Iterator iterator2;
                        WeakReference weakReference = (WeakReference)iterator2.next();
                        if (weakReference.get() == null) {
                            Iterator iterator3 = iterator2;
                            iterator = iterator3;
                            iterator3.remove();
                            continue;
                        }
                        a3.f((rI)entry.getKey(), (P)weakReference.get());
                        iterator = iterator2;
                    }
                }
            }
            return;
        }
    }

    public List<String> f() {
        sB a2;
        ArrayList<String> arrayList = new ArrayList<String>();
        Object object = a2.G;
        synchronized (object) {
            Iterator iterator;
            Iterator iterator2 = iterator = a2.i.entrySet().iterator();
            while (iterator2.hasNext()) {
                Map.Entry entry = iterator.next();
                arrayList.add((String)entry.getKey());
                iterator2 = iterator;
            }
            return arrayList;
        }
    }

    public void f(String a2, rI a3, P a4) {
        sB a5;
        Object object = a5.G;
        synchronized (object) {
            if (a5.i.containsKey(a2)) {
                ((p)a5.i.get(a2)).A((rI)a3, a4);
            }
            if (a5.D.containsKey(a2) && (a2 = (Map)a5.D.get(a2)).containsKey(a3)) {
                a2 = ((List)a2.get(a3)).iterator();
                while (a2.hasNext()) {
                    a3 = (WeakReference)a2.next();
                    if (((Reference)a3).get() != null && ((Reference)a3).get() != a4) continue;
                    a2.remove();
                }
            }
            return;
        }
    }

    public void f(long a2) {
        sB a3;
        Object object = a3.G;
        synchronized (object) {
            Iterator iterator;
            Iterator iterator2 = iterator = a3.i.values().iterator();
            while (iterator2.hasNext()) {
                ((p)iterator.next()).A(a2);
                iterator2 = iterator;
            }
            return;
        }
    }

    public p A(rH a2, ch a322) {
        sB a4;
        Object object = a4.G;
        synchronized (object) {
            p p2;
            block9: {
                p2 = (p)a4.i.get(((rH)a2).G);
                if (p2 != null) {
                    return p2;
                }
                sB sB2 = a4;
                p2 = sB2.A(sB2.I, (rH)a2, (ch)a322);
                if (p2 instanceof w) {
                    a4.A(((rH)a2).G, (w)p2);
                }
                sB sB3 = a4;
                sB3.i.put(((rH)a2).G, p2);
                if (!sB3.D.containsKey(((rH)a2).G)) break block9;
                for (Map.Entry a322 : ((Map)a4.D.get(((rH)a2).G)).entrySet()) {
                    Iterator iterator = ((List)a322.getValue()).iterator();
                    while (iterator.hasNext()) {
                        Iterator iterator2;
                        WeakReference weakReference = (WeakReference)iterator2.next();
                        if (weakReference.get() == null) {
                            Iterator iterator3 = iterator2;
                            iterator = iterator3;
                            iterator3.remove();
                            continue;
                        }
                        p2.f((rI)a322.getKey(), (P)weakReference.get());
                        iterator = iterator2;
                    }
                }
            }
            return p2;
        }
    }

    public void f(String a2, w a3) {
        sB a4;
        a4.I.f(a2, a3);
    }

    public H A() {
        sB a2;
        return a2.I;
    }

    public void A(String a2, w a3) {
        sB a4;
        a4.I.A(a2, a3);
    }

    public void A(long a2) {
        sB a3;
        Object object = a3.G;
        synchronized (object) {
            Iterator iterator;
            Iterator iterator2 = iterator = a3.i.values().iterator();
            while (iterator2.hasNext()) {
                ((p)iterator.next()).f(a2);
                iterator2 = iterator;
            }
            return;
        }
    }

    public abstract /* synthetic */ void f(String var1, boolean var2);

    public void f(String a2) {
        sB a3;
        Object object = a3.G;
        synchronized (object) {
            if (!a3.i.containsKey(a2)) {
                return;
            }
            a2 = (p)a3.i.get(a2);
            a2.A(true, true);
            a2.f();
            return;
        }
    }

    public sB(H a2, String a3, v a4) {
        sB a5;
        sB sB2 = a5;
        sB sB3 = a5;
        sB sB4 = a5;
        sB4.e = "";
        a5.G = new Object();
        sB4.i = new HashMap();
        sB3.D = new HashMap();
        sB3.F = new ArrayList();
        sB3.I = a2;
        sB2.e = a3;
        sB2.k = a4;
    }

    public void A(We a2) {
        sB a3;
        a3.F.add(a2);
    }

    public void a(String a2, rI a3, P a4) {
        sB a5;
        if (a5.k != null) {
            a5.k.A(a2, a3, a4);
        }
    }

    public p A(String a2) {
        sB a3;
        Object object = a3.G;
        synchronized (object) {
            return (p)a3.i.get(a2);
        }
    }

    public void a(String a2) {
        sB a3;
        a3.I.A(a2);
    }

    public String A() {
        sB a2;
        return a2.e;
    }

    public void A(String a2) {
        sB a3;
        Object object = a3.G;
        synchronized (object) {
            if (a3.i.containsKey(a2)) {
                a2 = (p)a3.i.remove(a2);
                a2.A();
                if (a3.k != null) {
                    a3.k.A((P)((Object)a2));
                }
            }
            return;
        }
    }

    public void A(rH a2, ch a3) {
        sB a4;
        Object object = a4.G;
        synchronized (object) {
            a4.A(a2, a3);
            return;
        }
    }
}

