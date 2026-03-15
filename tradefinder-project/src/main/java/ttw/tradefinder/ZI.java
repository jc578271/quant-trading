/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Nh
 *  ttw.tradefinder.ZI
 *  ttw.tradefinder.ca
 */
package ttw.tradefinder;

import java.util.ArrayList;
import java.util.concurrent.BlockingQueue;
import ttw.tradefinder.Nh;
import ttw.tradefinder.ca;

public class ZI
implements Runnable {
    private Boolean k;
    private final BlockingQueue<Nh> I;
    private final String G;
    private final ca D;

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 5;
        int cfr_ignored_0 = 5 << 3 ^ 2;
        int n5 = n3;
        int n6 = 4 << 4 ^ 5 << 1;
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

    public ZI(BlockingQueue<Nh> a2, String a3, ca a4) {
        ZI a5;
        ZI zI = a5;
        a5.k = Boolean.TRUE;
        a5.D = a4;
        zI.I = a2;
        zI.G = a3;
    }

    @Override
    public void run() {
        a2.k = Boolean.TRUE;
        try {
            ZI a2;
            while (a2.k.booleanValue()) {
                Nh nh2 = (Nh)a2.I.take();
                ArrayList<String> arrayList = new ArrayList<String>();
                Nh nh3 = nh2;
                String string = nh3.D;
                String string2 = nh3.G;
                ZI zI = a2;
                arrayList.add(nh2.I);
                while (!zI.I.isEmpty()) {
                    nh2 = (Nh)a2.I.take();
                    if (nh2.D.equals(string) && nh2.G.equals(string2)) {
                        zI = a2;
                        arrayList.add(nh2.I);
                        continue;
                    }
                    if (arrayList.contains(a2.G)) {
                        return;
                    }
                    zI = a2;
                    Nh nh4 = nh2;
                    a2.D.A(new Nh(string, string2, String.join((CharSequence)System.lineSeparator(), arrayList)));
                    string = nh4.D;
                    string2 = nh4.G;
                    ArrayList<String> arrayList2 = arrayList;
                    arrayList2.clear();
                    arrayList2.add(nh2.I);
                }
                if (arrayList.contains(a2.G)) {
                    return;
                }
                a2.D.A(new Nh(string, string2, String.join((CharSequence)System.lineSeparator(), arrayList)));
            }
            return;
        }
        catch (InterruptedException interruptedException) {
            Thread.currentThread().interrupt();
            return;
        }
    }
}

