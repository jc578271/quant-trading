/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasIcon
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasShape
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class iH {
    private final List<ScreenSpaceCanvas.CanvasIcon> I;
    private final Object G = new Object();
    private ScreenSpaceCanvas D = null;

    public int A() {
        iH a2;
        return a2.I.size();
    }

    public void f() {
        iH a2;
        Object object = a2.G;
        synchronized (object) {
            if (a2.D != null) {
                Iterator<ScreenSpaceCanvas.CanvasIcon> iterator;
                Iterator<ScreenSpaceCanvas.CanvasIcon> iterator2 = iterator = a2.I.iterator();
                while (iterator2.hasNext()) {
                    ScreenSpaceCanvas.CanvasIcon canvasIcon = iterator.next();
                    iterator2 = iterator;
                    a2.D.removeShape((ScreenSpaceCanvas.CanvasShape)canvasIcon);
                }
            }
            a2.I.clear();
            return;
        }
    }

    public void A() {
        iH a2;
        iH iH2 = a2;
        iH2.f();
        Object object = iH2.G;
        synchronized (object) {
            if (a2.D == null) {
                return;
            }
            a2.D.dispose();
            a2.D = null;
            return;
        }
    }

    public void A(ScreenSpaceCanvas.CanvasIcon a2) {
        iH a3;
        Object object = a3.G;
        synchronized (object) {
            if (a3.D == null) {
                return;
            }
            iH iH2 = a3;
            iH2.D.addShape((ScreenSpaceCanvas.CanvasShape)a2);
            iH2.I.add(a2);
            return;
        }
    }

    public iH(ScreenSpaceCanvas a2) {
        iH a3;
        a3.I = new ArrayList<ScreenSpaceCanvas.CanvasIcon>();
        a3.D = a2;
        a3.I.clear();
    }
}

