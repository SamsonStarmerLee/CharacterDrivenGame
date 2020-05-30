using Assets.Scripts.Notifications;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts
{
    public class Exit : MonoBehaviour
    {
        [SerializeField]
        private Color openColor;

        private LineRenderer exitBorder;
        private bool open;

        private void Start()
        {
            var borderObj = transform.Find("Border");
            exitBorder = borderObj.GetComponent<LineRenderer>();
        }

        private void OnEnable()
        {
            this.AddObserver(OnSubmit, GameManager.SubmitTurnNotification);
            this.AddObserver(OnOpen, GameManager.OpenExitNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnSubmit, GameManager.SubmitTurnNotification);
            this.RemoveObserver(OnOpen, GameManager.OpenExitNotification);
        }

        private void OnSubmit(object sender, object args)
        {
            if (!open)
            {
                return;
            }

            var x = Mathf.RoundToInt(transform.position.x);
            var y = Mathf.RoundToInt(transform.position.z);
            var boardPosition = new Vector2Int(x, y);

            foreach (var character in Board.Instance.Characters)
            {
                if (Utility.ManhattanDist(character.BoardPosition, boardPosition) > 1)
                {
                    return;
                }
            }

            // If we get here, that means all characters are inside the exit zone.
            Debug.Log("GO! POO POO");
        }

        private void OnOpen(object sender, object args)
        {
            open = true;
            exitBorder.material.DOColor(openColor, 0.5f); 
        }
    }
}
