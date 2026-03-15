/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ce
 *  ttw.tradefinder.Fd
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Vb
 *  ttw.tradefinder.kB
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.tC
 *  ttw.tradefinder.uc
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.event.ActionListener;
import java.awt.event.ItemListener;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import ttw.tradefinder.Ce;
import ttw.tradefinder.Fd;
import ttw.tradefinder.Nc;
import ttw.tradefinder.Vb;
import ttw.tradefinder.cc;
import ttw.tradefinder.kB;
import ttw.tradefinder.rH;
import ttw.tradefinder.uc;

public class tC
extends Nc {
    private final kB G;
    public static final long D = 74148236912335L;
    private final JCheckBox D;

    private /* synthetic */ void B() {
        tC a2;
        tC tC2 = a2;
        tC tC3 = a2;
        tC3.f((Component)tC3.D);
        tC2.A(false);
        tC3.D.addItemListener((ItemListener)new Fd(a2));
        JButton jButton = new JButton(Ce.A((Object)"\u001e"));
        jButton.addActionListener((ActionListener)new Vb(a2));
        JButton jButton2 = new JButton(cc.A((Object)"|"));
        jButton2.addActionListener((ActionListener)new uc(a2));
        tC2.A((Component)tC2.D, (Component)jButton, (Component)jButton2);
    }

    public tC(rH a2, kB a3) {
        tC a4;
        rH rH2 = a2;
        super(rH2.G, rH2.G);
        a4.D = new JCheckBox(cc.A((Object)"W,s ~'"));
        a4.G = a3;
        a4.B();
    }

    public void I() {
        tC a2;
        a2.D.setSelected(false);
    }
}

