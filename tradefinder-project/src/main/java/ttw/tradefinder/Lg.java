/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Lg
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import javax.swing.AbstractAction;
import javax.swing.JTextField;

public class Lg
extends AbstractAction {
    public static final long G = 8128747958723877L;
    public final /* synthetic */ JTextField D;

    @Override
    public /* synthetic */ void actionPerformed(ActionEvent a2) {
        Lg a3;
        a3.D.paste();
    }

    public /* synthetic */ Lg(String a2, JTextField a3) {
        Lg a4;
        a4.D = a3;
        super(a2);
    }
}

