using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    [Header("UI按钮")]
    public Button selectImageButton;
    public Button resetFaceButton;

    [Header("管理器引用")]
    public ImageUploadManager uploadManager;

    void Start()
    {
        // 绑定按钮事件
        if (selectImageButton != null)
            selectImageButton.onClick.AddListener(OnSelectImageClick);

        if (resetFaceButton != null)
            resetFaceButton.onClick.AddListener(OnResetFaceClick);
    }

    void OnSelectImageClick()
    {
        if (uploadManager != null)
        {
            uploadManager.OnSelectImage();
        }
    }

    void OnResetFaceClick()
    {
        if (uploadManager != null)
        {
            uploadManager.OnResetFace();
        }
    }
}