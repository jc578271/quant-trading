/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Mc
 *  ttw.tradefinder.ND
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.UB
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.bI
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.vE
 */
package ttw.tradefinder;

import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import ttw.tradefinder.Di;
import ttw.tradefinder.H;
import ttw.tradefinder.Ie;
import ttw.tradefinder.Mc;
import ttw.tradefinder.ND;
import ttw.tradefinder.P;
import ttw.tradefinder.Q;
import ttw.tradefinder.UB;
import ttw.tradefinder.bI;
import ttw.tradefinder.cc;
import ttw.tradefinder.eh;
import ttw.tradefinder.lg;
import ttw.tradefinder.mB;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import ttw.tradefinder.vE;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public abstract class Xb {
    public final rH e;
    private final Q i;
    private final Object k;
    private final Map<rI, List<WeakReference<P>>> I;
    public final H G;
    private List<eh> D;

    public void A(rI a2, ND a3) {
    }

    public void A(boolean a2) {
        Xb a3;
        Xb xb2 = a3;
        xb2.i.A(xb2.e.G, a2);
    }

    public void f(rI a2, int a3, mB a4, long a5) {
        Xb a6;
        Object object = a6.k;
        synchronized (object) {
            if (!a6.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a6.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4, a5);
            }
            return;
        }
    }

    public void A(rI a2, int a3, boolean a4, long a5) {
    }

    public void A(rI a2, lg a3) {
    }

    public void A() {
        Xb a2;
        for (Object object : a2.I.values()) {
            Object object2 = object = object.iterator();
            while (object2.hasNext()) {
                ((WeakReference)object.next()).clear();
                object2 = object;
            }
        }
        a2.I.clear();
    }

    public void A(rI a2, int a3, cc a4, long a5, long a6) {
    }

    public void A(eh a2) {
        Xb a3;
        a3.D.add(a2);
    }

    public void A(rI a2, boolean a3, boolean a4) {
    }

    public Di A() {
        Xb a2;
        Xb xb2 = a2;
        return xb2.G.A(xb2.e.G);
    }

    public void A(rI a2, int a3, mB a4, long a5) {
    }

    public void A(rI a2, int a3, mB a4, long a5, long a6) {
    }

    public void f(rI a2, P a3) {
        Xb a4;
        Object object = a4.k;
        synchronized (object) {
            int n2;
            if (!a4.I.containsKey(a2)) {
                return;
            }
            ((List)a4.I.get(a2)).add(new WeakReference<P>(a3));
            bI[] bIArray = bI.values();
            int n3 = bIArray.length;
            int n4 = n2 = 0;
            while (n4 < n3) {
                bI bI2 = bIArray[n2];
                rI rI2 = a2;
                a3.A(rI2, bI2, a4.A(rI2, bI2));
                n4 = ++n2;
            }
            return;
        }
    }

    /*
     * Enabled aggressive block sorting
     */
    public boolean A(rI a2, bI a3) {
        switch (a3) {
            case I: {
                return true;
            }
            case k: {
                return false;
            }
            case G: {
                return true;
            }
        }
        return true;
    }

    public void f(rI a2, int a3, mB a4, long a5, long a6) {
        Xb a7;
        Object object = a7.k;
        synchronized (object) {
            if (!a7.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a7.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4, a5, a6);
            }
            return;
        }
    }

    public void A(vE a2) {
        Xb a3;
        a3.G.f(a2);
    }

    public void A(P a2) {
        Xb a3;
        Object object = a3.k;
        synchronized (object) {
            block4: for (Object object2 : a3.I.values()) {
                object2 = object2.iterator();
                block5: while (true) {
                    Object object3 = object2;
                    while (object3.hasNext()) {
                        WeakReference weakReference = (WeakReference)object2.next();
                        if (weakReference.get() == null) {
                            Object object4 = object2;
                            object3 = object4;
                            object4.remove();
                            continue;
                        }
                        if (weakReference.get() != a2) continue block5;
                        weakReference.clear();
                        object2.remove();
                        continue block5;
                    }
                    continue block4;
                    break;
                }
            }
            return;
        }
    }

    public void f(rI a2, UB a3, long a4) {
        Xb a5;
        Object object = a5.k;
        synchronized (object) {
            if (!a5.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a5.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4);
            }
            return;
        }
    }

    public void f(rI a2, bI a3, boolean a4) {
        Xb a5;
        Object object = a5.k;
        synchronized (object) {
            if (!a5.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a5.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4);
            }
            return;
        }
    }

    public void j(long a2) {
    }

    public void A(String a2, String a3) {
        Xb a4;
        a4.G.f(a2, a3);
    }

    public void f(rI a2, lg a3) {
        Xb a4;
        Object object = a4.k;
        synchronized (object) {
            if (!a4.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a4.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3);
            }
            return;
        }
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = (2 ^ 5) << 4;
        int cfr_ignored_0 = (2 ^ 5) << 4 ^ 5;
        int n5 = n3;
        int n6 = (3 ^ 5) << 4 ^ 4 << 1;
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

    public void A(rI a2, bI a3, boolean a4) {
    }

    public void f(rI a2, int a3, cc a4, long a5, long a6) {
        Xb a7;
        Object object = a7.k;
        synchronized (object) {
            if (!a7.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a7.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4, a5, a6);
            }
            return;
        }
    }

    public void A(rI a2, UB a3, long a4) {
    }

    public Mc A() {
        Xb a2;
        return a2.G.A();
    }

    public Xb(H a2, rH a3, Q a4) {
        Xb a5;
        Xb xb2 = a5;
        Xb xb3 = a5;
        a5.k = new Object();
        xb3.I = new HashMap();
        xb3.D = Collections.synchronizedList(new ArrayList());
        xb3.G = a2;
        xb2.e = a3;
        xb2.i = a4;
    }

    public void A(bI a2, boolean a3) {
        Xb a4;
        Object object = a4.k;
        synchronized (object) {
            for (Map.Entry entry : a4.I.entrySet()) {
                for (WeakReference weakReference : (List)entry.getValue()) {
                    if (weakReference.get() == null) continue;
                    ((P)weakReference.get()).A((rI)entry.getKey(), a2, a3);
                }
            }
            return;
        }
    }

    public void f() {
        Xb a2;
        Iterator iterator;
        Iterator iterator2 = iterator = a2.D.iterator();
        while (iterator2.hasNext()) {
            ((eh)iterator.next()).A();
            iterator2 = iterator;
        }
    }

    public void A(rI a2, Double a3, long a4) {
        Xb a5;
        Object object = a5.k;
        synchronized (object) {
            if (!a5.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a5.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4);
            }
            return;
        }
    }

    public void f(rI a2, Ie a3) {
        Xb a4;
        Object object = a4.k;
        synchronized (object) {
            if (!a4.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a4.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3);
            }
            return;
        }
    }

    public void A(rI a2, P a3) {
        Xb a4;
        Object object = a4.k;
        synchronized (object) {
            if (!a4.I.containsKey(a2)) {
                return;
            }
            a2 = ((List)a4.I.get(a2)).iterator();
            block3: while (true) {
                Object object2 = a2;
                while (object2.hasNext()) {
                    WeakReference weakReference = (WeakReference)a2.next();
                    if (weakReference.get() == null) {
                        Object object3 = a2;
                        object2 = object3;
                        object3.remove();
                        continue;
                    }
                    if (weakReference.get() != a3) continue block3;
                    weakReference.clear();
                    a2.remove();
                    continue block3;
                }
                break;
            }
            return;
        }
    }

    public void A(rI a2, Ie a3) {
    }

    public void f(rI a2, boolean a3, boolean a4) {
        Xb a5;
        Object object = a5.k;
        synchronized (object) {
            if (!a5.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a5.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4);
            }
            return;
        }
    }

    public void f(rI a2, ND a3) {
        Xb a4;
        Object object = a4.k;
        synchronized (object) {
            if (!a4.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a4.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3);
            }
            return;
        }
    }

    public void f(rI a2, int a3, boolean a4, long a5) {
        Xb a6;
        Object object = a6.k;
        synchronized (object) {
            if (!a6.I.containsKey(a2)) {
                return;
            }
            for (WeakReference weakReference : (List)a6.I.get(a2)) {
                if (weakReference.get() == null) continue;
                ((P)weakReference.get()).A(a2, a3, a4, a5);
            }
            return;
        }
    }

    public void A(rI a2) {
        Xb a3;
        Object object = a3.k;
        synchronized (object) {
            if (a3.I.containsKey(a2)) {
                return;
            }
            a3.I.put(a2, new ArrayList());
            return;
        }
    }

    public void k(long a2) {
        Xb a3;
        Iterator iterator;
        Iterator iterator2 = iterator = a3.D.iterator();
        while (iterator2.hasNext()) {
            ((eh)iterator.next()).A(a2);
            iterator2 = iterator;
        }
    }

    public void A(rI a2, double a3, long a4) {
    }
}

