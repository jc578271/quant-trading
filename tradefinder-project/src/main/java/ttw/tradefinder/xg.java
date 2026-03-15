/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ca
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.Gf
 *  ttw.tradefinder.H
 *  ttw.tradefinder.KF
 *  ttw.tradefinder.Mf
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.PF
 *  ttw.tradefinder.We
 *  ttw.tradefinder.Ya
 *  ttw.tradefinder.ch
 *  ttw.tradefinder.ga
 *  ttw.tradefinder.rE
 *  ttw.tradefinder.rH
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.Dimension;
import java.awt.image.BufferedImage;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeMap;
import javax.imageio.ImageIO;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JLabel;
import javax.swing.JScrollPane;
import ttw.tradefinder.Ca;
import ttw.tradefinder.FH;
import ttw.tradefinder.Fa;
import ttw.tradefinder.Gf;
import ttw.tradefinder.H;
import ttw.tradefinder.KF;
import ttw.tradefinder.Mf;
import ttw.tradefinder.Nc;
import ttw.tradefinder.PF;
import ttw.tradefinder.UI;
import ttw.tradefinder.We;
import ttw.tradefinder.Ya;
import ttw.tradefinder.ch;
import ttw.tradefinder.ga;
import ttw.tradefinder.hI;
import ttw.tradefinder.q;
import ttw.tradefinder.rE;
import ttw.tradefinder.rH;
import ttw.tradefinder.uF;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class xg {
    private static final BufferedImage k = xg.f();
    private final Set<q> I = new HashSet<q>();
    private static final BufferedImage G = xg.A();
    private final Map<String, JScrollPane> D = new LinkedHashMap<String, JScrollPane>();

    private static /* synthetic */ BufferedImage f() {
        try {
            return ImageIO.read(Fa.class.getClassLoader().getResourceAsStream(Ya.A((Object)"=/5%11{116 +:%'l$,3")));
        }
        catch (Exception exception) {
            return new BufferedImage(1, 1, 2);
        }
    }

    /*
     * Unable to fully structure code
     */
    public Collection<? extends Nc> A(rH a, H a) {
        var7_4 = a.I;
        synchronized (var7_4) {
            var4_5 = new ArrayList<String>();
            var5_6 = new ArrayList<Nc>();
            var6_7 = a.I.iterator();
            block5: while (true) {
                v0 = var6_7;
                while (v0.hasNext()) {
                    var3_8 = var6_7.next();
                    var8_9 = var3_8.A();
                    if (var8_9.size() > 0) {
                        v1 = var3_8 = var8_9.iterator();
                        while (true) {
                            if (!v1.hasNext()) continue block5;
                            var8_9 = (We)var3_8.next();
                            v1 = var3_8;
                            var5_6.addAll(a.A(a, a, (We)var8_9, var4_5));
                        }
                    }
                    if (!var4_5.contains(var3_8.A())) ** break;
                    continue block5;
                    v0 = var6_7;
                    var4_5.add(var3_8.A());
                    var5_6.addAll(var3_8.A(a, null, Mf.U, null));
                }
                break;
            }
            return var5_6;
        }
    }

    public void f(long a2) {
        xg a3;
        Set<q> set = a3.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a3.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().A(a2);
                iterator2 = iterator;
            }
            return;
        }
    }

    public void f(String a2) {
        xg a3;
        Set<q> set = a3.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a3.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().f(a2);
                iterator2 = iterator;
            }
            return;
        }
    }

    public JButton A(String a2, String a3, Nc a4) {
        xg a5;
        String string = a2 + a4.A();
        Nc nc2 = a4;
        nc2.a();
        nc2.f();
        if (a5.D.containsKey(string)) {
            a5.D.get(string).setViewportView((Component)a4);
        } else {
            a4 = new JScrollPane((Component)a4, 20, 31);
            a4.setPreferredSize(new Dimension(550, 900));
            Nc nc3 = a4;
            nc3.setMaximumSize(new Dimension(550, 1200));
            nc3.setMinimumSize(new Dimension(550, 900));
            nc3.getVerticalScrollBar().setUnitIncrement(15);
            a5.D.put(string, (JScrollPane)a4);
        }
        a4 = new JButton(Ya.A((Object)"\u00070+ "), new ImageIcon(G));
        a4.addActionListener(new UI(a5, (JButton)a4, string, a3, a2));
        return a4;
    }

    private /* synthetic */ Collection<? extends Nc> A(rH a2, H a3, We a4, List<String> a5) {
        xg a6;
        We we;
        Nc nc2;
        ArrayList<Object> arrayList = new ArrayList<Object>();
        Nc nc3 = nc2 = a4.A().iterator();
        while (nc3.hasNext()) {
            we = (We)nc2.next();
            nc3 = nc2;
            arrayList.addAll(a6.A(a2, a3, we, a5));
        }
        if (a5.contains(a4.f())) {
            return arrayList;
        }
        a5.add(a4.f());
        if (!a4.A(a2.D, a3.A())) {
            return arrayList;
        }
        nc2 = new Nc(a2.G, a4, a3);
        xg xg2 = a6;
        xg2.A(a2, a3, a4, nc2, a5);
        arrayList.addAll(xg2.A(a2, a3, a4, nc2, (Nc)null));
        if (!nc2.j()) {
            if (nc2.A() == rE.G) {
                ArrayList<Object> arrayList2 = arrayList;
                arrayList2.add(nc2);
                return arrayList2;
            }
            if (nc2.A() == rE.k) {
                we = new Nc(a2.G, a4, a3);
                new Nc(a2.G, a4, a3).w = null;
                ArrayList<Object> arrayList3 = arrayList;
                we.I(new JLabel(nc2.w), (Component)a6.A(a2.G, nc2.w, nc2));
                arrayList3.add(we);
                return arrayList3;
            }
            we = new Nc(a2.G, a4, a3);
            new Nc(a2.G, a4, a3).w = "";
            we.I(new JLabel(nc2.w), (Component)a6.A(a2.G, nc2.w, nc2));
            arrayList.add(we);
        }
        return arrayList;
    }

    private /* synthetic */ Collection<? extends Nc> A(rH a2, H a3, We a4, Nc a5, Nc a6) {
        ArrayList arrayList = new ArrayList();
        a4 = a4.a().iterator();
        block0: while (true) {
            Object object = a4;
            while (object.hasNext()) {
                PF pF2 = (PF)a4.next();
                if (pF2.G == Mf.O || pF2.G == Mf.n && !a3.A()) continue block0;
                arrayList.addAll(pF2.D.A(a2, a5, pF2.G, a6));
                object = a4;
            }
            break;
        }
        return arrayList;
    }

    public void A(long a2) {
        xg a3;
        Set<q> set = a3.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a3.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().f(a2);
                iterator2 = iterator;
            }
            return;
        }
    }

    public void a() {
        xg a2;
        Set<q> set = a2.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a2.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().A();
                iterator2 = iterator;
            }
            return;
        }
    }

    public xg() {
        xg a2;
    }

    public void f() {
        xg a2;
        Set<q> set = a2.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a2.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().f();
                iterator2 = iterator;
            }
            a2.I.clear();
            return;
        }
    }

    public void A(String a2) {
        xg a3;
        Set<q> set = a3.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a3.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().A(a2);
                iterator2 = iterator;
            }
            return;
        }
    }

    public Set<String> A() {
        xg a2;
        HashSet<String> hashSet = new HashSet<String>();
        Set<q> set = a2.I;
        synchronized (set) {
            for (q q2 : a2.I) {
                Iterator iterator = q2.f().iterator();
                while (iterator.hasNext()) {
                    Iterator object;
                    String string = (String)object.next();
                    iterator = object;
                    hashSet.add(string);
                }
            }
            return hashSet;
        }
    }

    public void A() {
        xg a2;
        Set<q> set = a2.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a2.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().a();
                iterator2 = iterator;
            }
            return;
        }
    }

    public void A(List<q> a2, H a3) {
        xg a4;
        a3 = a4.I;
        synchronized (a3) {
            Object object = a2 = a2.iterator();
            while (object.hasNext()) {
                q q2 = (q)a2.next();
                object = a2;
                a4.I.add(q2);
            }
            return;
        }
    }

    public q A(String a2) {
        xg a3;
        Set<q> set = a3.I;
        synchronized (set) {
            for (q q2 : a3.I) {
                if (!q2.A(a2)) continue;
                return q2;
            }
            return null;
        }
    }

    private static /* synthetic */ BufferedImage A() {
        try {
            return ImageIO.read(Fa.class.getClassLoader().getResourceAsStream(hI.A("hP`ZdN.Zd\\s\u0013qSf")));
        }
        catch (Exception exception) {
            return new BufferedImage(1, 1, 2);
        }
    }

    private /* synthetic */ void A(rH a2, H a3, We a4, Nc a5, List<String> a6) {
        ArrayList<? extends Nc> arrayList = new ArrayList<Nc>();
        TreeMap<Integer, Nc> treeMap = new TreeMap<Integer, Nc>();
        a4 = a4.f().iterator();
        block0: while (true) {
            Object object = a4;
            while (object.hasNext()) {
                Nc nc2;
                xg a7;
                We we = (We)a4.next();
                if (a6.contains(we.f())) continue block0;
                We we2 = we;
                a6.add(we2.f());
                if (!we2.A(a2.D, a3.A())) continue block0;
                Nc nc3 = new Nc(a2.G, we, a3);
                xg xg2 = a7;
                xg2.A(a2, a3, we, nc3, a6);
                arrayList.addAll(xg2.A(a2, a3, we, nc3, a5));
                if (nc3.j()) continue block0;
                TreeMap<Integer, Nc> treeMap2 = treeMap;
                int n2 = nc3.A();
                while (treeMap2.containsKey(n2)) {
                    treeMap2 = treeMap;
                    ++n2;
                }
                if (nc3.A() == rE.G) {
                    object = a4;
                    treeMap.put(n2, nc3);
                    continue;
                }
                Nc nc4 = new Nc(a2.G, we, a3);
                JLabel jLabel = new JLabel(nc3.w);
                JButton jButton = a7.A(a2.G, nc3.w, nc3);
                if (nc3.A() != Gf.d) {
                    JLabel jLabel2 = jLabel;
                    jLabel2.setIcon(new ImageIcon(uF.A((Gf)nc3.A(), (int)28, (int)28)));
                    jLabel2.setHorizontalAlignment(2);
                }
                if (nc3.f()) {
                    Ca ca2 = nc3.A();
                    ca2 = ca2.A(a2);
                    JCheckBox jCheckBox = new JCheckBox();
                    jCheckBox.setSelected(ca2.A());
                    Nc nc5 = nc4;
                    nc2 = nc5;
                    jButton.setEnabled(ca2.A());
                    jCheckBox.addItemListener(new FH(a7, jCheckBox, (ga)ca2, jButton));
                    nc5.A(jCheckBox, jLabel, (Component)jButton);
                } else {
                    Nc nc6 = nc4;
                    nc2 = nc6;
                    nc6.I(jLabel, (Component)jButton);
                }
                nc2.w = nc3.A() == rE.k ? null : "";
                treeMap.put(n2, nc4);
                continue block0;
            }
            break;
        }
        for (Nc nc7 : treeMap.values()) {
            int n3 = a5.a((Component)new KF(nc7));
            if (!nc7.B() || a5.A() != rE.G) continue;
            a5.A(n3);
        }
        for (Nc nc8 : arrayList) {
            int n4 = a5.a((Component)new KF(nc8));
            if (!nc8.B() || a5.A() != rE.G) continue;
            a5.A(n4);
        }
    }

    public void A(rH a2, ch a3) {
        xg a4;
        Set<q> set = a4.I;
        synchronized (set) {
            Iterator<q> iterator;
            Iterator<q> iterator2 = iterator = a4.I.iterator();
            while (iterator2.hasNext()) {
                iterator.next().A(a2, a3);
                iterator2 = iterator;
            }
            return;
        }
    }
}

