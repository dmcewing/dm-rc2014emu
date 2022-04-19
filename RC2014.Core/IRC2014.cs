using System.Threading;
using System.Threading.Tasks;

namespace RC2014.Core
{

    public interface IRC2014
    {
        void Start();
        void Stop();
        void Restart();
        void Resume();
    }
}