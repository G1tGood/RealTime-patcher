using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controller_ui
{
    class MyModule
    {
        public MyModule(string _name, string _path)
        {
            this.sections = new List<MySection>();
            this.path = _path;
            this.name = _name;
        }
        public void addSection(MySection section)
        {
            this.sections.Add(section);
        }
        public class MySection
        {
            public MySection(string _baseAdress, uint _RegionSize,int _Protect,string _name= "unnamed")
            {
                this.baseAdress = _baseAdress;
                this.regionSize = _RegionSize;
                this.protect = _Protect;
                this.name = _name;
            }
            private string name;
            public string Name{
                get
                {
                    return this.name;
                }
                set
                {
                    this.name = value;
                }
            }
            private string baseAdress;
            public string BaseAdress
            {
                get
                {
                    return this.baseAdress;
                }
                private set
                {
                    this.baseAdress = value;
                }
            }
            private uint regionSize;
            public uint RegionSize
            {
                get
                {
                    return this.regionSize;
                }
                private set
                {
                    this.regionSize = value;
                }
            }
            private int protect;
            public int Protect
            {
                get
                {
                    return this.protect;
                }
                private set
                {
                    this.protect = value;
                }
            }
        }
        private string name;
        public string Name
        {
            get
            {
                return this.name;
            }
            private set
            {
                this.name = value;
            }
        }

        private string path;
        public string Path
        {
            get
            {
                return this.path;
            }
            private set
            {
                this.path = value;
            }
        }
        private List<MySection> sections;
        public List<MySection> Sections
        {
            get
            {
                return this.sections;
            }
            private set
            {
                this.sections = value;
            }
        }

    };
}
