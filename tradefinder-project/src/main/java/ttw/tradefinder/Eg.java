/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Eg
 *  ttw.tradefinder.MF
 *  ttw.tradefinder.yE
 */
package ttw.tradefinder;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.File;
import java.io.Serializable;
import javax.swing.JButton;
import javax.swing.JFileChooser;
import javax.swing.JLabel;
import javax.swing.LookAndFeel;
import javax.swing.UIManager;
import javax.swing.filechooser.FileNameExtensionFilter;
import ttw.tradefinder.MF;
import ttw.tradefinder.go;
import ttw.tradefinder.yE;

public class Eg
implements ActionListener {
    public final /* synthetic */ yE I;
    public final /* synthetic */ JLabel G;
    public final /* synthetic */ JButton D;

    @Override
    public void actionPerformed(ActionEvent a2) {
        JFileChooser jFileChooser;
        JFileChooser jFileChooser2;
        Eg a3;
        a2 = null;
        try {
            a2 = UIManager.getLookAndFeel();
            UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
        }
        catch (Exception exception) {}
        Serializable serializable = new File(a3.I.e.A());
        if (!((File)serializable).exists()) {
            serializable = new File(a3.I.e.I());
        }
        if (((File)serializable).exists()) {
            jFileChooser = jFileChooser2;
            jFileChooser2 = new JFileChooser((File)serializable);
        } else {
            jFileChooser = jFileChooser2;
            jFileChooser2 = new JFileChooser();
        }
        serializable = jFileChooser;
        jFileChooser.setFileSelectionMode(0);
        Serializable serializable2 = serializable;
        ((JFileChooser)serializable2).setAcceptAllFileFilterUsed(false);
        ((JFileChooser)serializable2).setFileFilter(new FileNameExtensionFilter(go.A("\u0007g!f0(\u0012a8m'(|\"z\u007f5~}"), MF.A((Object)"@qA")));
        if (((JFileChooser)serializable).showDialog(a3.D, go.A("[1d1k ")) == 0) {
            serializable = ((JFileChooser)serializable).getSelectedFile();
            a3.G.setText("Bid - " + ((File)serializable).getName());
            Eg eg2 = a3;
            eg2.G.setToolTipText(((File)serializable).getAbsolutePath());
            eg2.I.e.A(((File)serializable).getAbsolutePath());
        }
        try {
            if (a2 != null) {
                UIManager.setLookAndFeel((LookAndFeel)a2);
            }
            return;
        }
        catch (Exception exception) {
            return;
        }
    }

    /*
     * Ignored method signature, as it can't be verified against descriptor
     */
    public /* synthetic */ Eg(yE a2, JButton a3, JLabel a4) {
        Eg a5;
        a5.I = a2;
        a5.D = a3;
        a5.G = a4;
    }
}

