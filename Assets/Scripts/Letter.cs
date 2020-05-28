using Assets.Scripts;
using Assets.Scripts.Actions;
using Assets.Scripts.InputManagement;
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

    [Header("AttackAnimation")]

    [SerializeField]
    private float reelBackDuration = 0.2f;

    [SerializeField]
    private float attackDuration = 0.15f;

    [SerializeField]
    private float returnDuration = 0.35f;

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
                const float hitOffset = 0.3f;
                var toTarget = (character.WorldPosition - WorldPosition).normalized;
                var reelBack = WorldPosition - toTarget;
                var hitPoint = character.WorldPosition - toTarget * hitOffset;

                // TEMP
                // Lock input while the animation plays out.
                var input = Resources.FindObjectsOfTypeAll<InputSource>().FirstOrDefault();
                input.Lock();

                var attackSequence = DOTween.Sequence()
                    .Append(transform.DOMove(reelBack, reelBackDuration))
                    .Append(transform.DOMove(hitPoint, attackDuration).SetEase(Ease.InCubic))
                    .AppendCallback(() => this.PostNotification(Notify.Action<DamagePlayerAction>(), new DamagePlayerAction(1, character)))
                    .Append(transform.DOMove(WorldPosition, returnDuration).SetEase(Ease.OutSine))
                    .AppendCallback(() => input.Unlock());
                
                return;
            }
        }

        var ignore = new List<IOccupant> { this };
        var targets = Board.Instance.Characters
            .Where(x => Utility.ManhattanDist(BoardPosition, x.BoardPosition) <= pathfindRange)
            .Select(x => x.BoardPosition)
            .ToList();
        var path = PathFinder.GenerateAStarClosest(
            BoardPosition, 
            targets, 
            ignore, 
            pathfindRange,
            MovementCallbacks);

        if (path.Count != 0)
        {
            var moveTo = path[0];
            if (Board.Instance.GetAtPosition(moveTo, Board.OccupantType.Entity) == null)
            {
                Board.Instance.MoveOccupant(this, moveTo);
                transform.DOMove(new Vector3(moveTo.x, 0f, moveTo.y), 0.25f);
            }
        }
    }
}
