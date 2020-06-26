using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.InputManagement
{
    public struct ButtonState
    {
        public readonly bool Clicked;
        public readonly bool Held;
        public readonly bool Released;

        public ButtonState(bool clicked, bool held, bool released)
        {
            Clicked = clicked;
            Held = held;
            Released = released;
        }
    }

    [CreateAssetMenu]
    public class InputSource : ScriptableObject
    {
        public Vector2 SelectPosition { get; private set; }

        public ButtonState Select { get; private set; }

        public Vector2 DismissPosition { get; private set; }

        public ButtonState Dismiss { get; private set; }

        public ButtonState Submit { get; private set; }

        public Vector2 CameraAxis { get; private set; }

        private ButtonState[] _numbers = new ButtonState[10];
        public IReadOnlyList<ButtonState> Numbers => _numbers;

        public bool AnyAlphabeticalKeyClicked { get; private set; }

        public char AlphabeticalHeld { get; private set; }

        /// <summary>
        /// The InputSource can be 'locked' any number of times. Those locking sources are responsible for unlocking.
        /// As long as one lock remains, the whole InputProvider is considered locked.
        /// Being locked does not affect input. Consumers choose how to interpret it.
        /// </summary>
        public bool Locked => locks > 0;
        private int locks;

        // Keycodes for numbers, ordered 1234567890.
        static int[] numericKeycodes = { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48 };

        public void Prime()
        {
            SelectPosition = Input.mousePosition;
            Select = new ButtonState
            (
                clicked: Input.GetMouseButtonDown(0),
                held: Input.GetMouseButton(0),
                released: Input.GetMouseButtonUp(0)
            );

            DismissPosition = Input.mousePosition;
            Dismiss = new ButtonState
            (
                clicked: Input.GetMouseButtonDown(1),
                held: Input.GetMouseButton(1),
                released: Input.GetMouseButtonUp(1)
            );

            var submitClicked =
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetMouseButtonDown(1);
            var submitHeld =
                Input.GetKey(KeyCode.Return) ||
                Input.GetKey(KeyCode.KeypadEnter) ||
                Input.GetKey(KeyCode.Space) ||
                Input.GetMouseButton(1);
            var submitReleased =
                Input.GetKeyUp(KeyCode.Return) ||
                Input.GetKeyUp(KeyCode.KeypadEnter) ||
                Input.GetKeyUp(KeyCode.Space) ||
                Input.GetMouseButtonUp(1);

            Submit = new ButtonState
            (
                clicked: submitClicked,
                held: submitHeld,
                released: submitReleased
            );

            var cx = Input.GetAxis("CameraHorizontal");
            var cy = Input.GetAxis("CameraVertical");
            CameraAxis = new Vector2(cx, cy);

            // Loop through numeric buttons.
            for (var i = 0; i < 10; i++)
            {
                var keycode = (KeyCode)numericKeycodes[i];
                _numbers[i] = new ButtonState
                (
                    clicked: Input.GetKeyDown(keycode),
                    held: Input.GetKey(keycode),
                    released: Input.GetKeyUp(keycode)
                );
            }

            AnyAlphabeticalKeyClicked =
                Input.anyKeyDown &&
                Input.inputString.Length == 1 && 
                char.IsLetter(Input.inputString[0]);
            AlphabeticalHeld = AnyAlphabeticalKeyClicked
                ? Input.inputString[0]
                : '\0';
        }

        public void Lock()
        {
            locks = Mathf.Clamp(locks + 1, 0, int.MaxValue);
        }

        public void Unlock()
        {
            locks = Mathf.Clamp(locks - 1, 0, int.MaxValue);
        }
    }
}
