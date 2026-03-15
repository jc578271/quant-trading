/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.H
 *  ttw.tradefinder.I
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.Xb
 *  ttw.tradefinder.p
 *  ttw.tradefinder.pb
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.y
 *  ttw.tradefinder.zC
 */
package ttw.tradefinder;

import ttw.tradefinder.H;
import ttw.tradefinder.I;
import ttw.tradefinder.Q;
import ttw.tradefinder.Xb;
import ttw.tradefinder.p;
import ttw.tradefinder.pb;
import ttw.tradefinder.rH;
import ttw.tradefinder.y;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class zC
extends Xb
implements p,
y {
    private final pb D;

    public I A() {
        zC a2;
        return a2.D;
    }

    public boolean A(boolean a2) {
        zC a3;
        if (a3.D.A() == a2) {
            return false;
        }
        zC zC2 = a3;
        boolean bl = a2;
        zC2.D.A(bl);
        zC2.A(bl);
        return true;
    }

    public void A() {
        zC a2;
        zC zC2 = a2;
        super.A();
        zC2.D.A();
    }

    public boolean A() {
        zC a2;
        return a2.D.A();
    }

    public zC(H a2, rH a3, Q a4) {
        super(a2, a3, a4);
        zC a5;
        a5.D = new pb(a2, a3);
    }

    public void A(boolean a2, boolean a3) {
    }

    public void f() {
        zC a2;
        super.f();
    }
}

