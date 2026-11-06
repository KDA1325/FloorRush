public enum CallResult { Waiting, Boarded, WrongDirection, Missed }

public sealed class CallAttempt
{
    public double Deadline { get; }
    public CallResult Result { get; private set; }
    private readonly bool goingUp;

    public CallAttempt(int current, int destination, double deadline)
    {
        goingUp = destination > current;
        Deadline = deadline;
    }

    public void Evaluate(double now, bool? up = null)
    {
        if (Result != CallResult.Waiting) return;
        if (now >= Deadline) Result = CallResult.Missed;
        else if (up.HasValue) Result = up.Value == goingUp ? CallResult.Boarded : CallResult.WrongDirection;
    }
}
