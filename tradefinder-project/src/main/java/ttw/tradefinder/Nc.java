/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.AF
 *  ttw.tradefinder.Ca
 *  ttw.tradefinder.DA
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.Gf
 *  ttw.tradefinder.H
 *  ttw.tradefinder.KF
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Nh
 *  ttw.tradefinder.We
 *  ttw.tradefinder.bF
 *  ttw.tradefinder.rE
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.Dimension;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.LayoutManager;
import java.awt.event.ActionListener;
import java.awt.event.MouseListener;
import java.awt.image.BufferedImage;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeMap;
import java.util.function.Consumer;
import javax.imageio.ImageIO;
import javax.swing.BorderFactory;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JRadioButton;
import javax.swing.SwingUtilities;
import ttw.tradefinder.AF;
import ttw.tradefinder.Ca;
import ttw.tradefinder.DA;
import ttw.tradefinder.Fa;
import ttw.tradefinder.Gf;
import ttw.tradefinder.H;
import ttw.tradefinder.KF;
import ttw.tradefinder.Nh;
import ttw.tradefinder.We;
import ttw.tradefinder.bF;
import ttw.tradefinder.mF;
import ttw.tradefinder.rE;

/*
 * Duplicate member names - consider using --renamedupmembers true
 * Exception performing whole class analysis ignored.
 */
