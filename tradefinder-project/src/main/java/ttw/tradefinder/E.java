/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.B
 *  ttw.tradefinder.E
 */
package ttw.tradefinder;

import java.awt.Color;
import java.awt.Graphics2D;
import ttw.tradefinder.B;

public interface E
extends B {
    default public void A(Color a2, int a3) {
    }

    public void b(int var1, int var2, int var3, int var4, int var5, Graphics2D var6);

    public void A(int var1, int var2, int var3, int var4, int var5, Graphics2D var6);

    default public void g(int a2, int a3, int a4, int a5, int a6, Graphics2D a7) {
        E a8;
        a8.A(a2, a3, a4, a5, a6, a7);
    }

    default public void A(int a2) {
    }

    default public void a(int a2, int a3, int a4, int a5, int a6, Graphics2D a7) {
        E a8;
        a8.A(a2, a3, a4, a5, a6, a7);
    }

    default public void B(int a2, int a3, int a4, int a5, int a6, Graphics2D a7) {
        E a8;
        a8.A(a2, a3, a4, a5, a6, a7);
    }

    public int I();

    default public void k(int a2, int a3, int a4, int a5, int a6, Graphics2D a7) {
        E a8;
        a8.A(a2, a3, a4, a5, a6, a7);
    }

    public int a();
}

