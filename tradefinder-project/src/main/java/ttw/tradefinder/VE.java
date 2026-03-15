/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.VE
 *  ttw.tradefinder.Ya
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import ttw.tradefinder.Ya;
import ttw.tradefinder.es;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class VE
extends Ya<es> {
    private Map<String, Boolean> D = new HashMap();

    public Boolean A(String a2, boolean a3) {
        VE a4;
        if (a4.D == null) {
            a4.D = new HashMap();
        }
        if (!a4.D.containsKey(a2)) {
            a4.D.put(a2, a3);
        }
        return (Boolean)a4.D.get(a2);
    }

    public boolean A() {
        VE a2;
        Iterator iterator = a2.D.values().iterator();
        while (iterator.hasNext()) {
            if (!((Boolean)iterator.next()).booleanValue()) continue;
            return true;
        }
        return false;
    }

    public void A(String a2) {
        VE a3;
        if (a3.D == null) {
            a3.D = new HashMap();
        }
        if (a3.D.containsKey(a2)) {
            a3.D.remove(a2);
        }
    }

    public void A(VE a2) {
        VE a3;
        Iterator iterator;
        a2.D.clear();
        Iterator iterator2 = iterator = a3.D.entrySet().iterator();
        while (iterator2.hasNext()) {
            Map.Entry entry = iterator.next();
            a2.D.put((String)entry.getKey(), (Boolean)entry.getValue());
            iterator2 = iterator;
        }
    }

    public void A(String a2, Boolean a3) {
        VE a4;
        if (a4.D == null) {
            a4.D = new HashMap();
        }
        a4.D.put(a2, a3);
    }

    public es A() {
        VE a2;
        es es2 = new es();
        new es().IsDefault = a2.G;
        es es3 = es2;
        es3.b(a2.D);
        return es3;
    }

    public void A(es a2) {
        VE a3;
        es es2 = a2;
        a3.G = es2.IsDefault;
        es2.a(a3.D);
    }

    public Set<String> A() {
        VE a2;
        return a2.D.keySet();
    }

    public Boolean A(String a2) {
        VE a3;
        if (a3.D == null) {
            a3.D = new HashMap();
        }
        if (!a3.D.containsKey(a2)) {
            a3.D.put(a2, Boolean.FALSE);
        }
        return (Boolean)a3.D.get(a2);
    }

    public void A() {
        Iterator iterator;
        VE a2;
        if (a2.D == null) {
            a2.D = new HashMap();
        }
        Iterator iterator2 = iterator = a2.D.keySet().iterator();
        while (iterator2.hasNext()) {
            String string = (String)iterator.next();
            iterator2 = iterator;
            a2.D.put(string, Boolean.FALSE);
        }
    }

    public List<String> A() {
        VE a2;
        ArrayList<String> arrayList = new ArrayList<String>();
        for (Map.Entry entry : a2.D.entrySet()) {
            if (!((Boolean)entry.getValue()).booleanValue()) continue;
            arrayList.add((String)entry.getKey());
        }
        return arrayList;
    }

    public VE() {
        super(es.class);
        VE a2;
    }
}

