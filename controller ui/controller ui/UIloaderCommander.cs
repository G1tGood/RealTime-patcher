using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controller_ui
{
    class UIloaderCommander: loaderCommander
    {
        private Patcher patcher;
        UIloaderCommander(Patcher _patcher) : base(){
            patcher = _patcher;
        }
        public override ulong find(string regex, uint index = 0)
        {
            throw new NotImplementedException();
        }
        protected override void writeMemmory(UInt64 address, byte[] data)
        {
            this.patcher.write_memory(address, data);
        }
        protected override byte[] readMemmory(UInt64 address, uint len)
        {
            throw new NotImplementedException();
        }


    }
}
