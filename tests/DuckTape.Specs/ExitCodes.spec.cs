using DuckTape;
using static DuckTape.Test;

Run("exit_codes: Ok is 0", t =>
{
    t.Equal(ExitCodes.Ok, 0);
    t.End();
});

Run("exit_codes: Fail is 1", t =>
{
    t.Equal(ExitCodes.Fail, 1);
    t.End();
});

Run("exit_codes: WasStop is 2", t =>
{
    t.Equal(ExitCodes.WasStop, 2);
    t.End();
});

Run("exit_codes: Unhandled is 3", t =>
{
    t.Equal(ExitCodes.Unhandled, 3);
    t.End();
});

Run("exit_codes: InvalidOption is 4", t =>
{
    t.Equal(ExitCodes.InvalidOption, 4);
    t.End();
});

Run("exit_codes: Skipped is 5", t =>
{
    t.Equal(ExitCodes.Skipped, 5);
    t.End();
});