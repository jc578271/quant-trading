/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AC
 *  ttw.tradefinder.FA
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Id
 *  ttw.tradefinder.Jc
 *  ttw.tradefinder.Lc
 *  ttw.tradefinder.Mf
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Nh
 *  ttw.tradefinder.OC
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.Qe
 *  ttw.tradefinder.SC
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.UE
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.bC
 *  ttw.tradefinder.ch
 *  ttw.tradefinder.dC
 *  ttw.tradefinder.gD
 *  ttw.tradefinder.iC
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.kB
 *  ttw.tradefinder.la
 *  ttw.tradefinder.nb
 *  ttw.tradefinder.oF
 *  ttw.tradefinder.p
 *  ttw.tradefinder.qA
 *  ttw.tradefinder.qc
 *  ttw.tradefinder.r
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.t
 *  ttw.tradefinder.wB
 *  ttw.tradefinder.zB
 *  velox.gui.colors.ColorsConfigItem
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Component;
import java.awt.Dialog;
import java.awt.Window;
import java.awt.event.ActionListener;
import java.awt.event.ItemListener;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import java.util.function.Consumer;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JComboBox;
import javax.swing.JDialog;
import javax.swing.JLabel;
import javax.swing.JSlider;
import javax.swing.event.ChangeListener;
import ttw.tradefinder.FA;
import ttw.tradefinder.H;
import ttw.tradefinder.Id;
import ttw.tradefinder.Jc;
import ttw.tradefinder.Lc;
import ttw.tradefinder.Mf;
import ttw.tradefinder.Nc;
import ttw.tradefinder.Nh;
import ttw.tradefinder.OC;
import ttw.tradefinder.P;
import ttw.tradefinder.Q;
import ttw.tradefinder.Qe;
import ttw.tradefinder.SC;
import ttw.tradefinder.SE;
import ttw.tradefinder.UC;
import ttw.tradefinder.UE;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.bC;
import ttw.tradefinder.bg;
import ttw.tradefinder.ch;
import ttw.tradefinder.dC;
import ttw.tradefinder.gD;
import ttw.tradefinder.ha;
import ttw.tradefinder.iC;
import ttw.tradefinder.in;
import ttw.tradefinder.jF;
import ttw.tradefinder.kB;
import ttw.tradefinder.la;
import ttw.tradefinder.nb;
import ttw.tradefinder.oF;
import ttw.tradefinder.p;
import ttw.tradefinder.qA;
import ttw.tradefinder.qc;
import ttw.tradefinder.r;
import ttw.tradefinder.rH;
import ttw.tradefinder.rI;
import ttw.tradefinder.t;
import ttw.tradefinder.tb;
import ttw.tradefinder.wB;
import ttw.tradefinder.zB;
import velox.gui.colors.ColorsConfigItem;

