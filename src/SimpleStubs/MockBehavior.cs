namespace Etg.SimpleStubs
{
    /// <summary>
    /// Defines the types of behavior that can be applied to stub members that have not had a behavior applied.
    /// </summary>
    public enum MockBehavior
    {
        /// <summary>
        /// Methods and properties on a mock instance in strict mode throw an exception when called before any behaviors have been assigned.
        /// </summary>
        Strict,
        /// <summary>
        /// Methods and properties on a mock instance in loose mode use a default behavior when called before any behaviors have been assigned.
        /// </summary>
        Loose
    }
}