using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public static Board Instance { get; } = new Board();

    #region Public

    public void RegisterScorer(IScorer scorer)
    {
        if (!scorers.Contains(scorer))
        {
            scorers.Add(scorer);
        }
    }

    public void DeregisterScorer(IScorer scorer)
    {
        if (scorers.Contains(scorer))
        {
            scorers.Remove(scorer);
        }
    }

    public IOccupant GetAtPosition(Vector2Int position)
    {
        occupants.TryGetValue(position, out IOccupant occupant);
        return occupant;
    }

    public bool SetPosition(Vector2Int position, IOccupant p)
    {
        if (occupants.ContainsKey(position))
        {
            Debug.LogError($"Can't move to {position.x}, {position.y}. {occupants[position]}, occupies that position.");
            return false;
        }

        occupants.Add(position, p);
        return true;
    }

    public void ClearPosition(Vector2Int position)
    {
        occupants.Remove(position);
    }

    public bool MoveOccupant(IOccupant p, Vector2Int to)
    {
        if (occupants.ContainsKey(to) && occupants[to] != p)
        {
            Debug.LogError($"Can't move to {to.x}, {to.y}. {occupants[to]}, occupies that position.");
            return false;
        }

        ClearPosition(p.BoardPosition);
        SetPosition(to, p);
        p.BoardPosition = to;
        return true;
    }

    public void CheckForMatches()
    {
        // De-highlight and clear existing matches.
        SetMatchHighlighting(false);
        matches.Clear();

        foreach (var scorer in scorers)
        {
            var h = CheckForWords(scorer);
            if (h != null && h.Count != 0)
            {
                matches.AddRange(h);
            }
        }

        SetMatchHighlighting(true);
    }

    public void ScoreMatches()
    {
        var toDestroy = new List<IOccupant>();

        foreach (var match in matches)
        {
            var score = ScoreMatch(match);
            Game.Instance.Score += score;

            Debug.Log($"Matched '{match.Word}', Score: {score}.");
            Debug.Log($"Total Score: {Game.Instance.Score}.");

            foreach (var entity in match.Parts)
            {
                if (entity is IScorer || entity == null)
                {
                    continue;
                }

                toDestroy.Add(entity);
            }
        }

        foreach (var e in toDestroy)
        {
            e.Destroy();
            ClearPosition(e.BoardPosition);
        }

        SetMatchHighlighting(false);
        matches.Clear();
    }

    #endregion

    #region Private

    const int MinimumLength = 2;

    Dictionary<Vector2Int, IOccupant> occupants = new Dictionary<Vector2Int, IOccupant>();

    HashSet<IScorer> scorers = new HashSet<IScorer>();

    List<Match> matches = new List<Match>();

    class Match
    {
        public string Word;
        public List<IOccupant> Parts;

        public Match(string word, List<IOccupant> parts)
        {
            Word = word;
            Parts = parts;
        }
    }

    List<Match> CheckForWords(IScorer scorer)
    {
        var origin = scorer.BoardPosition;
        var allMatches = new List<Match>();
        var entity = scorer as Entity;

        // Horizontal Sweep
        {
            var hOccs = new List<IOccupant>();
            SweepLetters(origin, Vector2Int.left, hOccs);
            hOccs.Reverse();
            hOccs.Add(entity);
            SweepLetters(origin, Vector2Int.right, hOccs);

            if (FindBestMatch(hOccs, out Match match))
            {
                allMatches.Add(match);
            }
        }

        // Vertical Sweep
        {
            var vOccs = new List<IOccupant>();
            SweepLetters(origin, Vector2Int.up, vOccs);
            vOccs.Reverse();
            vOccs.Add(entity);
            SweepLetters(origin, Vector2Int.down, vOccs);

            if (FindBestMatch(vOccs, out Match match))
            {
                allMatches.Add(match);
            }
        }

        return allMatches;
    }

    void SweepLetters(Vector2Int pos, Vector2Int step, List<IOccupant> occsOut)
    {
        var iterations = 0;
        while (true)
        {
            iterations++;
            occupants.TryGetValue(pos + step * iterations, out var o);

            var e = o as Entity;
            if (e == null || string.IsNullOrWhiteSpace(e.Letter) || e.Solid)
            {
                break;
            }

            occsOut.Add(o);
        }
    }

    static bool FindBestMatch(IReadOnlyList<IOccupant> occupants, out Match match)
    {
        var finder = GameObject.Find("WordFinder").GetComponent<WordFinder>();

        var bestLength = 0;
        match = null;

        for (var i = 0; i < occupants.Count; ++i)
        {
            var word = "";
            var wordParts = new List<IOccupant>();
            var containsPlayer = false;

            for (var j = 0; j < occupants.Count - i; j++)
            {
                var occ = occupants[i + j];
                word += occ.Letter;
                wordParts.Add(occ);
                containsPlayer |= occ is IScorer;

                if (word.Length >= MinimumLength &&
                    word.Length > bestLength &&
                    containsPlayer &&
                    finder.CheckWord(word))
                {
                    bestLength = word.Length;
                    match = new Match(word, new List<IOccupant>(wordParts));
                }
            }
        }

        return match != null;
    }

    void SetMatchHighlighting(bool highlighted)
    {
        foreach (var word in matches)
        {
            foreach (Entity e in word.Parts)
            {
                e.SetHighlight(highlighted);
            }
        }
    }

    int ScoreMatch(Match match)
    {
        var word = match.Word;
        var l = word.Length - MinimumLength + 1;
        var triangularScore = l * (l + 1) / 2;

        // TODO: Special characters, abilities, etc.

        return triangularScore;
    }

    #endregion
}