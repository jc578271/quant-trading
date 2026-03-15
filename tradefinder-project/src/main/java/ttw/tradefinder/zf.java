/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.Fa
 *  ttw.tradefinder.fD
 *  ttw.tradefinder.me
 *  ttw.tradefinder.sD
 *  ttw.tradefinder.uE
 *  ttw.tradefinder.vE
 *  ttw.tradefinder.zf
 *  velox.api.layer1.layers.utils.SoundSynthHelper
 */
package ttw.tradefinder;

import java.io.BufferedInputStream;
import java.io.ByteArrayOutputStream;
import java.io.FileInputStream;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Map;
import javax.sound.sampled.AudioFileFormat;
import javax.sound.sampled.AudioInputStream;
import javax.sound.sampled.AudioSystem;
import ttw.tradefinder.Fa;
import ttw.tradefinder.fD;
import ttw.tradefinder.me;
import ttw.tradefinder.sD;
import ttw.tradefinder.uE;
import ttw.tradefinder.vE;
import velox.api.layer1.layers.utils.SoundSynthHelper;

public class zf {
    private Map<String, byte[]> D = new HashMap();

    public zf() {
        zf a2;
    }

    private /* synthetic */ byte[] f(String a2) {
        zf a3;
        if (a3.D.containsKey(a2)) {
            return (byte[])a3.D.get(a2);
        }
        try {
            return a3.A((String)a2, (InputStream)new BufferedInputStream(Fa.class.getClassLoader().getResourceAsStream((String)a2)));
        }
        catch (Exception exception) {
            a2 = exception;
            exception.printStackTrace();
            return null;
        }
    }

    private /* synthetic */ byte[] A(String a2) {
        zf a3;
        if (a3.D.containsKey(a2)) {
            return (byte[])a3.D.get(a2);
        }
        try {
            return a3.A((String)a2, (InputStream)new BufferedInputStream(new FileInputStream((String)a2)));
        }
        catch (Exception exception) {
            a2 = exception;
            exception.printStackTrace();
            return null;
        }
    }

    private /* synthetic */ byte[] A(String a2, InputStream a3) {
        zf a4;
        ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream(40960);
        if (a3 != null) {
            a3 = AudioSystem.getAudioInputStream(a3);
            AudioSystem.write((AudioInputStream)a3, AudioFileFormat.Type.WAVE, byteArrayOutputStream);
            a4.D.put(a2, byteArrayOutputStream.toByteArray());
            return (byte[])a4.D.get(a2);
        }
        a4.D.put(a2, null);
        return null;
    }

    /*
     * Enabled aggressive block sorting
     */
    private /* synthetic */ byte[] A(sD a2) {
        switch (uE.G[a2.ordinal()]) {
            case 1: {
                zf a3;
                return a3.f(me.A((Object)"4425#(h:3/\"532(5i,&-"));
            }
            case 2: {
                zf a3;
                return a3.f(fD.A((Object)"eJcKrV9KyQ\u007fC\u007fFwQ\u007fJx\u000baD`"));
            }
            case 3: {
                zf a3;
                return a3.f(me.A((Object)"((.)?4t42 5&7i,&-"));
            }
            case 4: {
                zf a3;
                return a3.f(fD.A((Object)"eJcKrV9FyKpLdHwQ\u007fJx\u000baD`"));
            }
            case 5: {
                zf a3;
                return a3.f(me.A((Object)"((.)?4t7).8\"7\"-\"7i,&-"));
            }
            case 6: {
                zf a3;
                return a3.f(fD.A((Object)"VyPxAe\neLz@xFs\u000baD`"));
            }
        }
        return null;
    }

    /*
     * Enabled aggressive block sorting
     */
    public byte[] A(vE a2) {
        zf a3;
        switch (uE.D[a2.k.ordinal()]) {
            case 1: {
                return a3.A(a2.D);
            }
            case 2: {
                if (a2.I.isEmpty()) {
                    return a3.A(sD.F);
                }
                return a3.A(a2.I);
            }
            case 3: 
            case 4: {
                if (a2.I.isEmpty()) {
                    return a3.A(sD.F);
                }
                return SoundSynthHelper.synthesize((String)a2.I);
            }
        }
        return a3.A(sD.F);
    }
}

