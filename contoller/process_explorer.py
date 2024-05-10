import psutil
def list_processes()->list[psutil.Process]:
    processes = []
    for proc in psutil.process_iter():
        try:
            pinfo = proc
            processes.append(pinfo)
        except psutil.NoSuchProcess:
            pass
    return processes
def find_process_by_name(name:str)->psutil.Process:
    for proc in psutil.process_iter():
        try:
           if proc.name()==name:
               return proc
        except psutil.NoSuchProcess:
            pass
