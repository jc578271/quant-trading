/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  ttw.tradefinder.JE
 *  ttw.tradefinder.MF
 *  ttw.tradefinder.ZI
 *  ttw.tradefinder.dD
 *  ttw.tradefinder.nF
 *  ttw.tradefinder.vE
 *  ttw.tradefinder.zf
 *  velox.api.layer1.common.Log
 */
package ttw.tradefinder;

import java.io.ByteArrayInputStream;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import javax.sound.sampled.AudioInputStream;
import javax.sound.sampled.AudioSystem;
import javax.sound.sampled.Clip;
import javax.sound.sampled.LineListener;
import ttw.tradefinder.MF;
import ttw.tradefinder.ZI;
import ttw.tradefinder.dD;
import ttw.tradefinder.nF;
import ttw.tradefinder.vE;
import ttw.tradefinder.zf;
import velox.api.layer1.common.Log;

public class JE {
    private boolean i;
    private zf k;
    private Clip I;
    private static Lock G = new ReentrantLock();
    private final ExecutorService D;

    public void A(vE a2) {
        JE a3;
        if (a2.k == dD.I) {
            return;
        }
        if (a3.i) {
            return;
        }
        byte[] byArray = a3.k.A(a2);
        a2 = byArray;
        if (byArray == null) {
            return;
        }
        if (!G.tryLock()) {
            return;
        }
        a3.D.execute(() -> a3.A((byte[])a2));
        G.unlock();
    }

    public JE() {
        JE a2;
        JE jE2 = a2;
        a2.k = new zf();
        jE2.i = false;
        jE2.I = null;
        jE2.D = Executors.newSingleThreadExecutor();
    }

    /*
     * Enabled aggressive block sorting
     * Enabled unnecessary exception pruning
     * Enabled aggressive exception aggregation
     */
    private /* synthetic */ void A(byte[] a222) {
        block9: {
            JE a3;
            if (!G.tryLock()) {
                return;
            }
            if (a3.I != null && a3.I.isActive()) {
                JE jE2 = a3;
                jE2.I.stop();
                jE2.I.flush();
                jE2.I = null;
            }
            try {
                Object a222;
                a3.I = AudioSystem.getClip();
                a3.I.addLineListener((LineListener)new nF(a3));
                a222 = AudioSystem.getAudioInputStream(new ByteArrayInputStream((byte[])a222));
                try {
                    JE jE3 = a3;
                    jE3.I.open((AudioInputStream)a222);
                    jE3.I.setFramePosition(0);
                    jE3.I.start();
                    if (a222 == null) break block9;
                }
                catch (Throwable throwable) {
                    Throwable throwable2;
                    if (a222 != null) {
                        try {
                            ((AudioInputStream)a222).close();
                            throwable2 = throwable;
                            throw throwable2;
                        }
                        catch (Throwable a222) {
                            throwable.addSuppressed(a222);
                        }
                    }
                    throwable2 = throwable;
                    throw throwable2;
                }
                ((AudioInputStream)a222).close();
            }
            catch (Exception a222) {
                Log.error((String)MF.A((Object)"@{Qn0d_b^s"), (Throwable)a222);
            }
        }
        G.unlock();
    }

    public void A() {
        JE a2;
        a2.i = true;
        try {
            JE jE2 = a2;
            jE2.D.shutdownNow();
            a2.D.shutdown();
            jE2.D.awaitTermination(3L, TimeUnit.SECONDS);
        }
        catch (Exception exception) {
            Log.error((String)ZI.A((Object)"A#v:j9`j@2`)p>j8"), (Throwable)exception);
        }
        G.lock();
        if (a2.I != null && a2.I.isActive()) {
            JE jE3 = a2;
            jE3.I.stop();
            jE3.I.flush();
        }
        a2.I = null;
        G.unlock();
    }
}

