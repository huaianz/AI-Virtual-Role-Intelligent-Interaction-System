// CharacterDisplayManager.cs
using UnityEngine;

public class CharacterDisplayManager : MonoBehaviour
{
    [Header("角色显示设置")]
    public GameObject characterModel;
    public Camera characterCamera;
    public Transform characterPivot;
    public Light characterLight;

    [Header("显示控制")]
    public Vector3 characterPosition = new Vector3(0, -1, 2);
    public Vector3 characterRotation = new Vector3(0, 180, 0);
    public float cameraDistance = 2.5f;
    public float cameraHeight = 1.2f;

    [Header("动画设置")]
    public float rotationSpeed = 10f;
    public bool autoRotate = true;

    void Start()
    {
        InitializeCharacterDisplay();
    }

    void Update()
    {
        if (autoRotate && characterPivot != null)
        {
            characterPivot.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    void InitializeCharacterDisplay()
    {
        // 确保角色模型存在
        if (characterModel == null)
        {
            characterModel = FindCharacterInScene();
        }

        // 设置角色位置和旋转
        if (characterModel != null)
        {
            characterModel.transform.position = characterPosition;
            characterModel.transform.eulerAngles = characterRotation;

            // 创建旋转支点
            CreateCharacterPivot();
        }

        // 设置角色摄像机
        SetupCharacterCamera();

        // 设置灯光
        SetupLighting();

        Debug.Log("角色显示系统初始化完成");
    }

    GameObject FindCharacterInScene()
    {
        // 查找可能的角色模型
        SkinnedMeshRenderer[] renderers = FindObjectsOfType<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0)
            {
                Debug.Log($"找到带BlendShape的角色: {renderer.gameObject.name}");
                return renderer.gameObject;
            }
        }

        // 查找Animator
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators)
        {
            if (animator.isHuman)
            {
                Debug.Log($"找到人形角色: {animator.gameObject.name}");
                return animator.gameObject;
            }
        }

        Debug.LogWarning("未找到合适的角色模型，请手动指定");
        return null;
    }

    void CreateCharacterPivot()
    {
        if (characterPivot != null) return;

        // 创建旋转支点
        GameObject pivot = new GameObject("CharacterPivot");
        pivot.transform.position = characterPosition;

        // 将角色设置为支点的子对象
        characterModel.transform.SetParent(pivot.transform);
        characterModel.transform.localPosition = Vector3.zero;

        characterPivot = pivot.transform;
    }

    void SetupCharacterCamera()
    {
        if (characterCamera == null)
        {
            // 创建或查找角色摄像机
            GameObject camObj = GameObject.Find("CharacterCamera");
            if (camObj == null)
            {
                camObj = new GameObject("CharacterCamera");
                characterCamera = camObj.AddComponent<Camera>();
            }
            else
            {
                characterCamera = camObj.GetComponent<Camera>();
            }
        }

        // 设置摄像机属性
        characterCamera.transform.position = characterPosition + new Vector3(0, cameraHeight, -cameraDistance);
        characterCamera.transform.LookAt(characterPosition + new Vector3(0, cameraHeight * 0.5f, 0));

        characterCamera.fieldOfView = 45f;
        characterCamera.clearFlags = CameraClearFlags.SolidColor;
        characterCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

        // 设置摄像机为子摄像机（如果已经有主摄像机）
        Camera.main.depth = 0;
        characterCamera.depth = 1;

        // 设置视口矩形（在屏幕右侧显示）
        characterCamera.rect = new Rect(0.6f, 0.1f, 0.35f, 0.8f);
    }

    void SetupLighting()
    {
        if (characterLight == null)
        {
            GameObject lightObj = GameObject.Find("CharacterLight");
            if (lightObj == null)
            {
                lightObj = new GameObject("CharacterLight");
                characterLight = lightObj.AddComponent<Light>();
            }
            else
            {
                characterLight = lightObj.GetComponent<Light>();
            }
        }

        characterLight.type = LightType.Directional;
        characterLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        characterLight.intensity = 1f;
        characterLight.shadows = LightShadows.Soft;
    }

    // 重置角色显示
    public void ResetCharacterDisplay()
    {
        if (characterPivot != null)
        {
            characterPivot.rotation = Quaternion.identity;
        }

        if (characterModel != null)
        {
            characterModel.transform.localPosition = Vector3.zero;
            characterModel.transform.localRotation = Quaternion.identity;
        }
    }

    // 设置角色旋转
    public void SetCharacterRotation(float yRotation)
    {
        if (characterPivot != null)
        {
            characterPivot.eulerAngles = new Vector3(0, yRotation, 0);
        }
    }

    // 切换自动旋转
    public void ToggleAutoRotate(bool enable)
    {
        autoRotate = enable;
    }

    // 更新摄像机距离
    public void UpdateCameraDistance(float distance)
    {
        cameraDistance = Mathf.Clamp(distance, 1f, 5f);
        if (characterCamera != null)
        {
            Vector3 cameraPos = characterPosition + new Vector3(0, cameraHeight, -cameraDistance);
            characterCamera.transform.position = cameraPos;
        }
    }

    // 显示/隐藏角色
    public void SetCharacterVisible(bool visible)
    {
        if (characterModel != null)
        {
            characterModel.SetActive(visible);
        }
        if (characterCamera != null)
        {
            characterCamera.gameObject.SetActive(visible);
        }
    }

    // 获取当前角色引用
    public GameObject GetCharacterModel()
    {
        return characterModel;
    }
}