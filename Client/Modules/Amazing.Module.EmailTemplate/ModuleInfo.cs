using Oqtane.Models;
using Oqtane.Modules;

namespace Amazing.Module.EmailTemplate
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "EmailTemplate",
            Description = "Amazing.Module.EmailTemplate is a module built for Oqtane",
            Version = "1.0.2",
            ServerManagerType = "Amazing.Module.EmailTemplate.Manager.EmailTemplateManager, Amazing.Module.EmailTemplate.Server.Oqtane",
            ReleaseVersions = "1.0.0,1.0.1,1.0.2",
            Dependencies = "Amazing.Module.EmailTemplate.Shared.Oqtane",
            PackageName = "Amazing.Module.EmailTemplate" 
        };
    }
}