public class Nc
extends JPanel {
    public String w;
    private final rE B;
    public static final BufferedImage A;
    private static final BufferedImage H;
    private int h;
    private static final int C = 10;
    private List<Component> c;
    private TreeMap<Integer, Boolean> L;
    private final String E;
    private Consumer<String> b;
    private static final int l = 5;
    public static final BufferedImage j;
    private boolean J;
    private static final int M = -1;
    private List<Integer> d;
    private boolean g;
    private final Gf f;
    private final H a;
    private final Ca K;
    private final int m;
    private static final int F = 10;
    private boolean e;
    private final String i;
    private int k;
    private final boolean I;
    private TreeMap<Integer, Set<Component>> G;
    public static final long D = 7484950625475698376L;

    public int I(Component a2, Component a3) {
        Nc a4;
        return a4.A(a2, a3, null);
    }

    public int a(Component a2, Component a3) {
        Nc a4;
        return a4.f(a2, a3, null);
    }

    public boolean A(Nc a2) {
        int n2;
        int n3;
        Nc a3;
        block3: {
            if ((a2 = a3.A(a2)) == null) {
                return false;
            }
            n3 = -1;
            for (Map.Entry entry : a3.G.entrySet()) {
                if (!((Set)entry.getValue()).remove(a2)) continue;
                if (((Set)entry.getValue()).size() != 0) break;
                n2 = n3 = ((Integer)entry.getKey()).intValue();
                break block3;
            }
            n2 = n3;
        }
        if (n2 != -1) {
            a3.G.remove(n3);
        }
        SwingUtilities.invokeLater(() -> a3.A((Component)a2));
        return true;
    }

    public int A(JLabel a22, Component a3, JButton a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints.anchor = 13;
        gridBagConstraints3.fill = 2;
        gridBagConstraints3.gridwidth = 3;
        gridBagConstraints.gridx = 3;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    public boolean j() {
        Nc a2;
        return a2.h == 0;
    }

    private /* synthetic */ void A(JPanel a2, Boolean a3) {
        int n2;
        a2.setEnabled(a3);
        Component[] componentArray = a2.getComponents();
        a2 = componentArray;
        a2 = componentArray;
        int n3 = componentArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            Nc a4;
            Component component = a2[n2];
            if (!a4.c.contains(component)) {
                if (component instanceof JPanel) {
                    a4.A((JPanel)component, a3);
                }
                component.setEnabled(a3);
            }
            n4 = ++n2;
        }
    }

    public int A(Component a2, int a3) {
        Nc a4;
        return a4.A(a2, null, a3);
    }

    private /* synthetic */ Component A(Nc a2) {
        int n2;
        Nc a3;
        Component[] componentArray = a3.getComponents();
        int n3 = componentArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            Component component = componentArray[n2];
            if (component == a2) {
                return component;
            }
            if (component instanceof KF && ((KF)component).A(a2)) {
                return component;
            }
            n4 = ++n2;
        }
        return null;
    }

    public void A(Consumer<String> a2) {
        a.b = a2;
    }

    public int A(JRadioButton a2, Component a3) {
        Nc a4;
        return a4.A(a2, a3, null);
    }

    public int b(Component a2) {
        Nc a3;
        return a3.k(a2, null);
    }

    public int A(JCheckBox a2, JLabel a32, Component a4, mF a5) {
        Nc a6;
        if (a2 == null) {
            return a6.A(a32, a4, a5);
        }
        if (!a6.c.contains(a32)) {
            a32.setEnabled(a6.g);
        }
        if (!a6.c.contains(a4)) {
            a4.setEnabled(a6.g);
        }
        if (!a6.c.contains(a2)) {
            a2.setEnabled(a6.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a6.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a6.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a32 = a6.A((Component)a32, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.anchor = 13;
        gridBagConstraints3.gridwidth = 3;
        gridBagConstraints.gridx = 3;
        a6.A(a4, gridBagConstraints, a32);
        GridBagConstraints gridBagConstraints4 = gridBagConstraints;
        gridBagConstraints.anchor = 13;
        gridBagConstraints4.gridwidth = 1;
        gridBagConstraints4.gridx = 0;
        gridBagConstraints4.insets.right = 10;
        a6.A((Component)a2, gridBagConstraints, a32);
        if (a5 != null && !a5.f()) {
            a6.A(gridBagConstraints, a5, a32);
        }
        return a32;
    }

    public Nc(String a2, String a3, int a4, boolean a5, rE a6, H a7) {
        Nc a8;
        String string;
        String string2;
        if (a3 == null) {
            string2 = "";
            string = a3;
        } else {
            string2 = a3.replaceAll(Nh.A((Object)"fg\u0011"), "");
            string = a3;
        }
        a8(a2, string2, string, a4, a5, false, a6, Gf.d, null, a7);
    }

    public int g(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 5;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 13;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public int f(JCheckBox a2, Component a3) {
        Nc a4;
        return a4.f(a2, a3, new mF());
    }

    public int g(Component a2) {
        Nc a3;
        return a3.g(a2, null);
    }

    public int C(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 5;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 10;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    private /* synthetic */ boolean A(int a2) {
        int n2;
        Nc a3;
        LayoutManager layoutManager = a3.getLayout();
        if (!(layoutManager instanceof GridBagLayout)) {
            return false;
        }
        layoutManager = (GridBagLayout)layoutManager;
        Component[] componentArray = a3.getComponents();
        int n3 = componentArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            Component component = componentArray[n2];
            if (((GridBagLayout)layoutManager).getConstraints((Component)component).gridy == a2) {
                return true;
            }
            n4 = ++n2;
        }
        return false;
    }

    public int a(JLabel a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints.anchor = 13;
        gridBagConstraints3.fill = 2;
        gridBagConstraints3.gridwidth = 3;
        gridBagConstraints.gridx = 3;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    public void f(Component a2) {
        Nc a3;
        a3.c.add(a2);
        a2.setEnabled(true);
    }

    public int k(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.gridwidth = 7;
        gridBagConstraints3.gridx = 0;
        gridBagConstraints3.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 10;
        gridBagConstraints2.fill = 2;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public int a(Component a2, Component a3, Component a4) {
        Nc a5;
        return a5.f(a2, a3, a4, null);
    }

    public int f(JCheckBox a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints.anchor = 13;
        gridBagConstraints3.fill = 2;
        gridBagConstraints3.gridwidth = 3;
        gridBagConstraints.gridx = 3;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    public int a(Component a22, Component a3, Component a4, mF a5) {
        Nc a6;
        if (!a6.c.contains(a22)) {
            a22.setEnabled(a6.g);
        }
        if (!a6.c.contains(a3)) {
            a3.setEnabled(a6.g);
        }
        if (!a6.c.contains(a4)) {
            a4.setEnabled(a6.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a6.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.fill = 2;
        if (a6.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a6.A(a22, gridBagConstraints, -1);
        gridBagConstraints.gridx = 3;
        gridBagConstraints.insets = new Insets(gridBagConstraints.insets.top, 10, 10, 0);
        a6.A(a3, gridBagConstraints, a22);
        gridBagConstraints.gridx = 5;
        gridBagConstraints.insets = new Insets(gridBagConstraints.insets.top, 10, 10, 0);
        a6.A(a4, gridBagConstraints, a22);
        if (a5 != null && !a5.f()) {
            a6.A(gridBagConstraints, a5, a22);
        }
        return a22;
    }

    public int A(Component a22, mF a3, int a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.gridwidth = 5;
        gridBagConstraints3.gridx = 1;
        gridBagConstraints3.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 10;
        gridBagConstraints2.fill = 2;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a5.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public int A(JCheckBox a2, Component a3) {
        Nc a4;
        return a4.A(a2, a3, null);
    }

    public Nc(String a2, String a3, int a4, boolean a5, boolean a6, rE a7, Gf a8, Ca a9, H a10) {
        Nc a11;
        String string;
        String string2;
        if (a3 == null) {
            string2 = "";
            string = a3;
        } else {
            string2 = a3.replaceAll(DA.A((Object)"\u0001bv"), "");
            string = a3;
        }
        a11(a2, string2, string, a4, a5, a6, a7, a8, a9, a10);
    }

    public int f(Component a2, Component a3, Component a4) {
        Nc a5;
        return a5.A(a2, a3, a4, null);
    }

    public int C(Component a2) {
        Nc a3;
        return a3.C(a2, null);
    }

    public Nc(String a2, String a3, String a4, int a5, boolean a6, boolean a7, rE a8, Gf a9, Ca a10, H a11) {
        Nc a12;
        Nc nc2 = a12;
        Nc nc3 = a12;
        Nc nc4 = a12;
        Nc nc5 = a12;
        Nc nc6 = a12;
        Nc nc7 = a12;
        Nc nc8 = a12;
        Nc nc9 = a12;
        Nc nc10 = a12;
        nc10.h = 0;
        nc9.b = null;
        nc10.G = new TreeMap();
        nc9.L = new TreeMap();
        nc9.d = new ArrayList();
        nc9.c = new ArrayList();
        nc8.g = true;
        nc8.e = false;
        nc7.J = false;
        nc7.k = 1;
        nc6.w = a4;
        nc6.m = a5;
        nc5.a = a11;
        nc5.i = a2;
        nc4.E = a3;
        nc4.I = a6;
        nc3.B = a8;
        nc3.f = a9;
        nc2.K = a10;
        nc2.e = a7 && a12.I;
        a2 = new GridBagLayout();
        new GridBagLayout().columnWidths = new int[]{10, 0, 10, 0, 10, 0, 10};
        ((GridBagLayout)a2).columnWeights = new double[]{0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0};
        Nc nc11 = a12;
        nc11.setLayout((LayoutManager)a2);
        nc11.setDoubleBuffered(true);
        if (nc11.a != null) {
            Nc nc12 = a12;
            a12.e = a12.a.A(nc12.i, nc12.E, a7);
        }
    }

    public int a(Component a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 10;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A(a22, gridBagConstraints, -1);
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints.gridx = 5;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    public int k(Component a2) {
        Nc a3;
        return a3.f(a2, null);
    }

    public int j(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.gridwidth = 5;
        gridBagConstraints3.gridx = 1;
        gridBagConstraints3.insets = new Insets(0, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        gridBagConstraints2.fill = 2;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public int I(JLabel a2, Component a3) {
        Nc a4;
        return a4.A(a2, a3, null);
    }

    public void a() {
        Component component;
        int n2;
        Nc a2;
        LayoutManager layoutManager = a2.getLayout();
        if (!(layoutManager instanceof GridBagLayout)) {
            return;
        }
        layoutManager = (GridBagLayout)layoutManager;
        int n3 = -1;
        Component[] componentArray = a2.getComponents();
        int n4 = componentArray.length;
        int n5 = n2 = 0;
        while (n5 < n4) {
            component = componentArray[n2];
            n3 = Math.max(n3, ((GridBagLayout)layoutManager).getConstraints((Component)component).gridy);
            n5 = ++n2;
        }
        if (n3 == -1) {
            return;
        }
        componentArray = a2.getComponents();
        n4 = componentArray.length;
        int n6 = n2 = 0;
        while (n6 < n4) {
            component = componentArray[n2];
            GridBagConstraints gridBagConstraints = ((GridBagLayout)layoutManager).getConstraints(component);
            if (gridBagConstraints.gridy == n3) {
                GridBagConstraints gridBagConstraints2 = gridBagConstraints;
                gridBagConstraints2.weighty = 1.0;
                gridBagConstraints2.anchor = 11;
                ((GridBagLayout)layoutManager).setConstraints(component, gridBagConstraints);
            }
            n6 = ++n2;
        }
    }

    public int A(JRadioButton a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.anchor = 13;
        gridBagConstraints3.gridwidth = 3;
        gridBagConstraints.gridx = 3;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    private /* synthetic */ void f(int a2) {
        Nc a3;
        if (!a3.G.containsKey(a2)) {
            return;
        }
        boolean bl = true;
        if (a3.d.contains(a2) && a3.e) {
            bl = false;
        }
        if (a3.L.containsKey(a2) && !((Boolean)a3.L.get(a2)).booleanValue()) {
            bl = false;
        }
        Iterator iterator = a2 = ((Set)a3.G.get(a2)).iterator();
        while (iterator.hasNext()) {
            ((Component)a2.next()).setVisible(bl);
            iterator = a2;
        }
    }

    public int a(JLabel a2, Component a3) {
        Nc a4;
        return a4.a(a2, a3, new mF());
    }

    public int B(Component a2, mF a3) {
        Nc a4;
        Nc nc2 = a4;
        return nc2.A(a2, a3, nc2.h++);
    }

    public int f(JLabel a2, Component a3) {
        Nc a4;
        return a4.f(a2, a3, new mF());
    }

    public int f(Component a22, Component a3, Component a4, mF a5) {
        Nc a6;
        if (!a6.c.contains(a22)) {
            a22.setEnabled(a6.g);
        }
        if (!a6.c.contains(a3)) {
            a3.setEnabled(a6.g);
        }
        if (!a6.c.contains(a4)) {
            a4.setEnabled(a6.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a6.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.fill = 2;
        if (a6.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a6.A(a22, gridBagConstraints, -1);
        gridBagConstraints.gridx = 5;
        gridBagConstraints.insets = new Insets(gridBagConstraints.insets.top, 10, 10, 0);
        a6.A(a4, gridBagConstraints, a22);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints.gridx = 3;
        gridBagConstraints3.fill = 0;
        gridBagConstraints3.anchor = 13;
        gridBagConstraints3.insets = new Insets(gridBagConstraints.insets.top, 10, 10, 0);
        a6.A(a3, gridBagConstraints, a22);
        if (a5 != null && !a5.f()) {
            a6.A(gridBagConstraints, a5, a22);
        }
        return a22;
    }

    static {
        j = Nc.A((String)Nh.A((Object)"Sy[s_g\u0015}TrU:Jz]"));
        A = Nc.A((String)DA.A((Object)"x0p:t.><}8c)?-\u007f:"));
        H = Nc.A((String)Nh.A((Object)"Sy[s_g\u0015`NcexUsU:Jz]"));
    }

    public int f(Component a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        GridBagConstraints gridBagConstraints4 = gridBagConstraints;
        GridBagConstraints gridBagConstraints5 = gridBagConstraints;
        gridBagConstraints.gridwidth = 2;
        gridBagConstraints5.gridx = 2;
        gridBagConstraints5.weightx = 2.0;
        gridBagConstraints4.fill = 2;
        gridBagConstraints4.anchor = 17;
        gridBagConstraints3.insets = new Insets(5, 0, 0, 0);
        int a22 = a5.A(a22, gridBagConstraints, -1);
        gridBagConstraints3.gridx = 5;
        gridBagConstraints2.gridwidth = 1;
        gridBagConstraints2.fill = 0;
        gridBagConstraints2.anchor = 13;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    public Nc(String a2, We a3, H a4) {
        a5(a2, a3.A(), a3.A(), a3.f(), a3.A(), a3.A(), a3.A(), a3.A(), a4);
        Nc a5;
    }

    public boolean B() {
        Nc a2;
        return a2.I;
    }

    public void f() {
        a.J = true;
    }

    public int I(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.gridwidth = 5;
        gridBagConstraints3.gridx = 1;
        gridBagConstraints3.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 13;
        gridBagConstraints2.fill = 2;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public int f(JLabel a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        GridBagConstraints gridBagConstraints4 = gridBagConstraints;
        gridBagConstraints4.anchor = 13;
        gridBagConstraints4.fill = 2;
        gridBagConstraints3.gridwidth = 3;
        gridBagConstraints3.gridx = 3;
        gridBagConstraints.insets.right = 15;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    public int f(Component a22, Component a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        if (!a4.c.contains(a3)) {
            a3.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 5;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.fill = 2;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.fill = 0;
        gridBagConstraints3.gridwidth = 1;
        gridBagConstraints.gridx = 6;
        a4.A(a3, gridBagConstraints, a22);
        return a22;
    }

    public int A(JCheckBox a2, JLabel a3, Component a4) {
        Nc a5;
        return a5.A(a2, a3, a4, null);
    }

    private static /* synthetic */ BufferedImage A(String a2) {
        try {
            return ImageIO.read(Fa.class.getClassLoader().getResourceAsStream(a2));
        }
        catch (Exception exception) {
            return new BufferedImage(1, 1, 2);
        }
    }

    public String A() {
        Nc a2;
        return a2.E;
    }

    public int j(Component a22) {
        Nc a3;
        if (!a3.c.contains(a22)) {
            a22.setEnabled(a3.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a3.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 2;
        gridBagConstraints2.gridx = 5;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 10);
        gridBagConstraints2.fill = 2;
        if (a3.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a3.A(a22, gridBagConstraints, -1);
        return a22;
    }

    public int A(Component a2, Component a3, Component a4) {
        Nc a5;
        return a5.a(a2, a3, a4, null);
    }

    @Override
    public Dimension getPreferredSize() {
        Nc a2;
        if (a2.J) {
            return new Dimension(a2.getParent().getSize().width, super.getPreferredSize().height);
        }
        return super.getPreferredSize();
    }

    public Nc(String a2, String a3) {
        a4(a2, a3, -1, true, false, rE.G, Gf.d, null, null);
        Nc a4;
    }

    public int a(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 5;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public Ca A() {
        Nc a2;
        return a2.K;
    }

    public void A() {
        Nc a2;
        Nc nc2 = a2;
        super.removeAll();
        nc2.G.clear();
        nc2.c.clear();
        nc2.d.clear();
    }

    public int B(Component a2) {
        Nc a3;
        return a3.j(a2, null);
    }

    public rE A() {
        Nc a2;
        return a2.B;
    }

    public static Nc A(String a2, H a3) {
        JLabel jLabel = new JLabel(new ImageIcon(H));
        jLabel.addMouseListener((MouseListener)new AF());
        a2 = new Nc(a2, null, -1000, false, false, rE.k, Gf.d, null, a3);
        a2.g((Component)jLabel);
        return a2;
    }

    public Gf A() {
        Nc a2;
        return a2.f;
    }

    public int A(Component a2, Component a3) {
        Nc a4;
        return a4.a(a2, a3, new mF());
    }

    public int A(JLabel a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.anchor = 13;
        gridBagConstraints3.gridwidth = 3;
        gridBagConstraints.gridx = 3;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    private /* synthetic */ void A(GridBagConstraints a2, mF a3, int a4) {
        Nc a5;
        a3 = Nc.A((mF)((Object)a3));
        a5.A(a2, (JButton)a3, a4);
    }

    public int I(Component a2) {
        Nc a3;
        return a3.A(a2, null);
    }

    public int f(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 5;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(0, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public void A(int a2) {
        Nc a3;
        if (!a3.d.contains(a2)) {
            a3.d.add(a2);
            a3.f(a2);
        }
    }

    public int A(Component a22, mF a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.gridwidth = 5;
        gridBagConstraints3.gridx = 1;
        gridBagConstraints3.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 17;
        gridBagConstraints2.fill = 2;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A(a22, gridBagConstraints, -1);
        if (a3 != null && !a3.f()) {
            a4.A(gridBagConstraints, a3, a22);
        }
        return a22;
    }

    public int a(Component a2) {
        Nc a3;
        Nc nc2 = a3;
        return nc2.A(a2, nc2.h++);
    }

    public int A(JLabel a22, Component a3) {
        Nc a4;
        if (!a4.c.contains(a22)) {
            a22.setEnabled(a4.g);
        }
        if (!a4.c.contains(a3)) {
            a3.setEnabled(a4.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a4.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints2.gridx = 1;
        gridBagConstraints2.insets = new Insets(10, 0, 0, 10);
        gridBagConstraints2.anchor = 17;
        if (a4.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a4.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints.anchor = 13;
        gridBagConstraints3.fill = 2;
        gridBagConstraints3.gridwidth = 2;
        gridBagConstraints.gridx = 5;
        a4.A(a3, gridBagConstraints, a22);
        return a22;
    }

    public static JButton A(mF a2) {
        if (a2 == null || a2.f()) {
            return null;
        }
        JButton jButton = new JButton(new ImageIcon(a2.D != false ? A : j));
        jButton.setBorder(BorderFactory.createEmptyBorder());
        JButton jButton2 = jButton;
        JButton jButton3 = jButton;
        jButton3.setToolTipText((String)a2.G);
        jButton3.setOpaque(false);
        jButton2.setContentAreaFilled(false);
        jButton2.setBorderPainted(false);
        if (!a2.A()) {
            jButton.addActionListener((ActionListener)new bF(a2));
        }
        return jButton;
    }

    public boolean I() {
        Nc a2;
        return a2.d.size() > 0;
    }

    public int f(Component a2) {
        Nc a3;
        return a3.a(a2, null);
    }

    public void f(boolean a2, int a3) {
        Nc a4;
        a4.L.put(a3, a2);
        a4.f(a3);
    }

    public int A(JCheckBox a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.gridwidth = 1;
        gridBagConstraints3.gridx = 1;
        gridBagConstraints3.insets = new Insets(10, 0, 0, 0);
        gridBagConstraints2.anchor = 13;
        gridBagConstraints2.fill = 2;
        if (a5.h == 1) {
            gridBagConstraints.insets.top = 5;
        }
        int a22 = a5.A((Component)a22, gridBagConstraints, -1);
        GridBagConstraints gridBagConstraints4 = gridBagConstraints;
        gridBagConstraints4.anchor = 17;
        gridBagConstraints4.gridwidth = 3;
        gridBagConstraints.gridx = 3;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    public boolean a() {
        Nc a2;
        return a2.e;
    }

    public boolean isFEnabled() {
        Nc a2;
        return a2.K != null;
    }

    public int A(Component a2) {
        Nc a3;
        return a3.I(a2, null);
    }

    public void A(boolean a2, int a3) {
        Nc a4;
        if (!a4.G.containsKey(a3)) {
            return;
        }
        Iterator iterator = a3 = ((Set)a4.G.get(a3)).iterator();
        while (iterator.hasNext()) {
            ((Component)a3.next()).setEnabled(a2);
            iterator = a3;
        }
    }

    public void f(Nc a2, Nc a3) {
        Nc a4;
        a4.A(a2, a3, 1);
    }

    public int A(Component a22, Component a3, Component a4, mF a5) {
        Nc a6;
        if (!a6.c.contains(a22)) {
            a22.setEnabled(a6.g);
        }
        if (!a6.c.contains(a3)) {
            a3.setEnabled(a6.g);
        }
        if (!a6.c.contains(a4)) {
            a4.setEnabled(a6.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a6.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        GridBagConstraints gridBagConstraints4 = gridBagConstraints;
        GridBagConstraints gridBagConstraints5 = gridBagConstraints;
        gridBagConstraints5.gridwidth = 1;
        gridBagConstraints5.gridx = 1;
        gridBagConstraints4.weightx = 0.0;
        gridBagConstraints4.anchor = 17;
        gridBagConstraints3.insets = new Insets(5, 0, 0, 0);
        int a22 = a6.A(a22, gridBagConstraints, -1);
        gridBagConstraints3.gridx = 2;
        gridBagConstraints2.gridwidth = 2;
        gridBagConstraints2.weightx = 2.0;
        gridBagConstraints2.fill = 2;
        a6.A(a3, gridBagConstraints, a22);
        GridBagConstraints gridBagConstraints6 = gridBagConstraints;
        GridBagConstraints gridBagConstraints7 = gridBagConstraints;
        gridBagConstraints7.gridx = 5;
        gridBagConstraints7.gridwidth = 1;
        gridBagConstraints6.fill = 0;
        gridBagConstraints6.anchor = 13;
        a6.A(a4, gridBagConstraints, a22);
        if (a5 != null && !a5.f()) {
            a6.A(gridBagConstraints, a5, a22);
        }
        return a22;
    }

    private /* synthetic */ int A(Component a2, GridBagConstraints a3, int a4) {
        Nc a5;
        if (a4 == -1) {
            a4 = a5.k++;
        }
        if (!a5.G.containsKey(a4)) {
            a5.G.put(a4, new HashSet());
        }
        ((Set)a5.G.get(a4)).add(a2);
        a5.add(a2, (Object)a3);
        return a4;
    }

    public int A() {
        Nc a2;
        return a2.m;
    }

    private /* synthetic */ void A(Component a2) {
        Nc a3;
        Nc nc2 = a3;
        super.remove(a2);
        super.repaint();
        super.revalidate();
    }

    public boolean A() {
        Iterator iterator;
        Nc a2;
        a2.e = !a2.e;
        Iterator iterator2 = iterator = a2.d.iterator();
        while (iterator2.hasNext()) {
            Integer n2 = (Integer)iterator.next();
            iterator2 = iterator;
            a2.f(n2.intValue());
        }
        if (a2.a != null) {
            Nc nc2 = a2;
            Nc nc3 = a2;
            nc2.a.A(nc2.i, nc3.E, nc3.e);
        }
        return a2.e;
    }

    public void A(boolean a2) {
        int n2;
        Nc a3;
        a3.g = a2;
        Component[] componentArray = a3.getComponents();
        Component[] componentArray2 = componentArray;
        componentArray2 = componentArray;
        int n3 = componentArray.length;
        int n4 = n2 = 0;
        while (n4 < n3) {
            Component component = componentArray2[n2];
            if (!a3.c.contains(component)) {
                if (component instanceof JPanel) {
                    a3.A((JPanel)component, Boolean.valueOf(a2));
                }
                component.setEnabled(a2);
            }
            n4 = ++n2;
        }
    }

    public int A(Component a22, Component a3, mF a4) {
        Nc a5;
        if (!a5.c.contains(a22)) {
            a22.setEnabled(a5.g);
        }
        if (!a5.c.contains(a3)) {
            a3.setEnabled(a5.g);
        }
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().gridy = a5.h++;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        GridBagConstraints gridBagConstraints4 = gridBagConstraints;
        GridBagConstraints gridBagConstraints5 = gridBagConstraints;
        gridBagConstraints5.gridx = 1;
        gridBagConstraints5.gridwidth = 1;
        gridBagConstraints4.weightx = 0.0;
        gridBagConstraints4.anchor = 17;
        gridBagConstraints3.insets = new Insets(5, 0, 0, 0);
        int a22 = a5.A(a22, gridBagConstraints, -1);
        gridBagConstraints3.gridx = 2;
        gridBagConstraints2.gridwidth = 4;
        gridBagConstraints2.weightx = 2.0;
        gridBagConstraints2.fill = 2;
        a5.A(a3, gridBagConstraints, a22);
        if (a4 != null && !a4.f()) {
            a5.A(gridBagConstraints, a4, a22);
        }
        return a22;
    }

    private /* synthetic */ void A(Nc a22, Nc a3, int a42) {
        Nc a5;
        Nc nc2 = a5;
        a22 = nc2.A(a22);
        LayoutManager layoutManager = nc2.getLayout();
        if (a22 == null || !(layoutManager instanceof GridBagLayout)) {
            SwingUtilities.invokeLater(() -> {
                Nc a3;
                int a22 = a3.a((Component)new KF(a3));
                if (a3.B() && a3.A() == rE.G) {
                    a3.A(a22);
                }
            });
            return;
        }
        layoutManager = (GridBagLayout)layoutManager;
        int a22 = ((GridBagLayout)layoutManager).getConstraints((Component)a22).gridy + a42;
        if (a5.A(a22)) {
            int n2;
            Component[] a42 = a5.getComponents();
            int n3 = a42.length;
            int n4 = n2 = 0;
            while (n4 < n3) {
                Component component = a42[n2];
                GridBagConstraints gridBagConstraints = ((GridBagLayout)layoutManager).getConstraints(component);
                if (gridBagConstraints.gridy >= a22) {
                    ++gridBagConstraints.gridy;
                    ((GridBagLayout)layoutManager).setConstraints(component, gridBagConstraints);
                }
                n4 = ++n2;
            }
        }
        SwingUtilities.invokeLater(() -> {
            Nc a4;
            a22 = a4.A((Component)new KF(a3), a22);
            if (a3.B() && a4.A() == rE.G) {
                a4.A(a22);
            }
        });
    }

    private /* synthetic */ void A(GridBagConstraints a2, JButton a3, int a4) {
        Nc a5;
        GridBagConstraints gridBagConstraints = a2;
        GridBagConstraints gridBagConstraints2 = a2;
        gridBagConstraints2.anchor = 10;
        gridBagConstraints2.fill = 0;
        gridBagConstraints2.insets = new Insets(a2.insets.top, 3, a2.insets.bottom, 3);
        gridBagConstraints.gridwidth = 1;
        gridBagConstraints.gridx = 6;
        Nc nc2 = a5;
        nc2.f((Component)a3);
        nc2.A((Component)a3, a2, a4);
    }

    public void A(String a2) {
        Nc a3;
        a3.w = a2;
        if (a3.b != null) {
            Nc nc2 = a3;
            nc2.b.accept(nc2.w);
        }
    }

    public void A(Nc a2, Nc a3) {
        Nc a4;
        a4.A(a2, a3, 0);
    }
}

