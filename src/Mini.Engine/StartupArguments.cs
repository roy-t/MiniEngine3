namespace Mini.Engine
{
    public static class StartupArguments
    {
        public static bool EnableRenderDoc => IsPresent("--renderdoc");

        private static bool IsPresent(string argument)
        {
            var args = Environment.GetCommandLineArgs();
            return args.Any(a => a.Equals(argument, StringComparison.OrdinalIgnoreCase));
        }
    }
}
