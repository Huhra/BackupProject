namespace Core.Backup
{
    public enum StatusFlag : long
    {
        Unset = 0,
        Unchanged = 1,
        Changed = 2,
        Deleted = 3,
    }
}
