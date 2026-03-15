/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Gc
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.oC
 *  ttw.tradefinder.p
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.y
 */
package ttw.tradefinder;

import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.Q;
import ttw.tradefinder.Xb;
import ttw.tradefinder.oC;
import ttw.tradefinder.p;
import ttw.tradefinder.rH;
import ttw.tradefinder.y;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class Gc
extends Xb
implements p,
y {
    private final oC D;

    public void f() {
        Gc a2;
        super.f();
    }

    public I A() {
        Gc a2;
        return a2.D;
    }

    public boolean A(boolean a2) {
        Gc a3;
        if (a3.D.A() == a2) {
            return false;
        }
        Gc gc = a3;
        boolean bl = a2;
        gc.D.A(bl);
        gc.A(bl);
        return true;
    }

    public void A(boolean a2, boolean a3) {
    }

    public Gc(H a2, rH a3, Q a4) {
        super(a2, a3, a4);
        Gc a5;
        a5.D = new oC(a2, a3);
    }

    public void A() {
        Gc a2;
        Gc gc = a2;
        super.A();
        gc.D.A();
    }

    public boolean A() {
        Gc a2;
        return a2.D.A();
    }
}

