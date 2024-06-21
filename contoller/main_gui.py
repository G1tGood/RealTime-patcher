import tkinter as tk
from tkinter import ttk
import process_explorer
from PIL import ImageTk
root = tk.Tk()
root.title("chose a process")
def center_window(window, width, height):
    screen_width = window.winfo_screenwidth()
    screen_height = window.winfo_screenheight()

    x_coordinate = (screen_width - width) // 2
    y_coordinate = (screen_height - height) // 2

    window.geometry(f"{width}x{height}+{x_coordinate}+{y_coordinate}")
center_window(root,600,500)

def on_configure(event):
    # Update the scroll region to cover the entire canvas
    canvas.configure(scrollregion=canvas.bbox("all"))

def on_mousewheel(event):
    canvas.yview_scroll(int(-1*(event.delta/120)), "units")

# Create a Canvas widget
canvas = tk.Canvas(root)
canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

# Create a frame inside the canvas to hold the list
frame = tk.Frame(canvas)
canvas.create_window((0, 0), window=frame, anchor=tk.NW)

# Add a scrollbar
scrollbar = tk.Scrollbar(root, command=canvas.yview)
scrollbar.pack(side=tk.RIGHT, fill=tk.Y)

# Configure the canvas to use the scrollbar
canvas.configure(yscrollcommand=scrollbar.set)
canvas.bind('<Configure>', on_configure)
# Bind mouse wheel event to the canvas for scrolling
canvas.bind_all("<MouseWheel>", on_mousewheel)

IMAGE_SIZE=12
default_exe_icon=ImageTk.PhotoImage(process_explorer.get_exe_icon(r"%systemroot%\system32\imageres.dll",12,12,11))
def proc_frame(parent,proc:process_explorer.psutil.Process)->tk.Frame:
    proc_frame = tk.Frame(parent, borderwidth=0, bg="white", relief=tk.SUNKEN,height=20,background="white")
    proc_frame.pack(side=tk.TOP, fill="x")
    try:
        proc_ico =ImageTk.PhotoImage(process_explorer.get_exe_icon(proc.exe(),IMAGE_SIZE,IMAGE_SIZE))
        proc_frame.picture =proc_ico
        proc_frame.label = tk.Label(proc_frame, image=proc_frame.picture,width=15,background="white")
        proc_frame.label.pack(side=tk.LEFT)
        # Create a Label Widget to display the text or Image
    except:
        proc_frame.picture =default_exe_icon
        proc_frame.label = tk.Label(proc_frame, image=proc_frame.picture,width=15,background="white")
        proc_frame.label.pack(side=tk.LEFT)
    proc_name = tk.Label(proc_frame,text=proc.name(),background="white",width=17,anchor="w")
    proc_name.pack(side=tk.LEFT)
    
    proc_PID = tk.Label(proc_frame,text=proc.pid,background="white",width=5,anchor="w")
    proc_PID.pack(side=tk.LEFT)
    try:
        proc_path = tk.Label(proc_frame,text=proc.exe(),background="white",width=70,anchor="w")
        proc_path.pack(side=tk.LEFT)
    except:pass
    return proc_frame


for i in process_explorer.list_processes():
    proc_frame(frame,i)
root.mainloop()
exit()

