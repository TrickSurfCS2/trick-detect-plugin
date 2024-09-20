using TrickDetect.Managers;

namespace TrickDetect.Modules;

public class TriggerTouchModule(TriggerManager triggerManager, TrickManager trickManager)
{
    public void OnPlayerStartTouch(EventOnStartTouchEvent e)
    {
        var player = e.Player;
        var triggerName = e.TriggerName;
        var mapTriggers = triggerManager.GetMapTriggers(player.SelectedMap);
        var trigger = mapTriggers.FirstOrDefault((trigger) => trigger.FullName == triggerName);

        if (trigger == null)
            return;

        trickManager.RouteChecker(player);
    }

    public void OnPlayerEndTouch(EventOnEndTouchEvent e)
    {
        var player = e.Player;
        var triggerName = e.TriggerName;
        var mapTriggers = triggerManager.GetMapTriggers(player.SelectedMap);
        var trigger = mapTriggers.FirstOrDefault((trigger) => trigger.FullName == triggerName);

        if (trigger == null)
            return;

        if (player.RouteTriggers.Count <= 1)
        {
            // TODO reset speed and time
            player.StartSpeed = player.Client.GetSpeed();

            player.StartType = player.StartSpeed > 405
                ? StartType.PreStrafe
                : player.StartType = StartType.Velocity;
        }

        player.RouteTriggers.Add(trigger);
    }
}
