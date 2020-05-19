internal interface IState
{
    bool CanTransition();

    void Enter();

    IState Execute();

    void Exit();
}

internal class StateMachine
{
    public IState CurrentState { get; private set; }
    public IState PreviousState { get; private set; }

    public void Execute()
    {
        var result = CurrentState?.Execute();

        if (result != null)
        {
            ChangeState(result);
        }
    }

    /// <summary>
    /// Transition to a new constructorless generic type IState if possible.
    /// </summary>
    public void ChangeState<T>() where T : class, IState, new()
    {
        var fromState = CurrentState;
        var toState = new T();

        if (fromState != null)
        {
            if (fromState == toState || !fromState.CanTransition())
            {
                return;
            }

            fromState.Exit();
        }

        CurrentState = toState;
        PreviousState = fromState;
        toState.Enter();
    }

    /// <summary>
    /// Transition to the provided IState if possible.
    /// </summary>
    public void ChangeState(IState toState)
    {
        var fromState = CurrentState;

        if (fromState != null)
        {
            if (fromState == toState || !fromState.CanTransition())
            {
                return;
            }

            fromState.Exit();
        }

        CurrentState = toState;
        PreviousState = fromState;
        toState.Enter();
    }
}