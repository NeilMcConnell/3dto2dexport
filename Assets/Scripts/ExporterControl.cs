using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


struct ARGB32
{
    public byte A, R, G, B;
}


public class ExporterControl : MonoBehaviour
{
    public Camera Camera;
    private Camera ExportCamera;
    public GameObject ExportObject;
    public Shader ColorShader;
    public Shader NormalShader;
    private RenderTexture renderTexture;



    bool ShouldExport = true;



    AsyncGPUReadbackRequest req;
    bool reqDone = false;
    NativeArray<ARGB32> colorInfo;

    // Start is called before the first frame update
    void Start()
    {
        Camera.transform.LookAt(ExportObject.transform);

        GameObject exportCameraGameObject = new GameObject();
        ExportCamera = exportCameraGameObject.AddComponent<Camera>();        
        ExportCamera.CopyFrom(Camera);

        renderTexture = new RenderTexture(1024, 1024, 32, RenderTextureFormat.ARGB32, 0);
    }

    //int frames = 0;

    // Update is called once per frame
    void Update()
    {
        //++frames; //hack for testing
        //float scaledFrames = frames / 1000f;
        //Camera.transform.position = new Vector3(Mathf.Sin(scaledFrames) * 10, 5, Mathf.Cos(scaledFrames) * 10);
        //Camera.transform.LookAt(ExportObject.transform);



        if (ShouldExport)
        {
            ShouldExport = false;

            RenderTexture.active = renderTexture;
            ExportCamera.targetTexture = renderTexture;
            //to review - SetReplacementShader, RenderRequest.  Probably not needed, if RenderWithShader can be used from the main thread, I guess?
            ExportCamera.RenderWithShader(NormalShader, null);

            Texture2D renderedTexture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
            renderedTexture.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0, false);
            renderedTexture.Apply();

            byte[] tga = renderedTexture.EncodeToTGA();
            string path = Path.Combine(  Path.GetDirectoryName(Application.dataPath), "Exports");
            string filename = Path.Combine(path, "proofOfConcept2.tga");
            Directory.CreateDirectory(path);
            Debug.Log(filename);
            File.WriteAllBytes(filename, tga);

            if (ColorShader == null)
            {
                ExportCamera.Render();
            }
            else
            {
                ExportCamera.RenderWithShader(ColorShader, null);
            }
            renderedTexture.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0, false);
            renderedTexture.Apply();
             tga = renderedTexture.EncodeToTGA();
            filename = Path.Combine(path, "proofOfConcept1.tga");
            File.WriteAllBytes(filename, tga);


            //colorInfo = new NativeArray<ARGB32>(1024 * 1024, Allocator.Persistent);
            //req = AsyncGPUReadback.RequestIntoNativeArray(ref colorInfo, renderedTexture);
        }



        //if (!reqDone && req.done)
        //{
        //    reqDone = true;
        //
        //}
    }
}
