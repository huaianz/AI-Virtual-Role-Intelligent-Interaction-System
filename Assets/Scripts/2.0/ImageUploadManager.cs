using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ImageUploadManager : MonoBehaviour
{
    [Header("组件引用")]
    public FaceRecognitionController recognitionController;
    public FaceCustomizer faceCustomizer;
    public UIManager uiManager;

    private Texture2D currentTexture;

    void Start()
    {
        // 绑定事件
        if (recognitionController != null)
        {
            recognitionController.OnFaceRecognized += OnFaceRecognized;
            recognitionController.OnError += OnRecognitionError;
        }

        // 初始化UI状态
        if (uiManager != null)
        {
            uiManager.ShowInfo("准备就绪");
        }
    }

    void OnDestroy()
    {
        if (recognitionController != null)
        {
            recognitionController.OnFaceRecognized -= OnFaceRecognized;
            recognitionController.OnError -= OnRecognitionError;
        }
    }

    // 选择图片按钮调用
    public void OnSelectImage()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("选择人脸图片", "", "jpg,png,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            LoadImageFromPath(path);
        }
#else
        // 移动端可以在这里集成相册选择
        LoadTestImage();
#endif
    }

    void LoadImageFromPath(string path)
    {
        if (File.Exists(path))
        {
            byte[] imageData = File.ReadAllBytes(path);
            ProcessImageData(imageData);
        }
        else
        {
            if (uiManager != null) uiManager.ShowError("文件不存在");
        }
    }

    // 测试用 - 加载资源图片
    void LoadTestImage()
    {
        Texture2D testTexture = Resources.Load<Texture2D>("TestFace");
        if (testTexture != null)
        {
            byte[] imageData = testTexture.EncodeToJPG();
            ProcessImageData(imageData);
        }
        else
        {
            if (uiManager != null) uiManager.ShowError("未找到测试图片，请在Resources文件夹放置TestFace.jpg");
        }
    }

    void ProcessImageData(byte[] imageData)
    {
        // 创建纹理并显示预览
        currentTexture = new Texture2D(2, 2);
        if (currentTexture.LoadImage(imageData))
        {
            if (uiManager != null)
            {
                uiManager.UpdatePreviewImage(currentTexture);
                uiManager.ShowInfo("正在识别人脸...");
            }

            // 开始识别
            if (recognitionController != null)
            {
                recognitionController.RecognizeFace(imageData);
            }
            else
            {
                if (uiManager != null) uiManager.ShowError("人脸识别控制器未设置");
            }
        }
        else
        {
            if (uiManager != null) uiManager.ShowError("图片加载失败");
        }
    }

    void OnFaceRecognized(FaceParams faceParams)
    {
        if (uiManager != null)
        {
            uiManager.ShowSuccess("识别成功！正在生成角色...");
        }

        if (faceCustomizer != null)
        {
            faceCustomizer.ApplyFaceParams(faceParams);

            if (uiManager != null)
            {
                uiManager.ShowSuccess("角色生成完成！");
            }
        }
        else
        {
            if (uiManager != null) uiManager.ShowError("面部定制器未设置");
        }
    }

    void OnRecognitionError(string error)
    {
        if (uiManager != null)
        {
            uiManager.ShowError("识别失败: " + error);
        }
    }

    // 重置按钮调用
    public void OnResetFace()
    {
        if (faceCustomizer != null)
        {
            faceCustomizer.ResetFace();
        }

        if (uiManager != null)
        {
            uiManager.ShowInfo("面部已重置");
            uiManager.ClearPreviewImage();
        }
    }
}