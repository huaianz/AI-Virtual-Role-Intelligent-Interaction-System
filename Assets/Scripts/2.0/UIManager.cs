using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI组件引用")]
    public Button selectImageButton;
    public Button resetFaceButton;
    public RawImage previewImage;
    public Text statusText;
    public Text titleText;

    [Header("角色显示引用")]
    public CharacterDisplayManager displayManager;
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button resetViewButton;
    public Slider zoomSlider;

    [Header("管理器引用")]
    public ImageUploadManager uploadManager;

    void Start()
    {
        InitializeUI();
        SetupButtonEvents();
    }

    void InitializeUI()
    {
        // 设置初始状态
        ShowInfo("欢迎使用自动捏脸系统");

        // 设置预览图像占位符
        if (previewImage != null)
        {
            previewImage.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
        }

        // 设置标题
        if (titleText != null)
        {
            titleText.text = "AI自动捏脸";
        }

        // 初始化角色显示控制
        SetupCharacterDisplayControls();

    }

    void SetupButtonEvents()
    {
        // 选择图片按钮
        if (selectImageButton != null)
        {
            selectImageButton.onClick.AddListener(OnSelectImageClicked);
        }

        // 重置面部按钮
        if (resetFaceButton != null)
        {
            resetFaceButton.onClick.AddListener(OnResetFaceClicked);
        }
    }

    void OnSelectImageClicked()
    {
        ShowInfo("正在选择图片...");

        if (uploadManager != null)
        {
            uploadManager.OnSelectImage();
        }
        else
        {
            ShowError("错误: 上传管理器未设置");
        }
    }

    void OnResetFaceClicked()
    {
        ShowInfo("重置面部...");

        if (uploadManager != null)
        {
            uploadManager.OnResetFace();
        }
    }

    public void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log("UI状态: " + message);
    }

    public void UpdatePreviewImage(Texture2D texture)
    {
        if (previewImage != null && texture != null)
        {
            previewImage.texture = texture;
            previewImage.color = Color.white;
        }
    }

    public void ClearPreviewImage()
    {
        if (previewImage != null)
        {
            previewImage.texture = null;
            previewImage.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
        }
    }

    // 显示成功消息
    public void ShowSuccess(string message)
    {
        UpdateStatus("✓ " + message);

        // 可以在这里添加成功动画或音效
        if (statusText != null)
        {
            statusText.color = Color.green;
        }
    }

    // 显示错误消息
    public void ShowError(string message)
    {
        UpdateStatus("✗ " + message);

        if (statusText != null)
        {
            statusText.color = Color.red;
        }
    }

    // 显示普通消息
    public void ShowInfo(string message)
    {
        UpdateStatus(message);

        if (statusText != null)
        {
            statusText.color = Color.black;
        }
    }

    void SetupCharacterDisplayControls()
    {
        // 旋转按钮
        if (rotateLeftButton != null)
            rotateLeftButton.onClick.AddListener(() => RotateCharacter(-45f));

        if (rotateRightButton != null)
            rotateRightButton.onClick.AddListener(() => RotateCharacter(45f));

        if (resetViewButton != null)
            resetViewButton.onClick.AddListener(ResetCharacterView);

        // 缩放滑块
        if (zoomSlider != null)
        {
            zoomSlider.onValueChanged.AddListener(OnZoomChanged);
            zoomSlider.value = 0.5f; // 默认值
        }
    }

    void RotateCharacter(float angle)
    {
        if (displayManager != null)
        {
            displayManager.SetCharacterRotation(angle);
        }
    }

    void ResetCharacterView()
    {
        if (displayManager != null)
        {
            displayManager.ResetCharacterDisplay();
        }
    }

    void OnZoomChanged(float value)
    {
        if (displayManager != null)
        {
            // 将0-1的值映射到1-5的距离范围
            float distance = 1f + value * 4f;
            displayManager.UpdateCameraDistance(distance);
        }
    }

    // 在捏脸成功后聚焦角色
    public void FocusOnCharacter()
    {
        // 可以添加一些视觉反馈，比如闪烁效果
        Debug.Log("聚焦角色显示");
    }
}