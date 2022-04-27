using UnityEngine;
using UnityEngine.UI;
 
/// <summary>
/// 使用UI的方式渲染3D模型
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class MeshImage : MaskableGraphic 
{
    public Vector3 overridePostion;
    public Vector3 overrideRotation;
    public Vector3 overrideScale = new Vector3(100f, 100f, 100f);
    public Texture texture;
    public Mesh mesh;
 
 public override Texture mainTexture
    {
        get { return texture; }
    }
 
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (mesh != null)
        {
            vh.Clear();
            //提取Mesh信息
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] UVs = mesh.uv;
            //处理缩放矩阵
            Matrix4x4 matrix4X4 = Matrix4x4.identity;
            matrix4X4.m00 = overrideScale.x;
            matrix4X4.m11 = overrideScale.y;
            matrix4X4.m22 = overrideScale.z;
 
            for (int i = 0; i < vertices.Length; i++)
            {
                //组合UI顶点信息
                UIVertex temp = new UIVertex();
                temp.position = matrix4X4.MultiplyPoint3x4((Quaternion.Euler(overrideRotation) * vertices[i]) + overridePostion);
                temp.uv0 = UVs[i];
                temp.normal = normals[i];
                temp.color = (Color32)color;
                vh.AddVert(temp);
            }
 
            //设置三角形索引
            for (int i = 0; i < triangles.Length; i += 3)
            {
                vh.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            }
        }
    }
 
    //只有编辑器才会执行
    //修改面板属性重新刷新
    protected override void OnValidate()
    {
        base.OnValidate();
        SetMaterialDirty();
        SetVerticesDirty();
    }
}