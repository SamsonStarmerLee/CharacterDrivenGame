using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public static Board Instance { get; } = new Board();

    #region Public

    public void RegisterScorer(Character character)
    {
        if (!scorers.Contains(character))
        {
            scorers.Add(character);
        }
    }

    public void DeregisterScorer(Character character)
    {
        if (scorers.Contains(character))
        {
            scorers.Remove(character);
        }
    }

    public Entity GetAtPosition(Vector2Int position)
    {
        entities.TryGetValue(position, out Entity entity);
        return entity;
    }

    public bool SetPosition(Vector2Int position, Entity entity)
    {
        if (entities.ContainsKey(position))
        {
            Debug.LogError($"Can't move to {position.x}, {position.y}. {entities[position]}, occupies that position.");
            return false;
        }

        entities.Add(position, entity);
        return true;
    }

    public void ClearPosition(Vector2Int position)
    {
        entities.Remove(position);
    }

    public bool MoveEntity(Entity entity, Vector2Int to)
    {
        if (entities.ContainsKey(to) && entities[to] != entity)
        {
            Debug.LogError($"Can't move to {to.x}, {to.y}. {entities[to]}, occupies that position.");
            return false;
        }

        ClearPosition(entity.BoardPosition);
        SetPosition(to, entity);
        entity.BoardPosition = to;
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
            if (h != null)
            {
                matches.AddRange(h);
            }
        }

        SetMatchHighlighting(true);
    }

    public void ScoreMatches()
    {
        var toDestroy = new List<Entity>();

        foreach (var match in matches)
        {
            var score = ScoreMatch(match);
            Game.Instance.Score += score;

            Debug.Log($"Matched '{match.Word}', Score: {score}.");
            Debug.Log($"Total Score: {Game.Instance.Score}.");

            foreach (var entity in match.Entities)
            {
                if (entity is Character character && character.IsPlayer)
                {
                    continue;
                }

                if (entity != null)
                {
                    toDestroy.Add(entity);
                }
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

    Dictionary<Vector2Int, Entity> entities = new Dictionary<Vector2Int, Entity>();

    HashSet<Character> scorers = new HashSet<Character>();

    List<Match> matches = new List<Match>();

    class Match
    {
        public string Word;
        public List<Entity> Entities;

        public Match(string word, List<Entity> entities)
        {
            Word = word;
            Entities = entities;
        }
    }

    List<Match> CheckForWords(Entity entity)
    {
        var origin = entity.BoardPosition;
        var allMatches = new List<Match>();

        // Horizontal Sweep
        {
            var hEnts = new List<Entity>();
            SweepLetters(origin, Vector2Int.left, hEnts);
            hEnts.Reverse();
            hEnts.Add(entity);
            SweepLetters(origin, Vector2Int.right, hEnts);

            if (FindBestMatch(hEnts, out Match match))
            {
                allMatches.Add(match);
            }
        }

        // Vertical Sweep
        {
            var vEnts = new List<Entity>();
            SweepLetters(origin, Vector2Int.up, vEnts);
            vEnts.Reverse();
            vEnts.Add(entity);
            SweepLetters(origin, Vector2Int.down, vEnts);

            if (FindBestMatch(vEnts, out Match match))
            {
                allMatches.Add(match);
            }
        }

        return allMatches;
    }

    void SweepLetters(Vector2Int pos, Vector2Int step, List<Entity> entsOut)
    {
        var iterations = 0;
        while (true)
        {
            iterations++;
            entities.TryGetValue(pos + step * iterations, out Entity e);

            if (e == null || string.IsNullOrWhiteSpace(e.Letter) || e.Solid)
            {
                break;
            }

            entsOut.Add(e);
        }
    }

    static bool FindBestMatch(IReadOnlyList<Entity> entities, out Match match)
    {
        var finder = GameObject.Find("WordFinder").GetComponent<WordFinder>();

        var bestLength = 0;
        match = null;

        for (var i = 0; i < entities.Count; ++i)
        {
            var word = "";
            var wordEnts = new List<Entity>();
            var containsPlayer = false;

            for (var j = 0; j < entities.Count - i; j++)
            {
                var l = entities[i + j];
                word += l.Letter;
                wordEnts.Add(l);
                containsPlayer ^= (l is Character character && character.IsPlayer);

                if (word.Length >= MinimumLength &&
                    word.Length > bestLength &&
                    containsPlayer &&
                    finder.CheckWord(word))
                {
                    bestLength = word.Length;
                    match = new Match(word, new List<Entity>(wordEnts));
                }
            }
        }

        return match != null;
    }

    void SetMatchHighlighting(bool highlighted)
    {
        foreach (var word in matches)
        {
            foreach (var e in word.Entities)
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