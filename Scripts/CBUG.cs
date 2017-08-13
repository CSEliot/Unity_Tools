using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// A statically available debugger for on-screen data visualization.
/// Focus on ease-of-use and not optimized.
///  - Eliot Carney-Seim
/// </summary>
///
[RequireComponent (typeof(Text))]
public class CBUG : MonoBehaviour {

    #region Public Unity-Assigned Vars
    public bool ALL_DEBUG_ENABLED;
    public bool SendToConsole;
    public bool DisableOnScreen;
    public float ClearTime;
    public bool Clear;
    public int ClearAmount;
    public bool ClearAll;
    #endregion

    #region Private Vars
    private bool showAnyway;
    private Text logText;
    private LinkedList<string> lines;
    private LinkedList<int> occurrences;
    private LinkedListNode<string> tempLinesIter;
    private LinkedListNode<int> tempOccurIter;
    private bool isParented;
    private float previousClear;
    private bool neverClear;
    private int maxLines = 33; //Tested, based on 24pt Min.
    private int tapsUntilEnable = 10;
    private int currentTaps = 0;
    private bool isTemp;
    #endregion


    // Use this for initialization
    void Awake()
    {
        showAnyway = false;
        logText = GetComponent<Text>();
        lines = new LinkedList<string>();
        occurrences = new LinkedList<int>();
        if (DisableOnScreen)
            logText.color = new Color(0, 0, 0, 0);
        if (ClearTime == 0)
            neverClear = true;
        if (ClearAll)
            ClearAmount = -1;

        transform.tag = "CBUG";
        previousClear = Time.time;
        isTemp = false;
    }

    private CBUG( bool isTemp)
    {
        if (isTemp)
            this.isTemp = true;
    }

    public CBUG()
    {

    }


    void Start()
    {
        showAnyway = (PlayerPrefs.GetInt("CBUG_ON", 0) == 1);
    }

    // Update is called once per frame
    void Update()
    {

        if (!ALL_DEBUG_ENABLED && !showAnyway)
            return;

        if (!Application.isEditor && !showAnyway)
        {
            ALL_DEBUG_ENABLED = false;
            Do("In-Build, CBUG Disabled!");
            return;
        }

        if (Clear)
        {
            Clear = false;
            _ClearLines(ClearAmount);
        }

        if (!isParented && GameObject.Find("CanvasGroup") != null)
        {
            isParented = true;
            GameObject.Find("CanvasGroup").transform.SetParent(transform, true);
        }

        logText.text = "";
        tempLinesIter = lines.First;
        tempOccurIter = occurrences.First;
        for (int x = 0; x < lines.Count; x++)
        {
            logText.text += tempLinesIter.Value + " || " + tempOccurIter.Value + "\n";
            tempLinesIter = tempLinesIter.Next;
            tempOccurIter = tempOccurIter.Next;
        }

        if (lines.Count > maxLines)
        {
            for (int x = 0; x < lines.Count - maxLines; x++)
            {
                lines.RemoveFirst();
                occurrences.RemoveFirst();
            }
        }

        if (!neverClear && Time.time - previousClear > ClearTime)
        {
            Clear = true;
            previousClear = Time.time;
        }
    }


    public void EnableCBUG ()
    {
        currentTaps++;
        if (currentTaps >= tapsUntilEnable)
        {
            if (!showAnyway)
                PlayerPrefs.SetInt("CBUG_ON", 1);
            else
                PlayerPrefs.SetInt("CBUG_ON", 0);

            PlayerPrefs.Save();
            Application.Quit();
        }
    }

    #region Debug Aliases
    public static void Log(string line)
    {
        GetRef()._Print(line);
    }

    public static void Do(string line)
    {
        GetRef()._Print(line);
    }

    public static void Log(string line, bool debugOn)
    {
        GetRef()._Print(line, debugOn);
    }

    public static void Do(string line, bool debugOn)
    {
        GetRef()._Print(line, debugOn);
    }
    #endregion

    #region Helper Functions
    private void _ClearLines(int amount)
    {
        if (lines.Count == 0)
            return;

        if(amount == -1) {
            lines.Clear();
            occurrences.Clear();
        }
        else 
        {
            amount = amount > lines.Count ? lines.Count : amount;
            for(int x = 0; x < amount; x++) {
                lines.RemoveFirst();
                occurrences.RemoveFirst();
            }
        }
    }

    private static CBUG GetRef()
    {
        GameObject myCBUG = GameObject.FindGameObjectWithTag("CBUG");
        if(myCBUG == null)
        {
            return new CBUG(true);
        }
        else
        {
            return myCBUG.GetComponent<CBUG>();
        }

    }

    private void _Print(string line)
    {

        if (isTemp)
        {
            Debug.Log(line);
            return;
        }

        if (!ALL_DEBUG_ENABLED)
            return;

        if(SendToConsole)
            Debug.Log(line);

        if (lines.Find(line) != null) {
            tempLinesIter = lines.First;
            tempOccurIter = occurrences.First;
            for (int x = 0; x < GetRef().lines.Count; x++) {
                if (tempLinesIter.Value == line) {
                    tempOccurIter.Value++;
                    break;
                }
                tempLinesIter = tempLinesIter.Next;
                tempOccurIter = tempOccurIter.Next;
            }
        } else {
            lines.AddLast(line);
            occurrences.AddLast(1);
        }
    }

    private void _Print(string line, bool debugOn)
    {
        if (ALL_DEBUG_ENABLED && debugOn) {
            if (line == null)
                _Print("Null @ " + System.Environment.StackTrace);
            else
                _Print(line);
        }
    }

    private void _Error(string line)
    {
        _Print("ERROR <~> " + line);
        Debug.Log("ERROR <~> " + line);
        logText.color = Color.red;
    }

    private void _SrsError(string line)
    {
        _Error(line);
        throw new System.Exception("ERROR <~> " + line);
    }
    #endregion

    #region Public Static Functions
    /// <summary>
    /// -1 for All lines.
    /// </summary>
    /// <param name="amount"></param>
    public static void ClearLines(int amount)
    {
        GetRef()._ClearLines(amount);
    }

    public static void Print(string line)
    {
        GetRef()._Print(line);
    }

    public static void Print(string line, bool debugOn)
    {
        GetRef()._Print(line, debugOn);
    }

    public static void Error(string line)
    {
        GetRef()._Error(line);
    }

    public static void SrsError(string line)
    {
        GetRef()._SrsError(line);
    }

    public static bool DEBUG_ON
    {
        get {
            return GetRef().ALL_DEBUG_ENABLED;
        }
    }
    #endregion
}
