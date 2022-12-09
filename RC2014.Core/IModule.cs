using System.IO;
using System.Runtime.Serialization;

namespace RC2014.Core
{
    public interface IModule
    {
        void SaveState(Stream saveStream);
        void LoadState(Stream loadStream);
    }
}
