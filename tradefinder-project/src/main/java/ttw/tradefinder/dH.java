/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.MB
 *  ttw.tradefinder.NF
 *  ttw.tradefinder.Nh
 *  ttw.tradefinder.ZI
 *  ttw.tradefinder.ca
 *  ttw.tradefinder.dH
 */
package ttw.tradefinder;

import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.security.GeneralSecurityException;
import java.security.SecureRandom;
import java.util.HashMap;
import java.util.Map;
import java.util.StringJoiner;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import ttw.tradefinder.MB;
import ttw.tradefinder.NF;
import ttw.tradefinder.Nh;
import ttw.tradefinder.ZI;
import ttw.tradefinder.ca;
import ttw.tradefinder.wI;

public class dH
implements ca {
    private BlockingQueue<Nh> G = new LinkedBlockingQueue();
    private static String D = MB.A((Object)"kWeG~AbGoTn");

    public void f(Nh a2) {
        dH a3;
        a3.G.offer(a2);
    }

    public void f() {
        dH a2;
        a2.G.offer(new Nh("", "", D));
    }

    public dH() {
        dH a2;
        TrustManager[] trustManagerArray = new TrustManager[]{new wI(a2)};
        try {
            SSLContext sSLContext = SSLContext.getInstance(MB.A((Object)"YfF"));
            sSLContext.init(null, trustManagerArray, new SecureRandom());
            HttpsURLConnection.setDefaultSSLSocketFactory(sSLContext.getSocketFactory());
            return;
        }
        catch (GeneralSecurityException generalSecurityException) {
            return;
        }
    }

    /*
     * Enabled force condition propagation
     * Lifted jumps to return sites
     */
    public void A(Nh a22) {
        if (((Nh)a22).D.startsWith(NF.A((Object)"S")) || ((Nh)a22).D.startsWith(MB.A((Object)"'"))) {
            return;
        }
        try {
            Object object;
            Object object2 = new URL(String.format(NF.A((Object)"a@}Dz\u000e&\u001bhD`\u001a}QeQnFhY'[{S&Vf@,G&GlZmylGzUnQ"), ((Nh)a22).G));
            object2 = ((URL)object2).openConnection();
            object2 = (HttpsURLConnection)object2;
            ((HttpURLConnection)object2).setRequestMethod(MB.A((Object)"eEf^"));
            ((URLConnection)object2).setDoOutput(true);
            Object object3 = new HashMap<String, String>();
            object3.put(NF.A((Object)"WaU}k`P"), ((Nh)a22).D);
            object3.put(MB.A((Object)"EkGyPUXeQo"), NF.A((Object)"dU{_m[~Z"));
            object3.put(MB.A((Object)"AoM~"), ((Nh)a22).I);
            a22 = new StringJoiner(NF.A((Object)"\u0012"));
            object3 = object3.entrySet().iterator();
            Object object4 = object3;
            while (object4.hasNext()) {
                object = (Map.Entry)object3.next();
                ((StringJoiner)a22).add(URLEncoder.encode((String)object.getKey(), MB.A((Object)"_aL\u00182")) + "=" + URLEncoder.encode((String)object.getValue(), NF.A((Object)"a]r$\f")));
                object4 = object3;
            }
            object3 = ((StringJoiner)a22).toString().getBytes(StandardCharsets.UTF_8);
            ((HttpURLConnection)object2).setFixedLengthStreamingMode(((Object)object3).length);
            Object object5 = object2;
            ((URLConnection)object5).connect();
            object = ((URLConnection)object5).getOutputStream();
            try {
                ((OutputStream)object).write((byte[])object3);
                if (object == null) return;
            }
            catch (Throwable a22) {
                Throwable throwable;
                if (object != null) {
                    try {
                        ((OutputStream)object).close();
                        throwable = a22;
                        throw throwable;
                    }
                    catch (Throwable throwable2) {
                        a22.addSuppressed(throwable2);
                    }
                }
                throwable = a22;
                throw throwable;
            }
            ((OutputStream)object).close();
            return;
        }
        catch (Exception exception) {
            return;
        }
    }

    public void A() {
        dH a2;
        new Thread((Runnable)new ZI(a2.G, D, (ca)a2)).start();
    }
}

