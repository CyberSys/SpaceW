﻿using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct QuadGenerationConstants
{
    public float planetRadius;
    public float spacing;
    public float terrainMaxHeight;

    public Vector3 cubeFaceEastDirection;
    public Vector3 cubeFaceNorthDirection;
    public Vector3 patchCubeCenter;

    public Vector3 topLeftCorner;
    public Vector3 bottomRightCorner;
    public Vector3 topRightCorner;
    public Vector3 bottomLeftCorner;
    public Vector3 middle;

    public static QuadGenerationConstants Init()
    {
        QuadGenerationConstants temp = new QuadGenerationConstants();

        temp.spacing = QS.nSpacing;
        temp.terrainMaxHeight = 64.0f;

        return temp;
    }
}

[Serializable]
public struct OutputStruct
{
    public float noise;

    public Vector3 patchCenter;

    public Vector4 vcolor;
    public Vector4 pos;
    public Vector4 cpos;
}

public class Quad : MonoBehaviour
{
    public QuadPostion Position;
    public QuadID ID;

    public Planetoid Planetoid;

    public NoiseParametersSetter Setter;

    public ComputeShader HeightShader;

    public ComputeBuffer QuadGenerationConstantsBuffer;
    public ComputeBuffer PreOutDataBuffer;
    public ComputeBuffer OutDataBuffer;
    public ComputeBuffer ToShaderData;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadGenerationConstants quadGC;

    public Quad Parent;
    public Quad OneLODParent;

    public List<Quad> Subquads = new List<Quad>();

    public int LODLevel = -1;

    public bool HaveSubQuads = false;

    public Quad()
    {

    }

    void Start()
    {
        Dispatch();
    }

    void OnDestroy()
    {
        if (ToShaderData != null)
            ToShaderData.Release();
    }

    void OnDrawGizmos()
    {
        /*
        if (!this.HaveSubQuads)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.quadGC.topLeftCorner, 100);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.quadGC.topRightCorner, 100);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.quadGC.bottomLeftCorner, 100);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.quadGC.bottomRightCorner, 100);
        }
        */
    }

    public void SetupCorners(QuadPostion pos)
    {
        float v = this.Planetoid.PlanetRadius / 2;

        switch (pos)
        {
            case QuadPostion.Top:
                this.quadGC.topLeftCorner = new Vector3(-v, v, v);
                this.quadGC.bottomRightCorner = new Vector3(v, v, -v);

                this.quadGC.topRightCorner = new Vector3(v, v, v);
                this.quadGC.bottomLeftCorner = new Vector3(-v, v, -v);
                break;
            case QuadPostion.Bottom:
                this.quadGC.topLeftCorner = new Vector3(-v, -v, -v);
                this.quadGC.bottomRightCorner = new Vector3(v, -v, v);

                this.quadGC.topRightCorner = new Vector3(v, -v, -v);
                this.quadGC.bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPostion.Left:
                this.quadGC.topLeftCorner = new Vector3(-v, v, v);
                this.quadGC.bottomRightCorner = new Vector3(-v, -v, -v);

                this.quadGC.topRightCorner = new Vector3(-v, v, -v);
                this.quadGC.bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPostion.Right:
                this.quadGC.topLeftCorner = new Vector3(v, v, -v);
                this.quadGC.bottomRightCorner = new Vector3(v, -v, v);

                this.quadGC.topRightCorner = new Vector3(v, v, v);
                this.quadGC.bottomLeftCorner = new Vector3(v, -v, -v);
                break;
            case QuadPostion.Front:
                this.quadGC.topLeftCorner = new Vector3(v, v, v);
                this.quadGC.bottomRightCorner = new Vector3(-v, -v, v);

                this.quadGC.topRightCorner = new Vector3(-v, v, v);
                this.quadGC.bottomLeftCorner = new Vector3(v, -v, v);
                break;
            case QuadPostion.Back:
                this.quadGC.topLeftCorner = new Vector3(-v, v, -v);
                this.quadGC.bottomRightCorner = new Vector3(v, -v, -v);

                this.quadGC.topRightCorner = new Vector3(v, v, -v);
                this.quadGC.bottomLeftCorner = new Vector3(-v, -v, -v);
                break;
         }

        this.quadGC.middle = (this.quadGC.topLeftCorner + this.quadGC.bottomRightCorner) / 2;
    }

