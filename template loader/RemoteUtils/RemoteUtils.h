#pragma once
#include<windows.h>
#include<psapi.h>

#include <msclr\auto_gcroot.h>
#include <msclr/marshal_cppstd.h>
#include<string>
#include "MyModule.h"
using namespace msclr::interop;
#define MY_MAX_PATH 512
namespace RemoteUtils {
    public ref class remoteUtils {
    private:
        int pid;
        HANDLE proc;
    public:
        remoteUtils(int pid) {
            proc = OpenProcess(PROCESS_ALL_ACCESS, 0, pid);
            if (!proc) {
                throw gcnew System::Exception("cannot open the specified process");
            }
        }
        cli::array<unsigned char>^ read_memory(UINT64 address, UINT64 len) {
            cli::array<unsigned char>^ byteArray = gcnew cli::array<unsigned char>(len);
            SIZE_T readenBytes = 0;
            pin_ptr<byte> p = &byteArray[0];
            byte* cp = p;
            if (!ReadProcessMemory(this->proc, (LPVOID)address,cp , len, &readenBytes)) {
                throw gcnew System::Exception("reading failed");
            }
            return byteArray;
        }

        void write_memory(UINT64 address, cli::array<unsigned char>^ data) {
            SIZE_T writeBytes = 0;
            pin_ptr<byte> p = &data[0];
            byte* cp = p;

            if (!WriteProcessMemory(this->proc, (LPVOID)address, cp, data->Length, &writeBytes)) {
                throw gcnew System::Exception("writing failed");
            }
        }

        HMODULE GetModuleHandleFromAddress(LPVOID address) {

            HMODULE hModules[1024];
            DWORD cbNeeded;
            if (EnumProcessModules(this->proc, hModules, sizeof(hModules), &cbNeeded)) {
                for (DWORD i = 0; i < (cbNeeded / sizeof(HMODULE)); i++) {
                    MODULEINFO moduleInfo;
                    if (GetModuleInformation(this->proc, hModules[i], &moduleInfo, sizeof(moduleInfo))) {
                        if (reinterpret_cast<LPBYTE>(address) >= reinterpret_cast<LPBYTE>(moduleInfo.lpBaseOfDll) &&
                            reinterpret_cast<LPBYTE>(address) < reinterpret_cast<LPBYTE>(moduleInfo.lpBaseOfDll) + moduleInfo.SizeOfImage) {
                            return hModules[i];
                        }
                    }
                    else {
                        throw gcnew System::Exception("GetModuleInformation failed");
                    }
                }
            }
            return NULL;
        }
        void get_module_sections(MyModule^ my_module) {
#ifdef _WIN64
            MEMORY_BASIC_INFORMATION64  memInfo;//MEMORY_BASIC_INFORMATION64
#else
            MEMORY_BASIC_INFORMATION32  memInfo;//MEMORY_BASIC_INFORMATION64
#endif    
            LPVOID baseAddress = 0;

            while (VirtualQueryEx(this->proc, baseAddress, (PMEMORY_BASIC_INFORMATION)&memInfo, sizeof(memInfo))) {
                using namespace Runtime::InteropServices;

                if (memInfo.State != MEM_FREE) {
                    HMODULE hmodule = GetModuleHandleFromAddress(baseAddress);
                    /*if (!GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                        (LPCSTR)baseAddress, &hmodule)) {

                        throw gcnew System::Exception("GetModuleHandleExA failed");
                    }*/

                    wchar_t lpFilename[MY_MAX_PATH];
                    if (GetModuleFileNameW(hmodule, lpFilename, MY_MAX_PATH)) {
                        if (!wcscmp((const wchar_t*)(Marshal::StringToHGlobalUni(my_module->Path)).ToPointer(), lpFilename)) {
                            my_module->addSection(gcnew MyModule::MySection(
                                Convert::ToString((long long)memInfo.BaseAddress, 16),
                                memInfo.RegionSize,
                                memInfo.Protect
                            ));
                        }
                    }
                    else {
                        throw gcnew System::Exception("VirtualQuery failed");
                    }
                }
                baseAddress = (LPVOID)(memInfo.BaseAddress + memInfo.RegionSize);
            }
        }

        MyModule::MySection^ get_address_section(UINT64 address) {
#ifdef _WIN64
            MEMORY_BASIC_INFORMATION64  memInfo;//MEMORY_BASIC_INFORMATION64
#else
            MEMORY_BASIC_INFORMATION32  memInfo;//MEMORY_BASIC_INFORMATION64
#endif    
            if (VirtualQueryEx(this->proc, (LPCVOID)address, (PMEMORY_BASIC_INFORMATION)&memInfo, sizeof(memInfo))) {
                if (memInfo.State != MEM_FREE) {
                    return gcnew MyModule::MySection(Convert::ToString((long long)memInfo.BaseAddress, 16),
                        memInfo.RegionSize,
                        memInfo.Protect
                    );
                    //memInfo.Type,
                    //memInfo.State,
                    //memInfo.AllocationProtect
                }
            }
            else {
                throw gcnew System::Exception("VirtualQuery failed");
            }
        }

        int inject(System::String^ dll_path) {
            //std::wstring wsdll_path = marshal_as<std::wstring>(dll_path);
            const wchar_t* wtdll_path = marshal_as<wchar_t*>(dll_path);//wsdll_path.c_str();

            LPVOID Loadlib = (LPVOID)GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryW");
            if (!Loadlib)
            {
                return GetLastError();
            }
            PVOID target_mem = (PVOID)VirtualAllocEx(this->proc, 0, (wcslen(wtdll_path) + 1) * sizeof(wchar_t), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!target_mem)
            {
                return GetLastError();
            }
            if (!WriteProcessMemory(this->proc, target_mem, wtdll_path, (wcslen(wtdll_path) + 1) * sizeof(wchar_t), 0))
            {
                return GetLastError();
            }
            HANDLE hRemote = CreateRemoteThread(this->proc, 0, 0, (LPTHREAD_START_ROUTINE)Loadlib, target_mem, 0, 0);
            if (!hRemote)
            {
                return GetLastError();
            }
            WaitForSingleObject(hRemote, INFINITE);
            if (!VirtualFreeEx(this->proc, target_mem, wcslen(wtdll_path) + 1, MEM_DECOMMIT))
            {
                return GetLastError();
            }
            CloseHandle(hRemote);
            return 0;
        }
    };

}
