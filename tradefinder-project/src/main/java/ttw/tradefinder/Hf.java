/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Hf
 *  ttw.tradefinder.Ya
 */
package ttw.tradefinder;

import java.awt.Color;
import java.util.HashMap;
import java.util.Map;
import ttw.tradefinder.Ya;
import ttw.tradefinder.ek;

public class Hf
extends Ya<ek> {
    private Map<String, Color> D = new HashMap();

    public void A(String a2, Color a3) {
        Hf a4;
        if (a4.D == null) {
            a4.D = new HashMap();
        }
        a4.D.put(a2, a3);
    }

    public Color A(String a2) {
        Hf a3;
        if (a3.D == null) {
            a3.D = new HashMap();
        }
        return (Color)a3.D.get(a2);
    }

    public ek A() {
        Hf a2;
        ek ek2 = new ek();
        new ek().IsDefault = a2.G;
        ek ek3 = ek2;
        ek3.b(a2.D);
        return ek3;
    }

    public void A(ek a2) {
        Hf a3;
        ek ek2 = a2;
        a3.G = ek2.IsDefault;
        ek2.a(a3.D);
    }

    public Hf() {
        super(ek.class);
        Hf a2;
    }
}

