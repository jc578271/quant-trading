/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Be
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.MB
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.UE
 *  ttw.tradefinder.Vf
 *  ttw.tradefinder.Zf
 *  ttw.tradefinder.ba
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.eE
 *  ttw.tradefinder.gD
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.oF
 *  ttw.tradefinder.qD
 *  ttw.tradefinder.r
 *  ttw.tradefinder.sD
 *  ttw.tradefinder.zF
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.event.ActionListener;
import java.awt.image.BufferedImage;
import javax.imageio.ImageIO;
import javax.swing.AbstractButton;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JLabel;
import ttw.tradefinder.Be;
import ttw.tradefinder.Fa;
import ttw.tradefinder.MB;
import ttw.tradefinder.Nc;
import ttw.tradefinder.UE;
import ttw.tradefinder.Vf;
import ttw.tradefinder.Zf;
import ttw.tradefinder.ba;
import ttw.tradefinder.dD;
import ttw.tradefinder.eE;
import ttw.tradefinder.gD;
import ttw.tradefinder.jF;
import ttw.tradefinder.qD;
import ttw.tradefinder.r;
import ttw.tradefinder.sD;
import ttw.tradefinder.zF;

public class oF {
    private static BufferedImage G = oF.A((String)MB.A((Object)"cXkRoF%EfTsjmGoPd\u001bz[m"));
    private static BufferedImage D = oF.A((String)Vf.A((Object)"\u0012'\u001a-\u001e9T:\u0017+\u0002\u0015\u0019&\u000e/U:\u0015-"));

    private static /* synthetic */ BufferedImage A(String a2) {
        try {
            return ImageIO.read(Fa.class.getClassLoader().getResourceAsStream(a2));
        }
        catch (Exception exception) {
            return new BufferedImage(1, 1, 2);
        }
    }

    public static void A(String a2, Nc a3, gD a4, r a5) {
        a4 = new zF(a4, a5);
        JButton jButton = new JButton(Vf.A((Object)">.\u0012>"));
        jButton.addActionListener((ActionListener)new eE(jButton, (String)a2, (ba)a4, a5));
        a2 = new JButton(new ImageIcon(G));
        ((AbstractButton)a2).setRolloverIcon(new ImageIcon(D));
        Object object = a2;
        ((AbstractButton)object).setContentAreaFilled(false);
        ((AbstractButton)object).addActionListener((ActionListener)new qD(a5));
        if (a5.f()) {
            Nc nc2 = a3;
            nc2.A(nc2.a((Component)new JLabel(MB.A((Object)"KYoG~\u0015YZ\u007f[n")), (Component)a2, (Component)jButton));
            return;
        }
        Nc nc3 = a3;
        nc3.A(nc3.f((Component)new JLabel(Vf.A((Object)"\u000b\u0017/\t>[\u0019\u0014?\u0015.")), (Component)a2, (Component)jButton, UE.A((String)Be.G, (boolean)true)));
    }

    public oF() {
        oF a2;
    }

    public static void A(Nc a2, gD a3, r a4) {
        int n2;
        if (a3.I != dD.k) {
            gD gD2 = a3;
            gD2.I = dD.k;
            gD2.D = sD.F;
            a4.f();
        }
        String string = jF.A((sD)a3.D);
        String[] stringArray = jF.A((sD)sD.F).toArray(new String[0]);
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
        jComboBox2.addActionListener((ActionListener)new Zf(jComboBox, a3, a4));
        if (a4.f()) {
            Nc nc2 = a2;
            nc2.A(nc2.a(new JLabel(Vf.A((Object)"\u000b\u0017/\t>[\u0019\u0014?\u0015.")), jComboBox));
            return;
        }
        Nc nc3 = a2;
        nc3.A(nc3.a(new JLabel(MB.A((Object)"KYoG~\u0015YZ\u007f[n")), jComboBox, UE.A((String)Be.G, (boolean)true)));
    }
}

