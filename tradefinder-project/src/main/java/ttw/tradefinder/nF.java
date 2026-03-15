/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.JE
 *  ttw.tradefinder.nF
 */
package ttw.tradefinder;

import javax.sound.sampled.LineEvent;
import javax.sound.sampled.LineListener;
import ttw.tradefinder.JE;

public class nF
implements LineListener {
    public final /* synthetic */ JE D;

    public /* synthetic */ nF(JE a2) {
        nF a3;
        a3.D = a2;
    }

    @Override
    public void update(LineEvent a2) {
        if (((LineEvent)a2).getType() == LineEvent.Type.STOP) {
            a2 = ((LineEvent)a2).getLine();
            a2.close();
        }
    }
}

