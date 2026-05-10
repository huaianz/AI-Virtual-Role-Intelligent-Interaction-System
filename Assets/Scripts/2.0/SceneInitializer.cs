using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        // 确保必要的组件存在
        EnsureComponent<FaceRecognitionController>();
        EnsureComponent<FaceCustomizer>();
        EnsureComponent<ImageUploadManager>();
        EnsureComponent<UIManager>();

        // 自动连接引用
        AutoConnectReferences();

        Debug.Log("场景初始化完成");
    }

    void EnsureComponent<T>() where T : Component
    {
        if (FindObjectOfType<T>() == null)
        {
            gameObject.AddComponent<T>();
            Debug.Log($"已添加 {typeof(T).Name} 组件");
        }
    }

    void AutoConnectReferences()
    {
        // 获取所有必要的组件
        UIManager uiManager = FindObjectOfType<UIManager>();
        ImageUploadManager uploadManager = FindObjectOfType<ImageUploadManager>();
        FaceRecognitionController recognitionController = FindObjectOfType<FaceRecognitionController>();
        FaceCustomizer faceCustomizer = FindObjectOfType<FaceCustomizer>();

        // 自动连接引用
        if (uiManager != null && uploadManager != null)
        {
            uiManager.uploadManager = uploadManager;
            uploadManager.uiManager = uiManager;
            Debug.Log("已连接 UI管理器 和 上传管理器");
        }

        if (uploadManager != null && recognitionController != null)
        {
            uploadManager.recognitionController = recognitionController;
            Debug.Log("已连接 上传管理器 和 人脸识别控制器");
        }

        if (uploadManager != null && faceCustomizer != null)
        {
            uploadManager.faceCustomizer = faceCustomizer;
            Debug.Log("已连接 上传管理器 和 面部定制器");
        }
    }
}