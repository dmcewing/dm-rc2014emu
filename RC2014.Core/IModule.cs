using System.IO;
using System.Runtime.Serialization;

namespace RC2014.Core
{
    public interface IModule
    {
        void SaveState(IFormatter formatter, Stream saveStream);
        void LoadState(IFormatter formatter, Stream loadStream);
    }
}
