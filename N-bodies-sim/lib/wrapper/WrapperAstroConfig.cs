using System.Dynamic;
using Raylib_cs;
namespace N_bodies_sim;

class WrapperAstroConfig
{
    public required List<AstroConfig> AstrosConfig { get; set; }
}