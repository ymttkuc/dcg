using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Number : MonoBehaviour {

    const string FILE_NAME = "Picture/Number";    //ファイル名
                                                  //使いたいものはResourcesフォルダに入れる
    
    public GameObject emp;  //空のオブジェクトの情報
    public bool isImage;

    public int num; //表示番号
    public float alpha;   //不透明度

    public bool isBuff;   //強化なら緑色
    public bool isDebuff; //弱体なら赤色

    public bool isVisible;  //見えるかどうか
    bool pre_isVisible;

    float width_blank = 1.0f;

    //[SerializedField]
    Sprite[] pic;

    List<GameObject> child = new List<GameObject>();
    int pre_num;
    bool pre_isBuff;
    bool pre_isDeuff;
    float pre_alpha;

    private void Awake() {
        isVisible = true;
    }

    // Use this for initialization
    void Start () {
        
        pre_isVisible = !isVisible;
        alpha = 1f;

        //Resourceフォルダ（特殊フォルダ）の中にある
        //画像を読み込む
        //sr = gameObject.GetComponent<SpriteRenderer>();
        pic = Resources.LoadAll<Sprite>(FILE_NAME);    //数字の画像を取得
        
        NumUpdate();
        

    }
	
	// Update is called once per frame
	void Update () {
        if (pre_num != num || pre_isBuff != isBuff ||
            pre_isDeuff != isDebuff || pre_alpha != alpha
            ||pre_isVisible != isVisible) { NumUpdate(); }
	}
    
    public void NumUpdate() {
        int hage = num;
        for (int i = 0; i < child.Count; ++i) { Destroy(child[i]); }
        child.Clear();

        if (isVisible) {

            bool isMinus = false;
            if (hage < 0) { hage *= -1; isMinus = true; }
            List<int> n = new List<int>();
            while (hage != 0) { n.Add((hage % 10 + 9) % 10); hage /= 10; }
            if (isMinus) { n.Add(10); }
            if (n.Count == 0) { n.Add(9); }

            if (isBuff && !isDebuff) {
                for (int i = 0; i < n.Count; ++i) { n[i] += 22; }
            } else if (!isBuff && isDebuff) {
                for (int i = 0; i < n.Count; ++i) { n[i] += 11; }
            }

            for (int i = 0; i < n.Count; ++i) {
                GameObject obj = Instantiate(emp, new Vector3(0, 0, 0), Quaternion.identity);
                obj.SetActive(true);

                Vector3 w = new Vector3() {
                    x = transform.lossyScale.x / Mathf.Sqrt(n.Count),
                    y = transform.lossyScale.y / Mathf.Sqrt(n.Count)
                };
                obj.transform.localScale = w;

                float s = ((float)(n.Count - 1) / 2 - i) * obj.transform.lossyScale.x * width_blank;
                obj.transform.localPosition = transform.position + new Vector3(s, 0, 0);

                obj.transform.localEulerAngles = transform.eulerAngles;
                obj.transform.SetParent(transform);
                if (isImage) {
                    var im = obj.gameObject.GetComponent<Image>();
                    im.preserveAspect = true;
                    var c = im.color; c.a = alpha; im.color = c;
                    if (pic != null) { im.sprite = pic[n[i]]; }
                } else {
                    var sr = obj.gameObject.GetComponent<SpriteRenderer>();
                    sr.sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder;
                    sr.sortingLayerID = gameObject.GetComponent<SpriteRenderer>().sortingLayerID;
                    var c = sr.color; c.a = alpha; sr.color = c;
                    if (pic != null) { sr.sprite = pic[n[i]]; }
                }
                child.Add(obj);
            }
        }
        pre_num = num;
        pre_isBuff = isBuff;
        pre_isDeuff = isDebuff;
        pre_alpha = alpha;
        pre_isVisible = isVisible;
    }
}
