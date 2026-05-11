namespace KSPMissionControl.Career.Internal;

// Volatile reference swap: sufficient because T is a class (reference type), reference
// writes are atomic on 64-bit, and the snapshot is immutable once published — readers
// see either the old or the new reference, never a torn one.
internal sealed class StateCache<T> where T : class
{
    private volatile T? _snapshot;

    public T? Snapshot => _snapshot;

    public void Update(T newSnapshot) => _snapshot = newSnapshot;
}
