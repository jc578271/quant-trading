/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Fb
 *  ttw.tradefinder.jB
 *  ttw.tradefinder.zD
 *  velox.gui.colors.Colors
 */
package ttw.tradefinder;

import java.awt.Graphics2D;
import java.awt.RenderingHints;
import java.awt.image.BufferedImage;
import ttw.tradefinder.Fb;
import ttw.tradefinder.zD;
import velox.gui.colors.Colors;

public class jB {
    private static final int G = 100;
    private static final int D = 20;

    public BufferedImage A(String a2, int a32) {
        if (a32 < 100) {
            return new BufferedImage(1, 1, 2);
        }
        a2 = new Fb(a2, zD.k);
        BufferedImage a32 = new BufferedImage(a32, 20, 2);
        Graphics2D graphics2D = a32.createGraphics();
        graphics2D.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
        Graphics2D graphics2D2 = graphics2D;
        Graphics2D graphics2D3 = graphics2D;
        graphics2D.setRenderingHint(RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_QUALITY);
        graphics2D3.setColor(Colors.TRANSPARENT);
        graphics2D3.fillRect(0, 0, a32.getWidth(), a32.getHeight());
        graphics2D3.setColor(zD.G);
        graphics2D2.fillRoundRect(0, 0, a32.getWidth(), a32.getHeight(), 5, 5);
        a2.A(0, 0, a32.getWidth(), a32.getHeight(), 0, graphics2D);
        graphics2D2.dispose();
        return a32;
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 2 << 3 ^ (2 ^ 5);
        int cfr_ignored_0 = 4 << 4 ^ 2 << 1;
        int n5 = n3;
        int n6 = 5 << 3;
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

    public jB() {
        jB a2;
    }
}

