/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.dH
 */
package ttw.tradefinder;

import java.security.cert.X509Certificate;
import javax.net.ssl.X509TrustManager;
import ttw.tradefinder.dH;

public class wI
implements X509TrustManager {
    public final /* synthetic */ dH D;

    @Override
    public X509Certificate[] getAcceptedIssuers() {
        return new X509Certificate[0];
    }

    @Override
    public void checkServerTrusted(X509Certificate[] a2, String a3) {
    }

    @Override
    public void checkClientTrusted(X509Certificate[] a2, String a3) {
    }

    public /* synthetic */ wI(dH a2) {
        wI a3;
        a3.D = a2;
    }
}

