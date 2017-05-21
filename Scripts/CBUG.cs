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
    public bool ALL_DEBUG_TOGGLE;
    public bool DebugOnOfficial;
    public bool SendToConsole;
    public bool DisableOnScreen;
    public float ClearTime;
    public bool Clear;
    public int ClearAmount;
    public bool ClearAll;
    #endregion

    #region Private Vars
    private Text logText;
    private LinkedList<string> lines;
    private LinkedList<int> occurrences;
    private LinkedListNode<string> tempLinesIter;
    private LinkedListNode<int> tempOccurIter;
    private bool isParented;
    private float previousClear;
    private bool neverClear;
    private int maxLines = 33; //Tested, based on 24pt Min.
    #endregion


    // Use this for initialization
    void Awake () {
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
	}
	
	// Update is called once per frame
	void Update () {

        if (!ALL_DEBUG_TOGGLE)
            return;

        if (!Debug.isDebugBuild && !DebugOnOfficial) {
            ALL_DEBUG_TOGGLE = false;
            Do("Official Build, Disabled!");
            return;
        }

        if (Clear) {
            Clear = false;
            _ClearLines(ClearAmount);
        }

        if (!isParented && GameObject.Find("CanvasGroup") != null) {
            isParented = true;
            GameObject.Find("CanvasGroup").transform.SetParent(transform, true);
        }

        logText.text = "";
        tempLinesIter = lines.First;
        tempOccurIter = occurrences.First;
        for(int x = 0; x < lines.Count; x++) {
            logText.text += tempLinesIter.Value + " || " + tempOccurIter.Value + "\n";
            tempLinesIter = tempLinesIter.Next;
            tempOccurIter = tempOccurIter.Next;
        }

        if (lines.Count > maxLines) {
            for (int x = 0; x < lines.Count - maxLines; x++) {
                lines.RemoveFirst();
                occurrences.RemoveFirst();
            }
        }

        if(!neverClear && Time.time - previousClear > ClearTime) {
            Clear = true;
            previousClear = Time.time;
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
        return GameObject.FindGameObjectWithTag("CBUG").GetComponent<CBUG>();
    }

    private void _Print(string line)
    {
        if (!ALL_DEBUG_TOGGLE)
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
        if (ALL_DEBUG_TOGGLE && debugOn) {
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
            return GetRef().ALL_DEBUG_TOGGLE;
        }
    }
    #endregion
}
