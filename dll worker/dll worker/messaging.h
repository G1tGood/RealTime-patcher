#pragma once
#include<windows.h>
#include<string>
class Messager {
	int buff_size;
	std::wstring pipeName;
	HANDLE pipe;
public:
	Messager(std::wstring pipeIdentifier,int buff_size = 4096) {
		this->pipeName = L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_" + pipeIdentifier;
		this->buff_size = buff_size+1;
		this->pipe;
		pipe = CreateFile(pipeName.c_str(), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
		if (pipe == INVALID_HANDLE_VALUE) {
			pipe = NULL;
			throw "faild to open pipe";
		}
	}
	~Messager() {
		if (pipe&&pipe!=INVALID_HANDLE_VALUE) {
			CloseHandle(pipe);
		}
	}
	/** read last message to given buffer
	* @return amount of bytes readen, -1 when error
	*/
	DWORD read_message(BYTE* buffer, size_t len) {
		DWORD bytesRead;
		if (!ReadFile(this->pipe, buffer, len, &bytesRead, NULL)) {
			return -1;//probably pipe closed,but can be a rare error
		}
		return bytesRead;
	}
	/** write message from buffer
	* @return amount of bytes writen, -1 when error
	*/
	DWORD write_message(BYTE* buffer) {
		DWORD bytesRead;
		if (!WriteFile(this->pipe, buffer, buff_size, &bytesRead, NULL)) {
			return -1;//probably pipe closed,but can be a rare error
		}
		FlushFileBuffers(this->pipe);//why wait?
		return bytesRead;
	}
	/** write message from buffer
	* @return amount of bytes writen, -1 when error
	*/
	DWORD write_message(BYTE* buffer,size_t len) {
		DWORD bytesRead;
		if (!WriteFile(this->pipe, buffer, len, &bytesRead, NULL)) {
			return -1;//probably pipe closed,but can be a rare error
		}
		FlushFileBuffers(this->pipe);//why wait?
		return bytesRead;
	}
	std::wstring get_pipeName() {
		return this->pipeName;
	}
};