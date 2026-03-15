/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.CF
 *  ttw.tradefinder.H
 *  ttw.tradefinder.JA
 *  ttw.tradefinder.MC
 *  ttw.tradefinder.Mf
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Q
 *  ttw.tradefinder.QA
 *  ttw.tradefinder.Sd
 *  ttw.tradefinder.UA
 *  ttw.tradefinder.Uc
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.Zc
 *  ttw.tradefinder.ch
 *  ttw.tradefinder.eA
 *  ttw.tradefinder.fC
 *  ttw.tradefinder.la
 *  ttw.tradefinder.p
 *  ttw.tradefinder.rH
 *  velox.api.layer1.messages.indicators.IndicatorColorScheme$ColorIntervalResponse
 *  velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator$GraphType
 *  velox.api.layer1.messages.indicators.WidgetDisplayInfo
 *  velox.api.layer1.messages.indicators.WidgetDisplayInfo$Type
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Component;
import java.awt.event.ActionListener;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JLabel;
import ttw.tradefinder.Bc;
import ttw.tradefinder.CF;
import ttw.tradefinder.H;
import ttw.tradefinder.JA;
import ttw.tradefinder.MC;
import ttw.tradefinder.Mf;
import ttw.tradefinder.Nc;
import ttw.tradefinder.P;
import ttw.tradefinder.Q;
import ttw.tradefinder.QA;
import ttw.tradefinder.Sd;
import ttw.tradefinder.Uc;
import ttw.tradefinder.W;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.Zc;
import ttw.tradefinder.ch;
import ttw.tradefinder.eA;
import ttw.tradefinder.fC;
import ttw.tradefinder.la;
import ttw.tradefinder.p;
import ttw.tradefinder.pB;
import ttw.tradefinder.rH;
import velox.api.layer1.messages.indicators.IndicatorColorScheme;
import velox.api.layer1.messages.indicators.Layer1ApiUserMessageModifyIndicator;
import velox.api.layer1.messages.indicators.WidgetDisplayInfo;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class UA
extends JA {
    private final la D;

    public Map<Double, String> A(la a2, String a3) {
        return Collections.singletonMap(0.0, a2.a());
    }

    public Collection<? extends Nc> A(rH a2, Nc a3, Mf a4, Nc a5) {
        UA a6;
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

    public WidgetDisplayInfo A() {
        return new WidgetDisplayInfo(WidgetDisplayInfo.Type.SYMMETRIC, 0.0);
    }

    public void A(Nc a2, String a3, String a4, String a5) {
        UA a6;
        YD yD2 = a6.A().A(a6.D.g(), a3, (String)a5, (Ya)a6.D.A());
        JLabel jLabel = new JLabel(a6.A(a3, (Zc)yD2.I));
        a5 = new pB(a6, a3, yD2, (String)a5, jLabel);
        a2.A((Component)new JLabel(a4), (Component)jLabel, a6.A(a3, a4, (W)a5));
    }

    public UA(H a2, la a3) {
        super(a2, a3);
        UA a4;
        a4.D = a3;
    }

    public Layer1ApiUserMessageModifyIndicator.GraphType A() {
        return Layer1ApiUserMessageModifyIndicator.GraphType.BOTTOM;
    }

    public String A(la a2, Double a3) {
        if (a3 >= 0.0) {
            return a2.j();
        }
        return a2.c();
    }

    public boolean A() {
        return true;
    }

    private /* synthetic */ ImageIcon A(String a2, Zc a3) {
        UA a4;
        UA uA = a4;
        UA uA2 = a4;
        return new ImageIcon(Bc.f((int)80, (int)20, (Color)uA.getColor(a2, uA.D.j()), (Color)uA2.getColor(a2, uA2.D.c()), (Zc)a3));
    }

    public Component A(String a2, String a3, W a4) {
        UA a5;
        JButton jButton = new JButton(CF.A((Object)"[~wn"));
        jButton.addActionListener((ActionListener)new MC(a5, jButton, a2, a3, a4));
        return jButton;
    }

    public List<fC> A(la a2) {
        UA a3;
        ArrayList<fC> arrayList = new ArrayList<fC>();
        arrayList.add((fC)new Uc(a3, a2));
        arrayList.add((fC)new Sd(a3, a2));
        arrayList.add((fC)new QA(a3, a2));
        return arrayList;
    }

    public p A(H a2, rH a3, ch a4) {
        UA a5;
        H h2 = a2;
        a2 = new eA(h2, a3, h2.A(a5.D.g(), a3.G, a5.A(), (Ya)a5.D.A()), a5.D.A(), (Q)a5, true);
        UA uA = a5;
        uA.a(a3.G, uA.D.A(), (P)a2);
        return a2;
    }

    public IndicatorColorScheme.ColorIntervalResponse A(la a2, double a3, double a4) {
        if (a4 < 0.0) {
            return new IndicatorColorScheme.ColorIntervalResponse(new String[]{a2.c()}, new double[0]);
        }
        if (a3 >= 0.0) {
            return new IndicatorColorScheme.ColorIntervalResponse(new String[]{a2.j()}, new double[0]);
        }
        return new IndicatorColorScheme.ColorIntervalResponse(new String[]{a2.c(), a2.j()}, new double[]{0.0});
    }

    public int A() {
        return 1000;
    }
}

