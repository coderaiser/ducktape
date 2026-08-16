namespace DuckTape.Cli;

public static class Help
{
    public const string Text = """
        ducktape: supertape-style test runner for C#

        Usage:
          dotnet run --project src/DuckTape -- 'src/**/*.spec.cs'

        Options:
          -h, --help                       show this help
          -v, --version                    print version
          -f, --format <name>              tap | fail | short | progress-bar | json-lines
              --no-worker                  run on a single thread
              --no-check-duplicates        disable duplicate message check
              --no-check-assertions-count  disable one-assertion-per-test check
              --no-check-scopes            disable scope: subject format check

        Environment:
          DUCKTAPE_TIMEOUT               per-test timeout in ms (default 3000)
          DUCKTAPE_CHECK_DUPLICATES      duplicate check (default 1)
          DUCKTAPE_CHECK_ASSERTIONS_COUNT  one-assertion check (default 1)
          DUCKTAPE_CHECK_SCOPES          scope: subject check (default 0)
          DUCKTAPE_CHECK_SKIPPED         exit SKIPPED when skipped > 0 (default 0)
          DUCKTAPE_PROGRESS_BAR          force progress bar on/off (1/0)
          DUCKTAPE_PROGRESS_BAR_MIN      min tests to show bar (default 100)
          DUCKTAPE_PROGRESS_BAR_STACK    show stack in progress-bar failures (default 1)
        """;
}
