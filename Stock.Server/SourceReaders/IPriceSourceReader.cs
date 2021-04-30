using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Server
{

    public enum PriceSourceType
    {
        JsonFile,
        JsonWebSource
    }

    public interface IPriceSourceReader
    {
        Task ReadSource(string source);

        PriceSourceType SourceType { get; }

    }
}
