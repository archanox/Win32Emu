# Makefile for building Win32 API test executables
# Can be used for cross-compilation with MinGW-w64 on Linux

# Compiler settings
CC = i686-w64-mingw32-gcc
CFLAGS = -Wall -Wextra -O2 -m32

# Output directory
OUTDIR = ../EXEs/NativeTests

# Source files
SOURCES = test_getmodulefilename.c test_environment.c \
          test_heap.c test_virtualalloc.c test_fileio.c test_version.c \
          test_commandline.c test_procaddress.c test_messages.c test_multimedia.c

# Output executables
EXECUTABLES = $(SOURCES:.c=.exe)
OUTPUT_EXES = $(addprefix $(OUTDIR)/, $(EXECUTABLES))

# Default target
all: $(OUTPUT_EXES)

# Create output directory
$(OUTDIR):
	mkdir -p $(OUTDIR)

# Build rules with target-specific library dependencies
# Tests that only need kernel32
$(OUTDIR)/test_getmodulefilename.exe: test_getmodulefilename.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

$(OUTDIR)/test_environment.exe: test_environment.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

$(OUTDIR)/test_heap.exe: test_heap.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

$(OUTDIR)/test_virtualalloc.exe: test_virtualalloc.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

$(OUTDIR)/test_fileio.exe: test_fileio.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

$(OUTDIR)/test_version.exe: test_version.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

$(OUTDIR)/test_commandline.exe: test_commandline.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

$(OUTDIR)/test_procaddress.exe: test_procaddress.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32
	@echo "Built $@"

# Tests that need kernel32 and user32
$(OUTDIR)/test_messages.exe: test_messages.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32 -luser32 -lgdi32
	@echo "Built $@"

# Tests that need kernel32 and winmm
$(OUTDIR)/test_multimedia.exe: test_multimedia.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< -lkernel32 -lwinmm
	@echo "Built $@"

# Clean build artifacts
clean:
	rm -f $(OUTPUT_EXES)
	@echo "Cleaned build artifacts"

# Phony targets
.PHONY: all clean

# Help target
help:
	@echo "Win32 API Test Executables Makefile"
	@echo "===================================="
	@echo ""
	@echo "Targets:"
	@echo "  all     - Build all test executables (default)"
	@echo "  clean   - Remove built executables"
	@echo "  help    - Show this help message"
	@echo ""
	@echo "Usage:"
	@echo "  make              # Build all tests"
	@echo "  make clean        # Clean build artifacts"
	@echo ""
	@echo "Cross-compilation:"
	@echo "  Requires i686-w64-mingw32-gcc (MinGW-w64)"
	@echo "  Install on Ubuntu/Debian: sudo apt-get install mingw-w64"
	@echo ""
	@echo "Building on Windows:"
	@echo "  Use Visual Studio projects or:"
	@echo "  cl test_getmodulefilename.c /Fe:test_getmodulefilename.exe"
	@echo "  cl test_environment.c /Fe:test_environment.exe"
