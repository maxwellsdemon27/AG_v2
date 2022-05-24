using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;
using System.Runtime.InteropServices;

public class MultiSimulateWorker : MonoBehaviour
{
    public WorkerState workerState = WorkerState.待命;

    public Camera shotCamera;
    public int workPointer = 0;
    public int workTarget = 10;
    public Text text_State;

    public ShipSettingControl the_SSC;
    public Controller controller;
    public RenderTexture renderTexture;

    public string savePath = "";
    public string folderName = "";
    public string simulate_Info = "";

    public void StartMultiSimulate(){
        workPointer = 0;
        StartCoroutine(CheckWork());
    }

    private static extern void _SavePhoto(string readAddr);

    IEnumerator CheckWork(){
        workPointer++;
        workerState = WorkerState.準備工作;
        text_State.text = workerState.ToString();
        yield return new WaitForSecondsRealtime(0.5f);

        if(workPointer <= workTarget){
            controller.End();
            the_SSC.End();

            StartCoroutine(StartWork());
        }
        else
            StartCoroutine(EndWork());
    }

    IEnumerator StartWork(){
        workerState = WorkerState.開始工作;
        text_State.text = workerState.ToString();

        yield return new WaitForSecondsRealtime(4.0f);
        StartCoroutine(SimulateReady());
    }

    IEnumerator EndWork(){
        workerState = WorkerState.工作完成;
        text_State.text = workerState.ToString();

        yield return new WaitForSecondsRealtime(0.5f);
        workerState = WorkerState.待命;
        text_State.text = workerState.ToString();
    }

    IEnumerator SimulateReady(){
        workerState = WorkerState.模擬準備;
        text_State.text = workerState.ToString();

        the_SSC.RadomInitShips();
     
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(SimulateWork());
    }

    IEnumerator SimulateWork(){
        workerState = WorkerState.模擬中;
        text_State.text = workerState.ToString();

        controller.Work();
        the_SSC.StartSimulator();

        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(EndCheck());
    }

    IEnumerator EndCheck(){
        yield return new WaitForSecondsRealtime(1.0f);
        if(!controller.startSimulator)
            StartCoroutine(SimulateEnd());
        else
            StartCoroutine(EndCheck());
    }

     IEnumerator SimulateEnd(){
        workerState = WorkerState.模擬結束;
        text_State.text = workerState.ToString();

        SimulateRecord();

        #region 存圖
        RenderTexture rt = new RenderTexture(1024,1024,24);
        shotCamera.targetTexture = rt;
        Texture2D targetShot = new Texture2D(1024,1024,TextureFormat.RGB24,false);
        shotCamera.Render();
        RenderTexture.active = shotCamera.targetTexture;
        targetShot.ReadPixels(new Rect(0,0,1024,1024),0,0);
        shotCamera.targetTexture = renderTexture;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] bytes = targetShot.EncodeToPNG();
        string fileName = GetFileName(savePath,folderName);
    
        System.IO.File.WriteAllBytes(fileName,bytes);

        #endregion

        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(CheckWork());
    }

    public void SelectOutputPlace(){
        SelectFolder(StandaloneFileBrowser.OpenFolderPanel("指定輸出資料夾","",false));
    }

    public void  SelectFolder(string[] paths){
        if(paths.Length ==0)
            return;
        savePath = "";
        foreach(var p in paths)
            savePath +=p;
    }

    public static string GetFileName(string path,string folder){
        if(folder == ""){
            System.DateTime date = System.DateTime.Now;
            folder = "SimulateWorks_"+date.Year+"_"+date.Month+"_"+date.Day;
        }
        var folderPath = string.Format("{0}/{1}",path,folder);

        if(!Directory.Exists(folderPath)){
            Debug.Log("Folder not exit! Create!");
            Directory.CreateDirectory(folderPath);
            return string.Format("{0}/SimulateEnd_{1}.png",folderPath,1);
        }else{
            var lastDataSize = Directory.GetFiles(folderPath).Length;
            return string.Format("{0}/SimulateEnd_{1}.png",folderPath,lastDataSize);
        }
    }

    private void SimulateRecord(){
        var R = the_SSC.ships[0].set_R_value;
        simulate_Info = "R:"+R;

        for(int i=0;i<the_SSC.ships.Count;i++){
            simulate_Info += " "+the_SSC.ships[i].ship.shipName+"["+the_SSC.ships[i].set_X_value+","+the_SSC.ships[i].set_Z_value+"]";
        }

        var cv_Find = true;
        if(controller.enmyTarget == null)
            cv_Find = false;

        var cv_No = -1;
        for(int i=0;i<controller.findShips.Count;i++){
            if(controller.findShips[i].guessName == "CVLL")
                cv_No = i +1;
        }

        simulate_Info+= " Length:"+ controller.moveLength / 1000.0f + " Guess:"+controller.predicted_CV+" CV_Find:"+ cv_Find +" CV_No:"+cv_No;

        System.DateTime date = System.DateTime.Now;
        var folder = "SimulateWorks_"+date.Year+"_"+date.Month+"_"+date.Day;

        var folderPath = string.Format("{0}/{1}",savePath,folder);

        if(!Directory.Exists(folderPath)){
            Debug.Log("Folder not exit! Create!");
            Directory.CreateDirectory(folderPath);
        }

        var fileName = string.Format("{0}/{1}",folderPath,"record.txt");

        var file = System.IO.File.AppendText(fileName);
        file.WriteLine(simulate_Info);
        file.Close();
    }

    private void Start(){
        savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
    }
}

public enum WorkerState
{
    待命,準備工作,開始工作,模擬準備,模擬中,模擬結束,工作完成
}