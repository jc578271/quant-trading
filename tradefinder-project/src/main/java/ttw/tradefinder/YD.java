/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ma
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.yf
 *  ttw.tradefinder.z
 */
package ttw.tradefinder;

import java.lang.ref.Reference;
import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import ttw.tradefinder.Ma;
import ttw.tradefinder.Ya;
import ttw.tradefinder.ha;
import ttw.tradefinder.yf;
import ttw.tradefinder.z;

public class YD<T extends Ya<TS>, TS extends ha>
implements Ma {
    public T I;
    public Object G;
    private List<WeakReference<z>> D;

    public void A(yf a2) {
        YD a3;
        Object object = new ArrayList();
        Iterator iterator = a3.G;
        synchronized (iterator) {
            object.addAll(a3.D);
        }
        Iterator iterator2 = iterator = object.iterator();
        while (iterator2.hasNext()) {
            object = (WeakReference)iterator.next();
            if (((Reference)object).get() == null) {
                Iterator iterator3 = iterator;
                iterator2 = iterator3;
                iterator3.remove();
                continue;
            }
            ((z)((Reference)object).get()).A(a2);
            iterator2 = iterator;
        }
    }

    /*
     * Enabled aggressive block sorting
     * Enabled unnecessary exception pruning
     * Enabled aggressive exception aggregation
     * Converted monitor instructions to comments
     * Lifted jumps to return sites
     */
    public void f(z a2) {
        Object object;
        WeakReference weakReference;
        YD a3;
        Object object2 = a3.G;
        // MONITORENTER : object2
        Iterator iterator = a3.D.iterator();
        block3: do {
            Iterator iterator2 = iterator;
            while (true) {
                if (!iterator2.hasNext()) {
                    object = object2;
                    // MONITOREXIT : object
                    return;
                }
                weakReference = (WeakReference)iterator.next();
                if (weakReference.get() != null) continue block3;
                Iterator iterator3 = iterator;
                iterator2 = iterator3;
                iterator3.remove();
            }
        } while (weakReference.get() != a2);
        object = object2;
        weakReference.clear();
        iterator.remove();
    }

    public YD(T a2) {
        YD a3;
        YD yD2 = a3;
        yD2.G = new Object();
        yD2.D = new ArrayList();
        yD2.I = a2;
    }

    public void A() {
        YD a2;
        Object object = a2.G;
        synchronized (object) {
            Iterator iterator;
            Iterator iterator2 = iterator = a2.D.iterator();
            while (iterator2.hasNext()) {
                ((WeakReference)iterator.next()).clear();
                iterator2 = iterator;
            }
            a2.D.clear();
            return;
        }
    }

    public void A(z a2) {
        YD a3;
        Object object = a3.G;
        synchronized (object) {
            a3.D.add(new WeakReference<z>(a2));
            return;
        }
    }
}

