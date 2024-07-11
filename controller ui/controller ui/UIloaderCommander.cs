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
        public UIloaderCommander(Patcher _patcher) : base(){
            patcher = _patcher;
        }
        public override ulong find(Byte[] what, ulong startAdress = 0)
        {
            throw new NotImplementedException();
        }
        protected override void writeMemory(UInt64 address, byte[] data)
        {
            this.patcher.write_memory(address, data);
        }
        protected override byte[] readMemory(UInt64 address, uint len)
        {
            return patcher.read_memory(address, len);
        }
    }
}
