/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.RI
 *  ttw.tradefinder.Xb
 */
package ttw.tradefinder;

import java.awt.BasicStroke;
import java.awt.Component;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Insets;
import java.awt.Paint;
import java.awt.RenderingHints;
import java.awt.Stroke;
import java.awt.image.BufferedImage;
import javax.imageio.ImageIO;
import javax.swing.border.AbstractBorder;
import ttw.tradefinder.Fa;
import ttw.tradefinder.Xb;
import ttw.tradefinder.hI;

/*
 * Exception performing whole class analysis ignored.
 */
public class RI
extends AbstractBorder {
    private static final BufferedImage a;
    private Paint K;
    private boolean m;
    private static int F;
    private Stroke e;
    private boolean i;
    private static final BufferedImage k;
    private static int I;
    public static final long G = 7481250625475698376L;
    private static int D;

    private static /* synthetic */ BufferedImage A(String a2) {
        try {
            return ImageIO.read(Fa.class.getClassLoader().getResourceAsStream(a2));
        }
        catch (Exception exception) {
            return new BufferedImage(1, 1, 2);
        }
    }

    public RI(Paint a2, boolean a3) {
        RI a4;
        RI rI2 = a4;
        RI rI3 = a4;
        a4.e = new BasicStroke(F);
        rI3.m = false;
        rI3.i = false;
        rI2.K = a2;
        rI2.m = a3;
    }

    /*
     * Unable to fully structure code
     */
    @Override
    public void paintBorder(Component a, Graphics a, int a, int a, int a, int a) {
        a = (Graphics2D)a;
        var7_8 = a.getPaint();
        v0 = a;
        var8_9 = v0.getStroke();
        var9_10 = v0.getRenderingHint(RenderingHints.KEY_ANTIALIASING);
        try {
            a.setPaint(a.K != null ? a.K : a.getForeground());
            v1 = a;
            a.setStroke(a.e);
            v1.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
            v1.setRenderingHint(RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_QUALITY);
            v2 = RI.I;
            v1.drawRoundRect(a + 1, a + RI.D, a - 2 * RI.F - 1, a - 2 * RI.F - RI.D, v2, v2);
            if (!a.m) ** GOTO lbl26
            v3 = a;
            v3.setColor(a.getBackground());
            v3.fillRect(a, a, 30, 25);
            v4 = a;
            if (a.i) {
                v4.drawImage(RI.a, a, a, 18, 18, null);
                v5 = a;
            } else {
                v4.drawImage(RI.k, a, a, 18, 18, null);
lbl26:
                // 2 sources

                v5 = a;
            }
            v5.setPaint(var7_8);
            v6 = a;
            v6.setStroke(var8_9);
            v6.setRenderingHint(RenderingHints.KEY_ANTIALIASING, var9_10);
            return;
        }
        catch (Throwable a) {
            v7 = a;
            v7.setPaint(var7_8);
            v7.setStroke(var8_9);
            v7.setRenderingHint(RenderingHints.KEY_ANTIALIASING, var9_10);
            throw a;
        }
    }

    @Override
    public Insets getBorderInsets(Component a2, Insets a3) {
        RI a4;
        if (a3 == null) {
            Insets insets;
            if (a4.m) {
                int n2 = I;
                insets = new Insets(I + 5, I - 3, n2, n2 - 3);
                a3 = insets;
                return insets;
            }
            int n3 = I;
            int n4 = I;
            insets = new Insets(n3, n3 - 3, n4, n4 - 3);
            a3 = insets;
            return insets;
        }
        Insets insets = a3;
        if (a4.m) {
            insets.left = I - 3;
            a3.right = I - 3;
            a3.bottom = I;
            a3.top = I + 5;
            return a3;
        }
        insets.left = insets.right = I - 3;
        Insets insets2 = a3;
        insets2.bottom = insets2.top = I;
        return a3;
    }

    public void A(boolean a2) {
        a.i = a2;
    }

    static {
        D = 5;
        F = 1;
        I = 10;
        k = RI.A((String)hI.A("Tl\\fXr\u0012`OsRvYnJo\u0013qSf"));
        a = RI.A((String)Xb.A((Object)"\u0001\u001d\t\u0017\r\u0003G\u0011\u001a\u0002\u0007\u0007\u001a\u0019\u000f\u0018\u001c^\u0018\u001e\u000f"));
    }
}

