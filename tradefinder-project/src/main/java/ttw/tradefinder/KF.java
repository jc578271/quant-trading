/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.KF
 *  ttw.tradefinder.Nc
 *  ttw.tradefinder.Se
 *  velox.gui.StrategyPanel
 */
package ttw.tradefinder;

import java.awt.Component;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.LayoutManager;
import ttw.tradefinder.Nc;
import ttw.tradefinder.Se;
import velox.gui.StrategyPanel;

public class KF
extends StrategyPanel {
    public static final long G = 7484950694675698376L;
    private final Nc D;

    public KF(Nc a2) {
        KF a3;
        KF kF2 = a3;
        super(null);
        kF2.D = a2;
        super.setLayout((LayoutManager)new GridBagLayout());
        GridBagConstraints gridBagConstraints = new GridBagConstraints();
        new GridBagConstraints().anchor = 19;
        GridBagConstraints gridBagConstraints2 = gridBagConstraints;
        GridBagConstraints gridBagConstraints3 = gridBagConstraints;
        gridBagConstraints3.fill = 2;
        gridBagConstraints3.gridx = 1;
        gridBagConstraints2.gridy = 0;
        gridBagConstraints2.weightx = 1.0;
        if (a2.w == null) {
            super.add((Component)a2, (Object)gridBagConstraints);
            return;
        }
        a2 = new Se(a2);
        super.add((Component)a2, (Object)gridBagConstraints);
    }

    public boolean A(Nc a2) {
        KF a3;
        return a3.D == a2;
    }
}

