/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.BA
 *  ttw.tradefinder.D
 *  ttw.tradefinder.Eb
 *  ttw.tradefinder.H
 *  ttw.tradefinder.La
 *  ttw.tradefinder.MF
 *  ttw.tradefinder.Mc
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.VE
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.la
 *  ttw.tradefinder.qI
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.t
 *  ttw.tradefinder.ze
 *  velox.api.layer1.Layer1ApiInstrumentSpecificEnabledStateProvider
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.Cursor;
import java.awt.event.MouseListener;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import ttw.tradefinder.BA;
import ttw.tradefinder.D;
import ttw.tradefinder.Eb;
import ttw.tradefinder.H;
import ttw.tradefinder.MF;
import ttw.tradefinder.Mc;
import ttw.tradefinder.Nc;
import ttw.tradefinder.UC;
import ttw.tradefinder.VE;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.es;
import ttw.tradefinder.la;
import ttw.tradefinder.mI;
import ttw.tradefinder.qI;
import ttw.tradefinder.rH;
import ttw.tradefinder.t;
import ttw.tradefinder.ze;
import velox.api.layer1.Layer1ApiInstrumentSpecificEnabledStateProvider;

public abstract class La
implements H,
D,
Layer1ApiInstrumentSpecificEnabledStateProvider {
    private boolean i;
    private YD<VE, es> k;
    private t I;
    private boolean G;
    private Map<String, Runnable> D;

    public void setStrategyEnabledRecheckCallback(String a2, Runnable a3) {
        La a4;
        a4.D.put(a2, a3);
    }

    public /* synthetic */ void A(la a2, Mc a3, boolean a4) {
        La a5;
        La la2 = a5;
        la la3 = a2;
        a5.I = la3.a(a3).A((D)a5);
        la2.i = la3.A();
        la2.G = a4;
    }

    /*
     * Unable to fully structure code
     */
    public void onStrategyCheckboxEnabled(String a, boolean a) {
        block6: {
            block8: {
                block9: {
                    block7: {
                        if (a.k == null) {
                            return;
                        }
                        if (!a) break block6;
                        var4_3 = a.A(a);
                        var3_4 = new Nc(a, "");
                        if (var4_3 != null) break block7;
                        var3_4.f((Component)new JLabel("Please wait until the instrument \"" + a + "\" is successfully loaded."));
                        v0 = var3_4;
                        break block8;
                    }
                    if (a.i || a.G) break block9;
                    v1 = var3_4;
                    v0 = v1;
                    v1.f((Component)new JLabel(MF.A((Object)"cx^c\u0017DcG\u0017`E\u007fSeTd\u0017yD0X~[i\u0017qAq^|Vr[u\u0017tBb^~P0{YaU\u0017cRcDyX~\u0019")));
                    break block8;
                }
                if (a.I.A(var4_3.D)) ** GOTO lbl53
                var4_3 = a.I.A(var4_3.D);
                if (var4_3.size() > 0) {
                    var3_4.f((Component)new JLabel(ze.A((Object)"p\rE\u0000S\u0004\u0000\u0011U\u0013C\tA\u0012EAO\u000fEAO\u0007\u0000\u0015H\u0004\u0000\u0007O\rL\u000eW\bN\u0006\u0000\u0012U\u0003S\u0002R\bP\u0015I\u000eN\u0012\u001a")));
                    var4_3 = var4_3.iterator();
                    v2 = var4_3;
                    while (v2.hasNext()) {
                        var5_5 = (UC)var4_3.next();
                        var6_6 = new JLabel("<html><a href=\"\" style=\"color: white\">" + Eb.A((UC)var5_5, (BA)BA.k) + "</a></html>");
                        var6_6.setCursor(Cursor.getPredefinedCursor(12));
                        v2 = var4_3;
                        v3 = var6_6;
                        v3.addMouseListener((MouseListener)new qI(a, var5_5));
                        var3_4.f((Component)v3);
                    }
                    v4 = var3_4;
                    v0 = v4;
                    v4.f((Component)new JLabel(MF.A((Object)"YQ0CxR0cD`0GbXtBsC0_qD0UuR~\u0017q[bRqSi\u0017qTd^fVdRt\u001b")));
                    var3_4.f((Component)new JLabel(ze.A((Object)"\u0011L\u0004A\u0012EAW\u0000I\u0015\u0000\u0000\u0000\u0007E\u0016\u0000\u0012E\u0002O\u000fD\u0012\u0000\u0015OAV\u0004R\bF\u0018\u0000\u0018O\u0014RAS\u0014B\u0012C\u0013I\u0011T\bO\u000f\u000e")));
                } else {
                    var3_4.f((Component)new JLabel(a.A() + " is not supported for instruments without MBO data."));
                    var3_4.f((Component)new JLabel(MF.A((Object)"q\u007fE0Z\u007fEu\u0017yYvXbZqCyX~\u001b0G|RqDu\u0017f^c^d\r")));
                    var4_3 = new JLabel(ze.A((Object)"]H\u0015M\r\u001e]AAH\u0013E\u0007\u001dC\u0002AS\u0015Y\rE\\\u0002\u0002O\rO\u0013\u001aAW\tI\u0015EC\u001e,A\u0013K\u0004TAB\u0018\u0000.R\u0005E\u0013\u0000Im#oH\u0000L\u0000\"m$\u0000&R\u000eU\u0011\u001cNA_\u001cNH\u0015M\r\u001e"));
                    var4_3.setCursor(Cursor.getPredefinedCursor(12));
                    v5 = var4_3;
                    v5.addMouseListener(new mI(a));
                    var3_4.f((Component)v5);
lbl53:
                    // 2 sources

                    v0 = var3_4;
                }
            }
            if (!v0.j()) {
                JOptionPane.showMessageDialog(new JFrame(), var3_4, MF.A((Object)"QTd^fVd^\u007fY0Y\u007fC0V|[\u007f@uS"), 1);
                return;
            }
        }
        if (a != ((VE)a.k.I).A(a)) {
            ((VE)a.k.I).A(a, Boolean.valueOf(a));
            v6 = a;
            v6.A(v6.A(), a.k);
            v6.A(a, a);
        }
    }

    public List<String> A() {
        La a2;
        if (a2.k == null) {
            return Collections.emptyList();
        }
        if (!a2.i && !a2.G) {
            return Collections.emptyList();
        }
        return ((VE)a2.k.I).A();
    }

    public /* synthetic */ void a() {
        La a2;
        La la2 = a2;
        la2.I.A();
        la2.D.clear();
    }

    public La() {
        La a2;
        La la2 = a2;
        La la3 = a2;
        a2.D = Collections.synchronizedMap(new HashMap());
        la3.k = null;
        la3.I = null;
        la2.i = false;
        la2.G = false;
    }

    public boolean isStrategyEnabled(String a2) {
        La a3;
        if (a3.k == null) {
            return false;
        }
        if (!a3.i && !a3.G) {
            return false;
        }
        return ((VE)a3.k.I).A(a2);
    }

    public boolean a() {
        La a2;
        if (a2.k == null) {
            return false;
        }
        if (!a2.i && !a2.G) {
            return false;
        }
        return ((VE)a2.k.I).A();
    }

    public /* synthetic */ void f() {
        La a2;
        La la2 = a2;
        a2.k = a2.A(la2.A(), (Ya)new VE());
        if (la2.I.isFEnabled()) {
            for (Object object : ((VE)a2.k.I).A()) {
                rH rH2 = a2.A((String)object);
                if (rH2 != null && a2.I.A(rH2.D)) continue;
                a2.onStrategyCheckboxEnabled((String)object, false);
            }
            Iterator<Object> iterator = a2.D.values().iterator();
            Iterator<Object> iterator2 = iterator;
            while (iterator2.hasNext()) {
                Object object;
                object = (Runnable)iterator.next();
                object.run();
                iterator2 = iterator;
            }
        }
    }

    public void A(t a2, boolean a32, boolean a42) {
        La a5;
        if (a5.k == null) {
            return;
        }
        if (a42) {
            for (String a32 : ((VE)a5.k.I).A()) {
                rH a42 = a5.A(a32);
                if (a42 != null && a5.I.A(a42.D)) continue;
                a5.onStrategyCheckboxEnabled(a32, false);
            }
            a2 = a5.D.values().iterator();
            Iterator<Object> iterator = a2;
            while (iterator.hasNext()) {
                Runnable a32 = (Runnable)a2.next();
                a32.run();
                iterator = a2;
            }
        }
    }

    public abstract boolean A(String var1, boolean var2);
}

