using Assets.Scripts;
using Assets.Scripts.Characters;
using System.Collections.Generic;
using UnityEngine;

class Cell
{
    public IOccupant Entity = null;
    public IOccupant Item   = null;
    // public Entity Prop;
}

public class Board
{
    public static Board Instance { get; } = new Board();

    #region Public

    public IReadOnlyCollection<IScorer>    Scorers => _scorers;
    public IReadOnlyCollection<ICharacter> Characters => _characters;
    public IReadOnlyCollection<Entity>     Entities => _entities;
    public IReadOnlyCollection<Letter>     Letters => _letters;

    public void Register(object a)
    {
        if (a is IScorer scorer && !_scorers.Contains(scorer))
        {
            _scorers.Add(scorer);
        }

        if (a is ICharacter character && !_characters.Contains(character))
        {
            _characters.Add(character);
        }

        if (a is Entity entity && !_entities.Contains(entity))
        {
            _entities.Add(entity);
        }

        if (a is Letter letter && !_letters.Contains(letter))
        {
            _letters.Add(letter);
        }
    }

    public void Deregister(object a)
    {
        if (a is IScorer scorer && _scorers.Contains(scorer))
        {
            _scorers.Remove(scorer);
        }

        if (a is ICharacter character && _characters.Contains(character))
        {
            _characters.Remove(character);
        }

        if (a is Entity entity && _entities.Contains(entity))
        {
            _entities.Remove(entity);
        }

        if (a is Letter letter && _letters.Contains(letter))
        {
            _letters.Remove(letter);
        }
    }

    public void RefreshCharacters()
    {
        foreach (var character in _characters)
        {
            character.HasActed = false;
        }

        foreach (var letter in _letters)
        {
            letter.Act();
        }

        CheckForMatches();
    }

    public IOccupant GetAtPosition(Vector2Int position)
    {
        cells.TryGetValue(position, out Cell cell);
        return cell?.Entity;
    }

    public bool SetPosition(IOccupant occ, Vector2Int atPos)
    {
        if (!cells.ContainsKey(atPos))
        {
            cells[atPos] = new Cell();
        }

        if (cells[atPos].Entity != null)
        {
            Debug.LogError($"Can't move {occ} to {atPos.x}, {atPos.y}. {cells[atPos].Entity} occupies that position.");
            return false;
        }

        cells[atPos].Entity = occ;
        return true;
    }

    public void ClearPosition(Vector2Int position)
    {
        cells.Remove(position);
    }

    public bool MoveOccupant(IOccupant occ, Vector2Int toPos)
    {
        if (!cells.ContainsKey(toPos))
        {
            cells[toPos] = new Cell();
        }

        if (cells[toPos].Entity != null)
        {
            Debug.LogError($"Can't move to {toPos.x}, {toPos.y}. {cells[toPos].Entity}, occupies that position.");
            return false;
        }

        ClearPosition(occ.BoardPosition);
        SetPosition(occ, toPos);
        occ.BoardPosition = toPos;
        return true;
    }

    public void CheckForMatches()
    {
        // De-highlight and clear existing matches.
        SetMatchHighlighting(false);
        _matches.Clear();

        foreach (var scorer in _scorers)
        {
            var h = CheckForWords(scorer);
            if (h != null && h.Count != 0)
            {
                _matches.AddRange(h);
            }
        }

        SetMatchHighlighting(true);
    }

    public void ScoreMatches()
    {
        var toDestroy = new List<IOccupant>();

        foreach (var match in _matches)
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
        _matches.Clear();
    }

    #endregion

    #region Private

    const int MinimumLength = 2;

    Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();

    HashSet<IScorer>    _scorers     = new HashSet<IScorer>();
    HashSet<ICharacter> _characters  = new HashSet<ICharacter>();
    HashSet<Entity>     _entities    = new HashSet<Entity>();
    HashSet<Letter>     _letters     = new HashSet<Letter>();
    List<Match>         _matches     = new List<Match>();

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

    private List<Match> CheckForWords(IScorer scorer)
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
        // TODO: Static C#8
        bool IsValid(IOccupant o) => 
            o is Entity e && 
            !string.IsNullOrWhiteSpace(e.Letter) && 
            !e.Solid;

        var iterations = 0;
        while (true)
        {
            iterations++;
            var p = pos + step * iterations;

            if (cells.TryGetValue(p, out var cell) && IsValid(cell?.Entity))
            {
                occsOut.Add(cell.Entity);
            }
            else
            {
                break;
            }
        }
    }

    void SetMatchHighlighting(bool highlighted)
    {
        foreach (var word in _matches)
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

    static bool FindBestMatch(IReadOnlyList<IOccupant> letters, out Match match)
    {
        var finder = GameObject.Find("WordFinder").GetComponent<WordFinder>();

        var bestLength = 0;
        match = null;

        for (var i = 0; i < letters.Count; ++i)
        {
            var word = "";
            var wordParts = new List<IOccupant>();
            var containsPlayer = false;

            for (var j = 0; j < letters.Count - i; j++)
            {
                var occ = letters[i + j];
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

    #endregion
}