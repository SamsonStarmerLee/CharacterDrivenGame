using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordFinder : MonoBehaviour
{
    private HashSet<string> wordList;

    private void Awake()
    {
        var r = (TextAsset)Resources.Load("wordlist", typeof(TextAsset));
        var words = r.text
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => string.Concat(word.Where(c => !char.IsWhiteSpace(c))))
            .ToList();

        wordList = new HashSet<string>(words);
        Resources.UnloadAsset(r);
    }

    public bool CheckWord(string word)
    {
        var wordLower = word.ToLower();
        return wordList.Contains(wordLower);
    }
}
