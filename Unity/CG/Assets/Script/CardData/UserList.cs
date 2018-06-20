using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserList {

    public enum User{
        higashiyamagenji, purple, size
    }

    public static string GetUserName(User userNum) {
        string re = "";
        switch ((User)userNum) {
            case User.higashiyamagenji: { re = "東山源治"; }break;
            case User.purple: { re = "パープル"; }break;
        }
        return re;
    }
    
}
