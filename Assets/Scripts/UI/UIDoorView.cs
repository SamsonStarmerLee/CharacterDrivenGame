using Assets.Scripts.Notifications;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIDoorView : MonoBehaviour
    {
        [SerializeField]
        private Image doorImage;

        [SerializeField]
        private Sprite doorOpen, doorClosed;

        [SerializeField]
        private List<Image> countdownImages;

        [SerializeField]
        private Sprite cdFull, cdEmpty;

        private void OnEnable()
        {
            this.AddObserver(OnDoorCountdown, GameManager.DoorCountdownNotification);
            this.AddObserver(OnFloorExitOpened, GameManager.OpenExitNotification);
            this.AddObserver(OnNewFloor, GameManager.ExitFloorNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnDoorCountdown, GameManager.DoorCountdownNotification);
            this.RemoveObserver(OnFloorExitOpened, GameManager.OpenExitNotification);
            this.RemoveObserver(OnNewFloor, GameManager.ExitFloorNotification);
        }

        private void OnDoorCountdown(object sender, object args)
        {
            var gm = args as GameManager;
            var turns = gm.DoorCountdown;
            var delta = countdownImages.Count - turns;

            for (var i = 0; i < delta; i++)
            {
                SetSpriteForTurnSlot(cdEmpty, i, 0.75f);
            }

            for (var i = delta; i < countdownImages.Count; i++)
            {
                SetSpriteForTurnSlot(cdFull, i, 1.0f);
            }
        }

        private void SetSpriteForTurnSlot(Sprite sprite, int index, float scale)
        {
            if (index < 0 || index >= countdownImages.Count)
            {
                Debug.Log("Tried to set turn timer slot outside of images range.");
                return;
            }

            countdownImages[index].sprite = sprite;
            countdownImages[index].transform.localScale = Vector3.one * scale;
        }

        private void OnFloorExitOpened(object sender, object args)
        {
            doorImage.sprite = doorOpen;
        }

        private void OnNewFloor(object sender, object args)
        {
            doorImage.sprite = doorClosed;
        }
    }
}
