namespace PersonnelManager.Domain;

/// <summary>
/// The lifecycle state of a person in the organisation.
/// An enum models a small, CLOSED set of options — far safer than magic strings/ints,
/// and the compiler + IDE can then check that switch statements cover every case.
/// The backing type is int by default: Active = 0, OnLeave = 1, Terminated = 2.
/// </summary>
public enum EmploymentStatus
{
    Active,
    OnLeave,
    Terminated,
}
