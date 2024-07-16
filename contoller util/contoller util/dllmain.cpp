#include "pch.h"
#include<windows.h>
#include<psapi.h>
#include<stdlib.h>
#include<string.h>
#include<cstring>
#include<string>

extern "C" {

	__declspec(dllexport) int inject(int pid, wchar_t* dll_path) {
		LPVOID Loadlib = (LPVOID)GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryW");
		if (!Loadlib)
		{
			return GetLastError();
		}
		HANDLE target_proc = OpenProcess(PROCESS_ALL_ACCESS, 0, pid);
		if (!target_proc)
		{
			return GetLastError();
		}
		PVOID target_mem = (PVOID)VirtualAllocEx(target_proc, 0, (wcslen(dll_path)+1)*sizeof(wchar_t), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
		if (!target_mem)
		{
			return GetLastError();
		}
		if (!WriteProcessMemory(target_proc, target_mem, dll_path, (wcslen(dll_path) + 1) * sizeof(wchar_t), 0))
		{
			return GetLastError();
		}
		HANDLE hRemote = CreateRemoteThread(target_proc, 0, 0, (LPTHREAD_START_ROUTINE)Loadlib, target_mem, 0, 0);
		if (!hRemote)
		{
			return GetLastError();
		}
		WaitForSingleObject(hRemote, INFINITE);
		if (!VirtualFreeEx(target_proc, target_mem, wcslen(dll_path) + 1, MEM_DECOMMIT))
		{
			return GetLastError();
		}

		CloseHandle(hRemote);
		CloseHandle(target_proc);
		return 0;
	}
	


}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:

	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


