/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.BE
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Se
 *  ttw.tradefinder.if
 */
package ttw.tradefinder;

import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import ttw.tradefinder.Nc;
import ttw.tradefinder.Se;
import ttw.tradefinder.if;

public class BE
extends MouseAdapter {
    public final /* synthetic */ Nc I;
    public final /* synthetic */ Se G;
    public final /* synthetic */ if D;

    public /* synthetic */ BE(Se a2, if a3, Nc a4) {
        BE a5;
        a5.G = a2;
        a5.D = a3;
        a5.I = a4;
    }

    @Override
    public void mouseClicked(MouseEvent a2) {
        BE a3;
        if (a2.getX() < 25 && a2.getY() < 25) {
            BE bE2 = a3;
            bE2.D.A(bE2.I.A());
        }
        a3.G.repaint();
    }
}

