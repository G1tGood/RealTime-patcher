import win32pipe
import win32file
from win32pipe import PIPE_ACCESS_DUPLEX,PIPE_WAIT,PIPE_READMODE_BYTE,PIPE_REJECT_REMOTE_CLIENTS,PIPE_UNLIMITED_INSTANCES


class messager:
    def __init__(self,pipeIdentifier:str,buff_size:int=4096):
        self.__buff_size=buff_size
        self.__pipeName=r"\\.\pipe\yesodot_memory_Dynamic_Memory_Patcher_"+pipeIdentifier
        self.__pipe=win32pipe.CreateNamedPipe(self.__pipeName,
            PIPE_ACCESS_DUPLEX,
            PIPE_WAIT|PIPE_READMODE_BYTE|PIPE_REJECT_REMOTE_CLIENTS,
            PIPE_UNLIMITED_INSTANCES ,
            self.__buff_size,
            self.__buff_size,
            0,
            None
        )
        win32pipe.ConnectNamedPipe(self.__pipe,None)
    def write_message(self,message:bytes):
        win32file.WriteFile(self.__pipe,message,None)
        win32file.FlushFileBuffers(self.__pipe)

    def read_message(self)->tuple[int, bytes]:
        return win32file.ReadFile(self.__pipe,self.__buff_size)

