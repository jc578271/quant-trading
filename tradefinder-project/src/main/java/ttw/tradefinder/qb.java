/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.A
 *  ttw.tradefinder.qb
 *  ttw.tradefinder.zD
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Graphics2D;
import ttw.tradefinder.A;
import ttw.tradefinder.zD;

public class qb
implements A {
    private Color D;

    public qb(Color a2) {
        this.D = a2;
    }

    public void a(int a2, int a3, int a4, int a5, Graphics2D a6) {
        this.f(a2, a3, a4, a5, a6);
    }

    public void f(int a2, int a3, int a4, int a5, Graphics2D a6) {
        Graphics2D graphics2D = a6;
        graphics2D.setColor(this.D);
        graphics2D.fillRect(a2, a3, a4, a5);
    }

    public void drawWithOpacity(int a2, int a3, int a4, int a5, Graphics2D a6) {
        this.f(a2, a3, a4, a5, a6);
    }

    public void setOpacity(int a2) {
        this.D = zD.f((Color)this.D, (int)a2);
    }
}