public class AC
extends nb {
    private final t i;
    private Object k;
    private Map<String, JDialog> I;
    private int G;
    private final t D;

    public void c(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        a3 = new ColorsConfigItem((Color)((tb)a4.I).i, (Color)((tb)a4.I).i, (Consumer)new Id(a5, a4, a3));
        Nc nc2 = a2;
        nc2.A(nc2.I(new JLabel(Nh.A((Object)"nf[w_4Y{V{H4x}^")), (Component)a3));
    }

    public void b(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        a3 = new ColorsConfigItem((Color)((tb)a4.I).D, (Color)((tb)a4.I).D, (Consumer)new dC(a5, a4, a3));
        Nc nc2 = a2;
        nc2.A(nc2.I(new JLabel(ha.A((Object)"1\u0015\u0004\u0004\u0000G\u0006\b\t\b\u0017G$\u0014\u000e")), (Component)a3));
    }

    private /* synthetic */ void g(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        if (!a5.A().A()) {
            return;
        }
        SC sC2 = new SC(a5, a3, a4);
        if (!a5.D.A(a3.D)) {
            oF.A((Nc)a2, (gD)((tb)a4.I).m, (r)sC2);
            return;
        }
        oF.A((String)a3.G, (Nc)a2, (gD)((tb)a4.I).m, (r)sC2);
    }

    public void A(String a2) {
        AC a3;
        AC aC2 = a3;
        super.A((String)a2);
        Object object = aC2.k;
        synchronized (object) {
            if (a3.I.containsKey(a2)) {
                a2 = (JDialog)a3.I.remove(a2);
                ((Dialog)a2).setVisible(false);
                ((Window)a2).dispose();
            }
            return;
        }
    }

    private /* synthetic */ void C(Nc a2, rH a3, YD<tb, in> a4) {
        int n2;
        AC a5;
        if (!a5.A().A().A(a3.D)) {
            return;
        }
        String string = jF.A((SE)((tb)a4.I).F);
        String[] stringArray = jF.A((SE)SE.D).toArray(new String[0]);
        JComboBox<String> jComboBox = new JComboBox<String>(stringArray);
        int n3 = stringArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            if (stringArray[n2].equals(string)) {
                jComboBox.setSelectedItem(string);
            }
            n4 = ++n2;
        }
        JComboBox<String> jComboBox2 = jComboBox;
        jComboBox2.setEditable(false);
        jComboBox2.addActionListener((ActionListener)new FA(a5, jComboBox, a4, a3));
        Nc nc2 = a2;
        nc2.A(nc2.a(new JLabel(ha.A((Object)"4\u0000\t\u0001G1\u0002\t\u0002\u0002\u0015\u0004\nE)\n\u0013\f\u0001\f\u0004\u0004\u0013\f\b\u000b")), jComboBox));
    }

    public p A(H a2, rH a3, ch a4) {
        AC a5;
        a2 = new kB(a5.A().g(), a2, a3, (Q)a5);
        AC aC2 = a5;
        rH rH2 = a3;
        a5.a(rH2.G, a5.A().A(), (P)a2);
        aC2.a(rH2.G, rI.Ia, (P)a2);
        aC2.a(a3.G, (rI)((Object)rI.K), (P)a2);
        return a2;
    }

    public void f() {
        AC a2;
        AC aC2 = a2;
        super.f();
        aC2.i.A();
        aC2.D.A();
        Object object = aC2.k;
        synchronized (object) {
            Iterator iterator;
            Iterator iterator2 = iterator = a2.I.values().iterator();
            while (iterator2.hasNext()) {
                ((JDialog)iterator.next()).dispose();
                iterator2 = iterator;
            }
            a2.I.clear();
            return;
        }
    }

    private /* synthetic */ void k(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        JCheckBox jCheckBox = new JCheckBox(ha.A((Object)"\"\u000b\u0006\u0007\u000b\u0000"));
        jCheckBox.setSelected(((tb)a4.I).a);
        Nc nc2 = a2;
        nc2.f((Component)jCheckBox);
        nc2.A(((tb)a4.I).a);
        JCheckBox jCheckBox2 = jCheckBox;
        jCheckBox2.addItemListener((ItemListener)new Jc(a5, jCheckBox, a4, a2, a3));
        a2.f((Component)jCheckBox2);
    }

    private /* synthetic */ void j(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        int n2;
        String string = jF.A((SE)((tb)a4.I).I);
        String[] stringArray = jF.A((SE)SE.D).toArray(new String[0]);
        JComboBox<String> jComboBox = new JComboBox<String>(stringArray);
        int n3 = stringArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            if (stringArray[n2].equals(string)) {
                jComboBox.setSelectedItem(string);
            }
            n4 = ++n2;
        }
        JComboBox<String> jComboBox2 = jComboBox;
        jComboBox2.setEditable(false);
        jComboBox2.addActionListener((ActionListener)new iC(a5, jComboBox, a4, a3));
        Nc nc2 = a2;
        nc2.A(nc2.a(new JLabel(Nh.A((Object)"x{U\u007fWuJ4{x_fN4wqIg[s_")), jComboBox));
    }

    public Collection<? extends Nc> A(rH a2, Nc a3, Mf a4, Nc a5) {
        AC a6;
        a5 = a6.A().A(a6.A().g(), a2.G, (Ya)new tb());
        ArrayList<Nc> arrayList = new ArrayList<Nc>();
        if (a3 == null) {
            a3 = new Nc(a2.G, a6.A() + " Settings");
            arrayList.add(a3);
        }
        if (a4 == Mf.U || a4 == Mf.u) {
            a6.k(a3, a2, (YD)a5);
        }
        if (a4 == Mf.U || a4 == Mf.Ca) {
            a6.f(a3, a2, (YD)a5);
        }
        if (a4 == Mf.U || a4 == Mf.V) {
            Nc nc2 = a3;
            rH rH2 = a2;
            AC aC2 = a6;
            Nc nc3 = a3;
            a6.B(nc3, a2, (YD)a5);
            aC2.I(nc3, a2, (YD)a5);
            aC2.b(a3, a2, (YD)a5);
            a6.c(a3, rH2, (YD)a5);
            a6.A(nc2, rH2, (YD)a5);
            a6.a(nc2, a2, (YD)a5);
        }
        if ((a4 == Mf.U || a4 == Mf.n) && a6.A().A()) {
            AC aC3 = a6;
            Nc nc4 = a3;
            a6.g(nc4, a2, (YD)a5);
            aC3.C(nc4, a2, (YD)a5);
            aC3.j(a3, a2, (YD)a5);
        }
        return arrayList;
    }

    private /* synthetic */ void B(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        JLabel jLabel = new JLabel(Integer.toString((int)((tb)a4.I).e) + " %");
        int n2 = Math.max(5, Math.min(100, (int)((tb)a4.I).e));
        JSlider jSlider = new JSlider(0, 5, 100, n2);
        jSlider.addChangeListener((ChangeListener)new wB(a5, a4, jLabel, a3));
        a2.a((Component)new JLabel(Nh.A((Object)"@H}]s_f\u001agSn_")), (Component)jLabel, (Component)jSlider, UE.A((String)Qe.m));
    }

    private /* synthetic */ void I(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        int n2 = Math.max(8, Math.min(18, ((tb)a4.I).G));
        JSlider jSlider = new JSlider(0, 8, 18, n2);
        jSlider.addChangeListener((ChangeListener)new qA(a5, a4, a3));
        Nc nc2 = a2;
        nc2.A(nc2.a(new JLabel(Nh.A((Object)"nqB`\u001agSn_")), (Component)jSlider));
    }

    private /* synthetic */ void a(Nc a2, rH a3, YD<tb, in> a4) {
        int n2;
        AC a5;
        if (!a5.i.A(a3.D)) {
            return;
        }
        String string = jF.A((SE)((tb)a4.I).K);
        String[] stringArray = jF.A((SE)SE.D).toArray(new String[0]);
        JComboBox<String> jComboBox = new JComboBox<String>(stringArray);
        int n3 = stringArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            if (stringArray[n2].equals(string)) {
                jComboBox.setSelectedItem(string);
            }
            n4 = ++n2;
        }
        JComboBox<String> jComboBox2 = jComboBox;
        jComboBox2.setEditable(false);
        jComboBox2.addActionListener((ActionListener)new bC(a5, jComboBox, a4, a3, a2));
        a2.a(new JLabel(ha.A((Object)"6\u000f\n\u0010E\u0006\t\u000bE3\u0017\u0006\u0006\u0002\u0016")), jComboBox);
    }

    private /* synthetic */ void f(Nc a2, rH a3, YD<tb, in> a4) {
        AC aC2;
        boolean bl;
        AC a5;
        if (!a5.i.A(a3.D)) {
            return;
        }
        JButton jButton = new JButton(Nh.A((Object)"{wN}LuNq\u001a\\Sp^qT4uf^qH4tuL}]uN{H"));
        p p2 = a5.A(a3.G);
        if (p2 == null || !(p2 instanceof kB)) {
            return;
        }
        p2 = (kB)p2;
        Nc nc2 = a2;
        jButton.addActionListener((ActionListener)new Lc(a5, a4, a3, (kB)p2));
        a5.G = nc2.B((Component)jButton, UE.A((String[])Qe.k));
        if (((tb)a4.I).K != SE.I) {
            bl = true;
            aC2 = a5;
        } else {
            bl = false;
            aC2 = a5;
        }
        nc2.f(bl, aC2.G);
    }

    private /* synthetic */ void A(Nc a2, rH a3, YD<tb, in> a4) {
        AC a5;
        int n2;
        String string = jF.A((OC)((tb)a4.I).k);
        String[] stringArray = jF.A((OC)OC.G).toArray(new String[0]);
        JComboBox<String> jComboBox = new JComboBox<String>(stringArray);
        int n3 = stringArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            if (stringArray[n2].equals(string)) {
                jComboBox.setSelectedItem(string);
            }
            n4 = ++n2;
        }
        JComboBox<String> jComboBox2 = jComboBox;
        jComboBox2.setEditable(false);
        jComboBox2.addActionListener((ActionListener)new zB(a5, jComboBox, a4, a3));
        Nc nc2 = a2;
        nc2.A(nc2.a(new JLabel(ha.A((Object)"#\u0000\u0014\f\u0000\u000b")), jComboBox, UE.A((String[])Qe.F)));
    }

    public AC(H a2, la a3) {
        AC a4;
        AC aC2 = a4;
        super(a2, a3);
        aC2.I = new HashMap();
        aC2.k = new Object();
        aC2.G = -1;
        a3 = new qc(ha.A((Object)"/\f\u0003\u0001\u0002\u000b(\u0017\u0003\u0000\u0015!\u0002\u00134\u0000\u0013\u0011\u000e\u000b\u0000\u0016"), Collections.singletonList(UC.i), Arrays.asList(bg.e), a2.A());
        AC aC3 = a4;
        aC3.i = a3.A(null);
        aC3.D = new qc(Nh.A((Object)"AV`HujuHuWqNqHBSgSvSxS`C"), Arrays.asList(UC.i, UC.F, UC.G), a2.A()).A(null);
    }
}

