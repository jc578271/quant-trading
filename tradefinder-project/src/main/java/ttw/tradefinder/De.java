/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ac
 *  ttw.tradefinder.De
 *  ttw.tradefinder.Gf
 *  ttw.tradefinder.IB
 *  ttw.tradefinder.Kd
 *  ttw.tradefinder.Mc
 *  ttw.tradefinder.Pc
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.Td
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.UF
 *  ttw.tradefinder.Wc
 *  ttw.tradefinder.Yc
 *  ttw.tradefinder.Zc
 *  ttw.tradefinder.iD
 *  ttw.tradefinder.jA
 *  ttw.tradefinder.jc
 *  ttw.tradefinder.kb
 *  ttw.tradefinder.la
 *  ttw.tradefinder.lb
 *  ttw.tradefinder.qc
 *  ttw.tradefinder.v
 *  ttw.tradefinder.yF
 *  ttw.tradefinder.yc
 *  ttw.tradefinder.zD
 *  velox.api.layer1.messages.indicators.StrategyUpdateGeneratorFilter$StrategyUpdateGeneratorEventType
 */
package ttw.tradefinder;

import java.awt.Color;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashSet;
import java.util.Set;
import ttw.tradefinder.Ac;
import ttw.tradefinder.GB;
import ttw.tradefinder.Gf;
import ttw.tradefinder.IB;
import ttw.tradefinder.Kd;
import ttw.tradefinder.Ld;
import ttw.tradefinder.Mc;
import ttw.tradefinder.Pc;
import ttw.tradefinder.SE;
import ttw.tradefinder.Td;
import ttw.tradefinder.UC;
import ttw.tradefinder.UF;
import ttw.tradefinder.Wc;
import ttw.tradefinder.X;
import ttw.tradefinder.Yc;
import ttw.tradefinder.Zc;
import ttw.tradefinder.cc;
import ttw.tradefinder.fA;
import ttw.tradefinder.iD;
import ttw.tradefinder.jA;
import ttw.tradefinder.jc;
import ttw.tradefinder.kb;
import ttw.tradefinder.la;
import ttw.tradefinder.lb;
import ttw.tradefinder.qc;
import ttw.tradefinder.rI;
import ttw.tradefinder.v;
import ttw.tradefinder.yF;
import ttw.tradefinder.yc;
import ttw.tradefinder.zD;
import velox.api.layer1.messages.indicators.StrategyUpdateGeneratorFilter;

