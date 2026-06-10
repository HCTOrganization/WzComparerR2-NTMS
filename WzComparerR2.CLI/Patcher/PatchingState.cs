namespace WzComparerR2.CLI.Patcher
{
    public enum PatchingState
    {
        PatchStart,
        VerifyOldChecksumBegin,
        VerifyOldChecksumEnd,
        VerifyNewChecksumBegin,
        VerifyNewChecksumEnd,
        TempFileCreated,
        TempFileBuildProcessChanged,
        TempFileClosed,
        CompareStarted,
        CompareProcessChanged,
        CompareFinished,

        PrepareVerifyOldChecksumBegin,
        PrepareVerifyOldChecksumEnd,
        ApplyFile,
        FileSkipped,
    }
}
