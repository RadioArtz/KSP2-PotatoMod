using SpaceWarp.API;
using UnityEngine;
using KSP.Game;
namespace LagRemover
{
    [MainMod]
    public class LagRemoverInit : Mod
    {
        string[] removeList = {"grass","light","cloud","tree", "emissive","bush","decals"};
        public override void Initialize()
        {
            Logger.Info("Mod is initialized");
        }

        public void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.antiAliasing = 0;
                QualitySettings.softParticles = false;
                Time.fixedDeltaTime = 0.035f;
                QualitySettings.maximumLODLevel = 4;
                QualitySettings.pixelLightCount = 1;
                QualitySettings.masterTextureLimit = 64;
                KSP.Rendering.Planets.PQS pqs = FindObjectOfType<KSP.Rendering.Planets.PQS>();
                pqs.settings.colliderCullingDistance = 2000;
                pqs.isSubdivisionEnabled = false;
                KSP.Rendering.Planets.AtmosphereScatterManager atmomanager = FindObjectOfType<KSP.Rendering.Planets.AtmosphereScatterManager>();
                Destroy(atmomanager);
                pqs.settings.subdivisionInfo.subdivData.maxLevel = 2;
                pqs.PQSRenderer.OceanQualitySetting = 0;
                pqs.PQSRenderer.EnableLowQualityLocal = true;
                KSP.Rendering.PostProcessingSystem ppsys = FindObjectOfType<KSP.Rendering.PostProcessingSystem>();
                Destroy(ppsys);
                Camera[] cams = Camera.allCameras;
                foreach (Camera cam in cams)
                {
                    if(cam.renderingPath == RenderingPath.DeferredLighting || cam.renderingPath == RenderingPath.DeferredShading)
                    {
                        cam.renderingPath = RenderingPath.VertexLit;
                    }
                }

                GameObject[] gos = FindObjectsOfType<GameObject>();
                foreach (GameObject go in gos)
                {
                    if (go.GetComponent<MeshRenderer>() != null)
                    {
                        foreach(string str in removeList)
                        {
                            if (go.name.Contains(str))
                                Destroy(go);
                        }
                    }
                }
            }
        }
    }
}