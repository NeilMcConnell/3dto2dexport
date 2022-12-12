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

class WorkItem
{
    public AnimationClip clip;
    public float AnimationTimeSeconds;
    public Quaternion Rotation;
    public string FileName;

    public enum Step
    {
        Setup,
        ExportColor,
        ExportNormal,
        Complete
    }
    public Step CurrentStep;
}


public class ExporterControl : MonoBehaviour
{
    public int FPS;
    public Camera Camera;
    private Camera ExportCamera;
    public GameObject ExportObject;
    public Shader ColorShader;
    public Shader NormalShader;
    private RenderTexture renderTexture;

    public int ExportWidth = 1024;
    public int ExportHeight = 1024;



    //bool ShouldExport = true;
    private string exportPath;


    //AsyncGPUReadbackRequest req;
    //bool reqDone = false;
    //NativeArray<ARGB32> colorInfo;

    private List<WorkItem> workItems;

    // Start is called before the first frame update
    void Start()
    {
        Camera.transform.LookAt(ExportObject.transform);

        GameObject exportCameraGameObject = new GameObject();
        ExportCamera = exportCameraGameObject.AddComponent<Camera>();        
        ExportCamera.CopyFrom(Camera);

        renderTexture = new RenderTexture(ExportWidth, ExportHeight, 32, RenderTextureFormat.ARGB32, 0);

        var animator = ExportObject.GetComponent<Animator>();
        
        Debug.Log("clip info count " + animator.runtimeAnimatorController.animationClips.Length);
        workItems = new List<WorkItem>();
        foreach(AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            Debug.Log(clip.name);
            AddClipToExportList(clip);
        }


        exportPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Exports");

    Directory.CreateDirectory(exportPath);

    }

    private void AddClipToExportList(AnimationClip clip)
    {
        int[] rotations = {0, 45, 90, 135, 180, 225, 270, 315};
        Quaternion originalRotation = ExportObject.transform.rotation;

        int frameCount = Mathf.RoundToInt(clip.length * FPS);
        float frameDuration = clip.length / frameCount;
        if (!clip.isLooping)
            frameCount++;//for non-looping, we want a frame right at the end of the anim
        frameCount = Mathf.Max(frameCount, 1); //make sure we export at least one frame

        Debug.Log($"for clip {clip.name}, exported {frameCount} frames at {1 / frameDuration} FPS (target was {FPS})");

        foreach (int rotation in rotations)
        {
            for (int frame = 0; frame < frameCount; ++frame)
            {
                var work = new WorkItem();
                work.AnimationTimeSeconds = frame * frameDuration;
                work.Rotation = originalRotation * Quaternion.Euler(0, rotation, 0);
                work.clip = clip;
                work.FileName = $"{ExportObject.name}_{clip.name}_R{rotation:D3}_{frame:D3}";
                Debug.Log(work.FileName);
                workItems.Add(work);
            }
        }
    }


    // Update is called once per frame
 //   void OriginalTestUpdate()
 //   {
 //       if (ShouldExport)
 //       {
 //           ShouldExport = false;
 //
 //           RenderTexture.active = renderTexture;
 //           ExportCamera.targetTexture = renderTexture;
 //           //to review - SetReplacementShader, RenderRequest.  Probably not needed, if RenderWithShader can be used from the main thread, I guess?
 //           ExportCamera.RenderWithShader(NormalShader, null);
 //
 //           Texture2D renderedTexture = new Texture2D(ExportWidth, ExportHeight, TextureFormat.ARGB32, false);
 //           renderedTexture.ReadPixels(new Rect(0, 0, ExportWidth, ExportHeight), 0, 0, false);
 //           renderedTexture.Apply();
 //
 //           byte[] tga = renderedTexture.EncodeToTGA();
 //           string path = Path.Combine(  Path.GetDirectoryName(Application.dataPath), "Exports");
 //           string filename = Path.Combine(path, "proofOfConcept2.tga");
 //           Directory.CreateDirectory(path);
 //           Debug.Log(filename);
 //           File.WriteAllBytes(filename, tga);
 //
 //           if (ColorShader == null)
 //           {
 //               ExportCamera.Render();
 //           }
 //           else
 //           {
 //               ExportCamera.RenderWithShader(ColorShader, null);
 //           }
 //           renderedTexture.ReadPixels(new Rect(0, 0, ExportWidth, ExportHeight), 0, 0, false);
 //           renderedTexture.Apply();
 //            tga = renderedTexture.EncodeToTGA();
 //           filename = Path.Combine(path, "proofOfConcept1.tga");
 //           File.WriteAllBytes(filename, tga);
 //
 //
 //           //colorInfo = new NativeArray<ARGB32>(1024 * 1024, Allocator.Persistent);
 //           //req = AsyncGPUReadback.RequestIntoNativeArray(ref colorInfo, renderedTexture);
 //       }
 //
 //
 //
 //       //if (!reqDone && req.done)
 //       //{
 //       //    reqDone = true;
 //       //
 //       //}
 //   }

    private void Update()
    {
        if (workItems.Count == 0)
            return;

        var work = workItems[0];
        switch(work.CurrentStep)
        {
            case WorkItem.Step.Setup:
                {
                    ExportObject.transform.rotation = work.Rotation;
                    var animator = ExportObject.GetComponent<Animator>();
                    animator.PlayInFixedTime(work.clip.name, -1, work.AnimationTimeSeconds);
                    animator.speed = 0;
                    work.CurrentStep++;
                }
                break;
            case WorkItem.Step.ExportColor:
                {

                    RenderTexture.active = renderTexture;
                    ExportCamera.targetTexture = renderTexture;

                    if (ColorShader == null)
                    {
                        ExportCamera.Render();
                    }
                    else
                    {
                        ExportCamera.RenderWithShader(ColorShader, null);
                    }

                    Texture2D renderedTexture = new Texture2D(ExportWidth, ExportHeight, TextureFormat.ARGB32, false);
                    renderedTexture.ReadPixels(new Rect(0, 0, ExportWidth, ExportHeight), 0, 0, false);
                    renderedTexture.Apply();

                    byte[] tga = renderedTexture.EncodeToTGA();
                    string filename = Path.Combine(exportPath, work.FileName +  ".tga");
                    File.WriteAllBytes(filename, tga);
                    work.CurrentStep++;
                }
                break;
            case WorkItem.Step.ExportNormal:
                work.CurrentStep++;
                break;
            case WorkItem.Step.Complete:
                workItems.RemoveAt(0);
                break;
        }

    }

}
