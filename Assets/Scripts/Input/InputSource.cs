﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.InputManagement
{
    public struct ButtonState
    {
        public readonly bool Down;
        public readonly bool Pressed;
        public readonly bool Released;

        public ButtonState(bool down, bool pressed, bool released)
        {
            Down = down;
            Pressed = pressed;
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

        public ButtonState[] _numbers = new ButtonState[10];
        public IReadOnlyList<ButtonState> Numbers => _numbers;

        public bool AnyAlphabeticalKeyDown { get; private set; }
        public char AlphabeticalPressed { get; private set; }

        // Keycodes for numbers, ordered 1234567890.
        static int[] numericKeycodes = { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48 };

        public void Prime()
        {
            SelectPosition = Input.mousePosition;
            Select = new ButtonState
            (
                down: Input.GetMouseButtonDown(0),
                pressed: Input.GetMouseButton(0),
                released: Input.GetMouseButtonUp(0)
            );

            DismissPosition = Input.mousePosition;
            Dismiss = new ButtonState
            (
                down: Input.GetMouseButtonDown(1),
                pressed: Input.GetMouseButton(1),
                released: Input.GetMouseButtonUp(1)
            );

            var submitDown =
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Space);
            var submitPressed =
                Input.GetKey(KeyCode.Return) ||
                Input.GetKey(KeyCode.KeypadEnter) ||
                Input.GetKey(KeyCode.Space);
            var submitReleased =
                Input.GetKeyUp(KeyCode.Return) ||
                Input.GetKeyUp(KeyCode.KeypadEnter) ||
                Input.GetKeyUp(KeyCode.Space);

            Submit = new ButtonState
            (
                down: submitDown,
                pressed: submitPressed,
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
                    down: Input.GetKeyDown(keycode),
                    pressed: Input.GetKey(keycode),
                    released: Input.GetKeyUp(keycode)
                );
            }

            AnyAlphabeticalKeyDown =
                Input.anyKeyDown &&
                Input.inputString.Length == 1 &&
                char.IsLetter(Input.inputString[0]);
            AlphabeticalPressed = AnyAlphabeticalKeyDown
                ? Input.inputString[0]
                : '\0';
        }
    }
}
