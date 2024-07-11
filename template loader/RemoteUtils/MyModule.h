#pragma once
#include <msclr\auto_gcroot.h>
#include <msclr/marshal_cppstd.h>

using namespace System;
namespace RemoteUtils {

    public ref class MyModule
    {
    public:
        ref class MySection
        {
        public:
            MySection(String^ _baseAdress, UINT64 _RegionSize, int _Protect, String^ _name)
            {
                this->BaseAdress = _baseAdress;
                this->RegionSize = _RegionSize;
                this->Protect = _Protect;
                this->Name = _name;
            }
            MySection(String^ _baseAdress, UINT64 _RegionSize, int _Protect)
            {
                this->BaseAdress = _baseAdress;
                this->RegionSize = _RegionSize;
                this->Protect = _Protect;
                this->Name = gcnew String("unnamed");
            }
            String^ Name;
            String^ BaseAdress;
            UINT64 RegionSize;
            int Protect;
        };
        MyModule(String^ _name, String^ _path)
        {
            this->Sections = gcnew Collections::Generic::List<MySection^>();
            this->Path = _path;
            this->Name = _name;
        }
        void addSection(MySection^ section)
        {
            this->Sections->Add(section);
        }
        String^ Name;
        String^ Path;
        System::Collections::Generic::List<MySection^>^ Sections;
    };
}