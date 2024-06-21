import psutil
import win32ui
import win32gui
import win32con
import win32api
from PIL import Image
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
def find_processes_by_name(name:str)->psutil.Process:
    procs=[]
    for proc in psutil.process_iter():
        try:
           if proc.name()==name:
               procs.append(proc)
        except psutil.NoSuchProcess:
            pass
    return procs
def get_exe_icon(exe_path,width,height,index=0,hd=0)->Image:#correct path assumed
    try:
        ico_x = win32api.GetSystemMetrics(win32con.SM_CXICON)
        ico_y = win32api.GetSystemMetrics(win32con.SM_CYICON)
        large, small = win32gui.ExtractIconEx(exe_path,index)
        hdc = win32ui.CreateDCFromHandle( win32gui.GetDC(0) )
        hbmp = win32ui.CreateBitmap()
        hbmp.CreateCompatibleBitmap( hdc, ico_x, ico_y )
        hdc = hdc.CreateCompatibleDC()
        hdc.SelectObject( hbmp )
        hdc.FillSolidRect((0, 0, ico_x, ico_y), 0xffffff) 
        hdc.DrawIcon( (0,0), small[0] )
        bmp_str = hbmp.GetBitmapBits(True)
        pil_image = Image.frombuffer(
            'RGB',(ico_x,ico_y),bmp_str,'raw','BGRX',0,1
        )
        win32gui.DestroyIcon(small[0])
        win32gui.DestroyIcon(large[0])
        return pil_image.resize((width,height))

    except Exception as e:
        print(f"An error occurred: {e}")
        return None