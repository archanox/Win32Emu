# Makefile for Win32Emu DirectDraw Examples
# Uses MinGW cross-compiler to build 32-bit Windows executables

CC = i686-w64-mingw32-gcc
CXX = i686-w64-mingw32-g++
WINDRES = i686-w64-mingw32-windres

# Compiler flags
CFLAGS = -O2 -Wall -m32 -mwindows
CXXFLAGS = -O2 -Wall -m32 -mwindows -fno-exceptions -fno-rtti

# Linker flags
LDFLAGS = -m32 -mwindows -Wl,--subsystem,windows
LIBS = -lkernel32 -luser32 -lgdi32 -lddraw -luuid -ldxguid

# Targets
EXAMPLES = hugi.exe simple_ddraw.exe

.PHONY: all clean

all: $(EXAMPLES)

# Hugi DirectDraw example (C version)
hugi.exe: hugi.c
	$(CC) $(CFLAGS) $< -o $@ $(LDFLAGS) $(LIBS)

# Simple DirectDraw example (C version)
simple_ddraw.exe: simple_ddraw.c
	$(CC) $(CFLAGS) $< -o $@ $(LDFLAGS) $(LIBS)

# Clean build artifacts
clean:
	rm -f $(EXAMPLES) *.o
