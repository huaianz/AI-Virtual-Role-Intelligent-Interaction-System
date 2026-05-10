using System;
using UnityEngine;

[Serializable]
public class TokenData
{
    public string access_token;//百度令牌
}

[Serializable]
public class SimpleFaceData
{
    public int error_code;//错误码
    public string error_msg;//错误描述
    public FaceResult result;//人脸数组
}

[Serializable]
public class FaceResult
{
    public FaceInfo[] face_list;//可能检测到多张人脸，用一个数组来保存
}

[Serializable]
public class FaceInfo//单个角色信息
{
    public Location location;
    public double beauty;
    public Gender gender;
    public FaceShape face_shape;
}

[Serializable]
public class Location
{
    public int width;
    public int height;
}

[Serializable]
public class Gender
{
    public string type;
}

[Serializable]
public class FaceShape
{
    public string type;
}

[Serializable]
public class FaceParams
{
    public float faceWidth = 0.5f;
    public float faceLength = 0.5f;
    public float eyeSize = 0.5f;
    public float noseSize = 0.5f;
    public float mouthSize = 0.5f;
}