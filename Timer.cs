using UnityEngine;

public class Timer : MonoBehaviour
{
    public delegate void TimerEventHandler ();
    public event TimerEventHandler TimerStartWork;
    public event TimerEventHandler TimerWorked;
    public event TimerEventHandler TimerEndWork;

    public bool work = false;
    public bool callShow = false;
    private bool preWork = false;
    // Start is called before the first frame update
    void Start()
    {
        TimerWorked += TimerCall;
    }

    public void TimerWork(){
        work = true;
    }

    public void TimerEnd(){
        work = false;
    }

    private void FixedUpdate() {
        if(!work){
            if(!preWork)
                return;
            TimerEndWork?.Invoke();
        }    
        else{
            if(!preWork)
                TimerStartWork?.Invoke();
            TimerWorked?.Invoke();
        }
        preWork = work;
    }

    public virtual void TimerCall(){
        if(callShow)
            Debug.Log("Timer Call");
    }
}