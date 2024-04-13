using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SetupLiteRP : MonoBehaviour
{
    public RenderPipelineAsset currentPipeLineAsset;
    private void OnEnable()
    {
        GraphicsSettings.defaultRenderPipeline = currentPipeLineAsset;
    }

    private void OnValidate()
    {
        GraphicsSettings.defaultRenderPipeline = currentPipeLineAsset;
    }
}
