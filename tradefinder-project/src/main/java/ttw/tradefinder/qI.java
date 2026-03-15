/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Eb
 *  ttw.tradefinder.La
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.jG
 *  ttw.tradefinder.qI
 */
package ttw.tradefinder;

import java.awt.event.MouseEvent;
import javax.swing.event.MouseInputAdapter;
import ttw.tradefinder.Eb;
import ttw.tradefinder.La;
import ttw.tradefinder.UC;
import ttw.tradefinder.jG;

public class qI
extends MouseInputAdapter {
    public final /* synthetic */ La G;
    public final /* synthetic */ UC D;

    public /* synthetic */ qI(La a2, UC a3) {
        qI a4;
        a4.G = a2;
        a4.D = a3;
    }

    @Override
    public void mouseClicked(MouseEvent a2) {
        qI a3;
        jG.A((String)Eb.A((UC)a3.D));
    }
}

