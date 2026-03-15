/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Eg
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.GD
 *  ttw.tradefinder.ID
 *  ttw.tradefinder.KD
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.SF
 *  ttw.tradefinder.Uf
 *  ttw.tradefinder.VF
 *  ttw.tradefinder.WE
 *  ttw.tradefinder.ba
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.hf
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.jg
 *  ttw.tradefinder.pF
 *  ttw.tradefinder.sD
 *  ttw.tradefinder.se
 *  ttw.tradefinder.yE
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.Container;
import java.awt.Dimension;
import java.awt.Insets;
import java.awt.event.ActionListener;
import java.awt.event.ItemListener;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.Serializable;
import javax.imageio.ImageIO;
import javax.swing.AbstractButton;
import javax.swing.ButtonGroup;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JComboBox;
import javax.swing.JComponent;
import javax.swing.JLabel;
import javax.swing.JRadioButton;
import ttw.tradefinder.Eg;
import ttw.tradefinder.Fa;
import ttw.tradefinder.GD;
import ttw.tradefinder.ID;
import ttw.tradefinder.KD;
import ttw.tradefinder.Nc;
import ttw.tradefinder.SF;
import ttw.tradefinder.Uf;
import ttw.tradefinder.VF;
import ttw.tradefinder.WE;
import ttw.tradefinder.ba;
import ttw.tradefinder.dD;
import ttw.tradefinder.hf;
import ttw.tradefinder.jF;
import ttw.tradefinder.jg;
import ttw.tradefinder.pF;
import ttw.tradefinder.sD;
import ttw.tradefinder.se;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class yE
extends Nc {
    public static final long D = 77147639832545L;
    private final JRadioButton m;
    private final JLabel F;
    private final ba e;
    private final JRadioButton i;
    private static final BufferedImage A = yE.A((String)WE.A((Object)"_\tW\u0003S\u0017\u0019\u0005Z\u0001D\u0010\u0018\u0014X\u0003"));
    private final JRadioButton k;
    private final JRadioButton I;
    private final JRadioButton G;
    private final JLabel D;

    private static /* synthetic */ BufferedImage A(String a2) {
        try {
            return ImageIO.read(Fa.class.getClassLoader().getResourceAsStream(a2));
        }
        catch (Exception exception) {
            return new BufferedImage(1, 1, 2);
        }
    }

    public yE(String a2, String a3, ba a4, boolean a5, boolean a6) {
        super((String)a2, a3 + " Settings");
        yE yE2;
        yE a7;
        a7.e = a4;
        if (!a6) {
            a7.a((Component)new JLabel(SF.A((Object)"\u0002.$/5a0-43%2q #$q&=.3 =-(a5(\" 3-4%"), new ImageIcon(A), 2));
        }
        yE yE3 = a7;
        yE3.I = new JRadioButton();
        yE3.k = new JRadioButton();
        yE3.m = new JRadioButton();
        yE3.G = new JRadioButton();
        yE3.i = new JRadioButton();
        yE3.I.setMargin(new Insets(0, 0, 0, 0));
        yE3.k.setMargin(new Insets(0, 0, 0, 0));
        yE3.m.setMargin(new Insets(0, 0, 0, 0));
        yE3.G.setMargin(new Insets(0, 0, 0, 0));
        yE3.i.setMargin(new Insets(0, 0, 0, 0));
        a2 = new ButtonGroup();
        ((ButtonGroup)a2).add(a7.I);
        Object object = a2;
        yE yE4 = a7;
        ((ButtonGroup)a2).add(a7.k);
        ((ButtonGroup)a2).add(yE4.m);
        ((ButtonGroup)object).add(yE4.G);
        ((ButtonGroup)object).add(a7.i);
        yE3.D = new JLabel("", 2);
        yE3.F = new JLabel("", 2);
        yE3.j();
        yE3.f(yE3.I, WE.A((Object)"f\u0016S\u0000S\u0002_\nS\u0000"));
        yE3.A(yE3.k, SF.A((Object)"\u00124\"5>,q\u0000\"*"));
        yE3.f(WE.A((Object)"u\u0011E\u0010Y\t\u0016&_\u0000"));
        yE3.I((Component)yE3.m, (Component)a7.D);
        yE yE5 = a7;
        yE5.I((Component)yE5.G, (Component)a7.F);
        yE yE6 = a7;
        yE6.I((Component)yE6.i, (Component)new JLabel(SF.A((Object)"\u0015(\" 3-4%"), 2));
        yE yE7 = a7;
        if (a5) {
            yE7.B();
            yE2 = a7;
        } else {
            yE7.e.A(false);
            yE2 = a7;
        }
        yE2.I();
        yE yE8 = a7;
        yE8.k();
        yE8.I.addItemListener((ItemListener)new hf(a7));
        yE8.k.addItemListener((ItemListener)new GD(a7));
        yE8.m.addItemListener((ItemListener)new jg(a7));
        yE8.G.addItemListener((ItemListener)new ID(a7));
        yE8.i.addItemListener((ItemListener)new KD(a7));
        a2 = yE8.getLayout().preferredLayoutSize((Container)a7);
        super.setPreferredSize(new Dimension(Math.max(((Dimension)a2).width, 500), ((Dimension)a2).height));
    }

    private /* synthetic */ void f(JRadioButton a2, String a3) {
        int n2;
        yE a4;
        if (a4.e.A() == sD.F) {
            a4.e.A(sD.i);
        }
        String string = jF.A((sD)a4.e.A());
        String[] stringArray = jF.A((sD)sD.F, (boolean)true).toArray(new String[0]);
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
        jComboBox2.addActionListener((ActionListener)new se(a4, jComboBox));
        a4.f((Component)a2, (Component)new JLabel(a3, 2), jComboBox);
    }

    private /* synthetic */ void k() {
        yE a2;
        dD dD2 = a2.e.A();
        if (dD2 == dD.k) {
            a2.I.setSelected(true);
            return;
        }
        if (dD2 == dD.G) {
            a2.k.setSelected(true);
            return;
        }
        if (dD2 == dD.D) {
            a2.m.setSelected(true);
            return;
        }
        if (dD2 == dD.i) {
            a2.G.setSelected(true);
            return;
        }
        a2.i.setSelected(true);
    }

    private /* synthetic */ void A(JRadioButton a2, String a3) {
        yE a4;
        a3 = new JLabel((String)a3, 2);
        Serializable serializable = new File(a4.e.I());
        if (serializable.exists()) {
            ((JLabel)a3).setText("Ask - " + serializable.getName());
            ((JComponent)a3).setToolTipText(serializable.getAbsolutePath());
        }
        serializable = new JButton(WE.A((Object)"e\u0001Z\u0001U\u0010"));
        ((AbstractButton)serializable).addActionListener((ActionListener)new pF(a4, (JButton)serializable, (JLabel)a3));
        a4.f((Component)a2, (Component)a3, (Component)serializable);
    }

    private /* synthetic */ void j() {
        yE a2;
        yE yE2 = a2;
        yE2.D.setText("Advanced - \"" + yE2.e.f() + "\"");
        yE yE3 = a2;
        yE3.F.setText("Basic - \"" + yE3.e.a() + "\"");
    }

    private /* synthetic */ void B() {
        yE a2;
        JCheckBox jCheckBox = new JCheckBox(WE.A((Object)"w\u0000RDF\u0016_\u0007SDB\u000b\u0016%R\u0012W\nU\u0001RKt\u0005E\rUDE\u000bC\nR\u0017"));
        jCheckBox.setSelected(a2.e.A());
        JCheckBox jCheckBox2 = jCheckBox;
        jCheckBox2.addItemListener((ItemListener)new Uf(a2, jCheckBox));
        a2.f((Component)jCheckBox2);
    }

    private /* synthetic */ void I() {
        yE a2;
        JButton jButton = new JButton(SF.A((Object)"\u0001-08q\u0012>4?%"));
        jButton.addActionListener((ActionListener)new VF(a2));
        a2.a((Component)jButton);
    }

    private /* synthetic */ void f(String a2) {
        yE a3;
        a2 = new JLabel((String)a2, 2);
        Serializable serializable = new File(a3.e.A());
        if (serializable.exists()) {
            ((JLabel)a2).setText("Bid - " + serializable.getName());
            ((JComponent)a2).setToolTipText(serializable.getAbsolutePath());
        }
        serializable = new JButton(SF.A((Object)"\u0002$=$25"));
        ((AbstractButton)serializable).addActionListener((ActionListener)new Eg(a3, (JButton)serializable, (JLabel)a2));
        a3.a((Component)a2, (Component)serializable);
    }
}

