/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Td
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.di
 *  ttw.tradefinder.eH
 *  ttw.tradefinder.tF
 *  ttw.tradefinder.yf
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Component;
import java.awt.event.FocusEvent;
import java.awt.event.FocusListener;
import javax.swing.BorderFactory;
import javax.swing.JOptionPane;
import javax.swing.JTextField;
import ttw.tradefinder.Jd;
import ttw.tradefinder.Nc;
import ttw.tradefinder.Td;
import ttw.tradefinder.YD;
import ttw.tradefinder.di;
import ttw.tradefinder.tF;
import ttw.tradefinder.yf;

public class eH
implements FocusListener {
    public final /* synthetic */ di k;
    public final /* synthetic */ Nc I;
    public final /* synthetic */ YD G;
    public final /* synthetic */ JTextField D;

    @Override
    public void focusLost(FocusEvent a2) {
        eH a3;
        a2 = a3.D.getText();
        if (((String)(a2 = ((String)a2).trim())).startsWith(Td.A((Object)"I")) || ((String)a2).startsWith(tF.A((Object)"."))) {
            eH eH2 = a3;
            eH2.D.setText((String)((Jd)eH2.G.I).k);
            JOptionPane.showMessageDialog((Component)a3.I, Td.A((Object)"i6A1^dC!]7O#K7\u000e*A0\u000e4\\+X-J!J"));
            return;
        }
        if (!((String)a2).matches(tF.A((Object)"]\r3{:\u000bxc/g6+'"))) {
            a3.D.setBorder(BorderFactory.createLineBorder(Color.red, 2));
            return;
        }
        a3.D.setBorder(BorderFactory.createEmptyBorder());
        if (!((String)a2).equals(((Jd)a3.G.I).k)) {
            ((Jd)a3.G.I).k = (float)a2;
            eH eH3 = a3;
            a3.k.K.A(eH3.G);
            eH3.G.A(yf.k);
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ eH(di a2, JTextField a3, YD a4, Nc a5) {
        eH a6;
        eH eH2 = a6;
        eH2.k = a2;
        eH2.D = a3;
        a6.G = a4;
        a6.I = a5;
    }

    @Override
    public void focusGained(FocusEvent a2) {
    }
}

