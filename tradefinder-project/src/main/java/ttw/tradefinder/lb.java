/*
 * Decompiled with CFR 0.152.
 * 
 * Could not load the following classes:
 *  org.apache.commons.lang3.tuple.Triple
 *  ttw.tradefinder.B
 *  ttw.tradefinder.Ce
 *  ttw.tradefinder.DB
 *  ttw.tradefinder.RA
 *  ttw.tradefinder.RC
 *  ttw.tradefinder.SB
 *  ttw.tradefinder.UB
 *  ttw.tradefinder.gA
 *  ttw.tradefinder.ic
 *  ttw.tradefinder.lb
 *  ttw.tradefinder.pc
 *  ttw.tradefinder.rH
 *  ttw.tradefinder.xe
 *  ttw.tradefinder.zD
 */
package ttw.tradefinder;

import java.awt.Graphics2D;
import java.awt.RenderingHints;
import java.awt.image.BufferedImage;
import java.text.DecimalFormat;
import java.util.Arrays;
import org.apache.commons.lang3.tuple.Triple;
import ttw.tradefinder.B;
import ttw.tradefinder.Ce;
import ttw.tradefinder.DB;
import ttw.tradefinder.RA;
import ttw.tradefinder.RC;
import ttw.tradefinder.SB;
import ttw.tradefinder.UB;
import ttw.tradefinder.gA;
import ttw.tradefinder.ic;
import ttw.tradefinder.mg;
import ttw.tradefinder.pc;
import ttw.tradefinder.rH;
import ttw.tradefinder.re;
import ttw.tradefinder.sb;
import ttw.tradefinder.xe;
import ttw.tradefinder.zD;

public class lb {
    private final sb g;
    private final pc f;
    private final sb a;
    private final re K;
    private final re m;
    private final gA F = new gA(xe.A((Object)"y\u001dprq7K7Q"), zD.D);
    private final pc e;
    private final pc i;
    private final pc k;
    private final gA I = new gA(Ce.A((Object)"\u0000G\"K?F"), zD.D);
    private static final int G = 4;
    private static final int D = 140;

    public lb(rH a2) {
        a3(a2, 140);
        lb a3;
    }

    public static String A(Object object) {
        Object object2 = object;
        object2 = (String)object2;
        int n2 = ((String)object2).length();
        int n3 = n2 - 1;
        Object a2 = new char[n2];
        int n4 = 2 << 3 ^ 5;
        int cfr_ignored_0 = 2 << 3 ^ (2 ^ 5);
        int n5 = n3;
        int n6 = 2 << 3 ^ 2;
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

    public void A(UB a2) {
        UB uB2;
        lb a3;
        if (Double.isNaN(a2.I) || Double.isNaN(a2.D)) {
            lb lb2 = a3;
            lb2.a.f(Double.NaN);
            lb2.e.A(a2.I, 1.0);
            lb2.i.A(a2.D, 1.0);
            uB2 = a2;
        } else {
            lb lb3 = a3;
            lb3.a.f((a2.I + a2.D) / 2.0);
            lb3.e.A(a2.I, 1.0 / (Math.max(a2.I, a2.D) / a2.I));
            lb3.i.A(a2.D, 1.0 / (Math.max(a2.I, a2.D) / a2.D));
            uB2 = a2;
        }
        if (Double.isNaN(uB2.i) || Double.isNaN(a2.k)) {
            lb lb4 = a3;
            lb4.g.f(Double.NaN);
            lb4.k.A(a2.i, 1.0);
            lb4.f.A(a2.k, 1.0);
            return;
        }
        lb lb5 = a3;
        lb5.g.f(a2.i + a2.k);
        lb5.k.A(a2.i, 1.0 / (Math.max(a2.i, a2.k) / a2.i));
        lb5.f.A(a2.k, 1.0 / (Math.max(a2.i, a2.k) / a2.k));
    }

    public BufferedImage A(RA a2) {
        lb a3;
        lb lb2 = a3;
        Triple triple = lb2.K.A(a2);
        Object object = lb2.m.A(a2);
        triple = Triple.of((Object)Math.max((Integer)triple.getLeft(), (Integer)object.getLeft()), (Object)Math.max((Integer)triple.getMiddle(), (Integer)object.getMiddle()), (Object)Math.max((Integer)triple.getRight(), (Integer)object.getRight()));
        if (a2.k == mg.G) {
            object = new BufferedImage(2 * (Integer)triple.getLeft() + 4, (Integer)triple.getRight(), 2);
            Graphics2D graphics2D = object.createGraphics();
            graphics2D.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
            lb lb3 = a3;
            graphics2D.setRenderingHint(RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_QUALITY);
            lb3.m.A(graphics2D, 0, 0, triple, a2);
            lb3.K.A(graphics2D, (Integer)triple.getLeft() + 4, 0, triple, a2);
            graphics2D.dispose();
            return object;
        }
        object = new BufferedImage((Integer)triple.getLeft(), 2 * (Integer)triple.getRight() + 4, 2);
        Graphics2D graphics2D = object.createGraphics();
        graphics2D.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
        lb lb4 = a3;
        graphics2D.setRenderingHint(RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_QUALITY);
        lb4.m.A(graphics2D, 0, 0, triple, a2);
        lb4.K.A(graphics2D, 0, (Integer)triple.getRight() + 4, triple, a2);
        graphics2D.dispose();
        return object;
    }

    public void A(float a2) {
        lb a3;
        lb lb2 = a3;
        lb2.K.A(a2);
        lb2.m.A(a2);
    }

    public lb(rH a2, int a3) {
        lb a4;
        a4.a = new sb(new DecimalFormat(a2.f(1)));
        a4.g = new sb(new DecimalFormat(a2.f(1)));
        a4.e = new pc(new DecimalFormat(a2.f(1)), new SB(zD.g, 0, 2));
        a4.i = new pc(new DecimalFormat(a2.f(1)), new SB(zD.m, 0, 2));
        a4.k = new pc(new DecimalFormat(a2.f(1)), new SB(zD.m, 0, 2));
        a4.f = new pc(new DecimalFormat(a2.f(1)), new SB(zD.g, 0, 2));
        lb lb2 = a4;
        a4.K = new re(xe.A((Object)"\u0013K5\u0013rq;L'T6T&D"), (B)lb2.a, (B)lb2.F, Arrays.asList(new RC((B)new DB(Ce.A((Object)"c#I"), zD.g), (B)a4.e), new RC((B)new DB(xe.A((Object)"\u007f;Y"), zD.m), (B)a4.i)), a3);
        lb lb3 = a4;
        a4.m = new re(Ce.A((Object)"o1P;G$\u0002\u0006M<\f"), (B)lb3.g, (B)lb3.I, Arrays.asList(new RC((B)new DB(xe.A((Object)"|!V"), zD.m), (B)a4.k), new RC((B)new DB(Ce.A((Object)"`9F"), zD.g), (B)a4.f)), a3);
    }

    public void A(ic a2) {
        lb a3;
        a3.F.f(String.format(xe.A((Object)"y\u001dprq7K7Qr\u0018!\u0010wN"), Integer.toString(a2.D), Integer.toString(a2.I)));
        a3.I.f(String.format(Ce.A((Object)"\u0000G\"K?Fp\u0007#Q"), Integer.toString(a2.G)));
    }
}

