using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FaceRecognitionController : MonoBehaviour
{
    [Header("百度AI配置")]
    public string apiKey = "你的API_KEY";
    public string secretKey = "你的SECRET_KEY";

    private string accessToken;
    private bool isInitialized = false;

    public System.Action<FaceParams> OnFaceRecognized;
    public System.Action<string> OnError;

    void Start()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return StartCoroutine(GetAccessToken());
        isInitialized = true;
        Debug.Log("人脸识别控制器初始化完成");
    }

    IEnumerator GetAccessToken()
    {
        string url = $"https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={apiKey}&client_secret={secretKey}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                TokenData tokenData = JsonUtility.FromJson<TokenData>(request.downloadHandler.text);
                accessToken = tokenData.access_token;
                Debug.Log("Token获取成功");
            }
            else
            {
                Debug.LogError("Token获取失败: " + request.error);
                OnError?.Invoke("Token获取失败");
            }
        }
    }

    public void RecognizeFace(byte[] imageData)
    {
        if (!isInitialized)
        {
            OnError?.Invoke("系统未初始化完成");
            return;
        }

        StartCoroutine(RecognizeFaceCoroutine(imageData));
    }

    IEnumerator RecognizeFaceCoroutine(byte[] imageData)
    {
        string base64Image = System.Convert.ToBase64String(imageData);
        string url = $"https://aip.baidubce.com/rest/2.0/face/v3/detect?access_token={accessToken}";

        // 构建请求数据
        string requestData = "{\"image\":\"" + base64Image + "\",\"image_type\":\"BASE64\",\"face_field\":\"beauty,gender,face_shape\"}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessRecognitionResult(request.downloadHandler.text);
            }
            else
            {
                string errorMsg = "识别失败: " + request.error;
                Debug.LogError(errorMsg);
                OnError?.Invoke(errorMsg);
            }
        }
    }

    void ProcessRecognitionResult(string jsonResult)
    {
        try
        {
            SimpleFaceData faceData = JsonUtility.FromJson<SimpleFaceData>(jsonResult);

            if (faceData.error_code == 0 && faceData.result.face_list.Length > 0)
            {
                FaceParams faceParams = ConvertToFaceParams(faceData.result.face_list[0]);
                OnFaceRecognized?.Invoke(faceParams);
            }
            else
            {
                string errorMsg = $"识别错误: {faceData.error_msg}";
                OnError?.Invoke(errorMsg);
            }
        }
        catch (System.Exception e)
        {
            string errorMsg = $"数据解析失败: {e.Message}";
            OnError?.Invoke(errorMsg);
        }
    }

    FaceParams ConvertToFaceParams(FaceInfo faceInfo)
    {
        FaceParams faceParams = new FaceParams();

        // 简单的参数映射
        faceParams.faceWidth = Mathf.Clamp(faceInfo.location.width / 200f, 0.3f, 0.8f);
        faceParams.faceLength = Mathf.Clamp(faceInfo.location.height / 250f, 0.4f, 0.9f);
        faceParams.eyeSize = Mathf.Clamp((float)faceInfo.beauty / 100f, 0.3f, 0.8f);

        // 根据性别调整
        if (faceInfo.gender.type == "male")
        {
            faceParams.noseSize = 0.7f;
            faceParams.mouthSize = 0.6f;
        }
        else
        {
            faceParams.noseSize = 0.5f;
            faceParams.mouthSize = 0.5f;
        }

        // 根据脸型微调
        switch (faceInfo.face_shape.type)
        {
            case "square": faceParams.faceWidth += 0.1f; break;
            case "round": faceParams.faceLength -= 0.1f; break;
        }

        return faceParams;
    }
}