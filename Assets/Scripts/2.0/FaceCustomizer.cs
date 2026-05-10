using UnityEngine;

public class FaceCustomizer : MonoBehaviour
{
    [Header("角色面部网格")]
    public SkinnedMeshRenderer faceMesh;

    [Header("BlendShape名称")]
    public string faceWidthShape = "Face_Width";
    public string faceLengthShape = "Face_Length";
    public string eyeSizeShape = "Eyes_Size";
    public string noseSizeShape = "Nose_Size";
    public string mouthSizeShape = "Mouth_Size";

    private FaceParams currentParams;

    void Start()
    {
        if (faceMesh == null)
        {
            // 自动查找面部网格
            faceMesh = FindFaceMesh();
        }

        if (faceMesh == null)
        {
            Debug.LogError("未找到面部网格，请手动设置");
        }

        currentParams = new FaceParams();

        // 初始化为默认面部
        ApplyFaceParams(currentParams);
    }

    public void ApplyFaceParams(FaceParams newParams)
    {
        currentParams = newParams;

        if (faceMesh == null)
        {
            Debug.LogError("面部网格未设置");
            return;
        }

        SetBlendShapeWeight(faceWidthShape, currentParams.faceWidth * 100f);
        SetBlendShapeWeight(faceLengthShape, currentParams.faceLength * 100f);
        SetBlendShapeWeight(eyeSizeShape, currentParams.eyeSize * 100f);
        SetBlendShapeWeight(noseSizeShape, currentParams.noseSize * 100f);
        SetBlendShapeWeight(mouthSizeShape, currentParams.mouthSize * 100f);

        Debug.Log("面部参数应用完成");
    }

    public void ResetFace()
    {
        // 重置所有BlendShape
        if (faceMesh != null)
        {
            for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
            {
                faceMesh.SetBlendShapeWeight(i, 0f);
            }
        }

        currentParams = new FaceParams();
        Debug.Log("面部已重置");
    }

    void SetBlendShapeWeight(string shapeName, float weight)
    {
        if (faceMesh == null) return;

        for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
        {
            string currentName = faceMesh.sharedMesh.GetBlendShapeName(i);
            if (currentName.ToLower().Contains(shapeName.ToLower()))
            {
                faceMesh.SetBlendShapeWeight(i, weight);
                break;
            }
        }
    }

    public FaceParams GetCurrentParams()
    {
        return currentParams;
    }

    SkinnedMeshRenderer FindFaceMesh()
    {
        // 自动查找带有BlendShape的面部网格
        SkinnedMeshRenderer[] renderers = FindObjectsOfType<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0)
            {
                return renderer;
            }
        }
        return null;
    }
}