public class De
implements la {
    private final rI k;
    public static final String I = Td.A((Object)"\u0010z\u0013\u0003\bG5[-J-Z=z6O'E!\\");
    private final yF G;
    private final iD D;

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ Color f() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 8: {
                return zD.d;
            }
        }
        return Color.WHITE;
    }

    public /* synthetic */ yF A() {
        De a2;
        return a2.G;
    }

    public /* synthetic */ Wc A() {
        return new Wc();
    }

    public /* synthetic */ Set<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> A() {
        HashSet<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType> hashSet = new HashSet<StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType>();
        hashSet.add(StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.TRADES);
        hashSet.add(StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.DEPTH_MBP);
        hashSet.add(StrategyUpdateGeneratorFilter.StrategyUpdateGeneratorEventType.INSTRUMENTS);
        return hashSet;
    }

    public /* synthetic */ rI A() {
        De a2;
        return a2.k;
    }

    public /* synthetic */ Pc A() {
        return new Pc();
    }

    public /* synthetic */ String c() {
        De a2;
        return a2.b();
    }

    public /* synthetic */ String f() {
        De a2;
        return a2.A();
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ String C() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 1: 
            case 3: {
                return a2.k() + ".relative.add";
            }
            case 2: 
            case 4: {
                return a2.k() + ".relative.remove";
            }
            case 5: 
            case 6: {
                return a2.k() + ".absolute";
            }
        }
        return a2.k();
    }

    public /* synthetic */ X f(Mc a2) {
        De a3;
        switch (UF.D[a3.k.ordinal()]) {
            case 1: 
            case 2: 
            case 3: 
            case 4: 
            case 5: 
            case 6: 
            case 7: 
            case 8: 
            case 9: 
            case 10: {
                return new qc(a3.C(), Arrays.asList(UC.m, UC.i, UC.F, UC.G), a2);
            }
        }
        return new kb();
    }

    public /* synthetic */ String g() {
        return Td.A((Object)"\u0010z\u0013\u0003\bG5[-J-Z=z6O'E!\\");
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ Color a() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 8: {
                return zD.K;
            }
        }
        return Color.WHITE;
    }

    public /* synthetic */ GB A() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 1: {
                return new GB(yc.D, (fA)fA.i, Color.decode(Td.A((Object)"\r\"H}\u0018}\u0018")), Yc.G, SE.D, 12);
            }
            case 2: {
                return new GB(yc.D, (fA)fA.i, Color.decode(lb.A((Object)"1st,$,$")), Yc.G, SE.D, 12);
            }
            case 3: {
                return new GB(yc.D, (fA)fA.i, Color.decode(Td.A((Object)"\r'\u0016\"H'\u0016")), Yc.G, SE.D, 12);
            }
            case 4: {
                return new GB(yc.D, (fA)fA.i, Color.decode(lb.A((Object)"1v*stv*")), Yc.G, SE.D, 12);
            }
            case 5: {
                return new GB(yc.i, (fA)fA.F, Color.decode(Td.A((Object)"\r\"Hs\u0016s\u0016")), Yc.G, SE.D, 15);
            }
            case 6: {
                return new GB(yc.i, (fA)fA.F, Color.decode(lb.A((Object)"1,$st,$")), Yc.G, SE.D, 15);
            }
        }
        return new GB();
    }

    public /* synthetic */ String j() {
        De a2;
        return a2.B();
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ v A() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 1: 
            case 2: 
            case 3: 
            case 4: 
            case 5: 
            case 6: 
            case 7: 
            case 8: 
            case 9: {
                return a2.D;
            }
        }
        return null;
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ Color I() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 8: {
                return zD.F;
            }
        }
        return Color.WHITE;
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ String b() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 8: {
                return Td.A((Object)"\u0006G \u000e\u0000W*O)G']");
            }
        }
        return "";
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ String B() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 8: {
                return lb.A((Object)"Ta~2Qk{sx{va");
            }
        }
        return "";
    }

    public /* synthetic */ X A(Mc a2) {
        De a3;
        switch (UF.G[a3.G.ordinal()]) {
            case 1: 
            case 2: 
            case 3: 
            case 4: 
            case 5: 
            case 6: 
            case 7: 
            case 8: {
                return new qc(a3.G.toString(), Arrays.asList(UC.m, UC.i, UC.F, UC.G), a2);
            }
        }
        return new kb();
    }

    public /* synthetic */ X a(Mc a2) {
        return new qc(Td.A((Object)"7Z6O0K#W"), Arrays.asList(UC.m, UC.i, UC.F, UC.G), a2);
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ String I() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 8: {
                return lb.A((Object)"E`|qp2Qk{sx{va5Hp`z2Y{{w");
            }
        }
        return "";
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ String k() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 1: {
                return lb.A((Object)"Sfy5^|c`{q{ak5Sqvpv");
            }
            case 2: {
                return Td.A((Object)"o7Edb-_1G G0Wd|!C+X!J");
            }
            case 5: {
                return lb.A((Object)"Ta~2Y{dg|v|fl2Vzt|rw");
            }
            case 3: {
                return Td.A((Object)"l-Jdb-_1G G0Wdo J!J");
            }
            case 4: {
                return lb.A((Object)"P|v5^|c`{q{ak5@p\u007fzdpv");
            }
            case 6: {
                return Td.A((Object)"\u0006G \u000e\bG5[-J-Z=\u000e\u0007F%@#K");
            }
            case 7: {
                return lb.A((Object)"Scwgsrw5^|c`{q{ak");
            }
            case 8: {
                return Td.A((Object)"\u0014\\-M!\u000e\u0000W*O)G']");
            }
            case 9: {
                return lb.A((Object)"Bg{vw5Vl|t\u007f|qf2E~zf");
            }
        }
        return "";
    }

    public /* synthetic */ Color A() {
        return Color.WHITE;
    }

    public /* synthetic */ Ac A() {
        De a2;
        if (a2.k == rI.w) {
            return new Ac(zD.I, zD.e, zD.G, (Ld)Ld.I, jA.G, Kd.k, jc.I, 10, 20, 10);
        }
        return new Ac();
    }

    public /* synthetic */ cc A() {
        return new cc(Td.A((Object)"\u0017O)^(K"), lb.A((Object)"#-&;!"), Gf.d, null, Collections.emptyList(), null);
    }

    public /* synthetic */ Zc A() {
        De a2;
        switch (UF.D[a2.k.ordinal()]) {
            case 8: {
                return new Zc(2, IB.I);
            }
        }
        return new Zc();
    }

    public /* synthetic */ De(iD a2, rI a3, yF a4) {
        De a5;
        De de2 = a5;
        a5.D = a2;
        de2.k = a3;
        de2.G = a4;
    }

    /*
     * Enabled aggressive block sorting
     */
    public /* synthetic */ String A() {
        De a2;
        switch (UF.G[a2.G.ordinal()]) {
            case 1: {
                return Td.A((Object)"\u0010K(K#\\%Cd`+Z-H-M%Z-A*]");
            }
            case 2: {
                return lb.A((Object)"Gwysa{cw5^|c`{q{ak5Q}s{up2T~p`aa");
            }
            case 3: {
                return Td.A((Object)"\u0005L7A([0Kdb-_1G G0Wdm,O*I!\u000e\u0005B!\\0]");
            }
            case 4: {
                return lb.A((Object)"Tdp`tup2C}ygxw:^|c`{q{ak5V|ae~tk");
            }
            case 5: {
                return Td.A((Object)"m1\\6K*Zdo2K6O#Kdb-_1G G0W");
            }
            case 6: {
                return lb.A((Object)"Vgg`p|a2Tdp`tup2C}ygxw");
            }
            case 7: {
                return Td.A((Object)"\u0014\\-M!\u000e\u0000W*O)G']");
            }
            case 8: {
                return lb.A((Object)"Bg{vw5Vl|t\u007f|qf2E~zf");
            }
        }
        return "";
    }

    public /* synthetic */ String a() {
        De a2;
        return a2.I();
    }
}

