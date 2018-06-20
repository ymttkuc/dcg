using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWindow : MonoBehaviour {

    public Vector3 numSmallPosFromCenter;   //数字の場所（手札とグリモア）
    public Vector3 numLargePosFromCenter;   //数字の場所（ブックマークとジャンクヤード）
    public float numSize;   //数字の大きさ

    public Vector3 posHide; //隠れているときの位置
    public Vector3 posView; //見えているときの位置

    public GameObject m_objNum; //数字のオブジェクト
    public GameObject m_objEmp; //空のオブジェクト

    public GameObject m_hand;   //アイコン
    public GameObject m_grimoir;
    public GameObject m_bookmark;
    public GameObject m_junkyard;

    GameObject m_handNum;   //数字
    GameObject m_grimoirNum;
    GameObject m_bookmarkNum;
    GameObject m_junkyardNum;

    int moveFrame;         //0で見えない
    public int movePeriod; //移動に費やす時間
    public bool isView; //隠れているか否か

    private void Awake() {
        m_handNum = Instantiate(m_objNum, m_hand.transform.position + numSmallPosFromCenter,
            m_hand.transform.rotation, m_hand.transform);
        m_grimoirNum = Instantiate(m_objNum, m_grimoir.transform.position + numSmallPosFromCenter,
            m_grimoir.transform.rotation, m_grimoir.transform);
        m_bookmarkNum = Instantiate(m_objNum, m_bookmark.transform.position + numLargePosFromCenter,
            m_bookmark.transform.rotation, m_bookmark.transform);
        m_junkyardNum = Instantiate(m_objNum, m_junkyard.transform.position + numLargePosFromCenter,
            m_junkyard.transform.rotation, m_junkyard.transform);
        m_handNum.transform.localScale = new Vector3(1, 1, 0) * numSize;
        m_grimoirNum.transform.localScale = new Vector3(1, 1, 0) * numSize;
        m_bookmarkNum.transform.localScale = new Vector3(1, 1, 0) * numSize;
        m_junkyardNum.transform.localScale = new Vector3(1, 1, 0) * numSize;

    }

    // Use this for initialization
    void Start () {
        transform.localPosition = posHide;
        isView = false;
        moveFrame = 0;		
	}
	
	// Update is called once per frame
	void Update () {
        if (0 <= moveFrame && moveFrame < movePeriod) {
            var p = posHide;
            var q = posView;
            transform.localPosition = p - (p - q) * moveFrame / (movePeriod - 1);
        }
        if (!isView && moveFrame >= 0) { --moveFrame; }
        if (isView && moveFrame < movePeriod) { ++moveFrame; }
		
	}

    //ステータスの更新
    public void Set(int hand, int gri, int boo, int junk) {
        m_handNum.GetComponent<Number>().num = hand;
        m_grimoirNum.GetComponent<Number>().num = gri;
        m_bookmarkNum.GetComponent<Number>().num = boo;
        m_junkyardNum.GetComponent<Number>().num = junk;

    }
}
