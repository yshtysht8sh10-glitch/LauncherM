using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherM._04_Infrastructure
{
    public interface IReadFileText
    {
        string ReadTextAll();

        List<string> ReadTextOneLineList();
    }
}
