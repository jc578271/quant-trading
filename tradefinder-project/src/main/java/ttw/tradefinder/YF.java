/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Fg
 *  ttw.tradefinder.H
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.SE
 *  ttw.tradefinder.Te
 *  ttw.tradefinder.Va
 *  ttw.tradefinder.YD
 *  ttw.tradefinder.YF
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.hD
 *  ttw.tradefinder.jF
 *  ttw.tradefinder.rE
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.t
 *  ttw.tradefinder.yF
 */
package ttw.tradefinder;

import java.awt.event.ActionListener;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import javax.swing.JComboBox;
import javax.swing.JLabel;
import ttw.tradefinder.Fg;
import ttw.tradefinder.H;
import ttw.tradefinder.Nc;
import ttw.tradefinder.SE;
import ttw.tradefinder.Te;
import ttw.tradefinder.Va;
import ttw.tradefinder.YD;
import ttw.tradefinder.Ya;
import ttw.tradefinder.hD;
import ttw.tradefinder.j;
import ttw.tradefinder.jF;
import ttw.tradefinder.rE;
import ttw.tradefinder.rH;
import ttw.tradefinder.t;
import ttw.tradefinder.yF;

public class YF {
    private final H k;
    private final rH I;
    private final t G;
    private final Te D;

    public Collection<? extends Nc> A(int a22) {
        YF a3;
        if (!a3.k.A()) {
            return Collections.emptyList();
        }
        if (!a3.G.A()) {
            return Collections.emptyList();
        }
        YF yF2 = a3;
        YD yD2 = yF2.k.A(yF2.k.A(), (Ya)new Va());
        Nc a22 = new Nc(a3.I.G, a3.D.A(), a22, false, rE.G, a3.k);
        yF2.A(a22, a3.I, yD2);
        return Arrays.asList(a22);
    }

    private /* synthetic */ void A(Nc a2, rH a3, YD<Va, j> a4) {
        YF a5;
        int n2;
        a3 = jF.A((SE)((Va)a4.I).D);
        String[] stringArray = jF.A((SE)SE.D).toArray(new String[0]);
        JComboBox<String> jComboBox = new JComboBox<String>(stringArray);
        int n3 = stringArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            if (stringArray[n2].equals(a3)) {
                jComboBox.setSelectedItem(a3);
            }
            n4 = ++n2;
        }
        JComboBox<String> jComboBox2 = jComboBox;
        jComboBox2.setEditable(false);
        jComboBox2.addActionListener((ActionListener)new hD(a5, jComboBox, a4));
        a2.a(new JLabel(Fg.A((Object)"/\u0007\t\u0006\u0018H=\u0004\u0019\u001a\b\u001b")), jComboBox);
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 5 << 3 ^ 3;
        int cfr_ignored_0 = 5 << 3 ^ 2;
        int n5 = n3;
        int n6 = (2 ^ 5) << 4 ^ (2 << 2 ^ 3);
        while (n5 >= 0) {
            int n7 = n3--;
            a2[n7] = (char)(((String)object2).charAt(n7) ^ n6);
            if (n3 < 0) break;
            int n8 = n3--;
            a2[n8] = (char)(((String)object2).charAt(n8) ^ n4);
            n5 = n3;
        }
        return new String((char[])a2);
    }

    public YF(H a2, rH a3) {
        YF a4;
        YF yF2 = a4;
        yF2.k = a2;
        a4.I = a3;
        yF2.D = new Te(yF.f);
        a4.G = a4.D.A(a4.k.A()).A(null);
    }
}

