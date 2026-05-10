using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text;

public class ConvertSkinToBlendShapes : MonoBehaviour {

    public class BlendShapeData
    {
        public SkinnedMeshRenderer OriginalSMR;
        public SkinnedMeshRenderer BakedSMR;
        public Mesh BakedMesh;

        public Vector3[] WorldVertices;
        public Vector3[] Normals;
        public Vector4[] Tangents;


        public Vector3[] DeltaVertices;
        public Vector3[] DeltaNormals;
        public Vector3[] DeltaTangents;
    }
    public class AnimatedValue
    {
        public GameObject SourceNode;
        public string TargetName;
        public string Type = "AVT_NODE_TRANSFORM";
        public List<Keyframe> KeyFrames = new List<Keyframe>();
        //public Matrix4x4 DefaultMat;
        public Vector3 DefaultTrans;
        public Quaternion DefaultRot;
        public Vector3 DefaultScale;
    }
    static void GatherNodes(GameObject Root, List<AnimatedValue> Values)
    {
        AnimatedValue NewAnimatedValue = new AnimatedValue();
        NewAnimatedValue.SourceNode = Root;
        NewAnimatedValue.TargetName = Root.name;
        //NewAnimatedValue.DefaultMat = GO.transform.localToWorldMatrix;
        NewAnimatedValue.DefaultTrans = Root.transform.localPosition;
        NewAnimatedValue.DefaultRot = Root.transform.localRotation;
        NewAnimatedValue.DefaultScale = Root.transform.localScale;
        Values.Add(NewAnimatedValue);

        for (int i = 0; i < Root.transform.childCount; i++)
        {
            GameObject Child = Root.transform.GetChild(i).gameObject;
            GatherNodes(Child, Values);
        }
    }
    static void ResetTransforms(GameObject Root, List<AnimatedValue> Values)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            AnimatedValue AV = Values[i];
            AV.SourceNode.transform.localPosition = AV.DefaultTrans;
            AV.SourceNode.transform.localRotation = AV.DefaultRot;
            AV.SourceNode.transform.localScale = AV.DefaultScale;
        }
    }

    public bool ApplyToAllMeshes = false;
    public List<float> AllBlendShapes;
    public List<Mesh> BakedMeshes;

    public AnimationClip IdleAnimation;
    public float IdleSampleTime = 0.5f;
    public float TargetSampleTime = 0.5f;
    AnimationClip GetClip( AnimationClip[] Clips, string Name )
    {
        for(int i=0; i<Clips.Length; i++)
        {
            AnimationClip AC = Clips[i];
            if (AC.name == Name)
                return AC;
        }

        return null;
    }
    

    public BlendShapeData GetBlendShapeData(List<BlendShapeData> BlendShapeDataArray, string Name )
    {
        for(int i=0;i < BlendShapeDataArray.Count; i++)
        {
            BlendShapeData BSD = BlendShapeDataArray[i];
            if ( BSD.OriginalSMR && BSD.OriginalSMR.sharedMesh && BSD.OriginalSMR.sharedMesh.name == Name )
            {
                return BSD;
            }
        }

        return null;
    }
    public static Quaternion ExtractRotation( Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition( Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 ExtractScale( Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
    public static void FromMatrix(ref Transform transform, Matrix4x4 matrix)
    {
        transform.localScale = ExtractScale(matrix);
        transform.rotation = ExtractRotation(matrix);
        transform.position = ExtractPosition(matrix);
    }
    public void ResetBindPoses( List<BlendShapeData> BlendShapeDataArray)
    {
        for (int i = 0; i < BlendShapeDataArray.Count; i++)
        {
            BlendShapeData BSD_A = BlendShapeDataArray[i];
            SkinnedMeshRenderer SMR = BSD_A.OriginalSMR;
            var BindPoses = SMR.sharedMesh.bindposes;
            Transform[] Bones = SMR.bones;
            for (int u = 0; u < Bones.Length; u++)
            {
                Transform transform = Bones[u].transform;
                Matrix4x4 matrix = BindPoses[u];

                //transform.localScale = ExtractScale(matrix);
                //transform.rotation = ExtractRotation(matrix);
                //transform.position = ExtractPosition(matrix);
                
                //Bones[u].transform = BindPoses[u];
                BindPoses[u] = Bones[u].worldToLocalMatrix;// * SMR.worldToLocalMatrix;
            }
            SMR.sharedMesh.bindposes = BindPoses;
        }
    }
    public BlendShapeData GetAnimatedData(SkinnedMeshRenderer SMR )
    {
        BlendShapeData NewBlendShapeData = new BlendShapeData();
        NewBlendShapeData.OriginalSMR = SMR;
        Mesh BakedMesh = new Mesh();
        SMR.BakeMesh(BakedMesh);

        BakedMeshes.Add(BakedMesh);

        NewBlendShapeData.BakedMesh = BakedMesh;
        NewBlendShapeData.WorldVertices = BakedMesh.vertices;
        NewBlendShapeData.Normals = BakedMesh.normals;
        NewBlendShapeData.Tangents = BakedMesh.tangents;

        BakedMesh.name = SMR.gameObject.name;

        return NewBlendShapeData;
    }
    public void GetBlendShapesData(GameObject GO, ref List<BlendShapeData> BlendShapeDataArray, bool ClearBlendShapes )
    {
        SkinnedMeshRenderer SMR = GO.GetComponent<SkinnedMeshRenderer>();
        if ( SMR != null && SMR.enabled)
        {
            BlendShapeData NewBlendShapeData = GetAnimatedData( SMR );
            
            BlendShapeDataArray.Add(NewBlendShapeData);
            if (ClearBlendShapes)
            {
                SMR.sharedMesh.ClearBlendShapes();                
            }

            bool ReplaceMesh = true;
            if (ReplaceMesh)
            {
                Material[] SharedMaterials = SMR.sharedMaterials;

                //SMR.enabled = false;

                GameObject ClonedGO = new GameObject();
                ClonedGO.transform.parent = GO.transform;
                ClonedGO.name = GO.name;// + "(Without Skinning)";

                SkinnedMeshRenderer NewSMR = ClonedGO.AddComponent<SkinnedMeshRenderer>();

                NewSMR.sharedMesh = NewBlendShapeData.BakedMesh;
                NewSMR.sharedMaterials = SharedMaterials;
                NewSMR.name = SMR.name;
                NewBlendShapeData.BakedMesh.name += "\\idle";
                NewBlendShapeData.BakedSMR = NewSMR;
                return;
            }
        }
        for (int i=0; i<GO.transform.childCount; i++)
        {
            GameObject Child = GO.transform.GetChild(i).gameObject;
            GetBlendShapesData( Child, ref BlendShapeDataArray, ClearBlendShapes );
        }
    }
    public void CalculateBlendShape( ref List<BlendShapeData> BlendShapeData1, ref List<BlendShapeData> BlendShapeData2, string Name )
    {
        for(int i= 0;i< BlendShapeData1.Count; i++)
        {
            BlendShapeData BSD_A = BlendShapeData1[i];
            BlendShapeData BSD_B = GetBlendShapeData( BlendShapeData2, BSD_A.OriginalSMR.name );
            if ( BSD_B != null )
            {
                if (BSD_A.WorldVertices.Length != BSD_B.WorldVertices.Length)
                    continue;

                BSD_B.DeltaVertices = new Vector3[BSD_B.WorldVertices.Length];
                BSD_B.DeltaTangents = new Vector3[BSD_B.WorldVertices.Length];
                BSD_B.DeltaNormals = new Vector3[BSD_B.WorldVertices.Length];

                for (int u = 0; u < BSD_A.WorldVertices.Length; u++)
                {
                    BSD_B.DeltaVertices[u] = BSD_B.WorldVertices[u] - BSD_A.WorldVertices[u];
                    BSD_B.DeltaTangents[u] = BSD_B.Tangents[u] - BSD_A.Tangents[u];
                    BSD_B.DeltaNormals[u] = BSD_B.Normals[u] - BSD_A.Normals[u];
                }

                int Index = BSD_B.BakedSMR.sharedMesh.GetBlendShapeIndex(Name);

                if (Index == -1)
                    BSD_B.BakedSMR.sharedMesh.AddBlendShapeFrame(Name, 100.0f, BSD_B.DeltaVertices, BSD_B.DeltaNormals, BSD_B.DeltaTangents);
                else
                    Index = Index;
            }            
        }
    }
    List<BlendShapeData> InitialBlendShapeData = new List<BlendShapeData>();
    //[MenuItem("CustomTools/SkinToBlendshape")]
    public void Convert( GameObject GO )
    {
        GameObject AnimatorOwner = GO;
        Animator A = GO.GetComponent<Animator>();
        if (A == null)
        {
            A = this.gameObject.GetComponent<Animator>();
            AnimatorOwner = this.gameObject;
        }
        
        var Controller = A.runtimeAnimatorController;

        AnimationClip[] Clips = Controller.animationClips;

        if (IdleAnimation == null)
        {
            Debug.LogError("Please assign an IdleAnimation to this script");
            return;
        }
        
        AnimationClip IdleFrame = GetClip(Clips, IdleAnimation.name );
        
        IdleFrame.SampleAnimation(AnimatorOwner, IdleSampleTime );
        
        List<AnimatedValue> InitialValues = new List<AnimatedValue>();
        GatherNodes( GO, InitialValues );

        GetBlendShapesData(GO, ref InitialBlendShapeData, true );
        //ResetTransforms(this.gameObject, InitialValues);
        //ResetBindPoses(InitialBlendShapeData);

        AllBlendShapes.Clear();
        BakedMeshes.Clear();

        for (int i = 0; i < Clips.Length; i++)
        {
            ResetTransforms( this.gameObject, InitialValues );

            AnimationClip Clip = Clips[i];

            List<BlendShapeData> AnimatedBlendShapeData = new List<BlendShapeData>();
            Clip.SampleAnimation(AnimatorOwner, TargetSampleTime );//Clip.length * TargetSampleTime

            for ( int u = 0; u < InitialBlendShapeData.Count; u++ )
            {
                BlendShapeData InitialBSD = InitialBlendShapeData[u];
                BlendShapeData AnimatedBSD = GetAnimatedData( InitialBSD.OriginalSMR );
                AnimatedBSD.BakedSMR = InitialBSD.BakedSMR;
                AnimatedBSD.BakedMesh.name += "\\";
                AnimatedBSD.BakedMesh.name += Clip.name;
                AnimatedBlendShapeData.Add(AnimatedBSD);
            }
            //GetBlendShapesData( GO, ref AnimatedBlendShapeData, false );

            CalculateBlendShape( ref InitialBlendShapeData, ref AnimatedBlendShapeData, Clip.name );
            AllBlendShapes.Add(0);
        }


        for (int u = 0; u < InitialBlendShapeData.Count; u++)
        {
            BlendShapeData InitialBSD = InitialBlendShapeData[u];
            InitialBSD.OriginalSMR.enabled = false;
        }

        bool ExportMeshes = true;
        if ( ExportMeshes )
        {
            for (int u = 0; u < BakedMeshes.Count; u++)
            {
                Mesh mesh = BakedMeshes[u];
                ExportMeshToOBJ(mesh, "Assets/ExportedBlendShapes", mesh.name );
            }
        }
    }
    // Use this for initialization
    void Start ()
    {
        
    }
    void ResetSkinning()
    {
        //ResetTransforms(this.gameObject, InitialValues);
        GetBlendShapesData(this.gameObject, ref InitialBlendShapeData, true);
        ResetBindPoses(InitialBlendShapeData);
    }
    bool Converted = false;
    public void ExportMeshToOBJ(Mesh mesh, string FolderPath, string MeshName )
    {
        System.IO.Directory.CreateDirectory(FolderPath);

        string MeshFolder = FolderPath + "\\" + MeshName;
        int LastSlash = MeshFolder.LastIndexOf("\\");
        MeshFolder = MeshFolder.Substring( 0, LastSlash );

        string[] MeshNameParts = MeshName.Split('\\');

        string BlendShapeName = MeshName;
        if ( MeshNameParts!= null && MeshNameParts.Length > 1)
            BlendShapeName = MeshNameParts[1];
        System.IO.Directory.CreateDirectory( MeshFolder );

        StringBuilder sb = new StringBuilder();

        string MTLFile = "TheMtl.mtl";
        sb.AppendLine("mtllib " + MTLFile);
        sb.AppendLine("g " + BlendShapeName);

        Vector3[] Vertices = mesh.vertices;
        Vector2[] Texcoords = mesh.uv;
        Vector3[] Normals = mesh.normals;
        int[] Indices = mesh.GetIndices( 0 );

        for (int v = 0; v < Vertices.Length; v++)
        {
            sb.AppendLine("v " + Vertices[v].x + " " + Vertices[v].y + " " + Vertices[v].z);
        }
        for (int v = 0; v < Texcoords.Length; v++)
        {
            sb.AppendLine("vt " + Texcoords[v].x + " " + Texcoords[v].y);
        }
        for (int v = 0; v < Normals.Length; v++)
        {
            sb.AppendLine("vn " + Normals[v].x + " " + Normals[v].y + " " + Normals[v].z);
        }

        sb.AppendLine("\n");

        for (int f = 0; f < Indices.Length; f += 3)
        {
            int i1 = Indices[f + 0] + 1;
            int i2 = Indices[f + 1] + 1;
            int i3 = Indices[f + 2] + 1;
            string i1str = i1 + "/" + i1;// + "/" + i1;
            string i2str = i2 + "/" + i2;// + "/" + i2;
            string i3str = i3 + "/" + i3;// + "/" + i3;

            sb.AppendLine("f " + i1str + " " + i2str + " " + i3str);
        }

        string ExportPath = FolderPath + "\\" + MeshName + ".obj";
        System.IO.File.WriteAllText(ExportPath, sb.ToString());

        sb = new StringBuilder();
        sb.AppendLine("newmtl " + "TheMat");
        //sb.AppendLine("map_Kd " + "Tex.png");
        sb.AppendLine("illum 2");
        
        ExportPath = FolderPath + "\\" + MTLFile;
        System.IO.File.WriteAllText(ExportPath, sb.ToString());

        Debug.Log("OBJ Exporterd");
    }
	// Update is called once per frame
	void Update ()
    {
        if (!Converted)
        {
            Convert(this.gameObject);
            //ResetSkinning();
            Converted = true;
        }

        if (!ApplyToAllMeshes)
            return;

		for(int i=0; i< AllBlendShapes.Count; i++)
        {
            for(int u=0; u< InitialBlendShapeData.Count; u++)
            {
                BlendShapeData BSD = InitialBlendShapeData[ u ];

                if ( i < BSD.BakedSMR.sharedMesh.blendShapeCount )
                    BSD.BakedSMR.SetBlendShapeWeight(i, AllBlendShapes[i]);
                else
                    Debug.Log("");
            }
        }
	}
}
