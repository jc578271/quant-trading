/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.DA
 *  ttw.tradefinder.RI
 *  ttw.tradefinder.if
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.Dimension;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Insets;
import java.awt.Paint;
import java.awt.Rectangle;
import java.awt.geom.Path2D;
import javax.swing.JComponent;
import javax.swing.JLabel;
import javax.swing.border.AbstractBorder;
import javax.swing.border.Border;
import ttw.tradefinder.DA;
import ttw.tradefinder.Ie;
import ttw.tradefinder.RI;

/*
 * Exception performing whole class analysis ignored.
 */
public class if
extends AbstractBorder {
    private static final int e = 25;
    private final JLabel i;
    public static final long k = 7481250625475798376L;
    private static final int I = 10;
    private final RI G;
    private String D;

    public Dimension A(Component a2) {
        Cloneable cloneable;
        if a3;
        if if_ = a3;
        Insets insets = cloneable = if_.getBorderInsets(a2);
        Insets insets2 = cloneable;
        cloneable = new Dimension(insets.right + insets.left, insets2.top + insets2.bottom);
        if (if_.D != null && !a3.D.isEmpty()) {
            a2 = a3.A(a2);
            a2 = ((JComponent)a2).getPreferredSize();
            ((Dimension)cloneable).width += ((Dimension)a2).width;
        }
        return cloneable;
    }

    @Override
    public Insets getBorderInsets(Component a2, Insets a3) {
        if a4;
        if if_ = a4;
        a3 = if.A((Border)if_.G, (Component)a2, (Insets)a3);
        if (if_.D != null && !a4.D.isEmpty()) {
            a2 = a4.A(a2);
            a2 = ((JComponent)a2).getPreferredSize();
            if (a3.top < ((Dimension)a2).height) {
                a3.top = ((Dimension)a2).height;
            }
            Insets insets = a3;
            insets.top += 10;
            insets.left += 10;
            insets.right += 10;
            insets.bottom += 10;
        }
        return a3;
    }

    private /* synthetic */ JLabel A(Component a2) {
        if a3;
        if if_ = a3;
        if if_2 = a3;
        if_.i.setText(if_2.D);
        if_.i.setFont(a2.getFont());
        if_2.i.setForeground(a2.getForeground());
        if_.i.setComponentOrientation(a2.getComponentOrientation());
        if_.i.setEnabled(a2.isEnabled());
        return if_.i;
    }

    private static /* synthetic */ Insets A(Border a2, Component a3, Insets a4) {
        Border border = a2;
        if (a2 instanceof AbstractBorder) {
            a2 = (AbstractBorder)border;
            a4 = ((AbstractBorder)a2).getBorderInsets(a3, a4);
            return a4;
        }
        a2 = border.getBorderInsets(a3);
        Insets insets = a4;
        Object object = a2;
        Object object2 = a2;
        insets.set(((Insets)object).top, ((Insets)object).left, ((Insets)object2).bottom, ((Insets)object2).right);
        return insets;
    }

    @Override
    public void paintBorder(Component a2, Graphics a3, int a4, int a5, int a6, int a7) {
        if a8;
        if (a8.D != null && !a8.D.isEmpty()) {
            if if_ = a8;
            JLabel jLabel = if_.A(a2);
            Dimension dimension = jLabel.getPreferredSize();
            Object object = if.A((Border)if_.G, (Component)a2, (Insets)new Insets(0, 0, 0, 0));
            int n2 = dimension.height;
            Insets insets = object;
            insets.top = insets.top / 2 - n2 / 2;
            int n3 = a5 + ((Insets)object).top;
            insets.left += 25;
            insets.right += 25;
            int n4 = a6 - ((Insets)object).left - ((Insets)object).right;
            if (n4 > dimension.width) {
                n4 = dimension.width;
            }
            int n5 = a4 + ((Insets)object).left;
            object = a3.create();
            if (object instanceof Graphics2D) {
                Graphics2D graphics2D = (Graphics2D)object;
                Path2D.Float float_ = new Path2D.Float();
                int n6 = a5;
                float_.append(new Rectangle(a4, n6, a6, n3 - n6), false);
                Path2D.Float float_2 = float_;
                int n7 = a4;
                float_2.append(new Rectangle(n7, n3, n5 - n7 - 10, n2), false);
                float_2.append(new Rectangle(n5 + n4 + 10, n3, a4 - n5 + a6 - n4 - 10, n2), false);
                float_2.append(new Rectangle(a4, n3 + n2, a6, a5 - n3 + a7 - n2), false);
                graphics2D.clip(float_2);
            }
            a8.G.paintBorder(a2, (Graphics)object, a4, a5, a6, a7);
            ((Graphics)object).dispose();
            Graphics graphics = a3;
            graphics.translate(n5, n3);
            jLabel.setSize(n4, n2);
            jLabel.paint(graphics);
            graphics.translate(-n5, -n3);
            return;
        }
        a8.G.paintBorder(a2, a3, a4, a5, a6, a7);
    }

    public void A(String a2) {
        a.D = a2;
    }

    public if(String a2, Paint a3, boolean a4) {
        if a5;
        a5.D = a2;
        a5.G = new RI(a3, a4);
        a5.i = new JLabel();
        a5.i.setOpaque(false);
    }

    @Override
    public int getBaseline(Component a2, int a2222, int a32) {
        if a4;
        if (a2 == null) {
            throw new NullPointerException(DA.A((Object)"\\(b)1.d-a1h}\u007f2\u007fp\u007f(}11>~0a2\u007f8\u007f)"));
        }
        if (a2222 < 0) {
            throw new IllegalArgumentException(Ie.A((Object)"fvUkY?\\jBk\u0011}T?\u000f\"\u0011/"));
        }
        if (a32 < 0) {
            throw new IllegalArgumentException(DA.A((Object)"\u0015t4v5e}|(b)1?t}/`1m"));
        }
        if (a4.D != null && !a4.D.isEmpty()) {
            if if_ = a4;
            JLabel a2222 = if_.A(a2);
            Dimension a32 = a2222.getPreferredSize();
            a2 = if.A((Border)if_.G, (Component)a2, (Insets)new Insets(0, 0, 0, 0));
            Dimension dimension = a32;
            int a2222 = a2222.getBaseline(dimension.width, dimension.height);
            ((Insets)a2).top = (((Insets)a2).top - a32.height) / 2;
            return a2222 + ((Insets)a2).top;
        }
        return -1;
    }

    public void A(boolean a2) {
        if a3;
        a3.G.A(a2);
    }

    @Override
    public Component.BaselineResizeBehavior getBaselineResizeBehavior(Component a2) {
        if a3;
        super.getBaselineResizeBehavior(a2);
        return Component.BaselineResizeBehavior.CONSTANT_ASCENT;
    }
}

