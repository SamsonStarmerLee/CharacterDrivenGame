using Assets.Scripts;
using Assets.Scripts.Characters;
using Assets.Scripts.Pathfinding;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Letter : Entity
{
    protected static char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    protected static char[] Consonants = "BCDFGHJKLMNPQRSTVWXYZ".ToCharArray();
    protected static char[] Vowels = "AEIOU".ToCharArray();

    [SerializeField]
    Mesh[] meshAlphabet;

    [SerializeField, Min(1)]
    int pathfindRange = 10;

    public IMovementCallbacks MovementCallbacks { get; } = new LetterMovementCallbacks();

    public override void Init()
    {
        base.Init();

        // This sucks
        if (string.IsNullOrWhiteSpace(Letter))
        {
            _letter = Consonants[Random.Range(0, Consonants.Length)].ToString();
            name = _letter;
            var mesh = meshAlphabet.First(x => x.name == $"{Letter}_Upper");
            var meshFilter = GetComponentInChildren<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }
    }

    public void Act()
    {
        if (Solid)
        {
            return;
        }

        var targets = FindObjectsOfType<AWarrior>()
            .Where(x => !x.Solid && Utility.ManhattanDist(BoardPosition, x.BoardPosition) <= pathfindRange)
            .Select(x => x.BoardPosition)
            .ToList();
        var ignore = new List<IOccupant> { this };
        var path = PathFinder.GenerateAStarClosest(
            BoardPosition, 
            targets, 
            ignore, 
            pathfindRange,
            MovementCallbacks);

        if (path.Count == 0)
        {
            return;
        }

        var previous = BoardPosition;
        for (var i = 0; i < path.Count; i++)
        {
            var point = path[i];
            Debug.DrawLine(
            new Vector3(previous.x, 0f, previous.y),
            new Vector3(point.x, 0f, point.y),
            Color.Lerp(Color.green, Color.red, (float)i / (float)path.Count));
            previous = point;
        }

        var moveTo = path[0];
        if (Board.Instance.GetAtPosition(moveTo) == null)
        {
            Board.Instance.MoveOccupant(this, moveTo);
            transform.DOMove(new Vector3(moveTo.x, 0f, moveTo.y), 0.25f);
        }
    }
}
