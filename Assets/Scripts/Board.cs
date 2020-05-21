using Assets.Scripts;
using Assets.Scripts.Actions;
using Assets.Scripts.Characters;
using Assets.Scripts.Notifications;
using System.Collections.Generic;
using UnityEngine;
using static Entity;

internal class Cell
{
    public IOccupant Entity = null;
    public IOccupant Item   = null;
    // public Entity Terrain;

    public IOccupant PrimaryOccupant => Entity ?? Item;

    public void ClearOccupant(IOccupant occ)
    {
        if (Entity == occ) Entity = null;
        if (Item == occ) Item = null;
        // TODO: if (Terrain == occ) Terrain = null;
    }
}

public class Board
{
    public static Board Instance { get; } = new Board();

    #region Public

    public IReadOnlyCollection<IScorer>    Scorers => _scorers;
    public IReadOnlyCollection<ICharacter> Characters => _characters;
    public IReadOnlyCollection<Entity>     Entities => _entities;
    public IReadOnlyCollection<Entity>     Items => _items;
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

        if (a is Entity entity)
        {
            if (entity.Type == EntityType.Entity && !_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
            else if (entity.Type == EntityType.Item && !_items.Contains(entity))
            {
                _items.Add(entity);
            }
            // TEMP. TODO: Action Solid case
            else if (entity.Type == EntityType.Solid && !_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
            // TEMP
            else
            {
                Debug.LogError($"Something went wrong trying to register entity {entity}.");
            }
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

        if (a is Entity entity)
        {
            if (entity.Type == EntityType.Entity && _entities.Contains(entity))
            {
                _entities.Remove(entity);
            }
            else if (entity.Type == EntityType.Item && _items.Contains(entity))
            {
                _items.Remove(entity);
            }
            // TODO: Solid case
            else
            {
                Debug.LogError($"Something went wrong trying to deregister entity {entity}.");
            }
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

    public enum OccupantType
    {
        Any,
        Entity,
        Item,
    }

    public IOccupant GetAtPosition(Vector2Int position, OccupantType type)
    {
        cells.TryGetValue(position, out Cell cell);

        if (cell == null)
        {
            return null;
        }

        switch (type)
        {
            case OccupantType.Any:
                return cell.PrimaryOccupant;
            case OccupantType.Entity:
                return cell.Entity;
            case OccupantType.Item:
                return cell.Item;
            default:
                return null;

                //case OccupantType.Solid: // TODO
                //    return cell.Solid;
        };
    }

    public bool SetPosition(IOccupant occ, Vector2Int atPos)
    {
        if (!cells.ContainsKey(atPos))
        {
            cells[atPos] = new Cell();
        }

        var ent = occ as Entity;
        switch (ent.Type)
        {
            case EntityType.Solid:
            case EntityType.Entity:
                return SetEntityPosition(ent, atPos);
            case EntityType.Item:
                return SetItemPosition(ent, atPos);
            // TODO:
            //case EntityType.Solid:
            //    return SetSolidPosition(ent, atPos);
            default:
                Debug.Log("Occupant was not an entity, or had an invalid type.");
                return false;
        }
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

        cells[occ.BoardPosition].ClearOccupant(occ);
        SetPosition(occ, toPos);
        occ.BoardPosition = toPos;

        return true;
    }

    public void CheckForMatches()
    {
        // De-highlight and clear existing matches.
        SetMatchHighlighting(false);
        matches.Clear();

        foreach (var scorer in _scorers)
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

            this.PostNotification(
                Notify.Action<ScoreAction>(),
                new ScoreAction(score));

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
            cells[e.BoardPosition].ClearOccupant(e);
        }

        SetMatchHighlighting(false);
        matches.Clear();
    }

    public void CollectItems()
    {
        foreach (var chr in Characters)
        {
            var item = GetAtPosition(chr.BoardPosition, OccupantType.Item);
            if (item != null)
            {
                // TEMP
                item.Destroy();
            } 
        }
    }

    #endregion

    #region Private

    private const int MinimumLength = 2;

    private Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();

    private HashSet<IScorer> _scorers = new HashSet<IScorer>();
    private HashSet<ICharacter> _characters = new HashSet<ICharacter>();
    private HashSet<Entity> _entities = new HashSet<Entity>();
    private HashSet<Entity> _items = new HashSet<Entity>();
    private HashSet<Letter> _letters = new HashSet<Letter>();

    private List<Match> matches = new List<Match>();

    private bool SetEntityPosition(Entity ent, Vector2Int position)
    {
        if (cells[position].Entity != null)
        {
            Debug.LogError(
                $"Can't move Entity {ent} to {position}. " +
                $"{cells[position].Entity} occupies that position.");
            return false;
        }

        cells[position].Entity = ent;
        return true;
    }

    private bool SetItemPosition(Entity itm, Vector2Int position)
    {
        if (cells[position].Item != null)
        {
            Debug.LogError(
                $"Can't move Item {itm} to {position}. " +
                $"{cells[position].Item} occupies that position.");
            return false;
        }

        cells[position].Item = itm;
        return true;
    }

    private class Match
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

    private void SweepLetters(Vector2Int pos, Vector2Int step, List<IOccupant> occsOut)
    {
        // TODO: Static C#8
        bool IsValid(IOccupant occ) => 
            occ is Entity ent && 
            !string.IsNullOrWhiteSpace(ent.Letter) && 
            ent.Type != EntityType.Solid;

        var iterations = 0;
        while (true)
        {
            iterations++;
            var p = pos + step * iterations;

            if (cells.TryGetValue(p, out var cell) && IsValid(cell.PrimaryOccupant))
            {
                occsOut.Add(cell.PrimaryOccupant);
            }   
            else
            {
                break;
            }
        }
    }

    private void SetMatchHighlighting(bool highlighted)
    {
        foreach (var word in matches)
        {
            foreach (Entity e in word.Parts)
            {
                e.SetHighlight(highlighted);
            }
        }
    }

    private int ScoreMatch(Match match)
    {
        var word = match.Word;
        var l = word.Length - MinimumLength + 1;
        var triangularScore = l * (l + 1) / 2;

        // TODO: Special characters, abilities, etc.

        return triangularScore;
    }

    private static bool FindBestMatch(IReadOnlyList<IOccupant> letters, out Match match)
    {
        var finder = GameObject.Find("WordFinder").GetComponent<WordFinder>();

        var bestLength = 0;
        match = null;

        for (var i = 0; i < letters.Count; ++i)
        {
            var word = "";
            var wordParts = new List<IOccupant>();
            var containsScorer = false;
            var containsNonScorer = false;

            for (var j = 0; j < letters.Count - i; j++)
            {
                var occ = letters[i + j];
                word += occ.Letter;
                wordParts.Add(occ);
                containsScorer |= occ is IScorer;
                containsNonScorer |= !(occ is IScorer);

                if (word.Length >= MinimumLength &&
                    word.Length >= bestLength &&
                    containsScorer &&
                    containsNonScorer &&
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