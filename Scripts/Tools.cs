using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A set of easily callable functions built to be used through a singleton in Unity.
/// Current Functionalities:
///  - Delay an animation
///  - Delay a function
///  - Fade a Text object.
/// 
/// Contributors:
///  - Eliot Leo Carney-Seim
///  
/// Further Reading:
/// http://wiki.unity3d.com/index.php/Singleton
/// http://wiki.unity3d.com/index.php/Toolbox
/// </summary>
public class Tools : MonoBehaviour {

    public delegate void VanillaFunction();

    #region Singleton Control
    private int priority;
    #endregion

    protected Tools ()
    {
        //No instantiation!
    }

    // Use this for initialization
    void Start () {
        tag = "Tools";
        ensureSingleton();
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region Public Static Hooks
    /// <summary>
    /// For delaying a function call by a single frame.
    /// </summary>
    /// <param name="Call">Parameter-less function</param>
    /// <param name="Time">How long to delay by. To delay by 1 frame, Time == 0.</param>
    /// <returns></returns>
    public static void DelayFunction(VanillaFunction Call, float Time)
    {
        GetSelf()._delayFunction(Call, Time);
    }

    /// <summary>
    /// Returns Tools class. GetComponent<Tools>() supplies the same functionality.
    /// </summary>
    /// <returns></returns>
    public static Tools GetSelf()
    {
        return GameObject.FindGameObjectWithTag("Tools").GetComponent<Tools>()._getSelf();
    }

    /// <summary>
    /// For delaying an animation call by a single frame.
    /// </summary>
    /// <param name="Call">Animation object</param>
    /// <param name="Time">How long to delay by. To delay by 1 frame, Time == 0.</param>
    /// <returns></returns>
    public static void DelayAnim(Animator anim, float time, 
        string clipName = "",
        string trigger = "",
        float floatParam = -1f,
        int intParam = -1,
        bool boolParam = false,
        bool hasBoolParam = false)
    {
        GetSelf()._delayAnim(anim, time, clipName,
            trigger: trigger,
            floatParam: floatParam,
            intParam: intParam,
            boolParam: boolParam,
            hasBoolParam: hasBoolParam);
    }
    #endregion


    #region Private Core Functionality 
    private IEnumerator __delayAnim(Animator anim, float time, 
        string clipName = "",
        string trigger = "",
        float floatParam = -1f,
        int intParam = -1,
        bool boolParam = false,
        bool hasBoolParam = false)
    {
        if (time <= 0f)
            yield return 0f;
        else
            yield return new WaitForSeconds(time);

        if (clipName != "" && trigger == "")
            CBUG.SrsError("clipName Required!");

        if (hasBoolParam)
            anim.SetBool(clipName, boolParam);
        else if (trigger != "")
            anim.SetTrigger(trigger);
        else if (floatParam != -1f)
            anim.SetFloat(clipName, floatParam);
        else if (intParam != -1)
            anim.SetFloat(clipName, intParam);
    }
    private IEnumerator __DelayFunction(VanillaFunction call, float time)
    {
        if (time <= 0f)
            yield return 0f;
        else
            yield return new WaitForSeconds(time);
        call();
    }

    private Tools _getSelf()
    {
        return this;
    }
    #endregion

    #region Helper Functions
    private void _delayAnim(Animator anim, float time, 
        string clipName = "",
        string trigger = "",
        float floatParam = -1f,
        int intParam = -1,
        bool boolParam = false,
        bool hasBoolParam = false)
    {
        StartCoroutine(__delayAnim(anim, time, clipName, 
            trigger: trigger,
            floatParam: floatParam,
            intParam  : intParam,
            boolParam : boolParam,
            hasBoolParam : hasBoolParam));
    }
    private void _delayFunction(VanillaFunction call, float time)
    {
        StartCoroutine(__DelayFunction(call, time));
    }
    private void _fadeText(Text textToFade, float from, float to, float speed)
    {

    }

    private void ensureSingleton()
    {
        //Give all clones a chance to instantiate themselves.
        _delayFunction(killSelfIfOthersExit, 0f);
    }

    private void killSelfIfOthersExit()
    {
        //All clones will get the list in the same order, applying th
        if (GameObject.FindObjectsOfType(typeof(Tools)).Length > 1) { 
            for (int x = 0; x < GameObject.FindObjectsOfType(typeof(Tools)).Length; x++)
            {
                (GameObject.FindObjectsOfType(typeof(Tools))[x] as Tools).Priority = x;
            }
            _delayFunction(killSelf, 0f);
        }
    }

    private void killSelf()
    {
        if (priority != 0)
            Destroy(gameObject);
    }
    #endregion

    #region Getters and Setters
    public int Priority {
        set {
            priority = value;
        }
    }
    #endregion
}
