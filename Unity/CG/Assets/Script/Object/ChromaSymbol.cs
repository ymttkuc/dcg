using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChromaSymbol : MonoBehaviour {

    public int[] type = new int[(int)Card.Color.size];    //シンボルの総計
    public List<GameObject> symbols;   //シンボルの実体
    public GameObject emp;

    int[] pre_type = new int[(int)Card.Color.size];
    public Sprite[] pic;
    
    public GameObject num_orig; // 数字のオブジェクト（オリジナル）
    public List<GameObject> num;   //数字のオブジェクト
    public Vector3 num_pos; //数字の位置（シンボルの中心からの相対位置）
    public float num_scl;   //数字の大きさ（シンボルとの相対的大きさ）

    public bool isVisible;
    public bool pre_isVisible;

    public Vector3 directory;    //並べる方向と間隔
    Vector3 pre_directory;

    public float border;    //距離の最大
    float pre_border;

    void Awake() {
        //num_orig.SetActive(true);
        if (symbols != null) {
            foreach (GameObject s in symbols) { Destroy(s); }
        }
        symbols = new List<GameObject>();
        if (num != null) {
            foreach (var n in num) { Destroy(n); }
        }
        num = new List<GameObject>();
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (type != pre_type || directory != pre_directory
           || isVisible != pre_isVisible || border != pre_border) { Reroad(); }
	}

    public void Set(Vector3 _directory, string _layer, int _order) {
        directory = _directory;
    }

    public void Reroad() {

        pre_type = type;
        pre_directory = directory;
        pre_isVisible = isVisible;
        pre_border = border;

        foreach (GameObject s in symbols) { Destroy(s); }
        symbols.Clear();
        
        foreach (GameObject n in num) { Destroy(n); }
        num.Clear();

        if (isVisible) {
            //スタート地点を見つける
            int st = 0;
            for (int i = 0; i < (int)Card.Color.size; ++i) {
                if (type[i] > 0) {
                    if (type[(i + (int)Card.Color.size - 1) % (int)Card.Color.size] == 0) {
                        var a = new List<int>();    //上から数える存在するものの塊
                        var b = new List<int>();    //下から数える存在しないものの塊
                        for (int j = 0; j < (int)Card.Color.size; ++j) {
                            a.Add(0);
                            b.Add(0);
                            for (int k = 0; k < (int)Card.Color.size; ++k) {
                                if (type[(i + j + k + (int)Card.Color.size)
                                    % (int)Card.Color.size] > 0) {
                                    ++a[a.Count - 1];
                                } else { break; }
                            }
                            for (int k = 1; k <= (int)Card.Color.size; ++k) {
                                if (type[(i + j - k + (int)Card.Color.size)
                                    % (int)Card.Color.size] == 0) {
                                    ++b[b.Count - 1];
                                } else { break; }
                            }
                        }
                        bool flag = true;
                        for (int j = 0; j < a.Count; ++j) {
                            if (a[0] < a[j]) { flag = false; break; }
                            if (b[0] < b[j]) { flag = false; break; }
                        }
                        if (flag) { st = i; break; }
                    }
                }
            }

            //シンボルの種類を数える
            int color_have = 0;
            for (int i = 0; i < (int)Card.Color.size; ++i) {
                if (type[i] > 0) {
                    ++color_have;
                }
            }

            //間隔の決定をする
            var vec = directory;
            if (directory.magnitude* (color_have - 1) > border) {
                vec = directory * border / (color_have - 1) / directory.magnitude;
            }

            //配置する
            for (int i = 0, a = 0; i < (int)Card.Color.size; ++i) {
                int b = (st + i) % (int)Card.Color.size;
                
                if (0 < type[b]) {
                    var g = Instantiate(emp, transform.position,
                        transform.rotation, transform);
                    g.transform.localPosition += vec * a++;
                    var sr = g.GetComponent<SpriteRenderer>();
                    sr.sprite = pic[b];
                    sr.sortingLayerName =
                        gameObject.GetComponent<SpriteRenderer>().sortingLayerName;
                    sr.sortingOrder =
                        gameObject.GetComponent<SpriteRenderer>().sortingOrder + i;
                    symbols.Add(g);

                    if (1 < type[b]) {
                        var n = Instantiate(num_orig, g.transform.position + num_pos,
                            g.transform.rotation, g.transform);
                        
                        n.GetComponent<Number>().num = type[b];
                        n.transform.localScale = new Vector3(1, 1, 0) * num_scl;
                        sr = n.GetComponent<SpriteRenderer>();
                        sr.sortingLayerName = gameObject.GetComponent<SpriteRenderer>()
                            .sortingLayerName;
                        sr.sortingOrder = gameObject.GetComponent<SpriteRenderer>()
                            .sortingOrder + (int)Card.Color.size + i;
                        num.Add(n);
                    }
                }
            }
        }
    }
}
