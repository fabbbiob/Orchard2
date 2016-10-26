﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Orchard.Environment.Extensions.Loaders
{
    public class PrecompiledExtensionLoader : IExtensionLoader
    {
        private readonly string[] ExtensionsSearchPaths;

        private readonly IExtensionLibraryService _extensionLibraryService;
        private readonly ILogger _logger;

        public PrecompiledExtensionLoader(
            IOptions<ExtensionOptions> optionsAccessor,
            IExtensionLibraryService extensionLibraryService,
            ILogger<PrecompiledExtensionLoader> logger)
        {
            ExtensionsSearchPaths = optionsAccessor.Value.SearchPaths.ToArray();
            _extensionLibraryService = extensionLibraryService;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 30;

        public ExtensionEntry Load(IExtensionInfo extensionInfo)
        {
            if (!ExtensionsSearchPaths.Any(x => extensionInfo.SubPath.StartsWith(x)))
            {
                return null;
            }

            try
            {
                var assembly = _extensionLibraryService.LoadPrecompiledExtension(extensionInfo);
            
                if (assembly == null)
                {
                    return null;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded referenced precompiled extension \"{0}\": assembly name=\"{1}\"", extensionInfo.Id, assembly.FullName);
                }

                return new ExtensionEntry
                {
                    ExtensionInfo = extensionInfo,
                    Assembly = assembly,
                    ExportedTypes = assembly.ExportedTypes
                };
            }
            catch
            {
                return null;
            }
       }
    }
}