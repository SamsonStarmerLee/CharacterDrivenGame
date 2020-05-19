using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SOWPODS
/// </summary>

public class WordFinder : MonoBehaviour
{
    private HashSet<string> wordList;

    private void Awake()
    {
        var r = Resources.Load("enable") as TextAsset;
        var words = r.text.Split(Environment.NewLine.ToCharArray());
        wordList = new HashSet<string>(words);
        Resources.UnloadAsset(r);
    }

    public bool CheckWord(string word)
    {
        return wordList.Contains(word.ToLower());
    }
}
