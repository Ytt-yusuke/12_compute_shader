using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

struct Particle
{
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 col;
    public float time;
}

public class ParticleBehaviourScript : MonoBehaviour
{
	Material material;// 描画用マテリアル
    [SerializeField]
	Shader shader;// 上記のマテリアル
    [SerializeField]
    Texture mainTexture;// 上記のテクスチャ


    [SerializeField]
    ComputeShader computeShader;
    int updateKernel;
    ComputeBuffer buffer;
    ComputeBuffer buffer_visible;
    ComputeBuffer buffer_args;

    int maxParticleNum = ((1024 * 1024 + 31) / 32) * 32;

    [SerializeField, Range(0,1)]
    float range_X = 1.0f;

    [SerializeField, Range(0,1)]
    float range_Y = 1.0f;

    [SerializeField]
    float initTime = 10.0f;


    /// <summary>
    /// 初期化
    /// </summary>
	void Start () {
	}
    
    void OnEnable()
    {
        // パーティクルの情報を格納するバッファ
        buffer = new ComputeBuffer(
            maxParticleNum, 
            Marshal.SizeOf(typeof(Particle)), 
            ComputeBufferType.Default);

        //可視パーティクル用バッファ
        buffer_visible = new ComputeBuffer(
            maxParticleNum,
            Marshal.SizeOf(typeof(uint)),
            ComputeBufferType.Append);

        //描画数管理バッファ
        buffer_args = new ComputeBuffer(
            1,
            16,
            ComputeBufferType.IndirectArguments);

        // 初期化関数の設定
        var initKernel = computeShader.FindKernel("initialize");
        computeShader.SetBuffer(initKernel, "Particles", buffer);
        computeShader.Dispatch(initKernel, maxParticleNum / 32, 1, 1);

        // 更新関数の設定
        updateKernel = computeShader.FindKernel("update");
        computeShader.SetBuffer(updateKernel, "Particles", buffer);
        computeShader.SetBuffer(updateKernel, "Visibles", buffer_visible);

        // 描画用マテリアルの設定
        material = new Material(shader);
        material.SetTexture("_MainTex", mainTexture);
        material.SetBuffer("particles", buffer);
        material.SetBuffer("visibles", buffer_visible);
    }

    void OnDisable()
    {
        buffer.Release();
        buffer_visible.Release();
        buffer_args.Release();
    }

    void Update()
    {
        int[] args_init = new int[] { 0, 1, 0, 0 };
        buffer_args.SetData(args_init);
        buffer_visible.SetCounterValue(0);

        // 運動
        Matrix4x4 mV = Camera.main.worldToCameraMatrix;
        Matrix4x4 mP = Camera.main.projectionMatrix;
        Matrix4x4 mVP = mP * mV;
        computeShader.SetMatrix("mVP", mVP);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(updateKernel, maxParticleNum / 32, 1, 1);
        ComputeBuffer.CopyCount(buffer_visible, buffer_args, 0);

        computeShader.SetFloat("range_X", range_X);
        computeShader.SetFloat("range_Y", range_Y);
        computeShader.SetFloat("initTime", initTime);

    }

    /// <summary>
    /// レンダリング
    /// </summary>
    void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Points, buffer_args, 0);
    }
}
