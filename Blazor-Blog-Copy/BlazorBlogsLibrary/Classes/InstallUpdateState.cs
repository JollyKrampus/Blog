namespace BlazorBlogsLibrary.Classes
{
    internal class InstallUpdateState
    {
        public string InstallUpgradeWizardStage { get; set; }
        public bool DatabaseReady { get; set; }
        public string DatabaseConectionString { get; set; }
    }
}