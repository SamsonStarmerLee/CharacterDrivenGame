using Assets.Scripts;
using Assets.Scripts.Actions;
using Assets.Scripts.Notifications;
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
    private Mesh[] meshAlphabet;

    [SerializeField, Min(1)]
    private int pathfindRange = 10;

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
        if (Type == EntityType.Solid)
        {
            return;
        }

        foreach (var character in Board.Instance.Characters)
        {
            if (Utility.ManhattanDist(BoardPosition, character.BoardPosition) == 1)
            {
                var toTarget = (character.WorldPosition - WorldPosition).normalized;
                var attackSequence = DOTween.Sequence()
                    .Append(transform.DOMove(WorldPosition - toTarget, 0.2f))
                    .Append(transform.DOMove(character.WorldPosition - toTarget * 0.3f, 0.15f).SetEase(Ease.InCubic))
                    .AppendCallback(() => this.PostNotification(Notify.Action<DamagePlayerAction>(), new DamagePlayerAction(1)))
                    .Append(transform.DOMove(WorldPosition, 0.35f).SetEase(Ease.OutSine));
                return;
            }
        }

        var targets = Board.Instance.Characters
            .Where(x => Utility.ManhattanDist(BoardPosition, x.BoardPosition) <= pathfindRange)
            .Select(x => x.BoardPosition)
            .ToList();

        if (targets.Count == 0)
        {
            return;
        }

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
        if (Board.Instance.GetAtPosition(moveTo, Board.OccupantType.Entity) == null)
        {
            Board.Instance.MoveOccupant(this, moveTo);
            transform.DOMove(new Vector3(moveTo.x, 0f, moveTo.y), 0.25f);
        }
    }
}
