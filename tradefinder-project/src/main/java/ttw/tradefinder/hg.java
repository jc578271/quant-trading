/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  org.jfree.data.xy.XYSeries
 *  ttw.tradefinder.hg
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.util.Map;
import org.jfree.data.xy.XYSeries;
import ttw.tradefinder.KB;
import ttw.tradefinder.fe;

public class hg
implements ActionListener {
    public final /* synthetic */ fe k;
    public final /* synthetic */ XYSeries I;
    public final /* synthetic */ Map G;
    public final /* synthetic */ XYSeries D;

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ hg(fe a2, XYSeries a3, Map a4, XYSeries a5) {
        hg a6;
        hg hg2 = a6;
        hg2.k = a2;
        hg2.D = a3;
        a6.G = a4;
        a6.I = a5;
    }

    @Override
    public void actionPerformed(ActionEvent a2) {
        hg hg2;
        hg a3;
        if (a3.D.getItemCount() > 0) {
            a2 = a3.D.getDataItem(0);
            a2 = Integer.toString(a2.getX().intValue()) + "-" + Integer.toString(a2.getY().intValue());
            hg hg3 = a3;
            if (a3.G.containsKey(a2)) {
                KB kB2 = (KB)hg3.G.get(a2);
                hg hg4 = a3;
                hg2 = hg4;
                hg4.G.clear();
                hg4.G.put(a2, kB2);
            } else {
                hg3.G.clear();
                hg2 = a3;
            }
        } else {
            hg hg5 = a3;
            hg2 = hg5;
            hg5.G.clear();
        }
        hg2.I.clear();
    }
}

