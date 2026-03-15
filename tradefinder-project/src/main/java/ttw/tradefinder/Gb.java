/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Gb
 *  ttw.tradefinder.H
 *  ttw.tradefinder.JA
 *  ttw.tradefinder.Jb
 *  ttw.tradefinder.Mf
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.Zc
 *  ttw.tradefinder.ch
 *  ttw.tradefinder.eA
 *  ttw.tradefinder.fC
 *  ttw.tradefinder.jC
 *  ttw.tradefinder.la
 *  ttw.tradefinder.p
 *  ttw.tradefinder.rA
 *  ttw.tradefinder.rH
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme$ColorIntervalResponse
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Component;
import java.awt.event.ActionListener;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JLabel;
import ttw.tradefinder.Bc;
import ttw.tradefinder.H;
import ttw.tradefinder.JA;
import ttw.tradefinder.Jb;
import ttw.tradefinder.KB;
import ttw.tradefinder.Mf;
import ttw.tradefinder.Nc;
import ttw.tradefinder.P;
import ttw.tradefinder.Q;
import ttw.tradefinder.W;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.Zc;
import ttw.tradefinder.ch;
import ttw.tradefinder.eA;
import ttw.tradefinder.fC;
import ttw.tradefinder.jC;
import ttw.tradefinder.la;
import ttw.tradefinder.p;
import ttw.tradefinder.rA;
import ttw.tradefinder.rH;
import velox.api.layer1.messages.indicators.IndicatorColorScheme;

public abstract class Gb
extends JA {
    private final la D;
    private boolean D;

    public abstract boolean isFEnabled();

    public Gb(H a2, la a3, boolean a4) {
        Gb a5;
        Gb gb = a5;
        super(a2, a3);
        a5.D = false;
        gb.D = a3;
        gb.D = a4;
    }

    public String A(la a2, Double a3) {
        Gb a4;
        return a4.A();
    }

    public p A(H a2, rH a3, ch a4) {
        Gb a5;
        H h2 = a2;
        Gb gb = a5;
        a2 = new eA(h2, a3, h2.A(a5.D.g(), a3.G, a5.A(), (Ya)a5.D.A()), a5.D.A(), (Q)gb, gb.f());
        Gb gb2 = a5;
        gb2.a(a3.G, gb2.D.A(), (P)a2);
        return a2;
    }

    private /* synthetic */ ImageIcon A(String a2, Zc a3) {
        Gb a4;
        Gb gb = a4;
        return new ImageIcon(Bc.A((int)80, (int)20, (Color)gb.getColor(a2, gb.D.C()), (Zc)a3));
    }

    public List<fC> A(la a2) {
        Gb a3;
        ArrayList<fC> arrayList = new ArrayList<fC>();
        arrayList.add((fC)new jC(a3, a2));
        return arrayList;
    }

    public IndicatorColorScheme.ColorIntervalResponse A(la a2, double a3, double a4) {
        Gb a5;
        return new IndicatorColorScheme.ColorIntervalResponse(new String[]{a5.A()}, new double[0]);
    }

    public void A(Nc a2, String a32, String a4, String a5) {
        Gb a6;
        YD yD = a6.A().A(a6.D.g(), a32, a5, (Ya)a6.D.A());
        JLabel jLabel = new JLabel(a6.A(a32, (Zc)yD.I));
        a5 = new rA(a6, a32, yD, a5, jLabel);
        int a32 = a2.A((Component)new JLabel(a4), (Component)jLabel, a6.A(a32, a4, (W)((Object)a5)));
        if (a6.D) {
            a2.A(a32);
        }
    }

    public Component A(String a2, String a3, W a4) {
        Gb a5;
        JButton jButton = new JButton(KB.A((Object)"66\u001a&"));
        jButton.addActionListener((ActionListener)new Jb(a5, jButton, a2, a3, a4));
        return jButton;
    }

    public Collection<? extends Nc> A(rH a2, Nc a3, Mf a4, Nc a5) {
        Gb a6;
        a5 = new ArrayList();
        if (a3 == null) {
            a3 = new Nc(a2.G, a6.A() + " Settings");
            a5.add(a3);
        }
        if (a4 == Mf.U || a4 == Mf.k) {
            a6.A(a3, a2.G, a6.D.k(), a6.A());
        }
        return a5;
    }
}

