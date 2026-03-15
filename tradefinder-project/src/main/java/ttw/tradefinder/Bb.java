/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Bb
 *  ttw.tradefinder.wC
 */
package ttw.tradefinder;

import java.util.ArrayList;
import ttw.tradefinder.wC;

public class Bb
extends ArrayList<wC> {
    private int G;
    private int D;

    public Bb() {
        Bb a2;
        Bb bb = a2;
        bb.D = 0;
        bb.G = 0;
    }

    public int f() {
        Bb a2;
        return a2.D;
    }

    public boolean A(wC a2) {
        Bb a3;
        Bb bb = a3;
        bb.D = Math.max(bb.D, a2.f());
        bb.G += a2.A();
        super.add(a2);
        return true;
    }

    public int A() {
        Bb a2;
        return a2.G;
    }
}

