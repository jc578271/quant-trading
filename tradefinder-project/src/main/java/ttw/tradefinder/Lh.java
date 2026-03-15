/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.MF
 *  ttw.tradefinder.xe
 */
package ttw.tradefinder;

import java.util.LinkedHashMap;
import java.util.TimerTask;
import ttw.tradefinder.MF;
import ttw.tradefinder.Rh;
import ttw.tradefinder.xe;

public class Lh
extends TimerTask {
    public final /* synthetic */ Rh D;

    public /* synthetic */ Lh(Rh a2) {
        Lh a3;
        a3.D = a2;
    }

    @Override
    public void run() {
        Lh a2;
        int n2 = a2.D.I;
        synchronized (n2) {
            a2.D.k = true;
            a2.D.e.addAll(a2.D.i.A());
            LinkedHashMap<String, String> linkedHashMap = new LinkedHashMap<String, String>();
            linkedHashMap.put(xe.A((Object)">R3Y7Y"), String.join((CharSequence)MF.A((Object)"\u001b"), (Iterable<? extends CharSequence>)a2.D.K));
            linkedHashMap.put(xe.A((Object)"X<\\0Q7Y"), String.join((CharSequence)MF.A((Object)"\u001b"), (Iterable<? extends CharSequence>)((Object)a2.D.e)));
            a2.D.a.trackEvent(xe.A((Object)"!I3O&\u0010'M\u007fT<N&O'P7S&N"), linkedHashMap, null);
            return;
        }
    }
}

