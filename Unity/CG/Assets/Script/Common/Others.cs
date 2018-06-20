using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//汎用関数
public static class Utility {

    //タグがない場合
    public const string UNTAG = "Untagged";

    //レイヤーが触れる
    public const string TOUCHABLE = "Touchable";

    //オブジェクトとその子を一括でタグ付けする
    public static void SetChildrenTag(Transform t, string TAG) {
        t.tag = TAG;
        foreach (Transform child in t) {
            SetChildrenTag(child, TAG);
        }
    }

    //値fromと値toの間を0～1としたときのratioの値を返す
    public static float GetRatio(float from, float to, float ratio) {
        return from + (to - from) * ratio;
    }
    public static Vector3 GetRatio(Vector3 from, Vector3 to, float ratio) {
        return from + (to - from) * ratio;
    }
    public static Color GetRatio(Color from, Color to, float ratio) {
        return from + (to - from) * ratio;
    }

    //ゲームオブジェクトを生成して返す
    public static GameObject InstantiateWithTransform(GameObject _make, Transform _parent) {
        var a = _parent.transform.transform.transform.position;
        return GameObject.Instantiate(_make,
            _parent.position, _parent.rotation, _parent);
    }

}

//ゲーム中で使われる単語
public static class WORD {

    //領域の名前
    public static string[] zone = { "自陣", "手札", "グリモア", "ブックマーク", "ジャンクヤード", "スタック", "スキマ" };

    //カードに書かれてある情報
    public static string[] color = { "赤色", "橙色", "黄色", "緑色", "青色", "藍色", "紫色" };

    //フェイズの名前
    public static string[] phase = { "スタート", "トランス", "メイン", "エンド" };

}

//ウィンドウのサイズを無理矢理変える
//必要がなくなったらコメントアウトしてね
public class WindowSize {

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad() {
        //画面サイズの初期設定
        //
        Screen.SetResolution(640, 360, false, 60);
        //Screen.SetResolution(1280, 720, false, 60);
    }

}

//メルセンヌツイスタ
public class MT {
    public const int N = 624;
    public const int M = 397;
    int W = 32; int R = 31;
    int U = 11; int S = 7; int T = 15; int L = 18;
    ulong A = 0x9908b0dful;
    ulong B = 0x9d2c5680ul;
    ulong C = 0xefc60000ul;

    ulong U_MASK = 0x80000000ul;
    ulong L_MASK = 0x7ffffffful;

    ulong[] mt = new ulong[N];
    int mti = N + 1;    //

    //コンストラクタ
    public MT() { }
    
    //初期化する
    public void Init(ulong s) {
        mt[0] = s & 0xfffffffful;
        for (mti = 1; mti < N; mti++) {
            mt[mti] = (1812433253ul * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + (ulong)mti);
            mt[mti] &= 0xfffffffful;
        }
    }

    public void Init(List<ulong> v) {
        Init(19650218ul);
        int i = 1;
        int j = 0;
        int k = N > v.Count ? N : v.Count;
        for (; k > 0; --k) {
            mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525ul)) + v[j] + (ulong)j;
            ++i; ++j;
            if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
            if (j >= v.Count) { j = 0; }
        }
        for (k = N - 1; k > 0; --k) {
            mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941ul)) - (ulong)i;
            mt[i] &= 0xfffffffful;
            ++i;
            if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
        }
        mt[0] = 0x80000000ul;
    }
    
    ulong RandInt32() {
        ulong re;
        ulong[] m = { 0x0ul, A };

        if (mti >= N) {
            int i;
            if (mti == N + 1) { Init(5489ul); }
            for (i = 0; i < N - M; ++i) {
                re = (mt[i] & U_MASK) | (mt[i + 1] & L_MASK);
                mt[i] = mt[i + M] ^ (re >> 1) ^ m[re & 0x1UL];
            }
            for (; i < N - 1; ++i) {
                re = (mt[i] & U_MASK) | (mt[i + 1] & L_MASK);
                mt[i] = mt[i + (M - N)] ^ (re >> 1) ^ m[re & 0x1ul];
            }
            re = (mt[N - 1] & U_MASK) | (mt[0] & L_MASK);
            mt[N - 1] = mt[M - 1] ^ (re >> 1) ^ m[re & 0x1ul];

            mti = 0;
        }
        re = mt[mti++];
        re ^= (re >> U);
        re ^= (re << S) & B;
        re ^= (re << T) & C;
        re ^= (re >> L);

        return re;
    }

    //ある範囲の少数値を返す
    public double Float(float min, float max) {
        return min + (max - min) * RandInt32() * (1f / 4294967295);
    }

    //ある範囲の整数値を返す
    public int Int(int min, int exclusive_max) {
        return min + Mathf.Abs((int)RandInt32()) % (exclusive_max - min);
    }

}