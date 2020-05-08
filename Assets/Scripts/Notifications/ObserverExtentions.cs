namespace Assets.Scripts.Notifications
{
    public static class ObserverExtentions
    {
        public static void AddObserver(this IObserver _, Handler handler, string notificationName)
        {
            NotificationCenter.Instance.AddObserver(handler, notificationName);
        }

        public static void RemoveObserver(this IObserver _, Handler handler, string notificationName)
        {
            NotificationCenter.Instance.RemoveObserver(handler, notificationName);
        }
    }
}
