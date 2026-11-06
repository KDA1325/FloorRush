using NUnit.Framework;

public class CallAttemptTests
{
    [TestCase(8, 1, false, 5.999, CallResult.Boarded)]
    [TestCase(8, 1, true, 5.999, CallResult.WrongDirection)]
    [TestCase(1, 6, true, 5.999, CallResult.Boarded)]
    [TestCase(1, 6, false, 5.999, CallResult.WrongDirection)]
    [TestCase(8, 1, false, 6.0, CallResult.Missed)]
    [TestCase(8, 1, false, 6.001, CallResult.Missed)]
    public void FirstResultIsFinal(int current, int target, bool up, double now, CallResult expected)
    {
        var attempt = new CallAttempt(current, target, 6.0);
        attempt.Evaluate(now, up);
        attempt.Evaluate(now, !up);
        attempt.Evaluate(7.0);
        Assert.AreEqual(expected, attempt.Result);
    }

    [Test]
    public void TimeoutBeforeInputWinsOnDeadline()
    {
        var attempt = new CallAttempt(8, 1, 6.0);
        attempt.Evaluate(5.99);
        Assert.AreEqual(CallResult.Waiting, attempt.Result);
        attempt.Evaluate(6.0);
        attempt.Evaluate(6.0, false);
        Assert.AreEqual(CallResult.Missed, attempt.Result);
    }
}
