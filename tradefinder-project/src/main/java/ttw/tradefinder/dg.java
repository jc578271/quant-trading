/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.UC
 *  ttw.tradefinder.dg
 *  ttw.tradefinder.oD
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.List;
import ttw.tradefinder.UC;
import ttw.tradefinder.bg;
import ttw.tradefinder.oD;

public class dg {
    public /* synthetic */ dg() {
        dg a2;
    }

    /*
     * Enabled aggressive block sorting
     */
    public static /* synthetic */ boolean A(bg a2, UC a3) {
        if (a3 == UC.D || a2 == bg.G) {
            return true;
        }
        if (a2 == bg.D) {
            switch (oD.D[a3.ordinal()]) {
                case 1: {
                    return true;
                }
            }
            return false;
        }
        if (a2 == bg.k) {
            switch (oD.D[a3.ordinal()]) {
                case 1: 
                case 2: {
                    return true;
                }
            }
            return false;
        }
        if (a2 == bg.i) {
            switch (oD.D[a3.ordinal()]) {
                case 1: 
                case 3: 
                case 4: 
                case 5: {
                    return true;
                }
            }
            return false;
        }
        if (a2 != bg.e) {
            return false;
        }
        switch (oD.D[a3.ordinal()]) {
            case 1: 
            case 3: 
            case 4: {
                return true;
            }
        }
        return false;
    }

    public static /* synthetic */ List<bg> A() {
        ArrayList<bg> arrayList = new ArrayList<bg>();
        arrayList.add((bg)bg.D);
        arrayList.add(bg.k);
        arrayList.add(bg.i);
        arrayList.add(bg.e);
        return arrayList;
    }
}

