# Makefile for building Win32 API test executables
# Can be used for cross-compilation with MinGW-w64 on Linux

# Compiler settings
CC = i686-w64-mingw32-gcc
CFLAGS = -Wall -Wextra -O2 -m32
LDFLAGS = -lkernel32

# Output directory
OUTDIR = ../EXEs/NativeTests

# Source files
SOURCES = test_getmodulefilename.c test_environment.c

# Output executables
EXECUTABLES = $(SOURCES:.c=.exe)
OUTPUT_EXES = $(addprefix $(OUTDIR)/, $(EXECUTABLES))

# Default target
all: $(OUTPUT_EXES)

# Create output directory
$(OUTDIR):
	mkdir -p $(OUTDIR)

# Build each executable
$(OUTDIR)/%.exe: %.c | $(OUTDIR)
	$(CC) $(CFLAGS) -o $@ $< $(LDFLAGS)
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
