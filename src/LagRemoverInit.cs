using SpaceWarp.API;
using UnityEngine;
using KSP.Game;
using System.Reflection;
using KSP.IO;

namespace LagRemover
{
    [MainMod]
    public class LagRemoverInit : Mod
    {
        string[] removeList = { "grass", "light", "tree", "emissive", "bush", "decal", "sign", "hvac", "lamp"};
        public override void Initialize()
        {
            UpdatePhysSettings();
            Logger.Info("Mod is initialized");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftControl))
            {
                Logger.Info("Loaded JOINT_RIGIDITY with value of" + PhysicsSettings.JOINT_RIGIDITY);

                Time.fixedDeltaTime = 0.035f;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                RenderSettings.defaultReflectionMode = 0;
                RenderSettings.defaultReflectionResolution = 0;
                
                QualitySettings.antiAliasing = 0;
                QualitySettings.softParticles = false;
                QualitySettings.maximumLODLevel = 4;
                QualitySettings.pixelLightCount = 1;
                QualitySettings.masterTextureLimit = 64;
                KSP.Rendering.Planets.PQS pqs = FindObjectOfType<KSP.Rendering.Planets.PQS>();
                pqs.settings.colliderCullingDistance = 2000;
                pqs.isSubdivisionEnabled = false;
                KSP.Rendering.CelestialBodyGIProbeManager CBGIPmgr = FindObjectOfType<KSP.Rendering.CelestialBodyGIProbeManager>();
                CBGIPmgr.enableGlobalIllumination = false;
                CBGIPmgr.enableCelestialBodyGI = false;
                CBGIPmgr.enableObserverGI = false;


                KSP.Rendering.Planets.AtmosphereScatterManager atmomanager = FindObjectOfType<KSP.Rendering.Planets.AtmosphereScatterManager>();
                Destroy(atmomanager);
                pqs.settings.subdivisionInfo.subdivData.maxLevel = 0;
                pqs.PQSRenderer.MaxSubdivision = 0;
                pqs.PQSRenderer.RenderDepthTexture = false;
                pqs.PQSRenderer.Layer = 0;
                pqs.PQSRenderer.OceanQualitySetting = 0;
                pqs.PQSRenderer.EnableLowQualityLocal = true;
                

                KSP.VolumeCloud.VolumeCloudRenderer[] clouds = FindObjectsOfType<KSP.VolumeCloud.VolumeCloudRenderer>();

                foreach (KSP.VolumeCloud.VolumeCloudRenderer cloud in clouds)
                {
                    cloud.enabled = false;
                }

                KSP.Rendering.PostProcessingSystem ppsys = FindObjectOfType<KSP.Rendering.PostProcessingSystem>();
                ppsys.enabled = false;

                //thanks Plexus du Menton for this ^^
                FieldInfo oceanSphereMaterialField = typeof(KSP.Rendering.Planets.PQSRenderer).GetField("_oceanSpereMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo oceanMaterialField = typeof(KSP.Rendering.Planets.PQSRenderer).GetField("_oceanMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo oceanBackMaterialField = typeof(KSP.Rendering.Planets.PQSRenderer).GetField("_oceanBackSurfaceMaterial", BindingFlags.NonPublic | BindingFlags.Instance);

                oceanSphereMaterialField.SetValue(pqs.PQSRenderer, null);
                oceanMaterialField.SetValue(pqs.PQSRenderer, null);
                oceanBackMaterialField.SetValue(pqs.PQSRenderer, null);

                GameObject[] gos = FindObjectsOfType<GameObject>();
                foreach (GameObject go in gos)
                {
                    if (go.GetComponent<MeshRenderer>() != null)
                    {
                        foreach (string str in removeList)
                        {
                            if (go.name.ToLower().Contains(str))
                            {
                                DestroyImmediate(go);
                                break;
                            }

                        }
                    }/*
                    if (go.GetComponent<Light>())
                    {
                        DestroyImmediate(go.GetComponent<Light>());
                    }*/
                }
                //This is for debugging purposes
                /*
                gos = FindObjectsOfType<GameObject>();
                foreach (GameObject go in gos)
                {
                    if (go.GetComponent<MeshRenderer>() != null)
                    {
                        foreach (string str in removeList)
                        {
                            if (!go.name.Contains(str))
                            {
                                Logger.Info(go.name);
                                break;
                            }    
                        }
                    }
                }*/
            }
        }

        /// <summary>
        /// this forces the Joint Rigidity to use a high value. this helps stabilize rockets and may even increase performance with wobbly rockets.
        /// </summary>
        void UpdatePhysSettings()
        {
            bool _physicsSettingsFileExists = IOProvider.JsonFileExists(IOProvider.DataLocation.Global, "PhysicsSettings");
            Logger.Error("No phyfile exists");
            string filepath = IOProvider.JoinPath(IOProvider.PathOfDataType(IOProvider.DataLocation.Global), "PhysicsSettings") + ".json";
            string loadedFile = IOProvider.ReadAllText(filepath);

            int targetJointRigidity = 1500000;

            int jointrigidityStartIndex = loadedFile.IndexOf("\"JOINT_RIGIDITY\": ");
            string tmpsubstring = loadedFile.Substring(jointrigidityStartIndex, (loadedFile.IndexOf(",", jointrigidityStartIndex)) - jointrigidityStartIndex);
            loadedFile = loadedFile.Replace(tmpsubstring, "\"JOINT_RIGIDITY\": " + targetJointRigidity + ".0");

            IOProvider.WriteAllText(filepath, loadedFile);
            bool physSettingsLoaded = PhysicsSettingsManager.IsPhysicsSettingsLoaded;
        }
    }
}