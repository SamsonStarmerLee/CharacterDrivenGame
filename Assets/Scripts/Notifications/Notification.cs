using UnityEngine;

namespace Assets.Scripts.Notifications
{
    public static class Notification
    {
        public static int GenerateID<T>() => GenerateID(typeof(T));

        public static int GenerateID(System.Type type) => Animator.StringToHash(type.Name);

        public static string Prepare<T>() => Prepare(typeof(T));

        public static string Prepare(System.Type type) => $"{type.Name}.PrepareNotification";

        public static string Perform<T>() => Perform(typeof(T));

        public static string Perform(System.Type type) => $"{type.Name}.PerformNotification";

        public static string Validate<T>() => Validate(typeof(T));

        public static string Validate(System.Type type) => $"{type.Name}.ValidateNotification";

        public static string Cancel<T>() => Cancel(typeof(T));

        public static string Cancel(System.Type type) => $"{type.Name}.CancelNotification";
    }

}
