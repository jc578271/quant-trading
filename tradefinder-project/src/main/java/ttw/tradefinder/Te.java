/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Mc
 *  ttw.tradefinder.SF
 *  ttw.tradefinder.Te
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.bb
 *  ttw.tradefinder.kb
 *  ttw.tradefinder.ma
 *  ttw.tradefinder.qc
 *  ttw.tradefinder.uf
 *  ttw.tradefinder.vC
 *  ttw.tradefinder.yF
 */
package ttw.tradefinder;

import java.util.Arrays;
import java.util.Collections;
import ttw.tradefinder.Mc;
import ttw.tradefinder.SF;
import ttw.tradefinder.UC;
import ttw.tradefinder.X;
import ttw.tradefinder.bb;
import ttw.tradefinder.kb;
import ttw.tradefinder.ma;
import ttw.tradefinder.qc;
import ttw.tradefinder.uf;
import ttw.tradefinder.vC;
import ttw.tradefinder.yF;

public class Te
implements ma {
    private final yF D;

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ String A() {
        Te a2;
        switch (uf.D[a2.D.ordinal()]) {
            case 2: {
                return SF.A((Object)"\u0005\u0015\u0006l\u0019(5%4/\u001e35$#2");
            }
            case 3: {
                return vC.A((Object)"6F5?+\u007f\u0000s\u000es\fq\u0007F\u0010s\u0001y\u0007`");
            }
            case 4: {
                return SF.A((Object)"\u0005\u0015\u0006l\u001d( 48%85(\u0015# 2*43");
            }
            case 5: {
                return vC.A((Object)"6F5?/s\u0010y\u0007f'j\u0012~\r`\u0007`");
            }
            case 1: {
                return SF.A((Object)"\u0005\u0015\u0006l\u000138\"4\u0000=$#5");
            }
            case 6: {
                return vC.A((Object)"F6EOA\u0012w\u0007v/w\u0016w\u0010a");
            }
            case 7: {
                return SF.A((Object)"\u0015\u0005\u0016|\u0015#$?%q\u0000? =8+$#arp");
            }
            case 8: {
                return vC.A((Object)"F6EOF\u0010w\fvBS\fs\u000ek\u0018w\u00102A ");
            }
            case 9: {
                return SF.A((Object)"\u0006=.3 =a\u0002$%58/62");
            }
            case 10: 
            case 11: {
                return vC.A((Object)"1w\u0016f\u000b|\u0005a");
            }
        }
        return "";
    }

    public /* synthetic */ String f() {
        Te a2;
        return a2.A();
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ boolean A() {
        Te a2;
        switch (uf.D[a2.D.ordinal()]) {
            case 1: {
                return false;
            }
        }
        return true;
    }

    public /* synthetic */ X A(Mc a2) {
        Te a3;
        switch (uf.D[a3.D.ordinal()]) {
            case 10: {
                return new qc(a3.D.toString() + " settings", Arrays.asList(UC.k, UC.m, UC.i, UC.F, UC.G), a2);
            }
            case 2: {
                return new qc(a3.D.toString() + " settings", Arrays.asList(UC.k, UC.m, UC.i, UC.F, UC.G), a2);
            }
            case 3: {
                return new qc(a3.D.toString() + " settings", Arrays.asList(UC.i, UC.F, UC.G), a2);
            }
            case 4: {
                return new qc(a3.D.toString() + " settings", Arrays.asList(UC.m, UC.i, UC.F, UC.G), a2);
            }
            case 5: {
                new qc(a3.D.toString() + " settings", Arrays.asList(UC.k, UC.m, UC.i, UC.F, UC.G), a2);
            }
            case 1: {
                return new qc(a3.D.toString() + " settings", Arrays.asList(UC.i, UC.F, UC.G), a2);
            }
            case 6: {
                return new qc(a3.D.toString() + " settings", Collections.emptyList(), a2);
            }
            case 7: 
            case 8: 
            case 11: {
                return new bb();
            }
            case 9: {
                return new qc(a3.D.toString() + " settings", Arrays.asList(UC.k, UC.m, UC.i, UC.F, UC.G), a2);
            }
        }
        return new kb();
    }

    public /* synthetic */ Te(yF a2) {
        Te a3;
        a3.D = a2;
    }
}

