/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Ff
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import javax.swing.AbstractAction;
import javax.swing.JTextField;

public class Ff
extends AbstractAction {
    public final /* synthetic */ JTextField G;
    public static final long D = 8128747958723878L;

    @Override
    public /* synthetic */ void actionPerformed(ActionEvent a2) {
        Ff a3;
        a3.G.selectAll();
    }

    public /* synthetic */ Ff(String a2, JTextField a3) {
        Ff a4;
        a4.G = a3;
        super(a2);
    }
}

