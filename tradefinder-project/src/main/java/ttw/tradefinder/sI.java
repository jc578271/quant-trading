/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasIcon
 *  velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas$CanvasShape
 */
package ttw.tradefinder;

import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import velox.api.layer1.layers.strategies.interfaces.ScreenSpaceCanvas;

/*
 * Duplicate member names - consider using --renamedupmembers true
 */
public class sI {
    private ScreenSpaceCanvas I = null;
    private final Object G = new Object();
    private final Map<String, ScreenSpaceCanvas.CanvasIcon> D = new HashMap<String, ScreenSpaceCanvas.CanvasIcon>();

    public void f() {
        sI a2;
        sI sI2 = a2;
        sI2.A();
        Object object = sI2.G;
        synchronized (object) {
            if (a2.I == null) {
                return;
            }
            a2.I.dispose();
            a2.I = null;
            return;
        }
    }

    public sI(ScreenSpaceCanvas a2) {
        sI a3;
        a3.I = a2;
        a3.D.clear();
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 5;
        int cfr_ignored_0 = 4 << 4 ^ 2 << 1;
        int n5 = n3;
        int n6 = 4 << 4 ^ 2 << 1;
        while (n5 >= 0) {
            int n7 = n3--;
            a2[n7] = (char)(((String)object2).charAt(n7) ^ n6);
            if (n3 < 0) break;
            int n8 = n3--;
            a2[n8] = (char)(((String)object2).charAt(n8) ^ n4);
            n5 = n3;
        }
        return new String((char[])a2);
    }

    public void A(String a2, ScreenSpaceCanvas.CanvasIcon a3) {
        sI a4;
        Object object = a4.G;
        synchronized (object) {
            if (a4.I == null) {
                return;
            }
            if (a4.D.containsKey(a2)) {
                sI sI2 = a4;
                sI2.I.removeShape((ScreenSpaceCanvas.CanvasShape)sI2.D.get(a2));
            }
            sI sI3 = a4;
            sI3.I.addShape((ScreenSpaceCanvas.CanvasShape)a3);
            sI3.D.put(a2, a3);
            return;
        }
    }

    public void A() {
        sI a2;
        Object object = a2.G;
        synchronized (object) {
            if (a2.I != null) {
                Iterator<ScreenSpaceCanvas.CanvasIcon> iterator;
                Iterator<ScreenSpaceCanvas.CanvasIcon> iterator2 = iterator = a2.D.values().iterator();
                while (iterator2.hasNext()) {
                    ScreenSpaceCanvas.CanvasIcon canvasIcon = iterator.next();
                    iterator2 = iterator;
                    a2.I.removeShape((ScreenSpaceCanvas.CanvasShape)canvasIcon);
                }
            }
            a2.D.clear();
            return;
        }
    }

    public int A() {
        sI a2;
        return a2.D.size();
    }

    public void A(String a2) {
        sI a3;
        Object object = a3.G;
        synchronized (object) {
            a2 = a3.D.remove(a2);
            if (a2 != null && a3.I != null) {
                a3.I.removeShape((ScreenSpaceCanvas.CanvasShape)a2);
            }
            return;
        }
    }
}

