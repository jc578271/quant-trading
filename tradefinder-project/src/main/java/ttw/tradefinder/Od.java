/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  com.google.gson.Gson
 *  ttw.tradefinder.Od
 *  ttw.tradefinder.Td
 *  ttw.tradefinder.gf
 *  ttw.tradefinder.hB
 *  ttw.tradefinder.lA
 */
package ttw.tradefinder;

import com.google.gson.Gson;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;
import java.nio.charset.StandardCharsets;
import java.security.GeneralSecurityException;
import java.security.SecureRandom;
import java.util.Collections;
import java.util.List;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import ttw.tradefinder.Qd;
import ttw.tradefinder.Td;
import ttw.tradefinder.gf;
import ttw.tradefinder.hB;
import ttw.tradefinder.lA;

public class Od {
    private SSLContext G;
    private X509TrustManager D;

    public /* synthetic */ lA A(String a2, String a3, String a4, int a5) {
        Od a6;
        if (a6.G == null) {
            throw new Exception(Td.A((Object)"\u0017}\bm+@0K<Zd@+ZdG*G0G%B-T!J"));
        }
        try {
            return a6.A(String.format("%s", a6.A((String)a2)));
        }
        catch (Exception exception) {
            a2 = exception;
            throw exception;
        }
    }

    private /* synthetic */ lA A(String a2) {
        lA lA2 = lA.a();
        if (!a2.equals("LY-KHOA-TTW372-XXXXX-TLG-FXAURUM")) {
            throw new Exception("parse license server response failed: " + a2);
        }
        return lA2;
    }

    private /* synthetic */ List<String> A(String a2) {
        hB hB2 = (hB)new Gson().fromJson(a2, hB.class);
        if (hB2 == null) {
            throw new Exception("parse license server list response failed: " + a2);
        }
        return hB2.A();
    }

    public /* synthetic */ void A() {
        Od a2;
        a2.D = a2.A();
        try {
            a2.G = SSLContext.getInstance(gf.A((Object)"T\u0010K"));
            a2.G.init(null, Collections.singletonList(a2.D).toArray(new TrustManager[0]), new SecureRandom());
            return;
        }
        catch (GeneralSecurityException generalSecurityException) {
            a2.G = null;
            generalSecurityException.printStackTrace();
            return;
        }
    }

    private /* synthetic */ X509TrustManager A() {
        Od a2;
        return new Qd(a2);
    }

    private /* synthetic */ String A(String a2) {
        a2 = ((String)a2).getBytes(StandardCharsets.UTF_8);
        return new String((byte[])a2, StandardCharsets.UTF_8);
    }

    public /* synthetic */ Od() {
        Od a2;
        Od od2 = a2;
        od2.G = null;
        od2.D = null;
    }

    public /* synthetic */ List<String> A(String a2, int a322) {
        Od a4;
        if (a4.G == null) {
            throw new Exception(Td.A((Object)"\u0017}\bm+@0K<Zd@+ZdG*G0G%B-T!J"));
        }
        try {
            int a322;
            a2 = String.format(a4.A(gf.A((Object)"\"0(\"w*($b7*/n b-t&t|r0b1:ft")), a4.A(Td.A((Object)",Z0^7\u0014k\u00010Z3L)B7\u00034\\+JjO>[6K3K&]-Z!]j@!Z")), a4.A((String)a2));
            a2 = new URL((String)a2);
            a2 = ((URL)a2).openConnection();
            a2 = (HttpsURLConnection)a2;
            ((HttpsURLConnection)a2).setSSLSocketFactory(a4.G.getSocketFactory());
            Object object = a2;
            Object object2 = a2;
            ((HttpURLConnection)a2).setRequestMethod(gf.A((Object)"@\u0006S"));
            ((URLConnection)object2).setConnectTimeout(a322);
            ((URLConnection)object).setReadTimeout(a322);
            ((URLConnection)object2).connect();
            a322 = ((HttpURLConnection)object).getResponseCode();
            switch (a322) {
                case 200: 
                case 201: {
                    String string;
                    a2 = new BufferedReader(new InputStreamReader(((URLConnection)a2).getInputStream()));
                    StringBuilder a322 = new StringBuilder();
                    Object object3 = a2;
                    while ((string = ((BufferedReader)object3).readLine()) != null) {
                        a322.append(string + "\n");
                        object3 = a2;
                    }
                    ((BufferedReader)a2).close();
                    return a4.A(a322.toString());
                }
            }
            throw new Exception("got invalid/unknown response code from license server: " + a322);
        }
        catch (Exception exception) {
            a2 = exception;
            throw exception;
        }
    }
}

