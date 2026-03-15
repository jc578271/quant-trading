/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Na
 *  velox.api.layer1.settings.StrategySettingsVersion
 */
package ttw.tradefinder;

import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.Map;
import ttw.tradefinder.Na;
import ttw.tradefinder.hL;
import ttw.tradefinder.ha;
import velox.api.layer1.settings.StrategySettingsVersion;

@StrategySettingsVersion(currentVersion=3, compatibleVersions={3})
public class hK
extends ha {
    public boolean a = true;
    private Map<Integer, hL> c = new LinkedHashMap<Integer, hL>();

    public void a(Map<Integer, Na> a2) {
        Iterator<Map.Entry<Integer, hL>> iterator;
        hK a3;
        if (a3.c == null) {
            a3.c = new LinkedHashMap<Integer, hL>();
        }
        a2.clear();
        Iterator<Map.Entry<Integer, hL>> iterator2 = iterator = a3.c.entrySet().iterator();
        while (iterator2.hasNext()) {
            Map.Entry<Integer, hL> entry = iterator.next();
            Na na2 = new Na();
            na2.A(entry.getValue());
            a2.put(entry.getKey(), na2);
            iterator2 = iterator;
        }
    }

    public void b(Map<Integer, Na> a2) {
        hK a3;
        if (a3.c == null) {
            a3.c = new LinkedHashMap<Integer, hL>();
        }
        a3.c.clear();
        Object object = a2 = a2.entrySet().iterator();
        while (object.hasNext()) {
            Map.Entry entry = (Map.Entry)a2.next();
            a3.c.put((Integer)entry.getKey(), ((Na)entry.getValue()).A());
            object = a2;
        }
    }

    public hL a(int a2) {
        hK a3;
        if (a3.c == null) {
            a3.c = new LinkedHashMap<Integer, hL>();
        }
        if (a3.c.containsKey(a2)) {
            return a3.c.get(a2);
        }
        return new hL();
    }

    public hK() {
        hK a2;
    }

    public void a(int a2, hL a3) {
        hK a4;
        if (a4.c == null) {
            a4.c = new LinkedHashMap<Integer, hL>();
        }
        a4.c.put(a2, a3);
    }
}

