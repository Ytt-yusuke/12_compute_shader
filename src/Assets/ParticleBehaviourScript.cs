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

    int maxParticleNum = ((1024 * 1024 + 31) / 32) * 32;


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

        // 初期化関数の設定
        var initKernel = computeShader.FindKernel("initialize");
        computeShader.SetBuffer(initKernel, "Particles", buffer);
        computeShader.Dispatch(initKernel, maxParticleNum / 32, 1, 1);

        // 更新関数の設定
        updateKernel = computeShader.FindKernel("update");
        computeShader.SetBuffer(updateKernel, "Particles", buffer);

        // 描画用マテリアルの設定
		material = new Material(shader);
        material.SetTexture("_MainTex", mainTexture);
        material.SetBuffer("particles", buffer);
    }

    void OnDisable()
    {
        buffer.Release();
    }

    void Update()
    {
        // 運動
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(updateKernel, maxParticleNum / 32, 1, 1);
    }

    /// <summary>
    /// レンダリング
    /// </summary>
    void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, maxParticleNum);
    }
}
