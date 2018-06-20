using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test01 : MonoBehaviour {

    Vector3 click_base; //クリックの始点
    Quaternion pre_angle; //クリックする直前のアングル
                          //(x,y,z,w)を保存する
                          //アングルは4次元

	// Use this for initialization
	void Start () {
	}

    // Update is called once per frame
    void Update()
    {

        //マウスが押された瞬間
        //0:左クリック
        //1:右クリック
        //2:ホイールクリック
        if (Input.GetMouseButtonDown(0))
        {
            //Vector3 Input.mousePosition.x
            //マウスの座標を表示する
            //x:0←→Screen.width
            //y:0↓↑Screen.height
            click_base = Input.mousePosition;
            pre_angle = transform.rotation;
        }


        //マウスが押されているとき
        if (Input.GetMouseButton(0)){

            //回転（加算）
            //Xの量,Yの量,Zの量
            /*
            transform.Rotate(
                Input.mousePosition.x - click_base.x,
                Input.mousePosition.y - click_base.y,
                0
            );
            */

            //回転（更新）
            //Xの量,Yの量,Zの量

            //以下はエラーが起こる
            //transform.rotation.x = pre_angle.x + Input.mousePosition.x - click_base.x;
            //transform.rotation.y = pre_angle.y + Input.mousePosition.y - click_base.y;
            //transform.rotation.xに直接データを入れるとエラーが生じる
            //Vector3 v, transform.rotation = v
            //と置くことで回避

            //Debug.Log("C : " + click_base.x + " , " + click_base.y);
            //Debug.Log("P : " + pre_angle.x + " , " + pre_angle.y + " , " + pre_angle.z + " , " + pre_angle.w);
            
            //角度には
            //・オイラー角表示(Vector3)
            //・クォータニオン表示(Quaternion)
            //の2種類がある
            //外部ではオイラー角で表示され
            //内部ではクォータニオンで処理される

            //オイラー角で角度を用意
            Vector3 v1 = pre_angle.eulerAngles;
            Vector3 v2;
            v2.x = (float)(Input.mousePosition.y - click_base.y) * 360 / Screen.height;
            v2.y = 0;
            v2.z = (float)(Input.mousePosition.x - click_base.x) * 360 / Screen.width;
            
            //オイラー角をクォータニオンに変換する
            transform.rotation = Quaternion.Euler(v1 - v2);

        }

        //マウスを離した瞬間
        if (Input.GetMouseButtonUp(0)) {
            pre_angle = transform.rotation;
        }
        
    }
}
