using System.Collections.Generic;

namespace OpenIPC_Config.Models;

public class Preset
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }

    public List<string> Files { get; set; }
}