    public void Init(Vector3 topLeft, Vector3 bottmoRight, Vector3 topRight, Vector3 bottomLeft)
    {
        this.quadGC.topLeftCorner = topLeft;
        this.quadGC.bottomRightCorner = bottmoRight;
        this.quadGC.topRightCorner = topRight;
        this.quadGC.bottomLeftCorner = bottomLeft;

        this.quadGC.middle = (topLeft + bottmoRight) / 2;
    }

    [ContextMenu("Split")]
    public void Split()
    {
        if (this.Subquads.Count != 0)
            Unsplit();

        int id = 0;

        int subdivisions = 2;

        Vector3 size = this.quadGC.bottomRightCorner - this.quadGC.topLeftCorner;
        Vector3 step = size / subdivisions;

        Log("Size: " + size.ToString());

        bool staticX = false, staticY = false, staticZ = false;

        if (step.x == 0)
            staticX = true;
        if (step.y == 0)
            staticY = true;
        if (step.z == 0)
            staticZ = true;

        for (int sY = 0; sY < subdivisions; sY++)
        {
            for (int sX = 0; sX < subdivisions; sX++, id++)
            {
                Vector3 subStart = Vector3.zero, subEnd = Vector3.zero;
                Vector3 subTopRight = Vector3.zero, subBottomLeft = Vector3.zero;

                if (staticX)
                {
                    subStart = new Vector3(this.quadGC.topLeftCorner.x, this.quadGC.topLeftCorner.y + step.y * sY, this.quadGC.topLeftCorner.z + step.z * sX);
                    subEnd = new Vector3(this.quadGC.topLeftCorner.x, this.quadGC.topLeftCorner.y + step.y * (sY + 1), this.quadGC.topLeftCorner.z + step.z * (sX + 1));

                    subTopRight = new Vector3(this.quadGC.topLeftCorner.x, this.quadGC.topLeftCorner.y + step.y * sY, this.quadGC.topLeftCorner.z + step.z * (sX + 1));
                    subBottomLeft = new Vector3(this.quadGC.topLeftCorner.x, this.quadGC.topLeftCorner.y + step.y * (sY + 1), this.quadGC.topLeftCorner.z + step.z * sX);
                }
                if (staticY)
                {
                    subStart = new Vector3(this.quadGC.topLeftCorner.x + step.x * sX, this.quadGC.topLeftCorner.y, this.quadGC.topLeftCorner.z + step.z * sY);
                    subEnd = new Vector3(this.quadGC.topLeftCorner.x + step.x * (sX + 1), this.quadGC.topLeftCorner.y, this.quadGC.topLeftCorner.z + step.z * (sY + 1));

                    subTopRight = new Vector3(this.quadGC.topLeftCorner.x + step.x * (sX + 1), this.quadGC.topLeftCorner.y, this.quadGC.topLeftCorner.z + step.z * sY);
                    subBottomLeft = new Vector3(this.quadGC.topLeftCorner.x + step.x * sX, this.quadGC.topLeftCorner.y, this.quadGC.topLeftCorner.z + step.z * (sY + 1));
                }
                if (staticZ)
                {
                    subStart = new Vector3(this.quadGC.topLeftCorner.x + step.x * sX, this.quadGC.topLeftCorner.y + step.y * sY, this.quadGC.topLeftCorner.z);
                    subEnd = new Vector3(this.quadGC.topLeftCorner.x + step.x * (sX + 1), this.quadGC.topLeftCorner.y + step.y * (sY + 1), this.quadGC.topLeftCorner.z);

                    subTopRight = new Vector3(this.quadGC.topLeftCorner.x + step.x * (sX + 1), this.quadGC.topLeftCorner.y + step.y * sY, this.quadGC.topLeftCorner.z);
                    subBottomLeft = new Vector3(this.quadGC.topLeftCorner.x + step.x * sX, this.quadGC.topLeftCorner.y + step.y * (sY + 1), this.quadGC.topLeftCorner.z);
                }

                Quad quad = Planetoid.SetupSubQuad(Position);
                quad.Init(subStart, subEnd, subTopRight, subBottomLeft);
                quad.Parent = this;
                quad.LODLevel = quad.Parent.LODLevel + 1;
                quad.ID = (QuadID)id;

                if (quad.LODLevel == 1)
                    quad.OneLODParent = quad.Parent;
                else if (quad.LODLevel > 1)
                    quad.OneLODParent = quad.Parent.OneLODParent;

                quad.transform.parent = this.transform;
                quad.gameObject.name += "_ID" + id + "_LOD" + quad.LODLevel;
                quad.SetupVectors(quad, id, staticX, staticY, staticZ);
                quad.Dispatch();

                this.Subquads.Add(quad);
                this.HaveSubQuads = true;

                BufferHelper.ReleaseAndDisposeBuffer(ToShaderData);
            }
        }
    }

