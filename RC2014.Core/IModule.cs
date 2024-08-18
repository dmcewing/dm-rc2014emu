using System.IO;
using System.Runtime.Serialization;

namespace RC2014.Core
{
    public interface IModule
    {
        void SaveState(ICustomFormatter formatter, Stream saveStream);
        void LoadState(ICustomFormatter formatter, Stream loadStream);
    }
}