    [ContextMenu("Unslpit")]
    public void Unsplit()
    {
        for (int i = 0; i < this.Subquads.Count; i++)
        {
            if(this.Subquads[i].HaveSubQuads)
            {
                this.Subquads[i].Unsplit();
            }

            if (this.Planetoid.Quads.Contains(this.Subquads[i]))
            {
                this.Planetoid.Quads.Remove(this.Subquads[i]);
            }

            if (this.Subquads[i] != null)
            {
                DestroyImmediate(this.Subquads[i].gameObject);
            }
        }

        this.HaveSubQuads = false;
        this.Subquads.Clear();
        this.Dispatch();
    }

    [ContextMenu("Displatch!")]
    public void Dispatch()
    {
        float time = Time.realtimeSinceStartup;

        if (ToShaderData != null)
            ToShaderData.Release();

        if (Setter != null)
        {
            Setter.LoadAndInit();
            Setter.SetUniforms(HeightShader);
        }

        QuadGenerationConstants[] quadGenerationConstantsData = new QuadGenerationConstants[] { quadGC, quadGC }; //Here we add 2 equal elements in to the buffer data, and nex we will set buffer size to 1. Bugfix. Idk.
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        QuadGenerationConstantsBuffer = new ComputeBuffer(1, 144);
        PreOutDataBuffer = new ComputeBuffer(QS.nVerts, 64);
        OutDataBuffer = new ComputeBuffer(QS.nVerts, 64);
        ToShaderData = new ComputeBuffer(QS.nVerts, 64);

        HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);
        NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);

        QuadGenerationConstantsBuffer.SetData(quadGenerationConstantsData);
        PreOutDataBuffer.SetData(outputStructData);
        OutDataBuffer.SetData(outputStructData);

        int kernel1 = HeightShader.FindKernel("CSMainNoise");
        int kernel2 = HeightShader.FindKernel("CSTexturesMain");

        SetupComputeShader(kernel1);

        Log("Buffers for first kernel ready!");

        HeightShader.Dispatch(kernel1,
        QS.THREADGROUP_SIZE_X_REAL,
        QS.THREADGROUP_SIZE_Y_REAL,
        QS.THREADGROUP_SIZE_Z_REAL);

        Log("First kernel ready!");

        SetupComputeShader(kernel2);

        Log("Buffers for second kernel ready!");

        HeightShader.Dispatch(kernel2,
        QS.THREADGROUP_SIZE_X_REAL,
        QS.THREADGROUP_SIZE_Y_REAL,
        QS.THREADGROUP_SIZE_Z_REAL);

        Log("Second kernel ready!");

        OutDataBuffer.GetData(outputStructData);
        ToShaderData.SetData(outputStructData);

        if (Setter != null)
        {
            Setter.MaterialToUpdate.SetBuffer("data", ToShaderData);
            Setter.MaterialToUpdate.SetTexture("_HeightTexture", HeightTexture);
            Setter.MaterialToUpdate.SetTexture("_NormalTexture", NormalTexture);
        }

        BufferHelper.ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, OutDataBuffer);

        Log("Dispatched in " + (Time.realtimeSinceStartup - time).ToString() + "ms");
    }

    private void SetupComputeShader(int kernel)
    {
        HeightShader.SetBuffer(kernel, "quadGenerationConstants", QuadGenerationConstantsBuffer);
        HeightShader.SetBuffer(kernel, "patchPreOutput", PreOutDataBuffer);
        HeightShader.SetBuffer(kernel, "patchOutput", OutDataBuffer);
        HeightShader.SetTexture(kernel, "Height", HeightTexture);
        HeightShader.SetTexture(kernel, "Normal", NormalTexture);
    }

    public Vector3 GetCubeFaceEastDirection(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = this.Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPostion.Left:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPostion.Right:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPostion.Front:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPostion.Back:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
        }

        return temp;
    }

    public Vector3 GetCubeFaceNorthDirection(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = this.Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
            case QuadPostion.Left:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPostion.Right:
                temp = new Vector3(0.0f, 0.0f, r);
                break;
            case QuadPostion.Front:
                temp = new Vector3(0.0f, -r, 0);
                break;
            case QuadPostion.Back:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenter(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = this.Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(0.0f, r, 0.0f);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPostion.Left:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
            case QuadPostion.Right:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPostion.Front:
                temp = new Vector3(0.0f, 0.0f, r);
                break;
            case QuadPostion.Back:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenterSplitted(QuadPostion quadPosition, int id, bool staticX, bool staticY, bool staticZ)
    {
        Vector3 temp = Vector3.zero;

        float v = this.Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                if (id == 0)
                    temp += new Vector3(-v / 2, v, v / 2);
                else if (id == 1)
                    temp += new Vector3(v / 2, v, v / 2);
                else if (id == 2)
                    temp += new Vector3(-v / 2, v, -v / 2);
                else if (id == 3)
                    temp += new Vector3(v / 2, v, -v / 2);
                break;
            case QuadPostion.Bottom:
                if (id == 0)
                    temp += new Vector3(-v / 2, -v, -v / 2);
                else if (id == 1)
                    temp += new Vector3(v / 2, -v, -v / 2);
                else if (id == 2)
                    temp += new Vector3(-v / 2, -v, v / 2);
                else if (id == 3)
                    temp += new Vector3(v / 2, -v, v / 2);
                break;
            case QuadPostion.Left:
                if (id == 0)
                    temp += new Vector3(-v, v / 2, v / 2);
                else if (id == 1)
                    temp += new Vector3(-v, v / 2, -v / 2);
                else if (id == 2)
                    temp += new Vector3(-v, -v / 2, v / 2);
                else if (id == 3)
                    temp += new Vector3(-v, -v / 2, -v / 2);
                break;
            case QuadPostion.Right:
                if (id == 0)
                    temp += new Vector3(v, v / 2, -v / 2);
                else if (id == 1)
                    temp += new Vector3(v, v / 2, v / 2);
                else if (id == 2)
                    temp += new Vector3(v, -v / 2, -v / 2);
                else if (id == 3)
                    temp += new Vector3(v, -v / 2, v / 2);
                break;
            case QuadPostion.Front:
                if (id == 0)
                    temp += new Vector3(v / 2, v / 2, v);
                else if (id == 1)
                    temp += new Vector3(-v / 2, v / 2, v);
                else if (id == 2)
                    temp += new Vector3(v / 2, -v / 2, v);
                else if (id == 3)
                    temp += new Vector3(-v / 2, -v / 2, v);
                break;
            case QuadPostion.Back:
                if (id == 0)
                    temp += new Vector3(-v / 2, v / 2, -v);
                else if (id == 1)
                    temp += new Vector3(v / 2, v / 2, -v);
                else if (id == 2)
                    temp += new Vector3(-v / 2, -v / 2, -v);
                else if (id == 3)
                    temp += new Vector3(v / 2, -v / 2, -v);
                break;
        }

        float tempStatic = 0;

        if (staticX)
            tempStatic = temp.x;
        if (staticY)
            tempStatic = temp.y;
        if (staticZ)
            tempStatic = temp.z;

        //WARNING!!! Magic! Ya, it works...
        if (this.LODLevel >= 1)
        {
            if(this.LODLevel == 1)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * 2.0f, 0.5f); //0.5f
            else if (this.LODLevel == 2)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * 1.33333333333f, 0.75f); //0.5f + 0.5f / 2.0f
            else if (this.LODLevel == 3)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 13.125f), 0.875f); //0.75f + ((0.5f / 2.0f) / 2.0f)
            else if (this.LODLevel == 4)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 14.0625f), 0.9375f); //0.875f + (((0.5f / 2.0f) / 2.0f) / 2.0f)
            else if (this.LODLevel == 5)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 14.53125f), 0.96875f); //0.9375f + ((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (this.LODLevel == 6)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 14.765625f), 0.984375f); //0.96875f + (((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
        }
        //End of magic here.

        if (staticX)
            temp.x = tempStatic;
        if (staticY)
            temp.y = tempStatic;
        if (staticZ)
            temp.z = tempStatic;

        temp = new Vector3(Mathf.RoundToInt(temp.x), Mathf.RoundToInt(temp.y), Mathf.RoundToInt(temp.z)); //Just make sure that our values is rounded...

        return temp;
    }

    public void SetupVectors(Quad quad, int id, bool staticX, bool staticY, bool staticZ)
    {
        Vector3 cfed = Parent.quadGC.cubeFaceEastDirection / 2;
        Vector3 cfnd = Parent.quadGC.cubeFaceNorthDirection / 2;

        quad.quadGC.cubeFaceEastDirection = cfed;
        quad.quadGC.cubeFaceNorthDirection = cfnd;
        quad.quadGC.patchCubeCenter = quad.GetPatchCubeCenterSplitted(quad.Position, id, staticX, staticY, staticZ);
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}